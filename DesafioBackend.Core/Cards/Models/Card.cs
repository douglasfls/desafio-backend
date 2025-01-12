namespace DesafioBackend.ApiService.Cards.Models;

public sealed class Card
{
    /// <summary>
    /// Private constructor for migration patterns
    /// </summary>
    private Card() { }

    /// <summary>
    /// Private constructor for migration patterns
    /// </summary>
    /// <param name="title">Some cool title</param>
    /// <param name="content">Some content</param>
    /// <param name="list">Some list</param>
    private Card(string title, string content, string list)
    {
        Title = title;
        Content = content;
        List = list;
    }
    
    public int Id { get; }
    public string Title { get; internal set; }
    public string Content { get; internal set; }
    public string? List { get; internal set; }
    
    /// <summary>
    /// Static constructor for Card
    /// </summary>
    /// <param name="title">Some cool title</param>
    /// <param name="content">Some content</param>
    /// <param name="list">Some list</param>
    /// <returns>New Card</returns>
    public static Card Create(string title, string content, string list) 
        => new (title, content, list);
}