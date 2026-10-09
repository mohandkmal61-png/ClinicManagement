namespace ClinicManagement.Application.DTOs.MedicalRecords
{
    public class CreateMedicalRecordDto
    {
        public int PatientId { get; init; }

        public int DoctorId { get; init; }

        public required string Diagnosis { get; init; }

        public required string Notes { get; init; }
    }
}
