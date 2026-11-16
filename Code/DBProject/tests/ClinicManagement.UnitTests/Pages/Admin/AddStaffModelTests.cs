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

/// <summary>Unit tests for AddStaffModel page model.</summary>
public class AddStaffModelTests
{
    private readonly Mock<IAdminService> _adminServiceMock;
    private readonly Mock<ILogger<AddStaffModel>> _loggerMock;
    private readonly AddStaffModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public AddStaffModelTests()
    {
        _adminServiceMock = new Mock<IAdminService>();
        _loggerMock = new Mock<ILogger<AddStaffModel>>();
        _pageModel = new AddStaffModel(_adminServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new AddStaffModel(_adminServiceMock.Object, _loggerMock.Object);
        Assert.NotNull(model);
    }

    [Fact]
    public void OnGet_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = _pageModel.OnGet();
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public void OnGet_WhenValidAdmin_ReturnsPage()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        var result = _pageModel.OnGet();
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnPostAsync("John", "1990-01-01", "1234567890", "M", "Address", 50000, "BSc", "Nurse", CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenAddStaffSucceeds_SetsSuccessMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.AddStaffAsync(It.IsAny<AddStaffDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _pageModel.OnPostAsync("John", "1990-01-01", "1234567890", "M", "123 Main St", 50000, "BSc", "Nurse", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Staff member added successfully!", _pageModel.SuccessMessage);
        Assert.Null(_pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenAddStaffFails_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.AddStaffAsync(It.IsAny<AddStaffDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

        // Act
        var result = await _pageModel.OnPostAsync("John", "1990-01-01", "1234567890", "M", "123 Main St", 50000, "BSc", "Nurse", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Failed to add staff member. Please try again.", _pageModel.ErrorMessage);
        Assert.Null(_pageModel.SuccessMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.AddStaffAsync(It.IsAny<AddStaffDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnPostAsync("John", "1990-01-01", "1234567890", "M", "123 Main St", 50000, "BSc", "Nurse", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("An unexpected error occurred.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenGenderIsEmpty_UsesDefaultMaleGender()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        AddStaffDto? capturedDto = null;
        _adminServiceMock.Setup(s => s.AddStaffAsync(It.IsAny<AddStaffDto>(), It.IsAny<CancellationToken>()))
            .Callback<AddStaffDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(1);

        // Act
        await _pageModel.OnPostAsync("John", "1990-01-01", "1234567890", string.Empty, "123 Main St", 50000, "BSc", "Nurse", CancellationToken.None);

        // Assert
        Assert.NotNull(capturedDto);
        Assert.Equal('M', capturedDto!.Gender);
    }

    [Fact]
    public async Task OnPostAsync_WhenGenderIsFemale_UsesFemaleGender()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        AddStaffDto? capturedDto = null;
        _adminServiceMock.Setup(s => s.AddStaffAsync(It.IsAny<AddStaffDto>(), It.IsAny<CancellationToken>()))
            .Callback<AddStaffDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(1);

        // Act
        await _pageModel.OnPostAsync("Jane", "1990-01-01", "1234567890", "F", "123 Main St", 50000, "BSc", "Nurse", CancellationToken.None);

        // Assert
        Assert.NotNull(capturedDto);
        Assert.Equal('F', capturedDto!.Gender);
    }
}
