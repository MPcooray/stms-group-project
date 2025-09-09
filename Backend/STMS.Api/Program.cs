using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using STMS.Api.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- JWT ---
var jwtSecret = builder.Configuration["JWT:Secret"] ?? "dev-placeholder-CHANGE-ME";

// --- Connection string ---
// Always read from configuration (User-Secrets / environment / appsettings).
var connString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Default is not configured. " +
        "Set it via dotnet user-secrets or App Service settings.");
}

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StmsDbContext>(opt =>
    opt.UseSqlServer(
        connString,
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    )
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// --- Middleware ---
app.UseSwagger();
app.UseSwaggerUI();

// If you require auth for your real endpoints, uncomment:
// app.UseAuthentication();
// app.UseAuthorization();

// --- Sanity endpoints ---
app.MapGet("/", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Verifies the API can reach Azure SQL through EF connection
app.MapGet("/db-test", async (StmsDbContext db) =>
{
    try
    {
        var ok = await db.Database.CanConnectAsync();
        return ok ? Results.Ok("DB OK") : Results.Problem("DB not reachable");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.ToString());
    }
});

// dev helper: make a bcrypt hash for seeding
app.MapGet("/_dev/hash/{pwd}", (string pwd) =>
    Results.Ok(new { pwd, hash = BCrypt.Net.BCrypt.HashPassword(pwd) }));

app.MapControllers();
app.Run();
