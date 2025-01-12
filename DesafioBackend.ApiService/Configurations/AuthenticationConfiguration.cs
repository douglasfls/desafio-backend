using DesafioBackend.ApiService.Authentication;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using static System.Text.Encoding;

namespace DesafioBackend.ApiService.Configurations;

public static class AuthenticationConfiguration
{
    /// <summary>
    /// Add Authentication configuration for application.
    /// </summary>
    /// <param name="builder">Current builder.</param>
    /// <returns>Fluent interface for builder.</returns>
    public static WebApplicationBuilder AddAuthenticationConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<JwtConfigration>(builder.Configuration.GetSection(JwtConfigration.SectionName));
        var jwtConfigration = builder.Configuration.GetSection(JwtConfigration.SectionName)
            .Get<JwtConfigration>();

        builder.Services.AddScoped<TokenGenerator>();
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(jwtConfigration.SecurityKeyBytes),
                    ValidIssuer = jwtConfigration.Issuer,
                    ValidAudience = jwtConfigration.Audience,
                    ValidateIssuerSigningKey = true,
                };
            });

        return builder;
    }
}

public class JwtConfigration
{
    /// <summary>
    /// Name of the section in config file.
    /// </summary>
    public const string SectionName = nameof(JwtConfigration);
    /// <summary>
    /// Audience for the JWT.
    /// </summary>
    public required string Audience { get; init; }
    /// <summary>
    /// Issuer for the JWT.
    /// </summary>
    public required string Issuer { get; init; }
    /// <summary>
    /// Security Key for the JWT.
    /// </summary>
    public required string SecurityKey { get; init; }
    /// <summary>
    /// Expiration in minutes for the JWT.
    /// </summary>
    public int ExpirationMinutes { get; init; } = 60;
    /// <summary>
    /// Get Security Key as byte array.
    /// </summary>
    public byte[] SecurityKeyBytes => UTF8.GetBytes(SecurityKey);
    /// <summary>
    /// Get Expiration Date.
    /// </summary>
    public DateTime ExpirationDate => DateTime.UtcNow.AddMinutes(ExpirationMinutes);
    /// <summary>
    /// Get Expiration in Milliseconds.
    /// </summary>
    public int ExpirationInMilliseconds => ExpirationMinutes * 60_000;
}