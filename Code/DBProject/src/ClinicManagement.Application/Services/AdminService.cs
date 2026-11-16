using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>Admin service implementation.</summary>
public class AdminService : IAdminService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILoginRepository _loginRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IDoctorRepository doctorRepository,
        IPatientRepository patientRepository,
        IStaffRepository staffRepository,
        IDepartmentRepository departmentRepository,
        IAppointmentRepository appointmentRepository,
        ILoginRepository loginRepository,
        ILogger<AdminService> logger)
    {
        _doctorRepository = doctorRepository;
        _patientRepository = patientRepository;
        _staffRepository = staffRepository;
        _departmentRepository = departmentRepository;
        _appointmentRepository = appointmentRepository;
        _loginRepository = loginRepository;
        _logger = logger;
    }

    /// <summary>Gets admin home dashboard information.</summary>
    public async Task<AdminHomeDto> GetAdminHomeInformationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving admin home information");

            var doctors = await _doctorRepository.GetAllActiveAsync(cancellationToken);
            var patients = await _patientRepository.GetAllAsync(cancellationToken);
            var departments = await _departmentRepository.GetAllAsync(cancellationToken);
            var appointments = await _appointmentRepository.GetByDoctorIdAsync(0, cancellationToken);

            var allAppointments = new List<Appointment>();
            foreach (var doc in doctors)
            {
                var docAppts = await _appointmentRepository.GetByDoctorIdAsync(doc.DoctorId, cancellationToken);
                allAppointments.AddRange(docAppts);
            }

            double totalIncome = allAppointments
                .Where(a => a.BillStatus == "Paid" && a.BillAmount.HasValue)
                .Sum(a => a.BillAmount!.Value);

            var deptSummaries = departments.Select(d => new DepartmentSummaryDto
            {
                DeptName = d.DeptName,
                Description = d.Description,
                DoctorCount = doctors.Count(doc => doc.DeptNo == d.DeptNo)
            });

            var apptSummaries = allAppointments.Take(20).Select(a => new AppointmentSummaryDto
            {
                AppointId = a.AppointId,
                DoctorName = a.Doctor?.Name,
                PatientName = a.Patient?.Name,
                Date = a.Date,
                Status = a.AppointmentStatus.ToString()
            });

            return new AdminHomeDto
            {
                TotalDoctors = doctors.Count(),
                TotalPatients = patients.Count(),
                TotalIncome = totalIncome,
                Departments = deptSummaries,
                Appointments = apptSummaries
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin home information");
            return new AdminHomeDto();
        }
    }

    /// <summary>Adds a new doctor to the system.</summary>
    public async Task<int> AddDoctorAsync(AddDoctorDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding doctor with email: {Email}", dto.Email);

            bool emailExists = await _loginRepository.EmailExistsAsync(dto.Email, cancellationToken);
            if (emailExists)
            {
                _logger.LogWarning("Doctor email already exists: {Email}", dto.Email);
                return 0;
            }

            var login = new LoginTable
            {
                Email = dto.Email,
                Password = dto.Password,
                Type = 2 // Doctor
            };
            var createdLogin = await _loginRepository.AddAsync(login, cancellationToken);

            var doctor = new Doctor
            {
                DoctorId = createdLogin.LoginId,
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                BirthDate = DateTime.Parse(dto.BirthDate),
                DeptNo = dto.DeptNo,
                Gender = dto.Gender,
                WorkExperience = dto.WorkExperience,
                MonthlySalary = dto.Salary,
                ChargesPerVisit = dto.ChargesPerVisit,
                Specialization = dto.Specialization,
                Qualification = dto.Qualification,
                Status = 1,
                ReputeIndex = 0,
                PatientsTreated = 0
            };

            await _doctorRepository.AddAsync(doctor, cancellationToken);
            _logger.LogInformation("Doctor added successfully with ID {DoctorId}", createdLogin.LoginId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding doctor with email {Email}", dto.Email);
            return -1;
        }
    }

    /// <summary>Adds a new staff member.</summary>
    public async Task<int> AddStaffAsync(AddStaffDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding staff member: {Name}", dto.Name);

            var staff = new OtherStaff
            {
                Name = dto.Name,
                BirthDate = DateTime.TryParse(dto.BirthDate, out var bd) ? bd : null,
                Phone = dto.Phone,
                Gender = dto.Gender,
                Salary = dto.Salary,
                Designation = dto.Designation,
                HighestQualification = dto.Qualification,
                Address = dto.Address
            };

            await _staffRepository.AddAsync(staff, cancellationToken);
            _logger.LogInformation("Staff member added successfully");
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding staff member {Name}", dto.Name);
            return -1;
        }
    }

    /// <summary>Soft-deletes a doctor (sets status to 0).</summary>
    public async Task<int> DeleteDoctorAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting doctor with ID: {DoctorId}", id);
            await _doctorRepository.SoftDeleteAsync(id, cancellationToken);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting doctor {DoctorId}", id);
            return -1;
        }
    }

    /// <summary>Deletes a staff member.</summary>
    public async Task<int> DeleteStaffAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting staff with ID: {StaffId}", id);
            await _staffRepository.DeleteAsync(id, cancellationToken);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff {StaffId}", id);
            return -1;
        }
    }

    /// <summary>Loads doctors with optional search.</summary>
    public async Task<IEnumerable<DoctorListItemDto>> LoadDoctorsAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<Doctor> doctors;
            if (string.IsNullOrEmpty(searchQuery))
                doctors = await _doctorRepository.GetAllActiveAsync(cancellationToken);
            else
                doctors = await _doctorRepository.SearchAsync(searchQuery, cancellationToken);

            var departments = await _departmentRepository.GetAllAsync(cancellationToken);
            var deptDict = departments.ToDictionary(d => d.DeptNo, d => d.DeptName);

            return doctors.Select(d => new DoctorListItemDto
            {
                DoctorId = d.DoctorId,
                Name = d.Name,
                Department = deptDict.TryGetValue(d.DeptNo, out var deptName) ? deptName : "Unknown"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading doctors with query {Query}", searchQuery);
            return Enumerable.Empty<DoctorListItemDto>();
        }
    }

    /// <summary>Loads patients with optional search.</summary>
    public async Task<IEnumerable<PatientListItemDto>> LoadPatientsAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<Patient> patients;
            if (string.IsNullOrEmpty(searchQuery))
                patients = await _patientRepository.GetAllAsync(cancellationToken);
            else
                patients = await _patientRepository.SearchAsync(searchQuery, cancellationToken);

            return patients.Select(p => new PatientListItemDto
            {
                PatientId = p.PatientId,
                Name = p.Name,
                Phone = p.Phone
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading patients with query {Query}", searchQuery);
            return Enumerable.Empty<PatientListItemDto>();
        }
    }

    /// <summary>Loads staff with optional search.</summary>
    public async Task<IEnumerable<StaffListItemDto>> LoadStaffAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<OtherStaff> staff;
            if (string.IsNullOrEmpty(searchQuery))
                staff = await _staffRepository.GetAllAsync(cancellationToken);
            else
                staff = await _staffRepository.SearchAsync(searchQuery, cancellationToken);

            return staff.Select(s => new StaffListItemDto
            {
                StaffId = s.StaffId,
                Name = s.Name,
                Designation = s.Designation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading staff with query {Query}", searchQuery);
            return Enumerable.Empty<StaffListItemDto>();
        }
    }

    /// <summary>Gets patient details by ID.</summary>
    public async Task<PatientDetailDto?> GetPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient == null) return null;

            int age = DateTime.Now.Year - patient.BirthDate.Year;
            if (DateTime.Now.DayOfYear < patient.BirthDate.DayOfYear) age--;

            return new PatientDetailDto
            {
                PatientId = patient.PatientId,
                Name = patient.Name,
                Phone = patient.Phone,
                Address = patient.Address,
                BirthDate = patient.BirthDate.ToString("yyyy-MM-dd"),
                Age = age,
                Gender = patient.Gender.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient {PatientId}", patientId);
            return null;
        }
    }

    /// <summary>Gets doctor profile by ID.</summary>
    public async Task<DoctorProfileDto?> GetDoctorProfileAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
            if (doctor == null) return null;

            int age = DateTime.Now.Year - doctor.BirthDate.Year;
            if (DateTime.Now.DayOfYear < doctor.BirthDate.DayOfYear) age--;

            var departments = await _departmentRepository.GetAllAsync(cancellationToken);
            var dept = departments.FirstOrDefault(d => d.DeptNo == doctor.DeptNo);

            return new DoctorProfileDto
            {
                DoctorId = doctor.DoctorId,
                Name = doctor.Name,
                Phone = doctor.Phone,
                Gender = doctor.Gender.ToString(),
                ChargesPerVisit = (float)doctor.ChargesPerVisit,
                ReputeIndex = (float)(doctor.ReputeIndex ?? 0),
                PatientsTreated = doctor.PatientsTreated,
                Qualification = doctor.Qualification,
                Specialization = doctor.Specialization,
                WorkExperience = doctor.WorkExperience ?? 0,
                Age = age,
                Department = dept?.DeptName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor profile {DoctorId}", doctorId);
            return null;
        }
    }

    /// <summary>Gets staff details by ID.</summary>
    public async Task<StaffDetailDto?> GetStaffAsync(int staffId, CancellationToken cancellationToken = default)
    {
        try
        {
            var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken);
            if (staff == null) return null;

            return new StaffDetailDto
            {
                StaffId = staff.StaffId,
                Name = staff.Name,
                Phone = staff.Phone,
                Gender = staff.Gender.ToString(),
                Address = staff.Address,
                Designation = staff.Designation,
                Salary = (int)(staff.Salary ?? 0)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff {StaffId}", staffId);
            return null;
        }
    }

    /// <summary>Checks if a doctor email already exists.</summary>
    public async Task<bool> DoctorEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _loginRepository.EmailExistsAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking doctor email {Email}", email);
            return false;
        }
    }
}
