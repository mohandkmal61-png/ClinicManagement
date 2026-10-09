namespace ClinicManagement.Domain.Entities
{
    public class Patient : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public DateTime DateOfBirth { get; set; }

        public Patient(string name, string email, string phone, DateTime dateOfBirth)
        {
            Name = name;
            Email = email;
            Phone = phone;
            DateOfBirth = dateOfBirth;
        }
    }
}
