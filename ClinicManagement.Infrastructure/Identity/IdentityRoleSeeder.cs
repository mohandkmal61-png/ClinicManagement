using ClinicManagement.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Identity
{
    public class IdentityRoleSeeder : IRoleSeeder
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _doctorUserEmail;

        public IdentityRoleSeeder(
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _doctorUserEmail = configuration["RoleSeeding:DoctorUserEmail"]
                ?? throw new InvalidOperationException(
                    "RoleSeeding:DoctorUserEmail must be configured.");
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var roleName in ClinicRoles.All)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var result = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create role '{roleName}': {string.Join("; ", result.Errors.Select(error => error.Description))}");
                }
            }

            var doctor = await _userManager.FindByEmailAsync(_doctorUserEmail);
            if (doctor is null)
            {
                throw new InvalidOperationException(
                    $"The configured Doctor role assignment user '{_doctorUserEmail}' does not exist. No user was created.");
            }

            if (await _userManager.IsInRoleAsync(doctor, ClinicRoles.Doctor))
            {
                return;
            }

            var assignment = await _userManager.AddToRoleAsync(doctor, ClinicRoles.Doctor);
            if (!assignment.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign the Doctor role: {string.Join("; ", assignment.Errors.Select(error => error.Description))}");
            }
        }
    }
}
