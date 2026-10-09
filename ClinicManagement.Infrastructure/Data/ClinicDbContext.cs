using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data
{
    public class ClinicDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        public DbSet<Doctor> Doctors { get; set; } = null!;

        public DbSet<Patient> Patients { get; set; } = null!;

        public DbSet<Appointment> Appointments { get; set; } = null!;

        public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;

        public DbSet<Prescription> Prescriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicDbContext).Assembly);
        }
    }
}
