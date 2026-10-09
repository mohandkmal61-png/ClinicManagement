namespace ClinicManagement.Infrastructure.Identity
{
    public static class ClinicRoles
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Receptionist = "Receptionist";

        public static IReadOnlyList<string> All { get; } =
        [
            Admin,
            Doctor,
            Receptionist
        ];
    }
}
