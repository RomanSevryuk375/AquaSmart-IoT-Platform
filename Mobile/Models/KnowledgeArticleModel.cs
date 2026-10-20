namespace Mobile.Models;

public class KnowledgeArticleModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ReadTimeMinutes { get; set; } = 3;
    public string Icon { get; set; } = "📖";
}
