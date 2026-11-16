using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages;

/// <summary>Page model for login and sign-up functionality.</summary>
public class SignUpModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<SignUpModel> _logger;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public SignUpModel(IAuthService authService, ILogger<SignUpModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public void OnGet()
    {
        HttpContext.Session.Clear();
    }

    /// <summary>Handles login form submission.</summary>
    public async Task<IActionResult> OnPostLoginAsync(
        string loginEmail,
        string loginPassword,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginEmail) || string.IsNullOrWhiteSpace(loginPassword))
            {
                ErrorMessage = "Email and password are required.";
                return Page();
            }

            var result = await _authService.ValidateLoginAsync(loginEmail, loginPassword, cancellationToken);

            if (result.Status == 0)
            {
                HttpContext.Session.SetInt32("UserId", result.UserId);
                HttpContext.Session.SetInt32("UserType", result.UserType);

                return result.UserType switch
                {
                    1 => RedirectToPage("/Patient/PatientHome"),
                    2 => RedirectToPage("/Doctor/DoctorHome"),
                    3 => RedirectToPage("/Admin/AdminHome"),
                    _ => RedirectToPage("/SignUp")
                };
            }

            ErrorMessage = result.ErrorMessage ?? "Login failed.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", loginEmail);
            ErrorMessage = "An unexpected error occurred. Please try again.";
            return Page();
        }
    }

    /// <summary>Handles sign-up form submission.</summary>
    public async Task<IActionResult> OnPostSignUpAsync(
        string sName,
        string sBirthDate,
        string sEmail,
        string sPassword,
        string Phone,
        string Address,
        string Gender,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sName) || string.IsNullOrWhiteSpace(sEmail) || string.IsNullOrWhiteSpace(sPassword))
            {
                ErrorMessage = "Name, email, and password are required.";
                return Page();
            }

            var dto = new PatientSignUpDto
            {
                Name = sName,
                BirthDate = sBirthDate,
                Email = sEmail,
                Password = sPassword,
                PhoneNo = Phone,
                Address = Address,
                Gender = Gender
            };

            var result = await _authService.SignUpPatientAsync(dto, cancellationToken);

            if (result.Status == 1)
            {
                HttpContext.Session.SetInt32("UserId", result.PatientId);
                HttpContext.Session.SetInt32("UserType", 1);
                return RedirectToPage("/Patient/PatientHome");
            }

            ErrorMessage = result.ErrorMessage ?? "Sign-up failed.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-up for {Email}", sEmail);
            ErrorMessage = "An unexpected error occurred. Please try again.";
            return Page();
        }
    }
}
