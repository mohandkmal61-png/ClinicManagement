namespace ClinicManagement.Application.DTOs.Appointments
{
    public class CreateAppointmentDto
    {
        public int DoctorId { get; init; }

        public int PatientId { get; init; }

        public DateTime AppointmentDate { get; init; }
    }
}
