using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DiplomBackend.Models;

namespace DiplomBackend.Services
{
    public class TokenProvider
    {
        private readonly IConfiguration _config;

        public TokenProvider(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("FullName", user.FullName)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow, // Важно: UTC
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine(token.Claims);
            
            

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}