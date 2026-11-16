using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using ClinicManagement.Web.Pages;
using ClinicManagement.UnitTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.UnitTests.Pages;

/// <summary>Unit tests for SignUpModel page model.</summary>
public class SignUpModelTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<SignUpModel>> _loggerMock;
    private readonly SignUpModel _pageModel;
    private readonly Mock<ISession> _sessionMock;

    public SignUpModelTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<SignUpModel>>();
        _pageModel = new SignUpModel(_authServiceMock.Object, _loggerMock.Object);

        _sessionMock = new Mock<ISession>();
        var httpContext = new DefaultHttpContext();
        httpContext.Session = _sessionMock.Object;
        _pageModel.PageContext = new PageContext { HttpContext = httpContext };
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var model = new SignUpModel(_authServiceMock.Object, _loggerMock.Object);
        Assert.NotNull(model);
    }

    [Fact]
    public void OnGet_ClearsSession()
    {
        _sessionMock.Setup(s => s.Clear());
        _pageModel.OnGet();
        _sessionMock.Verify(s => s.Clear(), Times.Once);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenEmailIsEmpty_SetsErrorMessageAndReturnsPage()
    {
        var result = await _pageModel.OnPostLoginAsync(string.Empty, "password", CancellationToken.None);
        Assert.IsType<PageResult>(result);
        Assert.Equal("Email and password are required.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenPasswordIsEmpty_SetsErrorMessageAndReturnsPage()
    {
        var result = await _pageModel.OnPostLoginAsync("test@test.com", string.Empty, CancellationToken.None);
        Assert.IsType<PageResult>(result);
        Assert.Equal("Email and password are required.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenBothEmpty_SetsErrorMessageAndReturnsPage()
    {
        var result = await _pageModel.OnPostLoginAsync(string.Empty, string.Empty, CancellationToken.None);
        Assert.IsType<PageResult>(result);
        Assert.Equal("Email and password are required.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenValidPatientCredentials_RedirectsToPatientHome()
    {
        // Arrange
        var loginResult = new LoginResultDto { Status = 0, UserId = 1, UserType = 1 };
        _authServiceMock.Setup(s => s.ValidateLoginAsync("patient@test.com", "pass123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);
        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        // Act
        var result = await _pageModel.OnPostLoginAsync("patient@test.com", "pass123", CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Patient/PatientHome", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenValidDoctorCredentials_RedirectsToDoctorHome()
    {
        // Arrange
        var loginResult = new LoginResultDto { Status = 0, UserId = 2, UserType = 2 };
        _authServiceMock.Setup(s => s.ValidateLoginAsync("doctor@test.com", "pass123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);
        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        // Act
        var result = await _pageModel.OnPostLoginAsync("doctor@test.com", "pass123", CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Doctor/DoctorHome", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenValidAdminCredentials_RedirectsToAdminHome()
    {
        // Arrange
        var loginResult = new LoginResultDto { Status = 0, UserId = 3, UserType = 3 };
        _authServiceMock.Setup(s => s.ValidateLoginAsync("admin@test.com", "pass123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);
        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        // Act
        var result = await _pageModel.OnPostLoginAsync("admin@test.com", "pass123", CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Admin/AdminHome", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenUnknownUserType_RedirectsToSignUp()
    {
        // Arrange
        var loginResult = new LoginResultDto { Status = 0, UserId = 4, UserType = 99 };
        _authServiceMock.Setup(s => s.ValidateLoginAsync("unknown@test.com", "pass123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);
        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        // Act
        var result = await _pageModel.OnPostLoginAsync("unknown@test.com", "pass123", CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/SignUp", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenLoginFails_SetsErrorMessage()
    {
        // Arrange
        var loginResult = new LoginResultDto { Status = 1, ErrorMessage = "Email not found." };
        _authServiceMock.Setup(s => s.ValidateLoginAsync("notfound@test.com", "pass123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        // Act
        var result = await _pageModel.OnPostLoginAsync("notfound@test.com", "pass123", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Email not found.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        _authServiceMock.Setup(s => s.ValidateLoginAsync("test@test.com", "pass123", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnPostLoginAsync("test@test.com", "pass123", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("An unexpected error occurred. Please try again.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostSignUpAsync_WhenNameIsEmpty_SetsErrorMessageAndReturnsPage()
    {
        var result = await _pageModel.OnPostSignUpAsync(string.Empty, "1990-01-01", "test@test.com", "pass123", "1234567890", "Address", "M", CancellationToken.None);
        Assert.IsType<PageResult>(result);
        Assert.Equal("Name, email, and password are required.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostSignUpAsync_WhenEmailIsEmpty_SetsErrorMessageAndReturnsPage()
    {
        var result = await _pageModel.OnPostSignUpAsync("John Doe", "1990-01-01", string.Empty, "pass123", "1234567890", "Address", "M", CancellationToken.None);
        Assert.IsType<PageResult>(result);
        Assert.Equal("Name, email, and password are required.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostSignUpAsync_WhenPasswordIsEmpty_SetsErrorMessageAndReturnsPage()
    {
        var result = await _pageModel.OnPostSignUpAsync("John Doe", "1990-01-01", "test@test.com", string.Empty, "1234567890", "Address", "M", CancellationToken.None);
        Assert.IsType<PageResult>(result);
        Assert.Equal("Name, email, and password are required.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostSignUpAsync_WhenSignUpSucceeds_RedirectsToPatientHome()
    {
        // Arrange
        var signUpResult = new SignUpResultDto { Status = 1, PatientId = 5 };
        _authServiceMock.Setup(s => s.SignUpPatientAsync(It.IsAny<PatientSignUpDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(signUpResult);
        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        // Act
        var result = await _pageModel.OnPostSignUpAsync("John Doe", "1990-01-01", "new@test.com", "pass123", "1234567890", "123 Main St", "M", CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Patient/PatientHome", redirectResult.PageName);
    }

    [Fact]
    public async Task OnPostSignUpAsync_WhenEmailAlreadyExists_SetsErrorMessage()
    {
        // Arrange
        var signUpResult = new SignUpResultDto { Status = 0, ErrorMessage = "Email already exists." };
        _authServiceMock.Setup(s => s.SignUpPatientAsync(It.IsAny<PatientSignUpDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(signUpResult);

        // Act
        var result = await _pageModel.OnPostSignUpAsync("John Doe", "1990-01-01", "existing@test.com", "pass123", "1234567890", "123 Main St", "M", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Email already exists.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostSignUpAsync_WhenServiceThrowsException_SetsErrorMessage()
    {
        // Arrange
        _authServiceMock.Setup(s => s.SignUpPatientAsync(It.IsAny<PatientSignUpDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _pageModel.OnPostSignUpAsync("John Doe", "1990-01-01", "test@test.com", "pass123", "1234567890", "123 Main St", "M", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("An unexpected error occurred. Please try again.", _pageModel.ErrorMessage);
    }

    [Fact]
    public async Task OnPostLoginAsync_WhenLoginFailsWithNullErrorMessage_SetsDefaultErrorMessage()
    {
        // Arrange
        var loginResult = new LoginResultDto { Status = 2, ErrorMessage = null };
        _authServiceMock.Setup(s => s.ValidateLoginAsync("test@test.com", "wrong", It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        // Act
        var result = await _pageModel.OnPostLoginAsync("test@test.com", "wrong", CancellationToken.None);

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.Equal("Login failed.", _pageModel.ErrorMessage);
    }
}
