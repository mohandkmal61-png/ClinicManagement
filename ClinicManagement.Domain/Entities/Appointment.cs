using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities
{
    public class Appointment : BaseEntity
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }

        public int PatientId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public AppointmentStatus Status { get; set; }

        public Appointment(int doctorId, int patientId, DateTime appointmentDate)
        {
            DoctorId = doctorId;
            PatientId = patientId;
            AppointmentDate = appointmentDate;
            Status = AppointmentStatus.Scheduled;
        }
    }
}
