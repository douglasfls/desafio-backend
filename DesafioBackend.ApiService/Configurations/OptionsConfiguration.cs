

namespace DesafioBackend.ApiService.Configurations;

internal static class OptionsConfiguration
{
    public static WebApplicationBuilder AddAppConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<LoginConfiguration>(
            builder.Configuration.GetSection(LoginConfiguration.SectionName));
        var loginConfiguration = builder.Configuration.GetSection(LoginConfiguration.SectionName)
            .Get<LoginConfiguration>();

        builder.Services.AddSingleton(loginConfiguration);

        return builder;
    }
}

public class LoginConfiguration
{
    /// <summary>
    /// Name of the section in config file.
    /// </summary>
    public const string SectionName = nameof(LoginConfiguration);
    /// <summary>
    /// Valid login from config file
    /// </summary>
    public required string Login { get; init; }
    /// <summary>
    /// Valid password from config file
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Check if login request matches with config file.
    /// </summary>
    /// <param name="request">Request from consumers.</param>
    /// <returns>True if content matches.</returns>
    public bool IsValid(LoginRequest request)
        => request.Login == Login && request.Password == Password;
}