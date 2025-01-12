using DesafioBackend.ApiService.Cards.Models;
using DesafioBackend.Core.Cards.Commands;

namespace DesafioBackend.Core.Services;

public interface ICardsService
{
    Task<List<Card>> GetAllCardsAsync(CancellationToken cancellationToken = default);
    ValueTask<Card?> GetCardByIdAsync(int cardId, CancellationToken cancellationToken = default);
    Task<Card> CreateCardAsync(CreateCardCommand card, CancellationToken cancellationToken = default);
    Task<Card?> UpdateCardAsync(int cardId, UpdateCardCommand card, CancellationToken cancellationToken = default);
    Task<bool> DeleteCardAsync(int id, CancellationToken cancellationToken = default);
}