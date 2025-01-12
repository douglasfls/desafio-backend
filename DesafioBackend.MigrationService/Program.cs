using DesafioBackend.Core.Data;
using DesafioBackend.MigrationService;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ApiDbInitializer>();

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString(DbContextInstaller.ConnectionName);
builder.Services.AddDbContextPool<DesafioBackendDbContext>(
    options => options.UseNpgsql(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(ApiDbInitializer).Assembly.FullName);
        sqlOptions.ExecutionStrategy(p => new NpgsqlRetryingExecutionStrategy(p));
    })
);

builder.EnrichNpgsqlDbContext<DesafioBackendDbContext>(settings =>
{
    settings.DisableRetry = true;
});

var host = builder.Build();
host.Run();