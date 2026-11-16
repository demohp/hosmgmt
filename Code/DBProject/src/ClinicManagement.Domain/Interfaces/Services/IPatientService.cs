using ClinicManagement.Domain.DTOs;

namespace ClinicManagement.Domain.Interfaces.Services;

/// <summary>Service interface for patient operations.</summary>
public interface IPatientService
{
    Task<PatientDetailDto?> GetPatientInfoAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BillHistoryItemDto>> GetBillHistoryAsync(int patientId, CancellationToken cancellationToken = default);
    Task<CurrentAppointmentDto?> GetCurrentAppointmentAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreatmentHistoryItemDto>> GetTreatmentHistoryAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<DoctorListItemDto>> GetDoctorsByDepartmentAsync(string deptName, CancellationToken cancellationToken = default);
    Task<DoctorProfileDto?> GetDoctorProfileAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentSlotDto>> GetFreeSlotsAsync(int doctorId, int patientId, CancellationToken cancellationToken = default);
    Task<int> InsertAppointmentAsync(int doctorId, int patientId, int freeSlot, CancellationToken cancellationToken = default);
    Task<NotificationDto?> GetNotificationsAsync(int patientId, CancellationToken cancellationToken = default);
    Task<PendingFeedbackDto?> GetPendingFeedbackAsync(int patientId, CancellationToken cancellationToken = default);
    Task<int> SubmitFeedbackAsync(int appointmentId, CancellationToken cancellationToken = default);
}
