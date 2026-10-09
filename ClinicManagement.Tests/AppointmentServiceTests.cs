using System.Linq.Expressions;
using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Application.Exceptions;
using ClinicManagement.Application.Services.Implementations;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManagement.Tests;

public class AppointmentServiceTests
{
    private readonly Mock<IRepository<Appointment>> _appointments = new();
    private readonly Mock<IRepository<Doctor>> _doctors = new();
    private readonly Mock<IRepository<Patient>> _patients = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ILogger<AppointmentService>> _logger = new();

    public AppointmentServiceTests()
    {
        _doctors
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new Doctor("Doctor", "General")
            {
                Id = id
            });

        _patients
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) =>
                new Patient("Patient", "patient@example.test", "01000000000", DateTime.UtcNow)
                {
                    Id = id
                });

        _appointments
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Expression<Func<Appointment, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _appointments
            .Setup(repository => repository.AddAsync(
                It.IsAny<Appointment>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task CreateAsync_WhenDoctorDoesNotExist_ThrowsKeyNotFoundException()
    {
        _doctors
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().CreateAsync(CreateDto()));

        Assert.Contains("Doctor with ID 1", exception.Message);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenPatientDoesNotExist_ThrowsKeyNotFoundException()
    {
        _patients
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().CreateAsync(CreateDto()));

        Assert.Contains("Patient with ID 2", exception.Message);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenAppointmentIsInThePast_ThrowsBusinessRuleException()
    {
        var dto = CreateDto(appointmentDate: DateTime.UtcNow.AddMinutes(-1));

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().CreateAsync(dto));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenDoctorAlreadyHasScheduledAppointment_ThrowsBusinessRuleException()
    {
        var dto = CreateDto();
        SetupConflict(new Appointment(dto.DoctorId, 99, dto.AppointmentDate)
        {
            Id = 10,
            Status = AppointmentStatus.Scheduled
        });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().CreateAsync(dto));

        Assert.Contains("doctor already has a scheduled appointment", exception.Message);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenPatientAlreadyHasScheduledAppointment_ThrowsBusinessRuleException()
    {
        var dto = CreateDto();
        SetupConflict(new Appointment(99, dto.PatientId, dto.AppointmentDate)
        {
            Id = 10,
            Status = AppointmentStatus.Scheduled
        });

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().CreateAsync(dto));

        Assert.Contains("patient already has a scheduled appointment", exception.Message);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task CreateAsync_WhenExistingAppointmentIsNotScheduled_AllowsBooking(
        AppointmentStatus existingStatus)
    {
        var dto = CreateDto();
        SetupConflict(new Appointment(dto.DoctorId, dto.PatientId, dto.AppointmentDate)
        {
            Id = 10,
            Status = existingStatus
        });

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal(dto.DoctorId, result.DoctorId);
        Assert.Equal(dto.PatientId, result.PatientId);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task CreateAsync_WhenDoctorAppointmentIsNotScheduled_AllowsBooking(
        AppointmentStatus existingStatus)
    {
        var dto = CreateDto();
        SetupConflict(new Appointment(dto.DoctorId, 99, dto.AppointmentDate)
        {
            Id = 10,
            Status = existingStatus
        });

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task CreateAsync_WhenPatientAppointmentIsNotScheduled_AllowsBooking(
        AppointmentStatus existingStatus)
    {
        var dto = CreateDto();
        SetupConflict(new Appointment(99, dto.PatientId, dto.AppointmentDate)
        {
            Id = 10,
            Status = existingStatus
        });

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNoConflictExists_CreatesScheduledAppointment()
    {
        var dto = CreateDto();

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal(dto.DoctorId, result.DoctorId);
        Assert.Equal(dto.PatientId, result.PatientId);
        Assert.Equal(dto.AppointmentDate, result.AppointmentDate);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExcludesCurrentAppointmentFromConflictChecks()
    {
        const int appointmentId = 5;
        var appointmentDate = DateTime.UtcNow.AddDays(1);
        var existing = new Appointment(1, 2, appointmentDate)
        {
            Id = appointmentId
        };

        _appointments
            .Setup(repository => repository.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        SetupConflict(existing);
        var dto = new UpdateAppointmentDto
        {
            DoctorId = 1,
            PatientId = 2,
            AppointmentDate = appointmentDate,
            Status = AppointmentStatus.Scheduled
        };

        await CreateService().UpdateAsync(appointmentId, dto);

        Assert.Equal(appointmentDate, existing.AppointmentDate);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task UpdateAsync_WhenStatusIsDefined_AcceptsStatus(AppointmentStatus status)
    {
        const int appointmentId = 5;
        var appointmentDate = status == AppointmentStatus.Scheduled
            ? DateTime.UtcNow.AddDays(1)
            : DateTime.UtcNow.AddDays(-1);
        var existing = new Appointment(1, 2, appointmentDate)
        {
            Id = appointmentId,
            Status = AppointmentStatus.Scheduled
        };
        SetupExistingAppointment(existing);

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = existing.AppointmentDate,
            Status = status
        };

        await CreateService().UpdateAsync(appointmentId, dto);

        Assert.Equal(status, existing.Status);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenStatusIsUndefined_ThrowsBusinessRuleExceptionBeforeLoadingOrSaving()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().UpdateAsync(5, new UpdateAppointmentDto
            {
                DoctorId = 1,
                PatientId = 2,
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                Status = (AppointmentStatus)99
            }));

        _appointments.Verify(repository => repository.GetByIdAsync(
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _appointments.Verify(repository => repository.Update(It.IsAny<Appointment>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task UpdateAsync_StatusOnlyLifecycleTransition_AllowsPastAppointment(
        AppointmentStatus status)
    {
        const int appointmentId = 5;
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var existing = new Appointment(1, 2, pastDate)
        {
            Id = appointmentId,
            Status = AppointmentStatus.Scheduled
        };
        SetupExistingAppointment(existing);

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = existing.AppointmentDate,
            Status = status
        };

        await CreateService().UpdateAsync(appointmentId, dto);

        Assert.Equal(status, existing.Status);
        Assert.Equal(1, existing.DoctorId);
        Assert.Equal(2, existing.PatientId);
        Assert.Equal(pastDate, existing.AppointmentDate);
        _appointments.Verify(repository => repository.ExistsAsync(
            It.IsAny<Expression<Func<Appointment, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task UpdateAsync_StatusOnlyLifecycleTransition_IgnoresOtherScheduledAppointment(
        AppointmentStatus status)
    {
        const int appointmentId = 5;
        var appointmentDate = DateTime.UtcNow.AddDays(1);
        var existing = new Appointment(1, 2, appointmentDate)
        {
            Id = appointmentId,
            Status = AppointmentStatus.Scheduled
        };
        SetupExistingAppointment(existing);
        SetupConflict(new Appointment(1, 2, appointmentDate)
        {
            Id = 6,
            Status = AppointmentStatus.Scheduled
        });

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = existing.AppointmentDate,
            Status = status
        };

        await CreateService().UpdateAsync(appointmentId, dto);

        Assert.Equal(status, existing.Status);
        _appointments.Verify(repository => repository.ExistsAsync(
            It.IsAny<Expression<Func<Appointment, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task UpdateAsync_WhenReschedulingToConflictWithFinalNonScheduledStatus_AllowsUpdate(
        AppointmentStatus status)
    {
        const int appointmentId = 5;
        var existing = new Appointment(1, 2, DateTime.UtcNow.AddDays(1))
        {
            Id = appointmentId,
            Status = AppointmentStatus.Scheduled
        };
        var newAppointmentDate = DateTime.UtcNow.AddDays(2);
        SetupExistingAppointment(existing);
        SetupConflict(new Appointment(1, 2, newAppointmentDate)
        {
            Id = 6,
            Status = AppointmentStatus.Scheduled
        });

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = newAppointmentDate,
            Status = status
        };

        await CreateService().UpdateAsync(appointmentId, dto);

        Assert.Equal(newAppointmentDate, existing.AppointmentDate);
        Assert.Equal(status, existing.Status);
        _appointments.Verify(repository => repository.ExistsAsync(
            It.IsAny<Expression<Func<Appointment, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenReschedulingToPastDate_ThrowsBusinessRuleException()
    {
        const int appointmentId = 5;
        var existing = new Appointment(1, 2, DateTime.UtcNow.AddDays(1))
        {
            Id = appointmentId
        };
        SetupExistingAppointment(existing);

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = DateTime.UtcNow.AddMinutes(-1),
            Status = AppointmentStatus.Scheduled
        };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().UpdateAsync(appointmentId, dto));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task UpdateAsync_WhenReschedulingToPastDateWithFinalNonScheduledStatus_ThrowsBusinessRuleException(
        AppointmentStatus status)
    {
        const int appointmentId = 5;
        var existing = new Appointment(1, 2, DateTime.UtcNow.AddDays(1))
        {
            Id = appointmentId
        };
        SetupExistingAppointment(existing);

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = DateTime.UtcNow.AddMinutes(-1),
            Status = status
        };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().UpdateAsync(appointmentId, dto));

        _appointments.Verify(repository => repository.ExistsAsync(
            It.IsAny<Expression<Func<Appointment, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenReschedulingConflictsWithScheduledAppointment_ThrowsBusinessRuleException()
    {
        const int appointmentId = 5;
        var existing = new Appointment(1, 2, DateTime.UtcNow.AddDays(1))
        {
            Id = appointmentId
        };
        var newAppointmentDate = DateTime.UtcNow.AddDays(2);
        SetupExistingAppointment(existing);
        SetupConflict(new Appointment(3, 99, newAppointmentDate)
        {
            Id = 6,
            Status = AppointmentStatus.Scheduled
        });

        var dto = new UpdateAppointmentDto
        {
            DoctorId = 3,
            PatientId = 2,
            AppointmentDate = newAppointmentDate,
            Status = AppointmentStatus.Scheduled
        };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().UpdateAsync(appointmentId, dto));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenReschedulingToAvailableFutureTime_Succeeds()
    {
        const int appointmentId = 5;
        var existing = new Appointment(1, 2, DateTime.UtcNow.AddDays(1))
        {
            Id = appointmentId
        };
        var newAppointmentDate = DateTime.UtcNow.AddDays(2);
        SetupExistingAppointment(existing);

        var dto = new UpdateAppointmentDto
        {
            DoctorId = 3,
            PatientId = 4,
            AppointmentDate = newAppointmentDate,
            Status = AppointmentStatus.Scheduled
        };

        await CreateService().UpdateAsync(appointmentId, dto);

        Assert.Equal(3, existing.DoctorId);
        Assert.Equal(4, existing.PatientId);
        Assert.Equal(newAppointmentDate, existing.AppointmentDate);
        Assert.Equal(AppointmentStatus.Scheduled, existing.Status);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingStatusToScheduled_ValidatesConflicts()
    {
        const int appointmentId = 5;
        var appointmentDate = DateTime.UtcNow.AddDays(1);
        var existing = new Appointment(1, 2, appointmentDate)
        {
            Id = appointmentId,
            Status = AppointmentStatus.Cancelled
        };
        SetupExistingAppointment(existing);
        SetupConflict(new Appointment(1, 99, appointmentDate)
        {
            Id = 6,
            Status = AppointmentStatus.Scheduled
        });

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = existing.AppointmentDate,
            Status = AppointmentStatus.Scheduled
        };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().UpdateAsync(appointmentId, dto));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingStatusToScheduledWithPatientConflict_ThrowsBusinessRuleException()
    {
        const int appointmentId = 5;
        var appointmentDate = DateTime.UtcNow.AddDays(1);
        var existing = new Appointment(1, 2, appointmentDate)
        {
            Id = appointmentId,
            Status = AppointmentStatus.Cancelled
        };
        SetupExistingAppointment(existing);
        SetupConflict(new Appointment(99, existing.PatientId, appointmentDate)
        {
            Id = 6,
            Status = AppointmentStatus.Scheduled
        });

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = existing.AppointmentDate,
            Status = AppointmentStatus.Scheduled
        };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().UpdateAsync(appointmentId, dto));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenChangingStatusToScheduledWithPastDate_ThrowsBusinessRuleException()
    {
        const int appointmentId = 5;
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var existing = new Appointment(1, 2, pastDate)
        {
            Id = appointmentId,
            Status = AppointmentStatus.Cancelled
        };
        SetupExistingAppointment(existing);

        var dto = new UpdateAppointmentDto
        {
            DoctorId = existing.DoctorId,
            PatientId = existing.PatientId,
            AppointmentDate = existing.AppointmentDate,
            Status = AppointmentStatus.Scheduled
        };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => CreateService().UpdateAsync(appointmentId, dto));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenAppointmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _appointments
            .Setup(repository => repository.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().UpdateAsync(5, UpdateDto()));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenAppointmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        _appointments
            .Setup(repository => repository.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().DeleteAsync(5));

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private AppointmentService CreateService()
    {
        return new AppointmentService(
            _appointments.Object,
            _doctors.Object,
            _patients.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    private void SetupConflict(Appointment existingAppointment)
    {
        _appointments
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Expression<Func<Appointment, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Expression<Func<Appointment, bool>> predicate,
                CancellationToken _) => predicate.Compile()(existingAppointment));
    }

    private void SetupExistingAppointment(Appointment appointment)
    {
        _appointments
            .Setup(repository => repository.GetByIdAsync(
                appointment.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
    }

    private static CreateAppointmentDto CreateDto(DateTime? appointmentDate = null)
    {
        return new CreateAppointmentDto
        {
            DoctorId = 1,
            PatientId = 2,
            AppointmentDate = appointmentDate ?? DateTime.UtcNow.AddDays(1)
        };
    }

    private static UpdateAppointmentDto UpdateDto()
    {
        return new UpdateAppointmentDto
        {
            DoctorId = 1,
            PatientId = 2,
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            Status = AppointmentStatus.Scheduled
        };
    }
}
