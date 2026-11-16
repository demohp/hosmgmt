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

/// <summary>Unit tests for PendingAppointmentModel page model.</summary>
public class PendingAppointmentModelTests
{
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly Mock<ILogger<PendingAppointmentModel>> _loggerMock;
    private readonly PendingAppointmentModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public PendingAppointmentModelTests()
    {
        _doctorServiceMock = new Mock<IDoctorService>();
        _loggerMock = new Mock<ILogger<PendingAppointmentModel>>();
        _pageModel = new PendingAppointmentModel(_doctorServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new PendingAppointmentModel(_doctorServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenValidDoctor_LoadsPendingAppointments()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        var appointments = new List<AppointmentListItemDto>
        {
            new AppointmentListItemDto { AppointId = 1, PatientName = "John Doe", Status = "Pending" },
            new AppointmentListItemDto { AppointId = 2, PatientName = "Jane Doe", Status = "Pending" }
        };
        _doctorServiceMock.Setup(s => s.GetPendingAppointmentsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(appointments);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.PendingAppointments);
        Assert.Equal(2, _pageModel.PendingAppointments!.Count());
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.GetPendingAppointmentsAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostApproveAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnPostApproveAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostApproveAsync_WhenValidDoctor_ApprovesAppointmentAndRedirects()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.ApproveAppointmentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _pageModel.OnPostApproveAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        _doctorServiceMock.Verify(s => s.ApproveAppointmentAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnPostRejectAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnPostRejectAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostRejectAsync_WhenValidDoctor_DeletesAppointmentAndRedirects()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.DeleteAppointmentAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _pageModel.OnPostRejectAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        _doctorServiceMock.Verify(s => s.DeleteAppointmentAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
