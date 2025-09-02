using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql;
using System.Text;

// ------------------- builder -------------------
var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (allow your React app)
var corsOrigin = builder.Configuration["CORS_ORIGIN"] ?? "http://localhost:5173";
builder.Services.AddCors(o => o.AddPolicy("ui", p =>
    p.WithOrigins(corsOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

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

// EF Core (MySQL) — using OnConfiguring in DbContext below
builder.Services.AddDbContext<StmsDbContext>();

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
app.MapGet("/_debug/admins", (StmsDbContext db) =>
    Results.Ok(db.Admins.Select(a => new { a.Id, a.Email }).ToList()));

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
    private readonly IConfiguration _cfg;
    public StmsDbContext(DbContextOptions<StmsDbContext> options, IConfiguration cfg) : base(options)
    {
        _cfg = cfg;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // if nothing in appsettings, this fallback is used
            var cs = _cfg.GetConnectionString("Default")
                     ?? "server=localhost;port=3306;database=stms;user=root;password=;";
            optionsBuilder.UseMySql(cs, ServerVersion.AutoDetect(cs));
        }
    }

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
