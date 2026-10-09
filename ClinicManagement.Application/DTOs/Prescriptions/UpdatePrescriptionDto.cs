namespace ClinicManagement.Application.DTOs.Prescriptions
{
    public class UpdatePrescriptionDto
    {
        public int PatientId { get; init; }

        public int DoctorId { get; init; }

        public required string MedicationName { get; init; }

        public required string Dosage { get; init; }

        public required string Instructions { get; init; }
    }
}
