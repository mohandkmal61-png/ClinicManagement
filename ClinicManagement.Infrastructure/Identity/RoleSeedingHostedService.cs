using ClinicManagement.Application.Abstractions.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClinicManagement.Infrastructure.Identity
{
    public class RoleSeedingHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RoleSeedingHostedService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var roleSeeder = scope.ServiceProvider.GetRequiredService<IRoleSeeder>();
            await roleSeeder.SeedAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
