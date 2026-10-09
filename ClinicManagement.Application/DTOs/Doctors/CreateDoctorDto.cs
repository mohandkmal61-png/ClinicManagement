namespace ClinicManagement.Application.DTOs.Doctors
{
    public class CreateDoctorDto
    {
        public required string Name { get; init; }

        public required string Specialization { get; init; }
    }
}
