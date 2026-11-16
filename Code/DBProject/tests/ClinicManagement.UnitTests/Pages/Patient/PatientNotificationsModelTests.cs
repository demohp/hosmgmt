using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages.Patient;
using ClinicManagement.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.UnitTests.Pages.Patient;

/// <summary>Unit tests for PatientNotificationsModel page model.</summary>
public class PatientNotificationsModelTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<PatientNotificationsModel>> _loggerMock;
    private readonly PatientNotificationsModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public PatientNotificationsModelTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<PatientNotificationsModel>>();
        _pageModel = new PatientNotificationsModel(_patientServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new PatientNotificationsModel(_patientServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 2);
        var result = await _pageModel.OnGetAsync(CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidPatient_LoadsNotification()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var notification = new NotificationDto { DoctorName = "Dr. Smith", Timings = "10:00 AM", Count = 1 };
        _patientServiceMock.Setup(s => s.GetNotificationsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Notification);
        Assert.Equal("Dr. Smith", _pageModel.Notification!.DoctorName);
        Assert.Equal(1, _pageModel.Notification.Count);
    }

    [Fact]
    public async Task OnGetAsync_WhenNoNotification_NotificationIsNull()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetNotificationsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationDto?)null);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.Notification);
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetNotificationsAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.Notification);
    }
}
