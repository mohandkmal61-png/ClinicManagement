namespace ClinicManagement.Application.DTOs.Authentication
{
    public class AuthResponseDto
    {
        public required string AccessToken { get; init; }

        public required string Email { get; init; }

        public DateTime ExpiresAtUtc { get; init; }
    }
}
