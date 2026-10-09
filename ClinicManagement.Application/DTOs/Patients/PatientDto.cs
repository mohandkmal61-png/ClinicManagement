namespace ClinicManagement.Application.DTOs.Patients
{
    public class PatientDto
    {
        public int Id { get; init; }

        public required string Name { get; init; }

        public required string Email { get; init; }

        public required string Phone { get; init; }

        public DateTime DateOfBirth { get; init; }
    }
}
