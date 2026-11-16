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

/// <summary>Unit tests for HistoryUpdateModel page model.</summary>
public class HistoryUpdateModelTests
{
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly Mock<ILogger<HistoryUpdateModel>> _loggerMock;
    private readonly HistoryUpdateModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public HistoryUpdateModelTests()
    {
        _doctorServiceMock = new Mock<IDoctorService>();
        _loggerMock = new Mock<ILogger<HistoryUpdateModel>>();
        _pageModel = new HistoryUpdateModel(_doctorServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new HistoryUpdateModel(_doctorServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenValidDoctor_LoadsTodaysAppointments()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        var appointments = new List<AppointmentListItemDto>
        {
            new AppointmentListItemDto { AppointId = 1, PatientName = "John Doe" },
            new AppointmentListItemDto { AppointId = 2, PatientName = "Jane Doe" }
        };
        _doctorServiceMock.Setup(s => s.GetTodaysAppointmentsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(appointments);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.TodaysAppointments);
        Assert.Equal(2, _pageModel.TodaysAppointments!.Count());
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.GetTodaysAppointmentsAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnPostAsync(1, "Flu", "Improving", "Paracetamol", CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserTypeIsNotDoctor_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnPostAsync(1, "Flu", "Improving", "Paracetamol", CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenValidDoctor_UpdatesPrescriptionAndSetsSuccessMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.UpdatePrescriptionAsync(2, 1, "Flu", "Improving", "Paracetamol", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _doctorServiceMock.Setup(s => s.GetTodaysAppointmentsAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentListItemDto>());

        // Act
        var result = await _pageModel.OnPostAsync(1, "Flu", "Improving", "Paracetamol", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Patient history updated successfully!", _pageModel.SuccessMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.UpdatePrescriptionAsync(2, 1, "Flu", "Improving", "Paracetamol", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));
        _doctorServiceMock.Setup(s => s.GetTodaysAppointmentsAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentListItemDto>());

        // Act
        var result = await _pageModel.OnPostAsync(1, "Flu", "Improving", "Paracetamol", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Null(_pageModel.SuccessMessage);
    }
}
