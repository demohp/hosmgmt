using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Patient;

/// <summary>Page model for viewing doctors by department.</summary>
public class ViewDoctorsModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly ILogger<ViewDoctorsModel> _logger;

    public IEnumerable<DepartmentDto>? Departments { get; set; }
    public IEnumerable<DoctorListItemDto>? Doctors { get; set; }
    public string? SelectedDept { get; set; }

    public ViewDoctorsModel(IPatientService patientService, ILogger<ViewDoctorsModel> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? deptName, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var userType = HttpContext.Session.GetInt32("UserType");

        if (userId == null || userType != 1) return RedirectToPage("/SignUp");

        try
        {
            Departments = await _patientService.GetDepartmentsAsync(cancellationToken);
            SelectedDept = deptName;

            if (!string.IsNullOrEmpty(deptName))
            {
                Doctors = await _patientService.GetDoctorsByDepartmentAsync(deptName, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctors view");
        }

        return Page();
    }
}
