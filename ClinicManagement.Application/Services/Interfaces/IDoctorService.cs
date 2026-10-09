using ClinicManagement.Application.DTOs.Doctors;

namespace ClinicManagement.Application.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<DoctorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<DoctorDto> CreateAsync(CreateDoctorDto dto, CancellationToken cancellationToken = default);

        Task UpdateAsync(int id, UpdateDoctorDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
