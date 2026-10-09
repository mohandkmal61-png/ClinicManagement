using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.DTOs.Appointments
{
    public class AppointmentDto
    {
        public int Id { get; init; }

        public int DoctorId { get; init; }

        public int PatientId { get; init; }

        public DateTime AppointmentDate { get; init; }

        public AppointmentStatus Status { get; init; }
    }
}
