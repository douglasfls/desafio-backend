using DesafioBackend.Application.Services;
using DesafioBackend.Core;
using DesafioBackend.Core.Services;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace DesafioBackend.Application;

public static class InstallServices
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICardsService, CardService>();
        services.AddValidatorsFromAssemblyContaining<ICoreAssemblyMarker>(includeInternalTypes: true);
        return services;
    }
}