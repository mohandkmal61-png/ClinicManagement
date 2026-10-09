using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors");
            builder.HasKey(doctor => doctor.Id);

            builder.Property(doctor => doctor.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(doctor => doctor.Specialization)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
