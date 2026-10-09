namespace ClinicManagement.Application.DTOs.MedicalRecords
{
    public class MedicalRecordDto
    {
        public int Id { get; init; }

        public int PatientId { get; init; }

        public int DoctorId { get; init; }

        public required string Diagnosis { get; init; }

        public required string Notes { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
