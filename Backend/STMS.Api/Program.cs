using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
// ------------------- builder -------------------
var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "STMS.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT auth using Bearer. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS (allow your React app)
var origins = (builder.Configuration["CORS_ORIGIN"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    
if (origins.Length == 0)
    origins = new[] { "http://localhost:5173" };

builder.Services.AddCors(o => o.AddPolicy("ui", p =>
    p.WithOrigins(origins)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));


// JWT Auth
var jwtSecret = builder.Configuration["JWT:Secret"] ?? "dev-placeholder-CHANGE-ME";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// EF Core (MySQL) — use DI + appsettings (Azure MySQL)
var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<StmsDbContext>(options =>
    options.UseMySql(cs, serverVersion, my =>
    {
        my.CommandTimeout(60);
        // Transient retry policy (Azure-friendly)
        my.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));
// ------------------- app -------------------
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("ui");
app.UseAuthentication();
app.UseAuthorization();

// health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// 🔎 TEMP: list admins the API can see (confirm DB/connection)
app.MapGet("/_debug/admins", async (StmsDbContext db) =>
{
    var rows = await db.Admins
        .Select(a => new { a.Id, a.Email })
        .ToListAsync();
    return Results.Ok(rows);
});

// 🔐 TEMP: generate a bcrypt hash for any password
app.MapGet("/_dev/hash/{pwd}", (string pwd) =>
{
    var hash = BCrypt.Net.BCrypt.HashPassword(pwd);
    return Results.Ok(new { pwd, hash });
});

app.MapControllers();

app.Run();


// ------------------- EF Core DbContext & Models -------------------
public class StmsDbContext : DbContext
{
    public StmsDbContext(DbContextOptions<StmsDbContext> options) : base(options) { }

    // NOTE: We intentionally removed OnConfiguring so it cannot fall back to localhost.
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<University> Universities => Set<University>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Timing> Timings => Set<Timing>();
}

public class Admin
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime? Date { get; set; }
    public string? Venue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class University
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int UniversityId { get; set; }
    public University? University { get; set; }
    public string? Event { get; set; }
    public int? Age { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Timing
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public string Event { get; set; } = "";
    public int TimeMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
