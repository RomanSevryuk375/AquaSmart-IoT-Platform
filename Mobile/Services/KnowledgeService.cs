using Mobile.Models;

namespace Mobile.Services;

public interface IKnowledgeService
{
    Task<List<KnowledgeArticleModel>> GetArticlesAsync();
    Task<List<KnowledgeArticleModel>> SearchArticlesAsync(string query);
}

public class KnowledgeService : IKnowledgeService
{
    private readonly List<KnowledgeArticleModel> _articles = new()
    {
        new KnowledgeArticleModel
        {
            Title = "Оптимальные параметры воды для пресноводных рыб",
            Category = "Гидрохимия",
            Summary = "Руководство по контролю температуры (24-27°C), pH (6.5-7.5) и жесткости воды dGH/dKH.",
            Content = "Стабильность химического состава воды важнее, чем точечные идеальные значения. Резкие скачки pH на 0.5 единиц вызывают стресс у рыб. Регулярная подмена 20-25% воды раз в неделю помогает удерживать нитраты ниже 20 мг/л.",
            ReadTimeMinutes = 4,
            Icon = "💧"
        },
        new KnowledgeArticleModel
        {
            Title = "Борьба с водорослями и настройка фотопериода",
            Category = "Освещение и растения",
            Summary = "Как избежать вспышки чёрной бороды и сине-зелёных водорослей с помощью смарт-реле таймера.",
            Content = "Световой день в аквариуме не должен превышать 8-10 часов. С помощью умного реле настройте ступенчатое включение или дневную световую паузу (4 часа свет - 2 часа пауза - 4 часа свет).",
            ReadTimeMinutes = 5,
            Icon = "🌿"
        },
        new KnowledgeArticleModel
        {
            Title = "Умное автоматическое кормление: дозировка и график",
            Category = "Кормление",
            Summary = "Предотвращение перекорма и закисания грунта с помощью автоматизированных сценариев.",
            Content = "Перекорм — причина №1 помутнения воды и скачка аммиака. Настройте автокормушку на выдачу минимальной дозы дважды в день, синхронизировав с отключением внешнего фильтра на 5 минут во время трапезы.",
            ReadTimeMinutes = 3,
            Icon = "🥣"
        },
        new KnowledgeArticleModel
        {
            Title = "Аэрация и насыщение кислородом O2 в жаркий период",
            Category = "Оборудование",
            Summary = "Почему при повышении температуры воды растворимость кислорода падает и как защитить рыб.",
            Content = "При температуре выше 28°C растворимость кислорода критически падает. Включите сценарий автоматического форсирования компрессора при превышении температуры выше 26.5°C.",
            ReadTimeMinutes = 4,
            Icon = "🫧"
        }
    };

    public Task<List<KnowledgeArticleModel>> GetArticlesAsync()
    {
        return Task.FromResult(_articles.ToList());
    }

    public Task<List<KnowledgeArticleModel>> SearchArticlesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(_articles.ToList());

        var q = query.ToLowerInvariant();
        var filtered = _articles.Where(a => a.Title.ToLowerInvariant().Contains(q) || a.Category.ToLowerInvariant().Contains(q)).ToList();
        return Task.FromResult(filtered);
    }
}
