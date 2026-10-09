using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.DTOs.MedicalRecords;
using ClinicManagement.Application.Services.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Services.Implementations
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IRepository<MedicalRecord> _medicalRecords;
        private readonly IRepository<Doctor> _doctors;
        private readonly IRepository<Patient> _patients;
        private readonly IUnitOfWork _unitOfWork;

        public MedicalRecordService(
            IRepository<MedicalRecord> medicalRecords,
            IRepository<Doctor> doctors,
            IRepository<Patient> patients,
            IUnitOfWork unitOfWork)
        {
            _medicalRecords = medicalRecords;
            _doctors = doctors;
            _patients = patients;
            _unitOfWork = unitOfWork;
        }

        public async Task<MedicalRecordDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var medicalRecord = await _medicalRecords.GetByIdAsync(id, cancellationToken);
            return medicalRecord is null ? null : ToDto(medicalRecord);
        }

        public async Task<IReadOnlyList<MedicalRecordDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var medicalRecords = await _medicalRecords.GetAllAsync(cancellationToken);
            return medicalRecords.Select(ToDto).ToList();
        }

        public async Task<MedicalRecordDto> CreateAsync(
            CreateMedicalRecordDto dto,
            CancellationToken cancellationToken = default)
        {
            await ValidateReferencesAsync(dto.DoctorId, dto.PatientId, cancellationToken);

            var medicalRecord = new MedicalRecord(
                dto.PatientId,
                dto.DoctorId,
                dto.Diagnosis,
                dto.Notes);

            await _medicalRecords.AddAsync(medicalRecord, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ToDto(medicalRecord);
        }

        public async Task UpdateAsync(
            int id,
            UpdateMedicalRecordDto dto,
            CancellationToken cancellationToken = default)
        {
            var medicalRecord = await GetRequiredAsync(id, cancellationToken);
            await ValidateReferencesAsync(dto.DoctorId, dto.PatientId, cancellationToken);

            medicalRecord.PatientId = dto.PatientId;
            medicalRecord.DoctorId = dto.DoctorId;
            medicalRecord.Diagnosis = dto.Diagnosis;
            medicalRecord.Notes = dto.Notes;
            _medicalRecords.Update(medicalRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var medicalRecord = await GetRequiredAsync(id, cancellationToken);
            _medicalRecords.Delete(medicalRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task ValidateReferencesAsync(
            int doctorId,
            int patientId,
            CancellationToken cancellationToken)
        {
            if (!await _doctors.ExistsAsync(doctor => doctor.Id == doctorId, cancellationToken))
            {
                throw new KeyNotFoundException($"Doctor with id {doctorId} was not found.");
            }

            if (!await _patients.ExistsAsync(patient => patient.Id == patientId, cancellationToken))
            {
                throw new KeyNotFoundException($"Patient with id {patientId} was not found.");
            }
        }

        private async Task<MedicalRecord> GetRequiredAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await _medicalRecords.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Medical record with ID {id} was not found.");
        }

        private static MedicalRecordDto ToDto(MedicalRecord medicalRecord)
        {
            return new MedicalRecordDto
            {
                Id = medicalRecord.Id,
                PatientId = medicalRecord.PatientId,
                DoctorId = medicalRecord.DoctorId,
                Diagnosis = medicalRecord.Diagnosis,
                Notes = medicalRecord.Notes,
                CreatedAt = medicalRecord.CreatedAt
            };
        }
    }
}
