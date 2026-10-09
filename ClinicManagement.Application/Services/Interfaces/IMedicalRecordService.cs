using ClinicManagement.Application.DTOs.MedicalRecords;

namespace ClinicManagement.Application.Services.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<MedicalRecordDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MedicalRecordDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<MedicalRecordDto> CreateAsync(
            CreateMedicalRecordDto dto,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            int id,
            UpdateMedicalRecordDto dto,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
