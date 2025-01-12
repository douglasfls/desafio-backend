using DesafioBackend.ApiService.Cards.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesafioBackend.Core.Data.Configurations;

public sealed class CardEntityConfiguration: IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id);
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(p => p.Content)
            .IsRequired()
            .HasMaxLength(10_000);
        builder.Property(p => p.List)
            .HasMaxLength(10_000);
    }
}