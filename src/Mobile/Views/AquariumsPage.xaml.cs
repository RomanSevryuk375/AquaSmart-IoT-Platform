using Mobile.Extensions;
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
        await Task.WhenAll(
            ListScroll.FadeInUpAsync(350, 15),
            AddButton.FadeInUpAsync(400, 25)
        );
    }
}
