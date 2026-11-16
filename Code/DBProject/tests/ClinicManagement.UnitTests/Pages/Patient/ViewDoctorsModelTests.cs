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

/// <summary>Unit tests for ViewDoctorsModel page model.</summary>
public class ViewDoctorsModelTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<ViewDoctorsModel>> _loggerMock;
    private readonly ViewDoctorsModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public ViewDoctorsModelTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<ViewDoctorsModel>>();
        _pageModel = new ViewDoctorsModel(_patientServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new ViewDoctorsModel(_patientServiceMock.Object, _loggerMock.Object);
        Assert.NotNull(model);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserIdIsNull_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, null, null);
        var result = await _pageModel.OnGetAsync(null, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenUserTypeIsNotPatient_RedirectsToSignUp()
    {
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 2);
        var result = await _pageModel.OnGetAsync(null, CancellationToken.None);
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidPatientNoDeptName_LoadsDepartmentsOnly()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var departments = new List<DepartmentDto>
        {
            new DepartmentDto { DeptNo = 1, DeptName = "Cardiology" },
            new DepartmentDto { DeptNo = 2, DeptName = "Neurology" }
        };
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(departments);

        // Act
        var result = await _pageModel.OnGetAsync(null, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Departments);
        Assert.Equal(2, _pageModel.Departments!.Count());
        Assert.Null(_pageModel.Doctors);
        Assert.Null(_pageModel.SelectedDept);
    }

    [Fact]
    public async Task OnGetAsync_WhenValidPatientWithDeptName_LoadsDepartmentsAndDoctors()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        var departments = new List<DepartmentDto> { new DepartmentDto { DeptNo = 1, DeptName = "Cardiology" } };
        var doctors = new List<DoctorListItemDto> { new DoctorListItemDto { DoctorId = 1, Name = "Dr. Smith", Department = "Cardiology" } };
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(departments);
        _patientServiceMock.Setup(s => s.GetDoctorsByDepartmentAsync("Cardiology", It.IsAny<CancellationToken>())).ReturnsAsync(doctors);

        // Act
        var result = await _pageModel.OnGetAsync("Cardiology", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.Departments);
        Assert.NotNull(_pageModel.Doctors);
        Assert.Equal("Cardiology", _pageModel.SelectedDept);
        Assert.Single(_pageModel.Doctors!);
    }

    [Fact]
    public async Task OnGetAsync_WhenServiceThrowsException_ReturnsPage()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnGetAsync(null, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_WhenEmptyDeptName_DoesNotLoadDoctors()
    {
        // Arrange
        SessionMockHelper.SetupUserSession(_sessionMock, 1, 1);
        _patientServiceMock.Setup(s => s.GetDepartmentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DepartmentDto>());

        // Act
        var result = await _pageModel.OnGetAsync(string.Empty, CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        _patientServiceMock.Verify(s => s.GetDoctorsByDepartmentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
