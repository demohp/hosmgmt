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

/// <summary>Unit tests for PreviousHistoryModel page model.</summary>
public class PreviousHistoryModelTests
{
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly Mock<ILogger<PreviousHistoryModel>> _loggerMock;
    private readonly PreviousHistoryModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public PreviousHistoryModelTests()
    {
        _doctorServiceMock = new Mock<IDoctorService>();
        _loggerMock = new Mock<ILogger<PreviousHistoryModel>>();
        _pageModel = new PreviousHistoryModel(_doctorServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new PreviousHistoryModel(_doctorServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenValidDoctor_LoadsHistory()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        var history = new List<PatientHistoryItemDto>
        {
            new PatientHistoryItemDto { AppointId = 1, PatientName = "John Doe", Disease = "Flu" }
        };
        _doctorServiceMock.Setup(s => s.GetPatientHistoryAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(history);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.History);
        Assert.Single(_pageModel.History!);
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsEmptyHistory()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.GetPatientHistoryAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.History);
        Assert.Empty(_pageModel.History!);
    }

    [Fact]
    public async Task OnGetAsync_WhenEmptyHistory_ReturnsEmptyList()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.GetPatientHistoryAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientHistoryItemDto>());

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.History);
        Assert.Empty(_pageModel.History!);
    }
}
