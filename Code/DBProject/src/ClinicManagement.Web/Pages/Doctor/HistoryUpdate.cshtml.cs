using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Doctor;

/// <summary>Page model for updating patient history/prescription.</summary>
public class HistoryUpdateModel : PageModel
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<HistoryUpdateModel> _logger;

    public IEnumerable<AppointmentListItemDto>? TodaysAppointments { get; set; }
    public string? SuccessMessage { get; set; }

    public HistoryUpdateModel(IDoctorService doctorService, ILogger<HistoryUpdateModel> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");

        if (userId == null || userType != 2) return RedirectToPage("/SignUp");

        try
        {
            TodaysAppointments = await _doctorService.GetTodaysAppointmentsAsync(userId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading today's appointments for doctor {UserId}", userId);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int appointmentId, string disease, string progress, string prescription,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");

        if (userId == null || userType != 2) return RedirectToPage("/SignUp");

        try
        {
            await _doctorService.UpdatePrescriptionAsync(
                userId.Value, appointmentId, disease, progress, prescription, cancellationToken);
            SuccessMessage = "Patient history updated successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription for appointment {AppointmentId}", appointmentId);
        }

        TodaysAppointments = await _doctorService.GetTodaysAppointmentsAsync(userId!.Value, cancellationToken);
        return Page();
    }
}
