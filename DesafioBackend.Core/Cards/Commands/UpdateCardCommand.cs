using DesafioBackend.ApiService.Cards.Models;

using FluentValidation;

namespace DesafioBackend.Core.Cards.Commands;

/// <summary>
/// Update card command - Contain necessary structure to update an existing card.
/// Null itens will not be updated. 
/// </summary>
/// <param name="Title">Card title can't be empty or greater than 100 characters.</param>
/// <param name="Content">Content of the card can't be empty or greater than 10000 characters.</param>
/// <param name="List">Some list string can't be greater than 10000 characters</param>
public record struct UpdateCardCommand(string? Title, string? Content, string? List)
{
    /// <summary>
    /// Update information from existing card.
    /// </summary>
    /// <param name="card">Card to be updated.</param>
    /// <returns>Card with changes applied.</returns>
    public Card UpdateCard(Card card)
    {
        card.Title = Title ?? card.Title;
        card.Content = Content ?? card.Content;
        card.List = List ?? card.List;
        return card;
    }
}

/// <summary>
/// Validator for UpdateCardCommandValidator
/// </summary>
internal sealed class UpdateCardCommandValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardCommandValidator()
    {
        RuleFor(p => p.Title)
            .MinimumLength(3)
            .MaximumLength(100)
            .NotEmpty();

        RuleFor(p => p.Content)
            .MinimumLength(3)
            .MaximumLength(10_000)
            .NotEmpty();

        RuleFor(p => p.List)
            .MaximumLength(10_000);
    }
}