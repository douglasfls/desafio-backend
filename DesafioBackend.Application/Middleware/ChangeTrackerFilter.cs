using DesafioBackend.ApiService.Cards.Models;
using DesafioBackend.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DesafioBackend.Application.Middleware;

public class ChangeTrackerFilter : IEndpointFilter
{
    private const string ActionUpdate = "Alterar";
    private const string ActionDelete = "Remover";

    private readonly ILogger<ChangeTrackerFilter> _logger;
    private readonly ICardsService _cardsService;

    public ChangeTrackerFilter(ILogger<ChangeTrackerFilter> logger, ICardsService cardsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cardsService = cardsService ?? throw new ArgumentNullException(nameof(cardsService));
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            var request = context.HttpContext.Request;
            if (!TryGetCardId(request, out int id))
            {
                return await next(context);
            }

            var card = await _cardsService.GetCardByIdAsync(id);
            if (card is null)
            {
                _logger.LogWarning("Card with ID {CardId} not found", id);
                return Results.NotFound();
            }

            if (request.Method == HttpMethods.Put)
            {
                LogAction(card, ActionUpdate);
            }
            else if (request.Method == HttpMethods.Delete)
            {
                LogAction(card, ActionDelete);
            }

            return await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request for path {Path}", context.HttpContext.Request.Path);
            throw;
        }
    }

    private static bool TryGetCardId(HttpRequest request, out int id)
    {
        id = 0;
        return request.RouteValues.TryGetValue("id", out var routeId) &&
               int.TryParse(routeId?.ToString(), out id);
    }

    private void LogAction(Card card, string action)
    {
        _logger.LogInformation(
            "Card action performed - {Action} - ID: {CardId} - Title: {CardTitle} - Timestamp: {Timestamp}",
            action,
            card.Id,
            card.Title,
            DateTime.UtcNow);
    }
}