using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicManagement.Application.DTOs.Authentication;
using ClinicManagement.Application.Exceptions;
using ClinicManagement.Application.Services.Interfaces;
using ClinicManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClinicManagement.Infrastructure.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(
            RegisterDto dto,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            cancellationToken.ThrowIfCancellationRequested();

            if (!result.Succeeded)
            {
                throw new AuthValidationException(result.Errors.Select(error => error.Description));
            }

            var roles = await _userManager.GetRolesAsync(user);
            cancellationToken.ThrowIfCancellationRequested();
            return CreateAuthResponse(user, roles);
        }

        public async Task<AuthResponseDto?> LoginAsync(
            LoginDto dto,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
            {
                return null;
            }

            var passwordIsValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            cancellationToken.ThrowIfCancellationRequested();

            if (!passwordIsValid)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            cancellationToken.ThrowIfCancellationRequested();
            return CreateAuthResponse(user, roles);
        }

        private AuthResponseDto CreateAuthResponse(ApplicationUser user, IEnumerable<string> roles)
        {
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(_jwtSettings.ExpirationInMinutes);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: signingCredentials);

            return new AuthResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email ?? string.Empty,
                ExpiresAtUtc = expiresAt
            };
        }
    }
}
