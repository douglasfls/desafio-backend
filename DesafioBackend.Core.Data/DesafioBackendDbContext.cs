using DesafioBackend.ApiService.Cards.Models;

using Microsoft.EntityFrameworkCore;

namespace DesafioBackend.Core.Data;

public sealed class DesafioBackendDbContext(DbContextOptions<DesafioBackendDbContext> options)
    : DbContext(options), IDesafioBackendDbContext
{
    public DbSet<Card> Cards { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DesafioBackendDbContext).Assembly);
    }
}