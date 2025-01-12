using DesafioBackend.ApiService.Cards.Models;

using Microsoft.EntityFrameworkCore;

namespace DesafioBackend.Core.Data;

public interface IDesafioBackendDbContext
{
    /// <summary>
    /// Set of cards in the database.
    /// </summary>
    DbSet<Card> Cards { get; }
    
    /// <summary>
    /// Save changes made to tracked objects.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected lines.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}