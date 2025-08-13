using DesafioBackend.ApiService.Authentication;
using DesafioBackend.ApiService.Configurations;
using DesafioBackend.ApiService.Scalar;
using DesafioBackend.Application;
using DesafioBackend.Application.WebModules;
using DesafioBackend.Core.Data;
using Microsoft.AspNetCore.OutputCaching;

using Microsoft.AspNetCore.Mvc;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppConfigurations();

builder.AddServiceDefaults();

builder.Services.AddDesafioBackendServices(builder.Configuration);

builder.Services.AddServices();

builder.Services.AddProblemDetails();

builder.Services.AddOutputCache(options =>
{
    options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(1);
    options.AddBasePolicy(builder =>
        builder.Cache());
});

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.AddAuthenticationConfiguration();

var app = builder.Build();

app.UseExceptionHandler();
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Desafio Backend";
        options.Servers = [];
    });
}

app.MapPost("/login", static (
        [FromServices] LoginConfiguration loginConfiguration,
        [FromServices] TokenGenerator tokenGenerator,
        [FromBody] LoginRequest request) =>
    {
        if (loginConfiguration.IsValid(request))
            return Results.Ok(tokenGenerator.GenerateToken(request.Login));
        return Results.BadRequest("Username or password is incorrect.");
    })
    .WithName("Login");

app.MapGroup("/cards")
    .RequireAuthorization()
    .AddCardsApi();

app.MapDefaultEndpoints();

app.Run();

public record struct LoginRequest(string Login, string Password);