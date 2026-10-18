using Mobile.ViewModels;

namespace Mobile.Views;

public partial class KnowledgeBasePage : ContentPage
{
    private readonly KnowledgeBaseViewModel _viewModel;

    public KnowledgeBasePage(KnowledgeBaseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
