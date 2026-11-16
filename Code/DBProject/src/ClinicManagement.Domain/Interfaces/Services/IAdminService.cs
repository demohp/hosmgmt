using ClinicManagement.Domain.DTOs;

namespace ClinicManagement.Domain.Interfaces.Services;

/// <summary>Service interface for admin operations.</summary>
public interface IAdminService
{
    Task<AdminHomeDto> GetAdminHomeInformationAsync(CancellationToken cancellationToken = default);
    Task<int> AddDoctorAsync(AddDoctorDto dto, CancellationToken cancellationToken = default);
    Task<int> AddStaffAsync(AddStaffDto dto, CancellationToken cancellationToken = default);
    Task<int> DeleteDoctorAsync(int id, CancellationToken cancellationToken = default);
    Task<int> DeleteStaffAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DoctorListItemDto>> LoadDoctorsAsync(string searchQuery, CancellationToken cancellationToken = default);
    Task<IEnumerable<PatientListItemDto>> LoadPatientsAsync(string searchQuery, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffListItemDto>> LoadStaffAsync(string searchQuery, CancellationToken cancellationToken = default);
    Task<PatientDetailDto?> GetPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<DoctorProfileDto?> GetDoctorProfileAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<StaffDetailDto?> GetStaffAsync(int staffId, CancellationToken cancellationToken = default);
    Task<bool> DoctorEmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
