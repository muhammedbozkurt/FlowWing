using FlowWing.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

public class FlowWingDbContextFactory : IDesignTimeDbContextFactory<FlowWingDbContext>
{
    public FlowWingDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "FlowWing.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<FlowWingDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DatabaseConnection"));

        return new FlowWingDbContext(optionsBuilder.Options, configuration);
    }
}
