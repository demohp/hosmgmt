using ClinicManagement.Application.Services;
using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManagement.UnitTests.Services;

/// <summary>Unit tests for AuthService.</summary>
public class AuthServiceTests
{
    private readonly Mock<ILoginRepository> _loginRepoMock;
    private readonly Mock<IPatientRepository> _patientRepoMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _loginRepoMock = new Mock<ILoginRepository>();
        _patientRepoMock = new Mock<IPatientRepository>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_loginRepoMock.Object, _patientRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ValidateLoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var loginEntry = new LoginTable { LoginId = 1, Email = "test@test.com", Password = "pass123", Type = 1 };
        _loginRepoMock.Setup(r => r.GetByEmailAsync("test@test.com", default))
            .ReturnsAsync(loginEntry);

        // Act
        var result = await _authService.ValidateLoginAsync("test@test.com", "pass123");

        // Assert
        Assert.Equal(0, result.Status);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.UserType);
    }

    [Fact]
    public async Task ValidateLoginAsync_WithEmailNotFound_ReturnsStatus1()
    {
        // Arrange
        _loginRepoMock.Setup(r => r.GetByEmailAsync("notfound@test.com", default))
            .ReturnsAsync((LoginTable?)null);

        // Act
        var result = await _authService.ValidateLoginAsync("notfound@test.com", "pass123");

        // Assert
        Assert.Equal(1, result.Status);
        Assert.Contains("Email not found", result.ErrorMessage ?? string.Empty);
    }

    [Fact]
    public async Task ValidateLoginAsync_WithWrongPassword_ReturnsStatus2()
    {
        // Arrange
        var loginEntry = new LoginTable { LoginId = 1, Email = "test@test.com", Password = "correct", Type = 1 };
        _loginRepoMock.Setup(r => r.GetByEmailAsync("test@test.com", default))
            .ReturnsAsync(loginEntry);

        // Act
        var result = await _authService.ValidateLoginAsync("test@test.com", "wrong");

        // Assert
        Assert.Equal(2, result.Status);
        Assert.Contains("Incorrect Password", result.ErrorMessage ?? string.Empty);
    }

    [Fact]
    public async Task SignUpPatientAsync_WithExistingEmail_ReturnsStatus0()
    {
        // Arrange
        _loginRepoMock.Setup(r => r.EmailExistsAsync("existing@test.com", default))
            .ReturnsAsync(true);

        var dto = new PatientSignUpDto
        {
            Name = "Test Patient",
            Email = "existing@test.com",
            Password = "pass123",
            BirthDate = "1990-01-01",
            Gender = "M"
        };

        // Act
        var result = await _authService.SignUpPatientAsync(dto);

        // Assert
        Assert.Equal(0, result.Status);
        Assert.Contains("Email already exists", result.ErrorMessage ?? string.Empty);
    }

    [Fact]
    public async Task SignUpPatientAsync_WithNewEmail_ReturnsStatus1()
    {
        // Arrange
        _loginRepoMock.Setup(r => r.EmailExistsAsync("new@test.com", default))
            .ReturnsAsync(false);

        var createdLogin = new LoginTable { LoginId = 5, Email = "new@test.com", Password = "pass123", Type = 1 };
        _loginRepoMock.Setup(r => r.AddAsync(It.IsAny<LoginTable>(), default))
            .ReturnsAsync(createdLogin);

        _patientRepoMock.Setup(r => r.AddAsync(It.IsAny<Patient>(), default))
            .ReturnsAsync(new Patient { PatientId = 5, Name = "New Patient", BirthDate = DateTime.Now, Gender = 'M' });

        var dto = new PatientSignUpDto
        {
            Name = "New Patient",
            Email = "new@test.com",
            Password = "pass123",
            BirthDate = "1990-01-01",
            Gender = "M"
        };

        // Act
        var result = await _authService.SignUpPatientAsync(dto);

        // Assert
        Assert.Equal(1, result.Status);
        Assert.Equal(5, result.PatientId);
    }
}
