using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Infrastructure.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(appointment => appointment.Id);

            builder.Property(appointment => appointment.DoctorId)
                .IsRequired();

            builder.Property(appointment => appointment.PatientId)
                .IsRequired();

            builder.Property(appointment => appointment.AppointmentDate)
                .IsRequired();

            builder.Property(appointment => appointment.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(appointment => new { appointment.DoctorId, appointment.AppointmentDate })
                .HasDatabaseName("UX_Appointments_Doctor_Scheduled_Date")
                .IsUnique()
                .HasFilter("[Status] = 0");

            builder.HasIndex(appointment => new { appointment.PatientId, appointment.AppointmentDate })
                .HasDatabaseName("UX_Appointments_Patient_Scheduled_Date")
                .IsUnique()
                .HasFilter("[Status] = 0");

            builder.HasIndex(appointment => appointment.DoctorId);
            builder.HasIndex(appointment => appointment.PatientId);

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(appointment => appointment.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Patient>()
                .WithMany()
                .HasForeignKey(appointment => appointment.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
