using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using STMS.Api.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---- config (MySQL 9 wants TLS) ----
var cs = builder.Configuration.GetConnectionString("Default")
    ?? "Server=127.0.0.1;Port=3306;Database=stms_dev;User Id=stms;Password=stms;SslMode=Required;TreatTinyAsBoolean=false";

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
                "http://127.0.0.1:3000"
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
app.Run();
