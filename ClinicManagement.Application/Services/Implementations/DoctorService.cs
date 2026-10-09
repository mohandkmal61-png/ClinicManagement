using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.DTOs.Doctors;
using ClinicManagement.Application.Services.Interfaces;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IRepository<Doctor> _doctors;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(IRepository<Doctor> doctors, IUnitOfWork unitOfWork)
        {
            _doctors = doctors;
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var doctor = await _doctors.GetByIdAsync(id, cancellationToken);
            return doctor is null ? null : ToDto(doctor);
        }

        public async Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var doctors = await _doctors.GetAllAsync(cancellationToken);
            return doctors.Select(ToDto).ToList();
        }

        public async Task<DoctorDto> CreateAsync(
            CreateDoctorDto dto,
            CancellationToken cancellationToken = default)
        {
            var doctor = new Doctor(dto.Name, dto.Specialization);
            await _doctors.AddAsync(doctor, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ToDto(doctor);
        }

        public async Task UpdateAsync(
            int id,
            UpdateDoctorDto dto,
            CancellationToken cancellationToken = default)
        {
            var doctor = await GetRequiredAsync(id, cancellationToken);
            doctor.Name = dto.Name;
            doctor.Specialization = dto.Specialization;
            _doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var doctor = await GetRequiredAsync(id, cancellationToken);
            _doctors.Delete(doctor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Doctor> GetRequiredAsync(int id, CancellationToken cancellationToken)
        {
            return await _doctors.GetByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Doctor with ID {id} was not found.");
        }

        private static DoctorDto ToDto(Doctor doctor)
        {
            return new DoctorDto
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Specialization = doctor.Specialization
            };
        }
    }
}
