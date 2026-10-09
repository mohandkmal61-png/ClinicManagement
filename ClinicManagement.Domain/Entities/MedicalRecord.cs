namespace ClinicManagement.Domain.Entities
{
    public class MedicalRecord : BaseEntity
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public string Diagnosis { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public MedicalRecord(int patientId, int doctorId, string diagnosis, string notes)
        {
            PatientId = patientId;
            DoctorId = doctorId;
            Diagnosis = diagnosis;
            Notes = notes;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
