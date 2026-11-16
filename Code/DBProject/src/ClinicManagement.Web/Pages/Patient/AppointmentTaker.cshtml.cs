using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>Page model for taking an appointment.</summary>
public class AppointmentTakerModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<AppointmentTakerModel> _logger;

    public DoctorProfileDto? DoctorProfile { get; set; }
    public IEnumerable<AppointmentSlotDto>? FreeSlots { get; set; }
    public string? ErrorMessage { get; set; }

    public AppointmentTakerModel(IPatientService patientService, ILogger<AppointmentTakerModel> logger)
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
            FreeSlots = await _patientService.GetFreeSlotsAsync(doctorId, userId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointment taker for doctor {DoctorId}", doctorId);
            ErrorMessage = "Error loading appointment information.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int doctorId, int freeSlot, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");

        if (userId == null || userType != 1) return RedirectToPage("/SignUp");

        try
        {
            int result = await _patientService.InsertAppointmentAsync(doctorId, userId.Value, freeSlot, cancellationToken);
            if (result == 0)
            {
                return RedirectToPage("/Patient/AppointmentRequestSent");
            }

            ErrorMessage = "Failed to book appointment. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking appointment for patient {UserId}", userId);
            ErrorMessage = "An unexpected error occurred.";
        }

        DoctorProfile = await _patientService.GetDoctorProfileAsync(doctorId, cancellationToken);
        FreeSlots = await _patientService.GetFreeSlotsAsync(doctorId, userId!.Value, cancellationToken);
        return Page();
    }
}
