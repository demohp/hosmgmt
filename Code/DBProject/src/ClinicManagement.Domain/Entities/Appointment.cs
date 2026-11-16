namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Represents an appointment between a patient and a doctor.
/// Appointment_Status: 1=Approved, 2=Pending, 3=Completed, 4=Rejected
/// Bill_Status: Paid / Unpaid
/// DoctorNotification: 1=Seen, 2=Unseen
/// PatientNotification: 1=Seen, 2=Unseen
/// FeedbackStatus: 1=Given, 2=Pending
/// </summary>
public class Appointment
{
    public int AppointId { get; set; }
    public int? DoctorId { get; set; }
    public int? PatientId { get; set; }
    public DateTime? Date { get; set; }
    public int AppointmentStatus { get; set; }

    public double? BillAmount { get; set; }
    public string? BillStatus { get; set; }

    public int? DoctorNotification { get; set; }
    public int? PatientNotification { get; set; }
    public int? FeedbackStatus { get; set; }

    public string? Disease { get; set; }
    public string? Progress { get; set; }
    public string? Prescription { get; set; }

    // Navigation properties
    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}
