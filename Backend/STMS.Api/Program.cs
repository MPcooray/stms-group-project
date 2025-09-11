using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using STMS.Api.Data;
using STMS.Api.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---- config (MySQL 9 wants TLS) ----
// If the server uses caching_sha2_password and TLS is not available locally,
// allow the connector to retrieve the server RSA public key (dev only).
var cs = builder.Configuration.GetConnectionString("Default")
    ?? "Server=127.0.0.1;Port=3306;Database=stms_dev;User Id=stms;Password=stms;SslMode=Required;AllowPublicKeyRetrieval=True;TreatTinyAsBoolean=false";

var jwtSecret = builder.Configuration["JWT:Secret"] ?? "dev-placeholder-CHANGE-ME";

// services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StmsDbContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs)));

// CORS for local frontend dev (Vite/Next)
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:3001",
                "http://127.0.0.1:3001"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
        );
});

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

app.UseCors("frontend");

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
// Development-only: seed a local admin if none exists so login works during dev
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StmsDbContext>();
    try
    {
        var pwd = "Admin#123"; // dev default - change if you like
        // Ensure the frontend default admin exists
        if (!db.Admins.Any(a => a.Email == "admin@stms.com"))
        {
            var admin = new Admin { Email = "admin@stms.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd) };
            db.Admins.Add(admin);
            db.SaveChanges();
            Console.WriteLine($"[DEV SEED] Created admin {admin.Email} with password {pwd}");
        }
        // Also keep the previous seed for convenience
        if (!db.Admins.Any(a => a.Email == "admin@local"))
        {
            var admin2 = new Admin { Email = "admin@local", PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd) };
            db.Admins.Add(admin2);
            db.SaveChanges();
            Console.WriteLine($"[DEV SEED] Created admin {admin2.Email} with password {pwd}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("[DEV SEED] Failed to seed admin: " + ex);
    }
}

app.Run();
