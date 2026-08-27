using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.Application.Auth;
using TaskManagement.Application.DTOs.Responses;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TaskManagement.Infrastructure.Identity;

/// <summary>
/// Creates signed JWT access tokens for authenticated users.
/// </summary>
public class JwtTokenGenerator(IOptions<JwtOptions> jwtOptions) : IJwtTokenGenerator
{
    public string GenerateToken(AuthenticatedUser user)
    {
        var options = jwtOptions.Value;
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(options.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role ?? "user"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            ]),
            Expires = DateTime.UtcNow.AddHours(options.ExpirationHours),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
