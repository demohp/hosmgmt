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

/// <summary>Unit tests for PatientHomeModel page model.</summary>
public class PatientHomeModelTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<PatientHomeModel>> _loggerMock;
    private readonly PatientHomeModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public PatientHomeModelTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<PatientHomeModel>>();
        _pageModel = new PatientHomeModel(_patientServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new PatientHomeModel(_patientServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenValidPatient_LoadsPatientInfoAndAppointment()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var patientInfo = new PatientDetailDto { PatientId = 1, Name = "John Doe" };
        var appointment = new CurrentAppointmentDto { DoctorName = "Dr. Smith", Timings = "10:00 AM" };
        _patientServiceMock.Setup(s => s.GetPatientInfoAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patientInfo);
        _patientServiceMock.Setup(s => s.GetCurrentAppointmentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(appointment);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.PatientInfo);
        Assert.Equal("John Doe", _pageModel.PatientInfo!.Name);
        Assert.NotNull(_pageModel.CurrentAppointment);
        Assert.Equal("Dr. Smith", _pageModel.CurrentAppointment!.DoctorName);
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetPatientInfoAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_WhenNoCurrentAppointment_PatientInfoLoadedAppointmentNull()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var patientInfo = new PatientDetailDto { PatientId = 1, Name = "Jane Doe" };
        _patientServiceMock.Setup(s => s.GetPatientInfoAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patientInfo);
        _patientServiceMock.Setup(s => s.GetCurrentAppointmentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((CurrentAppointmentDto?)null);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.PatientInfo);
        Assert.Null(_pageModel.CurrentAppointment);
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
