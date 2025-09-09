using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using STMS.Api.Data;

namespace STMS.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly StmsDbContext _db;
        private readonly SymmetricSecurityKey _signingKey;

        public AuthController(StmsDbContext db, IConfiguration cfg)
        {
            _db = db;
            var secret = cfg["JWT:Secret"] ?? "dev-placeholder-CHANGE-ME";
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }

        public record LoginDto(string Email, string Password);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required.");

            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email);
            if (admin is null) return Unauthorized(new { error = "Invalid credentials" });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
                return Unauthorized(new { error = "Invalid credentials" });

            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, admin.Email)
            };

            var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
