using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class StmsDbContextFactory : IDesignTimeDbContextFactory<StmsDbContext>
{
    public StmsDbContext CreateDbContext(string[] args)
    {
        // 1) Allow an override for EF CLI via env var (handy for CI or quick tests)
        var cs = Environment.GetEnvironmentVariable("STMS_EF_CS");

        if (string.IsNullOrWhiteSpace(cs))
        {
            // 2) Otherwise load appsettings.Development.json (or appsettings.json)
            var cfg = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            cs = cfg.GetConnectionString("Default")
                 ?? throw new InvalidOperationException("ConnectionStrings:Default not found for design-time.");
        }

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        var options = new DbContextOptionsBuilder<StmsDbContext>()
            .UseMySql(cs, serverVersion)
            .Options;

        return new StmsDbContext(options);
    }
}
