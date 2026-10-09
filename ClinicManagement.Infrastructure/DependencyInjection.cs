using ClinicManagement.Application.Abstractions.Persistence;
using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Services.Interfaces;
using ClinicManagement.Application.Services.Implementations;
using ClinicManagement.Infrastructure.Authentication;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Identity;
using ClinicManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;

namespace ClinicManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ClinicDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentityCore<ApplicationUser>(options =>
                options.User.RequireUniqueEmail = true)
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<ClinicDbContext>();
            services.AddScoped<IRoleSeeder, IdentityRoleSeeder>();
            services.AddHostedService<RoleSeedingHostedService>();

            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .Validate(
                    settings =>
                        !string.IsNullOrWhiteSpace(settings.Key) &&
                        Encoding.UTF8.GetByteCount(settings.Key) >= 32 &&
                        !string.IsNullOrWhiteSpace(settings.Issuer) &&
                        !string.IsNullOrWhiteSpace(settings.Audience) &&
                        settings.ExpirationInMinutes > 0,
                    "JwtSettings must include a key of at least 32 UTF-8 bytes, issuer, audience, and positive expiration.")
                .ValidateOnStart();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();

            return services;
        }
    }
}
