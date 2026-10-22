namespace Mobile.Models;

public class KnowledgeArticleModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ReadTimeMinutes { get; set; } = 3;
    public string Icon { get; set; } = "🐟";
    public string BorderColor { get; set; } = "#4152B0";
    public string HeaderBgColor { get; set; } = "#DDE4F8";
    
    // Rich content fields matching Figma Article view (1029:11925)
    public string Subtitle1 { get; set; } = string.Empty;
    public string Paragraph1 { get; set; } = string.Empty;
    public string Subtitle2 { get; set; } = string.Empty;
    public string Paragraph2 { get; set; } = string.Empty;
    public string CalloutTip { get; set; } = string.Empty;
    public string Subtitle3 { get; set; } = string.Empty;
    public string Paragraph3 { get; set; } = string.Empty;
    public string CalloutWarning { get; set; } = string.Empty;
}
