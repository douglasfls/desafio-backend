using DesafioBackend.ApiService.Cards.Models;

using FluentValidation;

namespace DesafioBackend.Core.Cards.Commands;

/// <summary>
/// Create card command - Contain necessary structure to crate a new card. 
/// </summary>
/// <param name="Title">Card title can't be empty or greater than 100 characters.</param>
/// <param name="Content">Content of the card can't be empty or greater than 10000 characters.</param>
/// <param name="List">Some list string can't be greater than 10000 characters</param>
public record struct CreateCardCommand(string Title, string Content, string? List)
{
    /// <summary>
    /// Build card based on current command.
    /// </summary>
    /// <returns>New card with content filled.</returns>
    public Card CreateCard() => Card.Create(Title, Content, List);
}

/// <summary>
/// Validator for CreateCardCommand
/// </summary>
internal sealed class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(p => p.Title)
            .MaximumLength(100)
            .NotEmpty()
            .NotNull();
        
        RuleFor(p => p.Content)
            .MaximumLength(10_000)
            .NotEmpty()
            .NotNull();

        RuleFor(p => p.List)
            .MaximumLength(10_000);
    }
}