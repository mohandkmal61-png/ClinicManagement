using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("Prescriptions");
            builder.HasKey(prescription => prescription.Id);

            builder.Property(prescription => prescription.PatientId)
                .IsRequired();

            builder.Property(prescription => prescription.DoctorId)
                .IsRequired();

            builder.Property(prescription => prescription.MedicationName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(prescription => prescription.Dosage)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(prescription => prescription.Instructions)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(prescription => prescription.CreatedAt)
                .IsRequired();

            builder.HasOne<Patient>()
                .WithMany()
                .HasForeignKey(prescription => prescription.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(prescription => prescription.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
