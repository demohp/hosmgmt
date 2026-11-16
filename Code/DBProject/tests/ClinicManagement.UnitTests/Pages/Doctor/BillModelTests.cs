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

/// <summary>Unit tests for BillModel page model.</summary>
public class BillModelTests
{
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly Mock<ILogger<BillModel>> _loggerMock;
    private readonly BillModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public BillModelTests()
    {
        _doctorServiceMock = new Mock<IDoctorService>();
        _loggerMock = new Mock<ILogger<BillModel>>();
        _pageModel = new BillModel(_doctorServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new BillModel(_doctorServiceMock.Object, _loggerMock.Object);
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
    public async Task OnGetAsync_WhenValidDoctor_LoadsBills()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        var bills = new List<BillItemDto>
        {
            new BillItemDto { AppointId = 1, PatientName = "John Doe", BillAmount = 150.0, BillStatus = "Unpaid" },
            new BillItemDto { AppointId = 2, PatientName = "Jane Doe", BillAmount = 200.0, BillStatus = "Paid" }
        };
        _doctorServiceMock.Setup(s => s.GenerateBillAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(bills);

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
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.GenerateBillAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Bills);
        Assert.Empty(_pageModel.Bills!);
    }

    [Fact]
    public async Task OnPostMarkPaidAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnPostMarkPaidAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostMarkPaidAsync_WhenValidDoctor_MarksBillPaidAndRedirects()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.MarkBillPaidAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _pageModel.OnPostMarkPaidAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        _doctorServiceMock.Verify(s => s.MarkBillPaidAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OnPostMarkUnpaidAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnPostMarkUnpaidAsync(1, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostMarkUnpaidAsync_WhenValidDoctor_MarksBillUnpaidAndRedirects()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 2, 2);
        _doctorServiceMock.Setup(s => s.MarkBillUnpaidAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _pageModel.OnPostMarkUnpaidAsync(1, CancellationToken.None);

        // Assert
        Assert.IsType<RedirectToPageResult>(result);
        _doctorServiceMock.Verify(s => s.MarkBillUnpaidAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
