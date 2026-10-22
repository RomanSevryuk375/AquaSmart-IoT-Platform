using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;

namespace Mobile.ViewModels;

[QueryProperty(nameof(Article), "Article")]
public partial class ArticleDetailsViewModel : ObservableObject
{
    [ObservableProperty]
    private KnowledgeArticleModel _article = new();

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
