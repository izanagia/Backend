using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using DiplomBackend.Models;
using DiplomBackend.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DiplomBackend.Services;

namespace DiplomBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly TokenProvider _tokenProvider;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, TokenProvider tokenProvider)
        {
            _context = context;
            _configuration = configuration;
            _tokenProvider = tokenProvider;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null || user.PasswordHash != ComputeHash(request.Password))
            {
                return Unauthorized("Неверный логин или пароль.");
            }

            var Token = _tokenProvider.GenerateToken(user);

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Role,
                Token
            });
        }

       

        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    
}