namespace ClinicManagement.Application.DTOs.Doctors
{
    public class DoctorDto
    {
        public int Id { get; init; }

        public required string Name { get; init; }

        public required string Specialization { get; init; }
    }
}
