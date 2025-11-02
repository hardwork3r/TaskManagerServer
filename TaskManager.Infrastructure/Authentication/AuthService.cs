namespace TaskManager.Infrastructure.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Application.Common.Interfaces;
using JwtClaims = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private const int AccessTokenExpireMinutes = 1440;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public string CreateAccessToken(string userId, string role)
    {
        var secretKey = _configuration["JWT:SecretKey"] ?? "your-secret-key-change-in-production";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(AccessTokenExpireMinutes);

        var claims = new[]
        {
            new Claim(JwtClaims.Sub, userId),
            new Claim("role", role),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtClaims.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtClaims.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        }
        catch
        {
            return null;
        }
    }
}