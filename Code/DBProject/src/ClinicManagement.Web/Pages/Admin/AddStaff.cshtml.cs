using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Admin;

/// <summary>Page model for adding staff members.</summary>
public class AddStaffModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AddStaffModel> _logger;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public AddStaffModel(IAdminService adminService, ILogger<AddStaffModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userType != 3) return RedirectToPage("/SignUp");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string Name, string BirthDate, string Phone, string Gender,
        string Address, int Salary, string Qualification, string Designation,
        CancellationToken cancellationToken)
    {
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userType != 3) return RedirectToPage("/SignUp");

        try
        {
            var dto = new AddStaffDto
            {
                Name = Name,
                BirthDate = BirthDate,
                Phone = Phone,
                Gender = string.IsNullOrEmpty(Gender) ? 'M' : Gender[0],
                Address = Address,
                Salary = Salary,
                Qualification = Qualification,
                Designation = Designation
            };

            int result = await _adminService.AddStaffAsync(dto, cancellationToken);
            if (result == 1)
            {
                SuccessMessage = "Staff member added successfully!";
            }
            else
            {
                ErrorMessage = "Failed to add staff member. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding staff member {Name}", Name);
            ErrorMessage = "An unexpected error occurred.";
        }

        return Page();
    }
}
