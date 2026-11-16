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

/// <summary>Unit tests for PatientFeedbackModel page model.</summary>
public class PatientFeedbackModelTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<PatientFeedbackModel>> _loggerMock;
    private readonly PatientFeedbackModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public PatientFeedbackModelTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<PatientFeedbackModel>>();
        _pageModel = new PatientFeedbackModel(_patientServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange & Act
        var model = new PatientFeedbackModel(_patientServiceMock.Object, _loggerMock.Object);

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 2); // UserType 2 = Doctor

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidPatient_LoadsPendingFeedback()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var pendingFeedback = new PendingFeedbackDto
        {
            AppointmentId = 10,
            DoctorName = "Dr. Smith",
            Timings = "10:00 AM"
        };
        _patientServiceMock.Setup(s => s.GetPendingFeedbackAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingFeedback);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.PendingFeedback);
        Assert.Equal(10, _pageModel.PendingFeedback!.AppointmentId);
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetPendingFeedbackAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.PendingFeedback);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);

        // Act
        var result = await _pageModel.OnPostAsync(1, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 3); // UserType 3 = Admin

        // Act
        var result = await _pageModel.OnPostAsync(1, CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenValidPatient_SubmitsFeedbackAndSetsSuccessMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.SubmitFeedbackAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _pageModel.OnPostAsync(10, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Thank you for your feedback!", _pageModel.SuccessMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.SubmitFeedbackAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnPostAsync(10, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.SuccessMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenNoPendingFeedback_PendingFeedbackIsNull()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetPendingFeedbackAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PendingFeedbackDto?)null);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.PendingFeedback);
    }
}
