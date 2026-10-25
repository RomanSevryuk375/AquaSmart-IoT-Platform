using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Views;

namespace Mobile.ViewModels;

public partial class KnowledgeBaseViewModel : ObservableObject
{
    private readonly IKnowledgeService _knowledgeService;
    private List<KnowledgeArticleModel> _allArticles = new();

    [ObservableProperty]
    private ObservableCollection<KnowledgeArticleModel> _articles = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "ВСЕ";

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<string> Categories { get; } = new()
    {
        "ВСЕ",
        "АКВАРИУМ",
        "ТЕРРАРИУМ",
        "ПАЛЮДАРИУМ"
    };

    public KnowledgeBaseViewModel(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        _allArticles = await _knowledgeService.GetArticlesAsync();
        ApplyFilter();
        IsBusy = false;
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) category = "ВСЕ";
        SelectedCategory = category;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            _allArticles = await _knowledgeService.GetArticlesAsync();
        }
        else
        {
            _allArticles = await _knowledgeService.SearchArticlesAsync(SearchQuery);
        }
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _allArticles.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SelectedCategory) && !SelectedCategory.Equals("ВСЕ", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(a => a.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.ToLowerInvariant();
            filtered = filtered.Where(a =>
                a.Title.ToLowerInvariant().Contains(q) ||
                a.Summary.ToLowerInvariant().Contains(q) ||
                a.Category.ToLowerInvariant().Contains(q));
        }

        Articles.Clear();
        foreach (var a in filtered)
        {
            Articles.Add(a);
        }
    }

    [RelayCommand]
    private async Task SelectArticleAsync(KnowledgeArticleModel article)
    {
        if (article == null) return;
        var navigationParameter = new Dictionary<string, object>
        {
            { "Article", article }
        };
        await Shell.Current.GoToAsync(nameof(ArticleDetailsPage), navigationParameter);
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await Shell.Current.GoToAsync(nameof(NotificationsPage));
    }
}
