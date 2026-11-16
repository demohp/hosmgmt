using ClinicManagement.Domain.DTOs;

namespace ClinicManagement.Domain.Interfaces.Services;

/// <summary>Service interface for doctor operations.</summary>
public interface IDoctorService
{
    Task<DoctorProfileDto?> GetDoctorInfoAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentListItemDto>> GetPendingAppointmentsAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<int> ApproveAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<int> DeleteAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentListItemDto>> GetTodaysAppointmentsAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<int> UpdatePrescriptionAsync(int doctorId, int appointmentId, string disease, string progress, string prescription, CancellationToken cancellationToken = default);
    Task<IEnumerable<BillItemDto>> GenerateBillAsync(int doctorId, CancellationToken cancellationToken = default);
    Task MarkBillPaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default);
    Task MarkBillUnpaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PatientHistoryItemDto>> GetPatientHistoryAsync(int doctorId, CancellationToken cancellationToken = default);
}
