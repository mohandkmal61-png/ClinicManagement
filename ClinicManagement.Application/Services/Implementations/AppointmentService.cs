using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Application.Exceptions;
using ClinicManagement.Application.Services.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<Appointment> _appointments;
        private readonly IRepository<Doctor> _doctors;
        private readonly IRepository<Patient> _patients;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IRepository<Appointment> appointments,
            IRepository<Doctor> doctors,
            IRepository<Patient> patients,
            IUnitOfWork unitOfWork,
            ILogger<AppointmentService> logger)
        {
            _appointments = appointments;
            _doctors = doctors;
            _patients = patients;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AppointmentDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var appointment = await _appointments.GetByIdAsync(id, cancellationToken);
            return appointment is null ? null : ToDto(appointment);
        }

        public async Task<IReadOnlyList<AppointmentDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var appointments = await _appointments.GetAllAsync(cancellationToken);
            return appointments.Select(ToDto).ToList();
        }

        public async Task<AppointmentDto> CreateAsync(
            CreateAppointmentDto dto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Attempting to create appointment for DoctorId {DoctorId}.",
                dto.DoctorId);

            try
            {
                await ValidateAppointmentAsync(
                    dto.DoctorId,
                    dto.PatientId,
                    dto.AppointmentDate,
                    cancellationToken);
                await ValidateAppointmentConflictsAsync(
                    dto.DoctorId,
                    dto.PatientId,
                    dto.AppointmentDate,
                    excludedAppointmentId: null,
                    cancellationToken);
            }
            catch (BusinessRuleException exception)
            {
                _logger.LogWarning(
                    "Appointment creation rejected for DoctorId {DoctorId}. Rule: {BusinessRule}",
                    dto.DoctorId,
                    exception.Message);
                throw;
            }

            var appointment = new Appointment(dto.DoctorId, dto.PatientId, dto.AppointmentDate);
            await _appointments.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created appointment {AppointmentId} for DoctorId {DoctorId}.",
                appointment.Id,
                appointment.DoctorId);

            return ToDto(appointment);
        }

        public async Task UpdateAsync(
            int id,
            UpdateAppointmentDto dto,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(dto.Status))
            {
                throw new BusinessRuleException("Appointment status is invalid.");
            }

            var appointment = await GetRequiredAsync(id, cancellationToken);
            var schedulingDataChanged =
                appointment.DoctorId != dto.DoctorId ||
                appointment.PatientId != dto.PatientId ||
                appointment.AppointmentDate != dto.AppointmentDate;

            if (schedulingDataChanged || dto.Status == AppointmentStatus.Scheduled)
            {
                await ValidateAppointmentAsync(
                    dto.DoctorId,
                    dto.PatientId,
                    dto.AppointmentDate,
                    cancellationToken);
            }

            if (dto.Status == AppointmentStatus.Scheduled)
            {
                await ValidateAppointmentConflictsAsync(
                    dto.DoctorId,
                    dto.PatientId,
                    dto.AppointmentDate,
                    excludedAppointmentId: id,
                    cancellationToken);
            }

            appointment.DoctorId = dto.DoctorId;
            appointment.PatientId = dto.PatientId;
            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.Status = dto.Status;
            _appointments.Update(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var appointment = await GetRequiredAsync(id, cancellationToken);
            _appointments.Delete(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Appointment> GetRequiredAsync(int id, CancellationToken cancellationToken)
        {
            return await _appointments.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Appointment with ID {id} was not found.");
        }

        private async Task ValidateAppointmentAsync(
            int doctorId,
            int patientId,
            DateTime appointmentDate,
            CancellationToken cancellationToken)
        {
            if (await _doctors.GetByIdAsync(doctorId, cancellationToken) is null)
            {
                throw new KeyNotFoundException($"Doctor with ID {doctorId} was not found.");
            }

            if (await _patients.GetByIdAsync(patientId, cancellationToken) is null)
            {
                throw new KeyNotFoundException($"Patient with ID {patientId} was not found.");
            }

            if (appointmentDate <= DateTime.UtcNow)
            {
                throw new BusinessRuleException("Appointment date must be in the future.");
            }
        }

        private async Task ValidateAppointmentConflictsAsync(
            int doctorId,
            int patientId,
            DateTime appointmentDate,
            int? excludedAppointmentId,
            CancellationToken cancellationToken)
        {
            var doctorHasConflict = await _appointments.ExistsAsync(
                appointment =>
                    appointment.DoctorId == doctorId &&
                    appointment.AppointmentDate == appointmentDate &&
                    appointment.Status == AppointmentStatus.Scheduled &&
                    (!excludedAppointmentId.HasValue || appointment.Id != excludedAppointmentId.Value),
                cancellationToken);

            if (doctorHasConflict)
            {
                throw new BusinessRuleException(
                    "The doctor already has a scheduled appointment at this date and time.");
            }

            var patientHasConflict = await _appointments.ExistsAsync(
                appointment =>
                    appointment.PatientId == patientId &&
                    appointment.AppointmentDate == appointmentDate &&
                    appointment.Status == AppointmentStatus.Scheduled &&
                    (!excludedAppointmentId.HasValue || appointment.Id != excludedAppointmentId.Value),
                cancellationToken);

            if (patientHasConflict)
            {
                throw new BusinessRuleException(
                    "The patient already has a scheduled appointment at this date and time.");
            }
        }

        private static AppointmentDto ToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status
            };
        }
    }
}
