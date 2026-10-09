namespace ClinicManagement.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Specialization { get; set; }  

        public Doctor(string name, string specialization)
        {
            Name = name;
            Specialization = specialization;
        }
    }
}
