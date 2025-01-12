using DesafioBackend.ApiService.Cards.Models;
using DesafioBackend.Core.Cards.Commands;
using DesafioBackend.Core.Data;
using DesafioBackend.Core.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DesafioBackend.Application.Services;

public sealed class CardService : ICardsService
{
    private readonly ILogger<CardService> _logger;
    private readonly IDesafioBackendDbContext _dbContext;

    public CardService(ILogger<CardService> logger, IDesafioBackendDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public Task<List<Card>> GetAllCardsAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Cards.ToListAsync(cancellationToken);

    public ValueTask<Card?> GetCardByIdAsync(int cardId, CancellationToken cancellationToken = default)
        => _dbContext.Cards.FindAsync(keyValues: [cardId], cancellationToken: cancellationToken);

    public async Task<Card> CreateCardAsync(CreateCardCommand card, CancellationToken cancellationToken = default)
    {
        var newCard = card.CreateCard();
        _dbContext.Cards.Add(newCard);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return newCard;
    }

    public async Task<Card?> UpdateCardAsync(int cardId, UpdateCardCommand card,
        CancellationToken cancellationToken = default)
    {
        var item = await GetCardByIdAsync(cardId, cancellationToken);
        if (item is null) return null;
        card.UpdateCard(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<bool> DeleteCardAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await GetCardByIdAsync(id, cancellationToken);
        if (item is null) return false;
        _dbContext.Cards.Remove(item);
        return (await _dbContext.SaveChangesAsync(cancellationToken)) > 0;
    }
}