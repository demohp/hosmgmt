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

/// <summary>Unit tests for DoctorRegistrationFormModel page model.</summary>
public class DoctorRegistrationFormModelTests
{
    private readonly Mock<IAdminService> _adminServiceMock;
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<DoctorRegistrationFormModel>> _loggerMock;
    private readonly DoctorRegistrationFormModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public DoctorRegistrationFormModelTests()
    {
        _adminServiceMock = new Mock<IAdminService>();
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<DoctorRegistrationFormModel>>();
        _pageModel = new DoctorRegistrationFormModel(_adminServiceMock.Object, _patientServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new DoctorRegistrationFormModel(_adminServiceMock.Object, _patientServiceMock.Object, _loggerMock.Object);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnGetAsync(CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidAdmin_LoadsDepartments()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        var departments = new List<DepartmentDto>
        {
            new DepartmentDto { DeptNo = 1, DeptName = "Cardiology" },
            new DepartmentDto { DeptNo = 2, DeptName = "Neurology" }
        };
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(departments);

        // Act
        var result = await _pageModel.OnGetAsync(CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Departments);
        Assert.Equal(2, _pageModel.Departments!.Count());
    }

    [Fact]
    public async Task OnPostAsync_WhenUserTypeIsNotAdmin_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var result = await _pageModel.OnPostAsync("Dr. Smith", "smith@test.com", "pass123", "1980-01-01", 1, "1234567890", "M", "123 Main St", 5, 80000, 200, "Cardiology", "MBBS", CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostAsync_WhenEmailAlreadyExists_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DepartmentDto>());
        _adminServiceMock.Setup(s => s.DoctorEmailExistsAsync("existing@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _pageModel.OnPostAsync("Dr. Smith", "existing@test.com", "pass123", "1980-01-01", 1, "1234567890", "M", "123 Main St", 5, 80000, 200, "Cardiology", "MBBS", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Email already exists. Please use a different email.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenAddDoctorSucceeds_SetsSuccessMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DepartmentDto>());
        _adminServiceMock.Setup(s => s.DoctorEmailExistsAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _adminServiceMock.Setup(s => s.AddDoctorAsync(It.IsAny<AddDoctorDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _pageModel.OnPostAsync("Dr. Smith", "new@test.com", "pass123", "1980-01-01", 1, "1234567890", "M", "123 Main St", 5, 80000, 200, "Cardiology", "MBBS", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Doctor registered successfully!", _pageModel.SuccessMessage);
        Assert.Null(_pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenAddDoctorFails_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DepartmentDto>());
        _adminServiceMock.Setup(s => s.DoctorEmailExistsAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _adminServiceMock.Setup(s => s.AddDoctorAsync(It.IsAny<AddDoctorDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

        // Act
        var result = await _pageModel.OnPostAsync("Dr. Smith", "new@test.com", "pass123", "1980-01-01", 1, "1234567890", "M", "123 Main St", 5, 80000, 200, "Cardiology", "MBBS", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Failed to register doctor. Please try again.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DepartmentDto>());
        _adminServiceMock.Setup(s => s.DoctorEmailExistsAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnPostAsync("Dr. Smith", "new@test.com", "pass123", "1980-01-01", 1, "1234567890", "M", "123 Main St", 5, 80000, 200, "Cardiology", "MBBS", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("An unexpected error occurred.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenGenderIsEmpty_UsesDefaultMaleGender()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 3, 3);
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DepartmentDto>());
        _adminServiceMock.Setup(s => s.DoctorEmailExistsAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        AddDoctorDto? capturedDto = null;
        _adminServiceMock.Setup(s => s.AddDoctorAsync(It.IsAny<AddDoctorDto>(), It.IsAny<CancellationToken>()))
            .Callback<AddDoctorDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(1);

        // Act
        await _pageModel.OnPostAsync("Dr. Smith", "new@test.com", "pass123", "1980-01-01", 1, "1234567890", string.Empty, "123 Main St", 5, 80000, 200, "Cardiology", "MBBS", CancellationToken.None);

        // Assert
        Assert.NotNull(capturedDto);
        Assert.Equal('M', capturedDto!.Gender);
    }
}
