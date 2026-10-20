using Mobile.Models;

namespace Mobile.Services;

public sealed class KnowledgeService : IKnowledgeService
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
            Title = "Автоматизация светового дня WRGB светильников",
            Category = "Освещение",
            Summary = "Настройка фаз рассвета, зенита и заката для подавления водорослей и стимуляции роста растений.",
            Content = "Резкое включение света вызывает шок у обитателей. Плавный рассвет за 30-40 минут имитирует природные условия. Длительность светового дня для травника не должна превышать 8-9 часов во избежание вспышки черной бороды и ксенококуса.",
            ReadTimeMinutes = 5,
            Icon = "💡"
        },
        new KnowledgeArticleModel
        {
            Title = "Уход за биофильтром и циркуляция",
            Category = "Фильтрация",
            Summary = "Правильное обслуживание канистровых фильтров и сохранение колоний нитрифицирующих бактерий.",
            Content = "Никогда не промывайте наполнители биофильтра водопроводной хлорированной водой — используйте только слитую воду из аквариума. Губки механической очистки промываются каждые 2 недели, биологические кольца — раз в 3-6 месяцев.",
            ReadTimeMinutes = 3,
            Icon = "🔄"
        },
        new KnowledgeArticleModel
        {
            Title = "Карантин новых гидробионтов и адаптация",
            Category = "Здоровье рыб",
            Summary = "Метод капельной адаптации и профилактика болезней при подселении новых жителей.",
            Content = "Капельная пересадка в течение 1.5–2 часов с выравниванием температуры и TDS снижает осмотический шок. Рекомендуется карантинный аквариум на 2–3 недели с наблюдением за кожными покровами и аппетитом.",
            ReadTimeMinutes = 6,
            Icon = "🛡️"
        }
    };

    public Task<List<KnowledgeArticleModel>> GetArticlesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_articles.ToList());
    }

    public Task<List<KnowledgeArticleModel>> SearchArticlesAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(_articles.ToList());
        }

        var lower = query.ToLowerInvariant();
        var filtered = _articles.Where(a =>
            a.Title.ToLowerInvariant().Contains(lower) ||
            a.Summary.ToLowerInvariant().Contains(lower) ||
            a.Category.ToLowerInvariant().Contains(lower)
        ).ToList();

        return Task.FromResult(filtered);
    }
}
