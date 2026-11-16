using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>Patient service implementation.</summary>
public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        IPatientRepository patientRepository,
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IDepartmentRepository departmentRepository,
        ILogger<PatientService> logger)
    {
        _patientRepository = patientRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    /// <summary>Gets patient information by ID.</summary>
    public async Task<PatientDetailDto?> GetPatientInfoAsync(int patientId, CancellationToken cancellationToken = default)
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
            _logger.LogError(ex, "Error getting patient info for {PatientId}", patientId);
            return null;
        }
    }

    /// <summary>Gets bill history for a patient.</summary>
    public async Task<IEnumerable<BillHistoryItemDto>> GetBillHistoryAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments = await _appointmentRepository.GetBillHistoryByPatientIdAsync(patientId, cancellationToken);
            return appointments.Select(a => new BillHistoryItemDto
            {
                AppointId = a.AppointId,
                DoctorName = a.Doctor?.Name,
                Date = a.Date,
                BillAmount = a.BillAmount,
                BillStatus = a.BillStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bill history for patient {PatientId}", patientId);
            return Enumerable.Empty<BillHistoryItemDto>();
        }
    }

    /// <summary>Gets current appointment for a patient.</summary>
    public async Task<CurrentAppointmentDto?> GetCurrentAppointmentAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetCurrentByPatientIdAsync(patientId, cancellationToken);
            if (appointment == null) return null;

            return new CurrentAppointmentDto
            {
                DoctorName = appointment.Doctor?.Name ?? "Unknown",
                Timings = appointment.Date?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current appointment for patient {PatientId}", patientId);
            return null;
        }
    }

    /// <summary>Gets treatment history for a patient.</summary>
    public async Task<IEnumerable<TreatmentHistoryItemDto>> GetTreatmentHistoryAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments = await _appointmentRepository.GetTreatmentHistoryByPatientIdAsync(patientId, cancellationToken);
            return appointments.Select(a => new TreatmentHistoryItemDto
            {
                AppointId = a.AppointId,
                DoctorName = a.Doctor?.Name,
                Date = a.Date,
                Disease = a.Disease,
                Progress = a.Progress,
                Prescription = a.Prescription
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treatment history for patient {PatientId}", patientId);
            return Enumerable.Empty<TreatmentHistoryItemDto>();
        }
    }

    /// <summary>Gets all departments.</summary>
    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var departments = await _departmentRepository.GetAllAsync(cancellationToken);
            return departments.Select(d => new DepartmentDto
            {
                DeptNo = d.DeptNo,
                DeptName = d.DeptName,
                Description = d.Description
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return Enumerable.Empty<DepartmentDto>();
        }
    }

    /// <summary>Gets doctors by department name.</summary>
    public async Task<IEnumerable<DoctorListItemDto>> GetDoctorsByDepartmentAsync(string deptName, CancellationToken cancellationToken = default)
    {
        try
        {
            var doctors = await _doctorRepository.GetByDepartmentAsync(deptName, cancellationToken);
            return doctors.Select(d => new DoctorListItemDto
            {
                DoctorId = d.DoctorId,
                Name = d.Name,
                Department = d.Department?.DeptName ?? deptName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctors for department {DeptName}", deptName);
            return Enumerable.Empty<DoctorListItemDto>();
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
                Department = doctor.Department?.DeptName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor profile {DoctorId}", doctorId);
            return null;
        }
    }

    /// <summary>Gets free appointment slots for a doctor.</summary>
    public async Task<IEnumerable<AppointmentSlotDto>> GetFreeSlotsAsync(int doctorId, int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var slots = await _appointmentRepository.GetFreeSlotsByDoctorIdAsync(doctorId, patientId, cancellationToken);
            return slots.Select((a, i) => new AppointmentSlotDto
            {
                SlotId = a.AppointId,
                Timings = a.Date?.ToString("yyyy-MM-dd HH:mm") ?? $"Slot {i + 1}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting free slots for doctor {DoctorId}", doctorId);
            return Enumerable.Empty<AppointmentSlotDto>();
        }
    }

    /// <summary>Inserts a new appointment request.</summary>
    public async Task<int> InsertAppointmentAsync(int doctorId, int patientId, int freeSlot, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                Date = DateTime.Now,
                AppointmentStatus = 2, // Pending
                DoctorNotification = 2, // Unseen
                PatientNotification = 2, // Unseen
                FeedbackStatus = 2 // Pending
            };

            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            _logger.LogInformation("Appointment inserted for patient {PatientId} with doctor {DoctorId}", patientId, doctorId);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting appointment for patient {PatientId}", patientId);
            return -1;
        }
    }

    /// <summary>Gets notifications for a patient.</summary>
    public async Task<NotificationDto?> GetNotificationsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetCurrentByPatientIdAsync(patientId, cancellationToken);
            if (appointment == null) return null;

            return new NotificationDto
            {
                DoctorName = appointment.Doctor?.Name ?? "Unknown",
                Timings = appointment.Date?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty,
                Count = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for patient {PatientId}", patientId);
            return null;
        }
    }

    /// <summary>Gets pending feedback for a patient.</summary>
    public async Task<PendingFeedbackDto?> GetPendingFeedbackAsync(int patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetPendingFeedbackByPatientIdAsync(patientId, cancellationToken);
            if (appointment == null) return null;

            return new PendingFeedbackDto
            {
                AppointmentId = appointment.AppointId,
                DoctorName = appointment.Doctor?.Name ?? "Unknown",
                Timings = appointment.Date?.ToString("yyyy-MM-dd HH:mm") ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending feedback for patient {PatientId}", patientId);
            return null;
        }
    }

    /// <summary>Submits feedback for an appointment.</summary>
    public async Task<int> SubmitFeedbackAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return -1;

            appointment.FeedbackStatus = 1; // Given
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            _logger.LogInformation("Feedback submitted for appointment {AppointmentId}", appointmentId);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for appointment {AppointmentId}", appointmentId);
            return -1;
        }
    }
}
