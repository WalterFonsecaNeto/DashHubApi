using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DashHubApi.Application.Interfaces;
using DashHubApi.Core.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DashHubApi.Application.Services;

public class JwtTokenService : IServicoTokenJwt
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key nao configurado");

        var issuer = _configuration["Jwt:Issuer"] ?? "DashHubApi";
        var audience = _configuration["Jwt:Audience"] ?? "DashHubApiUsers";
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var parsed)
            ? parsed
            : 120;

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (token, expiresAt);
    }
}
