using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(patient => patient.Id);

            builder.Property(patient => patient.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(patient => patient.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(patient => patient.Phone)
                .IsRequired()
                .HasMaxLength(20);
        }
    }
}
