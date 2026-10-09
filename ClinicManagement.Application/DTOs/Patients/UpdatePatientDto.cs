namespace ClinicManagement.Application.DTOs.Patients
{
    public class UpdatePatientDto
    {
        public required string Name { get; init; }

        public required string Email { get; init; }

        public required string Phone { get; init; }

        public DateTime DateOfBirth { get; init; }
    }
}
