namespace ClinicManagement.Application.DTOs;

/// <summary>Result DTO for login validation.</summary>
public class LoginResultDto
{
    /// <summary>0=Success, 1=Email not found, 2=Wrong password, -1=Error</summary>
    public int Status { get; set; }
    public int UserId { get; set; }
    /// <summary>1=Patient, 2=Doctor, 3=Admin</summary>
    public int UserType { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>Result DTO for patient sign-up.</summary>
public class SignUpResultDto
{
    /// <summary>0=Email exists, 1=Success, -1=Error</summary>
    public int Status { get; set; }
    public int PatientId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>DTO for patient sign-up form.</summary>
public class PatientSignUpDto
{
    public string Name { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>DTO for admin home dashboard.</summary>
public class AdminHomeDto
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public double TotalIncome { get; set; }
    public IEnumerable<DepartmentSummaryDto> Departments { get; set; } = new List<DepartmentSummaryDto>();
    public IEnumerable<AppointmentSummaryDto> Appointments { get; set; } = new List<AppointmentSummaryDto>();
}

/// <summary>Department summary for admin dashboard.</summary>
public class DepartmentSummaryDto
{
    public string DeptName { get; set; } = string.Empty;
    public int DoctorCount { get; set; }
    public string? Description { get; set; }
}

/// <summary>Appointment summary for admin dashboard.</summary>
public class AppointmentSummaryDto
{
    public int AppointId { get; set; }
    public string? DoctorName { get; set; }
    public string? PatientName { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
}

/// <summary>DTO for adding a new doctor.</summary>
public class AddDoctorDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public int DeptNo { get; set; }
    public string Phone { get; set; } = string.Empty;
    public char Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public int WorkExperience { get; set; }
    public int Salary { get; set; }
    public int ChargesPerVisit { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
}

/// <summary>DTO for adding a new staff member.</summary>
public class AddStaffDto
{
    public string Name { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public char Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public int Salary { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
}

/// <summary>Doctor list item for grid display.</summary>
public class DoctorListItemDto
{
    public int DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

/// <summary>Patient list item for grid display.</summary>
public class PatientListItemDto
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

/// <summary>Staff list item for grid display.</summary>
public class StaffListItemDto
{
    public int StaffId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
}

/// <summary>Detailed patient information.</summary>
public class PatientDetailDto
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? BirthDate { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
}

/// <summary>Doctor profile information.</summary>
public class DoctorProfileDto
{
    public int DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public float ChargesPerVisit { get; set; }
    public float ReputeIndex { get; set; }
    public int PatientsTreated { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public int WorkExperience { get; set; }
    public int Age { get; set; }
    public string? Department { get; set; }
}

/// <summary>Staff detail information.</summary>
public class StaffDetailDto
{
    public int StaffId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? Designation { get; set; }
    public int Salary { get; set; }
}

/// <summary>Bill history item for patient.</summary>
public class BillHistoryItemDto
{
    public int AppointId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime? Date { get; set; }
    public double? BillAmount { get; set; }
    public string? BillStatus { get; set; }
}

/// <summary>Current appointment for patient.</summary>
public class CurrentAppointmentDto
{
    public string DoctorName { get; set; } = string.Empty;
    public string Timings { get; set; } = string.Empty;
}

/// <summary>Treatment history item for patient.</summary>
public class TreatmentHistoryItemDto
{
    public int AppointId { get; set; }
    public string? DoctorName { get; set; }
    public DateTime? Date { get; set; }
    public string? Disease { get; set; }
    public string? Progress { get; set; }
    public string? Prescription { get; set; }
}

/// <summary>Department DTO.</summary>
public class DepartmentDto
{
    public int DeptNo { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>Appointment slot for booking.</summary>
public class AppointmentSlotDto
{
    public int SlotId { get; set; }
    public string Timings { get; set; } = string.Empty;
}

/// <summary>Notification DTO for patient.</summary>
public class NotificationDto
{
    public string DoctorName { get; set; } = string.Empty;
    public string Timings { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>Pending feedback DTO for patient.</summary>
public class PendingFeedbackDto
{
    public int AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Timings { get; set; } = string.Empty;
}

/// <summary>Appointment list item for doctor views.</summary>
public class AppointmentListItemDto
{
    public int AppointId { get; set; }
    public string? PatientName { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
    public string? Disease { get; set; }
    public string? Progress { get; set; }
    public string? Prescription { get; set; }
}

/// <summary>Bill item for doctor billing view.</summary>
public class BillItemDto
{
    public int AppointId { get; set; }
    public string? PatientName { get; set; }
    public DateTime? Date { get; set; }
    public double? BillAmount { get; set; }
    public string? BillStatus { get; set; }
}

/// <summary>Patient history item for doctor view.</summary>
public class PatientHistoryItemDto
{
    public int AppointId { get; set; }
    public string? PatientName { get; set; }
    public DateTime? Date { get; set; }
    public string? Disease { get; set; }
    public string? Progress { get; set; }
    public string? Prescription { get; set; }
    public string? BillStatus { get; set; }
}
