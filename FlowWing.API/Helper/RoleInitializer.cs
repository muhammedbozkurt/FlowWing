using FlowWing.DataAccess;
using FlowWing.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowWing.API.Helper
{
    public class RoleInitializer : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RoleInitializer(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FlowWingDbContext>();

                // Check if the default role "User" exists
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin", cancellationToken);
                if (userRole == null) // If not, create it
                {
                    userRole = new Role
                    {
                        Name = "User",
                        CreationDate = DateTime.UtcNow
                    };

                    context.Roles.Add(userRole);
                    await context.SaveChangesAsync(cancellationToken);
                }
                if (adminRole == null) // If not, create it
                {
                    adminRole = new Role
                    {
                        Name = "Admin",
                        CreationDate = DateTime.UtcNow
                    };

                    context.Roles.Add(adminRole);
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
