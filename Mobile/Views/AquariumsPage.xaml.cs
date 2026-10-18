using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AquariumsPage : ContentPage
{
    private readonly AquariumsViewModel _viewModel;

    public AquariumsPage(AquariumsViewModel viewModel)
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
