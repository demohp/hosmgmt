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

/// <summary>Unit tests for AppointmentTakerModel page model.</summary>
public class AppointmentTakerModelTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<AppointmentTakerModel>> _loggerMock;
    private readonly AppointmentTakerModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public AppointmentTakerModelTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<AppointmentTakerModel>>();
        _pageModel = new AppointmentTakerModel(_patientServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new AppointmentTakerModel(_patientServiceMock.Object, _loggerMock.Object);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnGetAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 2);
        var result = await _pageModel.OnGetAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidPatient_LoadsDoctorProfileAndSlots()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var doctorProfile = new DoctorProfileDto { DoctorId = 5, Name = "Dr. Smith" };
        var slots = new List<AppointmentSlotDto>
        {
            new AppointmentSlotDto { SlotId = 1, Timings = "10:00 AM" },
            new AppointmentSlotDto { SlotId = 2, Timings = "11:00 AM" }
        };
        _patientServiceMock.Setup(s => s.GetDoctorProfileAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(doctorProfile);
        _patientServiceMock.Setup(s => s.GetFreeSlotsAsync(5, 1, It.IsAny<CancellationToken>())).ReturnsAsync(slots);

        // Act
        var result = await _pageModel.OnGetAsync(5, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.DoctorProfile);
        Assert.Equal("Dr. Smith", _pageModel.DoctorProfile!.Name);
        Assert.NotNull(_pageModel.FreeSlots);
        Assert.Equal(2, _pageModel.FreeSlots!.Count());
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetDoctorProfileAsync(5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(5, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Error loading appointment information.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnPostAsync(1, 1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenAppointmentSucceeds_RedirectsToConfirmation()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.InsertAppointmentAsync(5, 1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // 0 = success

        // Act
        var result = await _pageModel.OnPostAsync(5, 2, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Patient/AppointmentRequestSent", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenAppointmentFails_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.InsertAppointmentAsync(5, 1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1); // non-zero = failure
        _patientServiceMock.Setup(s => s.GetDoctorProfileAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DoctorProfileDto { DoctorId = 5, Name = "Dr. Smith" });
        _patientServiceMock.Setup(s => s.GetFreeSlotsAsync(5, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSlotDto>());

        // Act
        var result = await _pageModel.OnPostAsync(5, 2, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Failed to book appointment. Please try again.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.InsertAppointmentAsync(5, 1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));
        _patientServiceMock.Setup(s => s.GetDoctorProfileAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DoctorProfileDto { DoctorId = 5, Name = "Dr. Smith" });
        _patientServiceMock.Setup(s => s.GetFreeSlotsAsync(5, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSlotDto>());

        // Act
        var result = await _pageModel.OnPostAsync(5, 2, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("An unexpected error occurred.", _pageModel.ErrorMessage);
    }
}
