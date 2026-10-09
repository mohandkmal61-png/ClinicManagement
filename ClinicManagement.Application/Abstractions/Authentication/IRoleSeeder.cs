namespace ClinicManagement.Application.Abstractions.Authentication
{
    public interface IRoleSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}
