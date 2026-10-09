using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.DTOs.Prescriptions;
using ClinicManagement.Application.Services.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Services.Implementations
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IRepository<Prescription> _prescriptions;
        private readonly IRepository<Doctor> _doctors;
        private readonly IRepository<Patient> _patients;
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionService(
            IRepository<Prescription> prescriptions,
            IRepository<Doctor> doctors,
            IRepository<Patient> patients,
            IUnitOfWork unitOfWork)
        {
            _prescriptions = prescriptions;
            _doctors = doctors;
            _patients = patients;
            _unitOfWork = unitOfWork;
        }

        public async Task<PrescriptionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var prescription = await _prescriptions.GetByIdAsync(id, cancellationToken);
            return prescription is null ? null : ToDto(prescription);
        }

        public async Task<IReadOnlyList<PrescriptionDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var prescriptions = await _prescriptions.GetAllAsync(cancellationToken);
            return prescriptions.Select(ToDto).ToList();
        }

        public async Task<PrescriptionDto> CreateAsync(
            CreatePrescriptionDto dto,
            CancellationToken cancellationToken = default)
        {
            await ValidateReferencesAsync(dto.DoctorId, dto.PatientId, cancellationToken);

            var prescription = new Prescription(
                dto.PatientId,
                dto.DoctorId,
                dto.MedicationName,
                dto.Dosage,
                dto.Instructions);

            await _prescriptions.AddAsync(prescription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ToDto(prescription);
        }

        public async Task UpdateAsync(
            int id,
            UpdatePrescriptionDto dto,
            CancellationToken cancellationToken = default)
        {
            var prescription = await GetRequiredAsync(id, cancellationToken);
            await ValidateReferencesAsync(dto.DoctorId, dto.PatientId, cancellationToken);

            prescription.PatientId = dto.PatientId;
            prescription.DoctorId = dto.DoctorId;
            prescription.MedicationName = dto.MedicationName;
            prescription.Dosage = dto.Dosage;
            prescription.Instructions = dto.Instructions;
            _prescriptions.Update(prescription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var prescription = await GetRequiredAsync(id, cancellationToken);
            _prescriptions.Delete(prescription);
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

        private async Task<Prescription> GetRequiredAsync(
            int id,
            CancellationToken cancellationToken)
        {
            return await _prescriptions.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Prescription with ID {id} was not found.");
        }

        private static PrescriptionDto ToDto(Prescription prescription)
        {
            return new PrescriptionDto
            {
                Id = prescription.Id,
                PatientId = prescription.PatientId,
                DoctorId = prescription.DoctorId,
                MedicationName = prescription.MedicationName,
                Dosage = prescription.Dosage,
                Instructions = prescription.Instructions,
                CreatedAt = prescription.CreatedAt
            };
        }
    }
}
