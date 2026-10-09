namespace ClinicManagement.Application.DTOs.Prescriptions
{
    public class PrescriptionDto
    {
        public int Id { get; init; }

        public int PatientId { get; init; }

        public int DoctorId { get; init; }

        public required string MedicationName { get; init; }

        public required string Dosage { get; init; }

        public required string Instructions { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
