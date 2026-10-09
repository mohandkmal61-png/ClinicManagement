namespace ClinicManagement.Domain.Entities
{
    public class Prescription : BaseEntity
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public string MedicationName { get; set; }

        public string Dosage { get; set; }

        public string Instructions { get; set; }

        public DateTime CreatedAt { get; set; }

        public Prescription(
            int patientId,
            int doctorId,
            string medicationName,
            string dosage,
            string instructions)
        {
            PatientId = patientId;
            DoctorId = doctorId;
            MedicationName = medicationName;
            Dosage = dosage;
            Instructions = instructions;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
