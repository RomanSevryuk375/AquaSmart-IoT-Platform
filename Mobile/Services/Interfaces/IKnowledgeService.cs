namespace Mobile.Services.Interfaces;

public interface IKnowledgeService
{
    Task<List<KnowledgeArticleModel>> GetArticlesAsync();
    Task<List<KnowledgeArticleModel>> SearchArticlesAsync(string query);
}
