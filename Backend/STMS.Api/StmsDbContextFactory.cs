// Backend/STMS.Api/StmsDbContextFactory.cs
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class StmsDbContextFactory : IDesignTimeDbContextFactory<global::StmsDbContext>
{
    public global::StmsDbContext CreateDbContext(string[] args)
    {
        // Prefer an env var, fall back to a safe default for local dev
        var conn = Environment.GetEnvironmentVariable("EF_CONNECTION")
                  ?? "server=server-stms.mysql.database.azure.com;port=3306;database=dbstms;user=stms_app;password=dulran@321;TreatTinyAsBoolean=false;SslMode=Required;";

        // Avoid AutoDetect (which opens a socket immediately).
        // Azure MySQL Flexible Server is MySQL 8.0.x
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 42));

        var options = new DbContextOptionsBuilder<global::StmsDbContext>()
            .UseMySql(conn, serverVersion, o => o.EnableRetryOnFailure())
            .Options;

        return new global::StmsDbContext(options, new ConfigurationBuilder().AddEnvironmentVariables().Build());
    }
}
