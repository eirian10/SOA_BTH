using AuthService.Data;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly string _secretKey = "DayLaChuoiBiMatChoBaiThucHanhSOA2026!!";

        public AuthController(AuthDbContext context) => _context = context;

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            // Tự động tạo tài khoản mặc định admin/123456 nếu DB chưa có để bạn test nhanh
            if (!_context.Users.Any())
            {
                _context.Users.Add(new User { UserName = "admin", Password = Convert.ToBase64String(Encoding.UTF8.GetBytes("123456")) });
                _context.SaveChanges();
            }

            var user = _context.Users.FirstOrDefault(u => u.UserName == req.UserName && u.Password == req.Password);
            if (user == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName) }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            user.Token = token;
            _context.SaveChanges();

            return Ok(new { token });
        }

        [HttpGet("auth")]
        public IActionResult ValidateToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { valid = false });

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return Ok(new { valid = true });
            }
            catch
            {
                return Unauthorized(new { valid = false });
            }
        }
    }

    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}