using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using STMS.Api.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---- config ----
var cs = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrEmpty(cs))
    throw new InvalidOperationException("No connection string 'Default' found.");

var jwtSecret = builder.Configuration["JWT:Secret"] ?? "dev-placeholder";

// ---- services ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext -> SQL Server (single registration)
builder.Services.AddDbContext<StmsDbContext>(opt =>
    opt.UseSqlServer(cs, sql =>
    {
        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        sql.CommandTimeout(60);
    })
);

// Health check for DB
builder.Services.AddHealthChecks().AddSqlServer(cs);

// CORS for local frontend dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(
            "http://localhost:5173", "http://127.0.0.1:5173",
            "http://localhost:3000", "http://127.0.0.1:3000",
            "http://localhost:3001", "http://127.0.0.1:3001"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

// JWT
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

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    // Use full type name (and replace '+' for nested types) to avoid schema ID collisions
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

var app = builder.Build();

app.MapHealthChecks("/health/db");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// app.UseHttpsRedirection(); // enable in prod behind HTTPS

app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

// sanity endpoints
app.MapGet("/", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/db-test", async (StmsDbContext db) =>
{
    try { return await db.Database.CanConnectAsync() ? Results.Ok("DB OK") : Results.Problem("DB not reachable"); }
    catch (Exception ex) { return Results.Problem(ex.ToString()); }
});

app.MapControllers();
app.Run();
