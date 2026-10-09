using ClinicManagement.Application.DTOs.Appointments;

namespace ClinicManagement.Application.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AppointmentDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<AppointmentDto> CreateAsync(
            CreateAppointmentDto dto,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            int id,
            UpdateAppointmentDto dto,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
