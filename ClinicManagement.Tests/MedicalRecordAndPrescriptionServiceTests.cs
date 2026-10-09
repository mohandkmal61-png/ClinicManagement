using System.Linq.Expressions;
using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.DTOs.MedicalRecords;
using ClinicManagement.Application.DTOs.Prescriptions;
using ClinicManagement.Application.Services.Implementations;
using ClinicManagement.Domain.Entities;
using Moq;

namespace ClinicManagement.Tests;

public class MedicalRecordAndPrescriptionServiceTests
{
    private const int DoctorId = 10;
    private const int PatientId = 20;
    private const int RecordId = 30;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MedicalRecordCreateAsync_WhenReferenceIsMissing_ThrowsAndDoesNotSave(
        bool missingDoctor)
    {
        var fixture = new MedicalRecordFixture(
            doctorExists: !missingDoctor,
            patientExists: missingDoctor);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => fixture.Service.CreateAsync(CreateMedicalRecordDto()));

        Assert.Contains(missingDoctor ? "Doctor with id 10" : "Patient with id 20", exception.Message);
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Records.Verify(repository => repository.AddAsync(
            It.IsAny<MedicalRecord>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task MedicalRecordUpdateAsync_WhenReferenceIsMissing_ThrowsAndDoesNotSave(
        bool missingDoctor)
    {
        var fixture = new MedicalRecordFixture(
            doctorExists: !missingDoctor,
            patientExists: missingDoctor,
            includeExistingRecord: true);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => fixture.Service.UpdateAsync(RecordId, UpdateMedicalRecordDto()));

        Assert.Contains(missingDoctor ? "Doctor with id 10" : "Patient with id 20", exception.Message);
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Records.Verify(repository => repository.Update(It.IsAny<MedicalRecord>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PrescriptionCreateAsync_WhenReferenceIsMissing_ThrowsAndDoesNotSave(
        bool missingDoctor)
    {
        var fixture = new PrescriptionFixture(
            doctorExists: !missingDoctor,
            patientExists: missingDoctor);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => fixture.Service.CreateAsync(CreatePrescriptionDto()));

        Assert.Contains(missingDoctor ? "Doctor with id 10" : "Patient with id 20", exception.Message);
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Prescriptions.Verify(repository => repository.AddAsync(
            It.IsAny<Prescription>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PrescriptionUpdateAsync_WhenReferenceIsMissing_ThrowsAndDoesNotSave(
        bool missingDoctor)
    {
        var fixture = new PrescriptionFixture(
            doctorExists: !missingDoctor,
            patientExists: missingDoctor,
            includeExistingPrescription: true);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => fixture.Service.UpdateAsync(RecordId, UpdatePrescriptionDto()));

        Assert.Contains(missingDoctor ? "Doctor with id 10" : "Patient with id 20", exception.Message);
        fixture.UnitOfWork.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        fixture.Prescriptions.Verify(repository => repository.Update(It.IsAny<Prescription>()), Times.Never);
    }

    private static CreateMedicalRecordDto CreateMedicalRecordDto()
    {
        return new CreateMedicalRecordDto
        {
            DoctorId = DoctorId,
            PatientId = PatientId,
            Diagnosis = "Diagnosis",
            Notes = "Notes"
        };
    }

    private static UpdateMedicalRecordDto UpdateMedicalRecordDto()
    {
        return new UpdateMedicalRecordDto
        {
            DoctorId = DoctorId,
            PatientId = PatientId,
            Diagnosis = "Diagnosis",
            Notes = "Notes"
        };
    }

    private static CreatePrescriptionDto CreatePrescriptionDto()
    {
        return new CreatePrescriptionDto
        {
            DoctorId = DoctorId,
            PatientId = PatientId,
            MedicationName = "Medication",
            Dosage = "One",
            Instructions = "Instructions"
        };
    }

    private static UpdatePrescriptionDto UpdatePrescriptionDto()
    {
        return new UpdatePrescriptionDto
        {
            DoctorId = DoctorId,
            PatientId = PatientId,
            MedicationName = "Medication",
            Dosage = "One",
            Instructions = "Instructions"
        };
    }

    private sealed class MedicalRecordFixture
    {
        public Mock<IRepository<MedicalRecord>> Records { get; } = new();
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public MedicalRecordService Service { get; }

        public MedicalRecordFixture(
            bool doctorExists,
            bool patientExists,
            bool includeExistingRecord = false)
        {
            var doctors = CreateDoctorRepository(doctorExists);
            var patients = CreatePatientRepository(patientExists);
            if (includeExistingRecord)
            {
                Records.Setup(repository => repository.GetByIdAsync(
                        RecordId,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new MedicalRecord(PatientId, DoctorId, "Old diagnosis", "Old notes")
                    {
                        Id = RecordId
                    });
            }

            Service = new MedicalRecordService(
                Records.Object,
                doctors.Object,
                patients.Object,
                UnitOfWork.Object);
        }
    }

    private sealed class PrescriptionFixture
    {
        public Mock<IRepository<Prescription>> Prescriptions { get; } = new();
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public PrescriptionService Service { get; }

        public PrescriptionFixture(
            bool doctorExists,
            bool patientExists,
            bool includeExistingPrescription = false)
        {
            var doctors = CreateDoctorRepository(doctorExists);
            var patients = CreatePatientRepository(patientExists);
            if (includeExistingPrescription)
            {
                Prescriptions.Setup(repository => repository.GetByIdAsync(
                        RecordId,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Prescription(
                        PatientId,
                        DoctorId,
                        "Old medication",
                        "Old dosage",
                        "Old instructions")
                    {
                        Id = RecordId
                    });
            }

            Service = new PrescriptionService(
                Prescriptions.Object,
                doctors.Object,
                patients.Object,
                UnitOfWork.Object);
        }
    }

    private static Mock<IRepository<Doctor>> CreateDoctorRepository(bool exists)
    {
        var repository = new Mock<IRepository<Doctor>>();
        repository.Setup(item => item.ExistsAsync(
                It.IsAny<Expression<Func<Doctor, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Doctor, bool>> predicate, CancellationToken _) =>
                exists && predicate.Compile()(new Doctor("Doctor", "General") { Id = DoctorId }));
        return repository;
    }

    private static Mock<IRepository<Patient>> CreatePatientRepository(bool exists)
    {
        var repository = new Mock<IRepository<Patient>>();
        repository.Setup(item => item.ExistsAsync(
                It.IsAny<Expression<Func<Patient, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Patient, bool>> predicate, CancellationToken _) =>
                exists && predicate.Compile()(new Patient(
                    "Patient",
                    "patient@example.test",
                    "01000000000",
                    DateTime.UtcNow)
                {
                    Id = PatientId
                }));
        return repository;
    }
}
