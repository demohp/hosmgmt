using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages.Doctor;
using ClinicManagement.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.UnitTests.Pages.Doctor;

/// <summary>Unit tests for DoctorHomeModel page model.</summary>
public class DoctorHomeModelTests
{
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly Mock<ILogger<DoctorHomeModel>> _loggerMock;
    private readonly DoctorHomeModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public DoctorHomeModelTests()
    {
        _doctorServiceMock = new Mock<IDoctorService>();
        _loggerMock = new Mock<ILogger<DoctorHomeModel>>();
        _pageModel = new DoctorHomeModel(_doctorServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new DoctorHomeModel(_doctorServiceMock.Object, _loggerMock.Object);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnGetAsync(CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnGetAsync(CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidDoctor_LoadsDoctorInfo()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        var doctorInfo = new DoctorProfileDto { DoctorId = 2, Name = "Dr. Smith", Specialization = "Cardiology", Department = "Cardiology" };
        _doctorServiceMock.Setup(s => s.GetDoctorInfoAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(doctorInfo);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.DoctorInfo);
        Assert.Equal("Dr. Smith", _pageModel.DoctorInfo!.Name);
        Assert.Equal("Cardiology", _pageModel.DoctorInfo.Specialization);
    }

    [Fact]
    public async Task OnGetAsync_WhenDoctorNotFound_DoctorInfoIsNull()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.GetDoctorInfoAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DoctorProfileDto?)null);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.DoctorInfo);
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.GetDoctorInfoAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.DoctorInfo);
    }

    [Fact]
    public async Task OnGetAsync_WhenAdminUserType_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 3);
        var result = await _pageModel.OnGetAsync(CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }
}
