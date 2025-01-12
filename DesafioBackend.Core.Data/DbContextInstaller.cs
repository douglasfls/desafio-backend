using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DesafioBackend.Core.Data;

public static class DbContextInstaller
{
    public const string ConnectionName = "desafio-backend";
    public static IServiceCollection AddDesafioBackendServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionName);
        
        services.AddDbContext<IDesafioBackendDbContext, DesafioBackendDbContext>(options => { options.UseNpgsql(connectionString); });
        
        return services;
    }
}