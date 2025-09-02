using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly StmsDbContext _db;
    private readonly IConfiguration _cfg;
    public AuthController(StmsDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    public record LoginDto(string Email, string Password);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email and password are required.");

        var admin = _db.Admins.FirstOrDefault(a => a.Email == dto.Email);
        if (admin is null) return Unauthorized("Invalid credentials");

        // verify bcrypt hash
        var ok = BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash);
        if (!ok) return Unauthorized("Invalid credentials");

        // issue JWT
        var secret = _cfg["JWT:Secret"] ?? "dev-placeholder-CHANGE-ME";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Email)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
