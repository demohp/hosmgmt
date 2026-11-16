using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>Authentication service implementation.</summary>
public class AuthService : IAuthService
{
    private readonly ILoginRepository _loginRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ILoginRepository loginRepository,
        IPatientRepository patientRepository,
        ILogger<AuthService> logger)
    {
        _loginRepository = loginRepository;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    /// <summary>Validates user login credentials.</summary>
    public async Task<LoginResultDto> ValidateLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating login for email: {Email}", email);

            var loginEntry = await _loginRepository.GetByEmailAsync(email, cancellationToken);
            if (loginEntry == null)
            {
                _logger.LogWarning("Login failed: email not found {Email}", email);
                return new LoginResultDto { Status = 1, ErrorMessage = "Email not found. Try Again!" };
            }

            if (loginEntry.Password != password)
            {
                _logger.LogWarning("Login failed: incorrect password for {Email}", email);
                return new LoginResultDto { Status = 2, ErrorMessage = "Incorrect Password. Try Again!" };
            }

            _logger.LogInformation("Login successful for user {UserId} type {Type}", loginEntry.LoginId, loginEntry.Type);
            return new LoginResultDto
            {
                Status = 0,
                UserId = loginEntry.LoginId,
                UserType = loginEntry.Type
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating login for {Email}", email);
            return new LoginResultDto { Status = -1, ErrorMessage = "There was some error. Try Again!" };
        }
    }

    /// <summary>Registers a new patient.</summary>
    public async Task<SignUpResultDto> SignUpPatientAsync(PatientSignUpDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Signing up patient with email: {Email}", dto.Email);

            bool emailExists = await _loginRepository.EmailExistsAsync(dto.Email, cancellationToken);
            if (emailExists)
            {
                _logger.LogWarning("Sign-up failed: email already exists {Email}", dto.Email);
                return new SignUpResultDto { Status = 0, ErrorMessage = "Email already exists. Please choose a different one." };
            }

            var login = new LoginTable
            {
                Email = dto.Email,
                Password = dto.Password,
                Type = 1 // Patient
            };

            var createdLogin = await _loginRepository.AddAsync(login, cancellationToken);

            var patient = new Patient
            {
                PatientId = createdLogin.LoginId,
                Name = dto.Name,
                Phone = dto.PhoneNo,
                Address = dto.Address,
                BirthDate = DateTime.Parse(dto.BirthDate),
                Gender = string.IsNullOrEmpty(dto.Gender) ? 'M' : dto.Gender[0]
            };

            await _patientRepository.AddAsync(patient, cancellationToken);

            _logger.LogInformation("Patient signed up successfully with ID {PatientId}", createdLogin.LoginId);
            return new SignUpResultDto { Status = 1, PatientId = createdLogin.LoginId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing up patient with email {Email}", dto.Email);
            return new SignUpResultDto { Status = -1, ErrorMessage = "There was some error. Try again!" };
        }
    }
}
