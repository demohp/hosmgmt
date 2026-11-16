using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>Page model for take appointment landing page.</summary>
public class TakeAppointmentModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<TakeAppointmentModel> _logger;

    public IEnumerable<DepartmentDto>? Departments { get; set; }

    public TakeAppointmentModel(IPatientService patientService, ILogger<TakeAppointmentModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");

        if (userId == null || userType != 1) return RedirectToPage("/SignUp");

        try
        {
            Departments = await _patientService.GetDepartmentsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading departments for take appointment");
        }

        return Page();
    }
}
