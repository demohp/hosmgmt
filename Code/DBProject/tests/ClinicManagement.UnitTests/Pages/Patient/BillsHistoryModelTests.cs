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

/// <summary>Unit tests for BillsHistoryModel page model.</summary>
public class BillsHistoryModelTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<BillsHistoryModel>> _loggerMock;
    private readonly BillsHistoryModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public BillsHistoryModelTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<BillsHistoryModel>>();
        _pageModel = new BillsHistoryModel(_patientServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new BillsHistoryModel(_patientServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenValidPatient_LoadsBills()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var bills = new List<BillHistoryItemDto>
        {
            new BillHistoryItemDto { AppointId = 1, DoctorName = "Dr. Smith", BillAmount = 150.0, BillStatus = "Paid" },
            new BillHistoryItemDto { AppointId = 2, DoctorName = "Dr. Jones", BillAmount = 200.0, BillStatus = "Unpaid" }
        };
        _patientServiceMock.Setup(s => s.GetBillHistoryAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(bills);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Bills);
        Assert.Equal(2, _pageModel.Bills!.Count());
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsEmptyBills()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetBillHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Bills);
        Assert.Empty(_pageModel.Bills!);
    }

    [Fact]
    public async Task OnGetAsync_WhenNoBills_ReturnsEmptyList()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetBillHistoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BillHistoryItemDto>());

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Bills);
        Assert.Empty(_pageModel.Bills!);
    }
}
