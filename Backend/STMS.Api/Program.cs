using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using STMS.Api.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---- config (MySQL 9 wants TLS) ----
var cs = builder.Configuration.GetConnectionString("Default")
    ?? "Server=tcp:fffff.database.windows.net,1433;Database=dbstms;User ID=stms_app;Password=nadil@321;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

var jwtSecret = builder.Configuration["JWT:Secret"] ?? "dev-placeholder-CHANGE-ME";

// services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StmsDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
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

// middleware
app.UseSwagger(); app.UseSwaggerUI();
// app.UseHttpsRedirection(); // off for local http

// app.UseAuthentication();
// app.UseAuthorization();

// sanity endpoints
app.MapGet("/", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/db-test", async (StmsDbContext db) =>
{
    try { return await db.Database.CanConnectAsync() ? Results.Ok("DB OK") : Results.Problem("DB not reachable"); }
    catch (Exception ex) { return Results.Problem(ex.ToString()); }
});
// dev helper: make a bcrypt hash for seeding
app.MapGet("/_dev/hash/{pwd}", (string pwd) => Results.Ok(new { pwd, hash = BCrypt.Net.BCrypt.HashPassword(pwd) }));

app.MapControllers();
app.Run();
