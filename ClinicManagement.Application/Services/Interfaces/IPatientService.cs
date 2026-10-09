using ClinicManagement.Application.DTOs.Common;
using ClinicManagement.Application.DTOs.Patients;

namespace ClinicManagement.Application.Services.Interfaces
{
    public interface IPatientService
    {
        Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<PagedResponse<PatientDto>> GetPageAsync(
            int pageNumber,
            int pageSize,
            string? search,
            CancellationToken cancellationToken = default);

        Task<PatientDto> CreateAsync(CreatePatientDto dto, CancellationToken cancellationToken = default);

        Task UpdateAsync(int id, UpdatePatientDto dto, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
