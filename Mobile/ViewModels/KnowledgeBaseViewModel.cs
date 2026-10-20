using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class KnowledgeBaseViewModel : ObservableObject
{
    private readonly IKnowledgeService _knowledgeService;

    [ObservableProperty]
    private ObservableCollection<KnowledgeArticleModel> _articles = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public KnowledgeBaseViewModel(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var list = await _knowledgeService.GetArticlesAsync();
        Articles.Clear();
        foreach (var a in list) Articles.Add(a);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        var list = await _knowledgeService.SearchArticlesAsync(SearchQuery);
        Articles.Clear();
        foreach (var a in list) Articles.Add(a);
    }

    [RelayCommand]
    private async Task SelectArticleAsync(KnowledgeArticleModel article)
    {
        await Shell.Current.DisplayAlert(article.Title, $"{article.Content}\n\nКатегория: {article.Category}", "Закрыть");
    }
}
