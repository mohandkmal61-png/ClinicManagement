using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations
{
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.ToTable("MedicalRecords");
            builder.HasKey(medicalRecord => medicalRecord.Id);

            builder.Property(medicalRecord => medicalRecord.PatientId)
                .IsRequired();

            builder.Property(medicalRecord => medicalRecord.DoctorId)
                .IsRequired();

            builder.Property(medicalRecord => medicalRecord.Diagnosis)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(medicalRecord => medicalRecord.Notes)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(medicalRecord => medicalRecord.CreatedAt)
                .IsRequired();

            builder.HasOne<Patient>()
                .WithMany()
                .HasForeignKey(medicalRecord => medicalRecord.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(medicalRecord => medicalRecord.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
