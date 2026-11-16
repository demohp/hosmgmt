using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagement.Web.Pages.Admin;

/// <summary>Page model for doctor registration.</summary>
public class DoctorRegistrationFormModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IPatientService _patientService;
    private readonly ILogger<DoctorRegistrationFormModel> _logger;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public IEnumerable<DepartmentDto>? Departments { get; set; }

    public DoctorRegistrationFormModel(
        IAdminService adminService,
        IPatientService patientService,
        ILogger<DoctorRegistrationFormModel> logger)
    {
        _adminService = adminService;
        _patientService = patientService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userType != 3) return RedirectToPage("/SignUp");

        Departments = await _patientService.GetDepartmentsAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string Name, string Email, string Password, string BirthDate,
        int DeptNo, string Phone, string Gender, string Address,
        int WorkExperience, int Salary, int ChargesPerVisit,
        string Specialization, string Qualification,
        CancellationToken cancellationToken)
    {
        var userType = HttpContext.Session.GetInt32("UserType");
        if (userType != 3) return RedirectToPage("/SignUp");

        Departments = await _patientService.GetDepartmentsAsync(cancellationToken);

        try
        {
            bool emailExists = await _adminService.DoctorEmailExistsAsync(Email, cancellationToken);
            if (emailExists)
            {
                ErrorMessage = "Email already exists. Please use a different email.";
                return Page();
            }

            var dto = new AddDoctorDto
            {
                Name = Name,
                Email = Email,
                Password = Password,
                BirthDate = BirthDate,
                DeptNo = DeptNo,
                Phone = Phone,
                Gender = string.IsNullOrEmpty(Gender) ? 'M' : Gender[0],
                Address = Address,
                WorkExperience = WorkExperience,
                Salary = Salary,
                ChargesPerVisit = ChargesPerVisit,
                Specialization = Specialization,
                Qualification = Qualification
            };

            int result = await _adminService.AddDoctorAsync(dto, cancellationToken);
            if (result == 1)
            {
                SuccessMessage = "Doctor registered successfully!";
            }
            else
            {
                ErrorMessage = "Failed to register doctor. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering doctor {Email}", Email);
            ErrorMessage = "An unexpected error occurred.";
        }

        return Page();
    }
}
