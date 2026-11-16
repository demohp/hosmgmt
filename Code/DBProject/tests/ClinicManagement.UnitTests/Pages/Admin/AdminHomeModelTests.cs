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

/// <summary>Unit tests for AdminHomeModel page model.</summary>
public class AdminHomeModelTests
{
    private readonly Mock<IAdminService> _adminServiceMock;
    private readonly Mock<ILogger<AdminHomeModel>> _loggerMock;
    private readonly AdminHomeModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public AdminHomeModelTests()
    {
        _adminServiceMock = new Mock<IAdminService>();
        _loggerMock = new Mock<ILogger<AdminHomeModel>>();
        _pageModel = new AdminHomeModel(_adminServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new AdminHomeModel(_adminServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1); // Patient
        var result = await _pageModel.OnGetAsync(CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenDoctorUserType_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2); // Doctor
        var result = await _pageModel.OnGetAsync(CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidAdmin_LoadsHomeData()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        var homeData = new AdminHomeDto { TotalDoctors = 10, TotalPatients = 50, TotalIncome = 5000.0 };
        _adminServiceMock.Setup(s => s.GetAdminHomeInformationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(homeData);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.HomeData);
        Assert.Equal(10, _pageModel.HomeData!.TotalDoctors);
        Assert.Equal(50, _pageModel.HomeData.TotalPatients);
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsEmptyHomeData()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _adminServiceMock.Setup(s => s.GetAdminHomeInformationAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.HomeData);
        Assert.Equal(0, _pageModel.HomeData!.TotalDoctors);
    }
}
