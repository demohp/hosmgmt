using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>Page model for doctor profile view.</summary>
public class DoctorProfileModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<DoctorProfileModel> _logger;

    public DoctorProfileDto? DoctorProfile { get; set; }

    public DoctorProfileModel(IPatientService patientService, ILogger<DoctorProfileModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(int doctorId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");

        if (userId == null || userType != 1) return RedirectToPage("/SignUp");

        try
        {
            DoctorProfile = await _patientService.GetDoctorProfileAsync(doctorId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctor profile {DoctorId}", doctorId);
        }

        return Page();
    }
}
