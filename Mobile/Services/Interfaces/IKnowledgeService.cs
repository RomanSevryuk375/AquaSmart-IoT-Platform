namespace Mobile.Services.Interfaces;

public interface IKnowledgeService
{
    Task<List<KnowledgeArticleModel>> GetArticlesAsync(CancellationToken cancellationToken = default);
    Task<List<KnowledgeArticleModel>> SearchArticlesAsync(string query, CancellationToken cancellationToken = default);
}
