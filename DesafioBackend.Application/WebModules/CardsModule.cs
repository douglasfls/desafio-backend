using DesafioBackend.ApiService.Cards.Models;
using DesafioBackend.Application.Middleware;
using DesafioBackend.Core.Cards.Commands;
using DesafioBackend.Core.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DesafioBackend.Application.WebModules;

public static class CardsModule
{
    public static RouteGroupBuilder AddCardsApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllCards)
            .WithName("GetAllCards")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces<Card[]>();

        group.MapGet("/{id:int}", GetCardById)
            .WithName("GetCardById")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces<Card>();

        group.MapPut("/{id:int}", UpdateCard)
            .WithName("UpdateCard")
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces<Card>()
            .AddEndpointFilter<ChangeTrackerFilter>()
            .AddValidationFilter<UpdateCardCommand>();

        group.MapPost("/", CreateCard)
            .WithName("CreateCard")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces<Card>()
            .AddValidationFilter<CreateCardCommand>();

        group.MapDelete("/{id:int}", DeleteCard)
            .WithName("DeleteCard")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status200OK)
            .AddEndpointFilter<ChangeTrackerFilter>();

        return group;
    }

    static async Task<IResult> GetAllCards(
        [FromServices] ICardsService cardsService,
        CancellationToken cancellation)
    {
        var item = await cardsService.GetAllCardsAsync(cancellation);
        if (item.Any())
        {
            return Results.Ok(item);
        }

        return Results.Empty;
    }

    static async Task<IResult> GetCardById(
        [FromServices] ICardsService cardsService,
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var item = await cardsService.GetCardByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(item);
    }

    static async Task<IResult> UpdateCard(
        [FromServices] ICardsService cardsService,
        [FromRoute] int id,
        [FromBody] UpdateCardCommand command,
        CancellationToken cancellationToken)
    {
        var card = await cardsService.UpdateCardAsync(id, command, cancellationToken);
        if (card is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(card);
    }

    static async Task<IResult> CreateCard(
        [FromServices] ICardsService cardsService,
        [FromBody] CreateCardCommand command,
        CancellationToken cancellationToken)
    {
        var card = await cardsService.CreateCardAsync(command, cancellationToken);
        return Results.Ok(card);
    }

    static async Task<IResult> DeleteCard(
        [FromServices] ICardsService cardsService,
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var hasChange = await cardsService.DeleteCardAsync(id, cancellationToken);
        return hasChange ? Results.Ok() : Results.NotFound();
    }
}