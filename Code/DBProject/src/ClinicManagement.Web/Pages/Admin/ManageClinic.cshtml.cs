using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Admin;

/// <summary>Page model for managing clinic doctors and staff.</summary>
public class ManageClinicModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<ManageClinicModel> _logger;

    public IEnumerable<DoctorListItemDto>? Doctors { get; set; }
    public IEnumerable<StaffListItemDto>? Staff { get; set; }
    public string? DoctorSearch { get; set; }
    public string? StaffSearch { get; set; }
    public string? Message { get; set; }

    public ManageClinicModel(IAdminService adminService, ILogger<ManageClinicModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(
        string? doctorSearch,
        string? staffSearch,
        CancellationToken cancellationToken)
    {
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userType != 3) return RedirectToPage("/SignUp");

        DoctorSearch = doctorSearch ?? string.Empty;
        StaffSearch = staffSearch ?? string.Empty;

        Doctors = await _adminService.LoadDoctorsAsync(DoctorSearch, cancellationToken);
        Staff = await _adminService.LoadStaffAsync(StaffSearch, cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteDoctorAsync(int doctorId, CancellationToken cancellationToken)
    {
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userType != 3) return RedirectToPage("/SignUp");

        try
        {
            await _adminService.DeleteDoctorAsync(doctorId, cancellationToken);
            _logger.LogInformation("Doctor {DoctorId} deleted by admin", doctorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor {DoctorId}", doctorId);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteStaffAsync(int staffId, CancellationToken cancellationToken)
    {
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userType != 3) return RedirectToPage("/SignUp");

        try
        {
            await _adminService.DeleteStaffAsync(staffId, cancellationToken);
            _logger.LogInformation("Staff {StaffId} deleted by admin", staffId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff {StaffId}", staffId);
        }

        return RedirectToPage();
    }
}
