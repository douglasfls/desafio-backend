using System.Net.Mime;
using DesafioBackend.ApiService.Cards.Models;
using DesafioBackend.Application.Middleware;
using DesafioBackend.Core.Cards.Commands;
using DesafioBackend.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DesafioBackend.Application.WebModules;

/// <summary>
/// Provides extension methods for configuring card-related API endpoints.
/// </summary>
public static class CardsModule
{
    private const string GetAllCardsEndpointName = "GetAllCards";
    private const string GetCardByIdEndpointName = "GetCardById";
    private const string UpdateCardEndpointName = "UpdateCard";
    private const string CreateCardEndpointName = "CreateCard";
    private const string DeleteCardEndpointName = "DeleteCard";

    private const string CardsCollectionTag = "cards-all";
    private const string CardTag = "card-";

    /// <summary>
    /// Adds card-related API endpoints to the specified route group.
    /// </summary>
    /// <param name="group">The route group builder to add the endpoints to.</param>
    /// <returns>The route group builder for method chaining.</returns>
    public static RouteGroupBuilder AddCardsApi(this RouteGroupBuilder group)
    {
        ArgumentNullException.ThrowIfNull(group);
        // GET all cards endpoint
        group.MapGet("/", GetAllCards)
            .WithName(GetAllCardsEndpointName)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get all cards";
                operation.Description = "Retrieves all cards from the system.";
                return operation;
            })
            .Produces<Card[]>(StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags("Cards")
            .CacheOutput(policy => policy.Tag(CardsCollectionTag));

        // GET card by ID endpoint
        group.MapGet("/{id:int}", GetCardById)
            .WithName(GetCardByIdEndpointName)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get a card by ID";
                operation.Description = "Retrieves a specific card by its unique identifier.";
                return operation;
            })
            .Produces<Card>(StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .CacheOutput(x => x.Expire(TimeSpan.FromMinutes(1)))
            .WithTags("Cards");

        // PUT update card endpoint
        group.MapPut("/{id:int}", UpdateCard)
            .WithName(UpdateCardEndpointName)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update a card";
                operation.Description = "Updates an existing card with new information.";
                return operation;
            })
            .Accepts<UpdateCardCommand>(MediaTypeNames.Application.Json)
            .Produces<Card>(StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ChangeTrackerFilter>()
            .AddValidationFilter<UpdateCardCommand>()
            .WithTags("Cards");

        // POST create card endpoint
        group.MapPost("/", CreateCard)
            .WithName(CreateCardEndpointName)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create a new card";
                operation.Description = "Creates a new card in the system.";
                return operation;
            })
            .Accepts<CreateCardCommand>(MediaTypeNames.Application.Json)
            .Produces<Card>(StatusCodes.Status201Created, contentType: MediaTypeNames.Application.Json)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .AddValidationFilter<CreateCardCommand>()
            .WithTags("Cards");

        // DELETE card endpoint
        group.MapDelete("/{id:int}", DeleteCard)
            .WithName(DeleteCardEndpointName)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Delete a card";
                operation.Description = "Removes a card from the system.";
                return operation;
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .AddEndpointFilter<ChangeTrackerFilter>()
            .WithTags("Cards");

        return group;
    }

    /// <summary>
    /// Retrieves all cards from the system.
    /// </summary>
    private static async Task<IResult> GetAllCards(
        [FromServices] ICardsService cardsService,
        [FromServices] IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        var cards = await cardsService.GetAllCardsAsync(cancellationToken);
        return cards.Any()
            ? Results.Ok(cards)
            : Results.NoContent();
    }

    /// <summary>
    /// Retrieves a specific card by its ID.
    /// </summary>
    private static async Task<IResult> GetCardById(
        [FromServices] ICardsService cardsService,
        [FromRoute] int id,
        [FromServices] IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        var card = await cardsService.GetCardByIdAsync(id, cancellationToken);
        return card is not null
            ? Results.Ok(card)
            : Results.NotFound($"Card with ID {id} not found");
    }

    /// <summary>
    /// Updates an existing card.
    /// </summary>
    private static async Task<IResult> UpdateCard(
        [FromServices] ICardsService cardsService,
        [FromRoute] int id,
        [FromBody] UpdateCardCommand command,
        [FromServices] IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        var card = await cardsService.UpdateCardAsync(id, command, cancellationToken);
        if (card is not null)
        {
            await InvalidateCache(cacheStore, id, cancellationToken);
            return Results.Ok(card);
        }
        return Results.NotFound($"Card with ID {id} not found");
    }

    /// <summary>
    /// Creates a new card.
    /// </summary>
    private static async Task<IResult> CreateCard(
        [FromServices] ICardsService cardsService,
        [FromBody] CreateCardCommand command,
        [FromServices] IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        var card = await cardsService.CreateCardAsync(command, cancellationToken);
        await InvalidateCacheCollection(cacheStore, cancellationToken);
        return Results.Created($"/cards/{card.Id}", card);
    }

    /// <summary>
    /// Deletes a card by its ID.
    /// </summary>
    private static async Task<IResult> DeleteCard(
        [FromServices] ICardsService cardsService,
        [FromRoute] int id,
        [FromServices] IOutputCacheStore cacheStore,
        CancellationToken cancellationToken)
    {
        var deleted = await cardsService.DeleteCardAsync(id, cancellationToken);
        if (deleted)
        {
            await InvalidateCache(cacheStore, id, cancellationToken);
            return Results.NoContent();
        }
        return Results.NotFound($"Card with ID {id} not found");
    }

    private static async ValueTask InvalidateCache(IOutputCacheStore cacheStore, int cardId, CancellationToken cancellationToken)
    {
        await InvalidateCacheCollection(cacheStore, cancellationToken);
        await cacheStore.EvictByTagAsync($"{CardTag}{cardId}", cancellationToken);
    }

    private static ValueTask InvalidateCacheCollection(IOutputCacheStore cacheStore, CancellationToken cancellationToken)
        => cacheStore.EvictByTagAsync(CardsCollectionTag, cancellationToken);
}
