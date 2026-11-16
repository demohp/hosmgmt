using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages.Admin;
using ClinicManagement.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.UnitTests.Pages.Admin;

/// <summary>Unit tests for ManageClinicModel page model.</summary>
public class ManageClinicModelTests
{
    private readonly Mock<IAdminService> _adminServiceMock;
    private readonly Mock<ILogger<ManageClinicModel>> _loggerMock;
    private readonly ManageClinicModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public ManageClinicModelTests()
    {
        _adminServiceMock = new Mock<IAdminService>();
        _loggerMock = new Mock<ILogger<ManageClinicModel>>();
        _pageModel = new ManageClinicModel(_adminServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new ManageClinicModel(_adminServiceMock.Object, _loggerMock.Object);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnGetAsync(null, null, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidAdmin_LoadsDoctorsAndStaff()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        var doctors = new List<DoctorListItemDto> { new DoctorListItemDto { DoctorId = 1, Name = "Dr. Smith", Department = "Cardiology" } };
        var staff = new List<StaffListItemDto> { new StaffListItemDto { StaffId = 1, Name = "John Nurse", Designation = "Nurse" } };
        _adminServiceMock.Setup(s => s.LoadDoctorsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(doctors);
        _adminServiceMock.Setup(s => s.LoadStaffAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(staff);

        // Act
        var result = await _pageModel.OnGetAsync(null, null, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Doctors);
        Assert.NotNull(_pageModel.Staff);
        Assert.Single(_pageModel.Doctors!);
        Assert.Single(_pageModel.Staff!);
    }

    [Fact]
    public async Task OnGetAsync_WhenSearchProvided_PassesSearchToService()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.LoadDoctorsAsync("Smith", It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorListItemDto>());
        _adminServiceMock.Setup(s => s.LoadStaffAsync("Nurse", It.IsAny<CancellationToken>())).ReturnsAsync(new List<StaffListItemDto>());

        // Act
        var result = await _pageModel.OnGetAsync("Smith", "Nurse", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Smith", _pageModel.DoctorSearch);
        Assert.Equal("Nurse", _pageModel.StaffSearch);
    }

    [Fact]
    public async Task OnGetAsync_WhenNullSearch_UsesEmptyString()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.LoadDoctorsAsync(string.Empty, It.IsAny<CancellationToken>())).ReturnsAsync(new List<DoctorListItemDto>());
        _adminServiceMock.Setup(s => s.LoadStaffAsync(string.Empty, It.IsAny<CancellationToken>())).ReturnsAsync(new List<StaffListItemDto>());

        // Act
        var result = await _pageModel.OnGetAsync(null, null, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal(string.Empty, _pageModel.DoctorSearch);
        Assert.Equal(string.Empty, _pageModel.StaffSearch);
    }

    [Fact]
    public async Task OnPostDeleteDoctorAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnPostDeleteDoctorAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostDeleteDoctorAsync_WhenValidAdmin_DeletesDoctorAndRedirects()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.DeleteDoctorAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _pageModel.OnPostDeleteDoctorAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        _adminServiceMock.Verify(s => s.DeleteDoctorAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnPostDeleteDoctorAsync_WhenServiceThrowsException_RedirectsToPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.DeleteDoctorAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnPostDeleteDoctorAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostDeleteStaffAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnPostDeleteStaffAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostDeleteStaffAsync_WhenValidAdmin_DeletesStaffAndRedirects()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.DeleteStaffAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _pageModel.OnPostDeleteStaffAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        _adminServiceMock.Verify(s => s.DeleteStaffAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnPostDeleteStaffAsync_WhenServiceThrowsException_RedirectsToPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.DeleteStaffAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnPostDeleteStaffAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
    }
}
