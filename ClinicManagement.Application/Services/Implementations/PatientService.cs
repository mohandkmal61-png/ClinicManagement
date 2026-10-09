using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.DTOs.Common;
using ClinicManagement.Application.DTOs.Patients;
using ClinicManagement.Application.Services.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IRepository<Patient> _patients;
        private readonly IUnitOfWork _unitOfWork;

        public PatientService(IRepository<Patient> patients, IUnitOfWork unitOfWork)
        {
            _patients = patients;
            _unitOfWork = unitOfWork;
        }

        public async Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var patient = await _patients.GetByIdAsync(id, cancellationToken);
            return patient is null ? null : ToDto(patient);
        }

        public async Task<PagedResponse<PatientDto>> GetPageAsync(
            int pageNumber,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1.");
            }

            if (pageSize is < 1 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
            }

            var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var page = await _patients.GetPageAsync(
                normalizedSearch is null
                    ? null
                    : patient => patient.Name.Contains(normalizedSearch),
                patient => patient.Id,
                pageNumber,
                pageSize,
                cancellationToken);

            return new PagedResponse<PatientDto>(
                page.Items.Select(ToDto).ToList(),
                pageNumber,
                pageSize,
                page.TotalCount);
        }

        public async Task<PatientDto> CreateAsync(
            CreatePatientDto dto,
            CancellationToken cancellationToken = default)
        {
            var patient = new Patient(dto.Name, dto.Email, dto.Phone, dto.DateOfBirth);
            await _patients.AddAsync(patient, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ToDto(patient);
        }

        public async Task UpdateAsync(
            int id,
            UpdatePatientDto dto,
            CancellationToken cancellationToken = default)
        {
            var patient = await GetRequiredAsync(id, cancellationToken);
            patient.Name = dto.Name;
            patient.Email = dto.Email;
            patient.Phone = dto.Phone;
            patient.DateOfBirth = dto.DateOfBirth;
            _patients.Update(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var patient = await GetRequiredAsync(id, cancellationToken);
            _patients.Delete(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Patient> GetRequiredAsync(int id, CancellationToken cancellationToken)
        {
            return await _patients.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Patient with ID {id} was not found.");
        }

        private static PatientDto ToDto(Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                Name = patient.Name,
                Email = patient.Email,
                Phone = patient.Phone,
                DateOfBirth = patient.DateOfBirth
            };
        }
    }
}
