using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace STMS.Api.Data
{
    // Provides a design-time factory for EF tools so they don't try to auto-detect server version
    // which would attempt to connect to the database.
    public class DesignTimeStmsDbContextFactory : IDesignTimeDbContextFactory<StmsDbContext>
    {
        public StmsDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var cs = config.GetConnectionString("Default") ?? "Server=127.0.0.1;Port=3306;Database=stms_dev;User Id=stms;Password=stms;SslMode=None;AllowPublicKeyRetrieval=True;TreatTinyAsBoolean=false";

            var options = new DbContextOptionsBuilder<StmsDbContext>();
            // Use a fixed ServerVersion to avoid AutoDetect connecting at design time
            options.UseMySql(cs, new MySqlServerVersion(new System.Version(8, 0, 30)));

            return new StmsDbContext(options.Options);
        }
    }
}
