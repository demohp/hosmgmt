using ClinicManagement.Domain.DTOs;
using ClinicManagement.Domain.Interfaces.Repositories;
using ClinicManagement.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services;

/// <summary>Doctor service implementation.</summary>
public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IAppointmentRepository appointmentRepository,
        IDepartmentRepository departmentRepository,
        ILogger<DoctorService> logger)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    /// <summary>Gets doctor information by ID.</summary>
    public async Task<DoctorProfileDto?> GetDoctorInfoAsync(int doctorId, CancellationToken cancellationToken = default)
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
            _logger.LogError(ex, "Error getting doctor info for {DoctorId}", doctorId);
            return null;
        }
    }

    /// <summary>Gets pending appointments for a doctor.</summary>
    public async Task<IEnumerable<AppointmentListItemDto>> GetPendingAppointmentsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments = await _appointmentRepository.GetPendingByDoctorIdAsync(doctorId, cancellationToken);
            return appointments.Select(a => new AppointmentListItemDto
            {
                AppointId = a.AppointId,
                PatientName = a.Patient?.Name,
                Date = a.Date,
                Status = a.AppointmentStatus.ToString(),
                Disease = a.Disease,
                Progress = a.Progress,
                Prescription = a.Prescription
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending appointments for doctor {DoctorId}", doctorId);
            return Enumerable.Empty<AppointmentListItemDto>();
        }
    }

    /// <summary>Approves an appointment.</summary>
    public async Task<int> ApproveAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null) return 0;

            appointment.AppointmentStatus = 1; // Approved
            appointment.DoctorNotification = 1; // Seen
            appointment.PatientNotification = 2; // Unseen (notify patient)
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            _logger.LogInformation("Appointment {AppointmentId} approved", appointmentId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving appointment {AppointmentId}", appointmentId);
            return 0;
        }
    }

    /// <summary>Deletes an appointment.</summary>
    public async Task<int> DeleteAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _appointmentRepository.DeleteAsync(appointmentId, cancellationToken);
            _logger.LogInformation("Appointment {AppointmentId} deleted", appointmentId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting appointment {AppointmentId}", appointmentId);
            return -1;
        }
    }

    /// <summary>Gets today's appointments for a doctor.</summary>
    public async Task<IEnumerable<AppointmentListItemDto>> GetTodaysAppointmentsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments = await _appointmentRepository.GetTodaysByDoctorIdAsync(doctorId, cancellationToken);
            return appointments.Select(a => new AppointmentListItemDto
            {
                AppointId = a.AppointId,
                PatientName = a.Patient?.Name,
                Date = a.Date,
                Status = a.AppointmentStatus.ToString(),
                Disease = a.Disease,
                Progress = a.Progress,
                Prescription = a.Prescription
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's appointments for doctor {DoctorId}", doctorId);
            return Enumerable.Empty<AppointmentListItemDto>();
        }
    }

    /// <summary>Updates prescription for an appointment.</summary>
    public async Task<int> UpdatePrescriptionAsync(int doctorId, int appointmentId, string disease, string progress, string prescription, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null || appointment.DoctorId != doctorId) return 0;

            appointment.Disease = disease;
            appointment.Progress = progress;
            appointment.Prescription = prescription;
            appointment.AppointmentStatus = 3; // Completed

            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            _logger.LogInformation("Prescription updated for appointment {AppointmentId}", appointmentId);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription for appointment {AppointmentId}", appointmentId);
            return 0;
        }
    }

    /// <summary>Generates bill for doctor's appointments.</summary>
    public async Task<IEnumerable<BillItemDto>> GenerateBillAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId, cancellationToken);
            var completedAppts = appointments.Where(a => a.AppointmentStatus == 3); // Completed

            return completedAppts.Select(a => new BillItemDto
            {
                AppointId = a.AppointId,
                PatientName = a.Patient?.Name,
                Date = a.Date,
                BillAmount = a.BillAmount,
                BillStatus = a.BillStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bill for doctor {DoctorId}", doctorId);
            return Enumerable.Empty<BillItemDto>();
        }
    }

    /// <summary>Marks a bill as paid.</summary>
    public async Task MarkBillPaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null || appointment.DoctorId != doctorId) return;

            appointment.BillStatus = "Paid";
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            _logger.LogInformation("Bill marked as paid for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking bill paid for appointment {AppointmentId}", appointmentId);
        }
    }

    /// <summary>Marks a bill as unpaid.</summary>
    public async Task MarkBillUnpaidAsync(int doctorId, int appointmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null || appointment.DoctorId != doctorId) return;

            appointment.BillStatus = "Unpaid";
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            _logger.LogInformation("Bill marked as unpaid for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking bill unpaid for appointment {AppointmentId}", appointmentId);
        }
    }

    /// <summary>Gets patient history for a doctor.</summary>
    public async Task<IEnumerable<PatientHistoryItemDto>> GetPatientHistoryAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId, cancellationToken);
            var completed = appointments.Where(a => a.AppointmentStatus == 3); // Completed

            return completed.Select(a => new PatientHistoryItemDto
            {
                AppointId = a.AppointId,
                PatientName = a.Patient?.Name,
                Date = a.Date,
                Disease = a.Disease,
                Progress = a.Progress,
                Prescription = a.Prescription,
                BillStatus = a.BillStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient history for doctor {DoctorId}", doctorId);
            return Enumerable.Empty<PatientHistoryItemDto>();
        }
    }
}
