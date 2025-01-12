using DesafioBackend.ApiService.Cards.Models;
using DesafioBackend.Core.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DesafioBackend.Application.Middleware;

public class ChangeTrackerFilter : IEndpointFilter
{
    private readonly ILogger<ChangeTrackerFilter> _logger;
    private readonly ICardsService _cardsService;

    public ChangeTrackerFilter(ILogger<ChangeTrackerFilter> logger, ICardsService cardsService)
    {
        _logger = logger;
        _cardsService = cardsService;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        if (request.RouteValues.TryGetValue("id", out var routeId) &&
            int.TryParse(routeId?.ToString(), out int id))
        {
            var method = request.Method;
            var card = await _cardsService.GetCardByIdAsync(id);
            if (card is null) return Results.NotFound();

            if (method == HttpMethods.Put) LogAction(card, "Alterar");
            if (method == HttpMethods.Delete) LogAction(card, "Remover");
        }

        return await next(context);
    }

    private void LogAction(Card card, string action)
        => _logger.LogInformation("{execution} - Card {id} - {title} - {action}", DateTime.UtcNow, card.Id, card.Title,
            action);
}