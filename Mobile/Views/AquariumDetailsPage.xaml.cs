using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AquariumDetailsPage : ContentPage
{
    private readonly AquariumDetailsViewModel _viewModel;

    public AquariumDetailsPage(AquariumDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.WhenAll(
            HeaderCard.FadeInUpAsync(300, 15),
            ContentScroll.FadeInUpAsync(350, 20)
        );
    }
}
