using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class ControllersPage : ContentPage
{
    private readonly ControllersViewModel _viewModel;

    public ControllersPage(ControllersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
        await Task.WhenAll(
            ContentScroll.FadeInUpAsync(350, 15),
            AddButton.FadeInUpAsync(400, 25)
        );
    }
}
