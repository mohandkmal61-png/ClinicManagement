using ClinicManagement.Application.DTOs.Prescriptions;

namespace ClinicManagement.Application.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PrescriptionDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<PrescriptionDto> CreateAsync(
            CreatePrescriptionDto dto,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            int id,
            UpdatePrescriptionDto dto,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
