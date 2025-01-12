using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using DesafioBackend.ApiService.Configurations;

using Microsoft.IdentityModel.Tokens;

namespace DesafioBackend.ApiService.Authentication;

/// <summary>
/// Output token model
/// </summary>
/// <param name="Username">Username of the requester.</param>
/// <param name="AccessToken">A Json Web Token that can be used to login.</param>
/// <param name="ExpiresIn">Expiration of current token</param>
public record struct JwtToken(string Username, string AccessToken, int ExpiresIn);

public sealed class TokenGenerator
{
    private readonly JwtConfigration _jwtConfigration;

    public TokenGenerator(JwtConfigration jwtConfigration)
    {
        _jwtConfigration = jwtConfigration;
    }

    /// <summary>
    /// Generate token for user.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public JwtToken GenerateToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = _jwtConfigration.SecurityKeyBytes;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Name, username)
        };
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = _jwtConfigration.ExpirationDate,
            Issuer = _jwtConfigration.Issuer,
            Audience = _jwtConfigration.Audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new JwtToken(username, tokenHandler.WriteToken(token), _jwtConfigration.ExpirationInMilliseconds);
    }
}