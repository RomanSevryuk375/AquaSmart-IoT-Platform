using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class ControllerDetailsPage : ContentPage
{
    public ControllerDetailsPage(ControllerDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.WhenAll(
            HeaderCard.FadeInUpAsync(300, 15),
            DetailsCard.FadeInUpAsync(350, 20),
            ActionsCard.FadeInUpAsync(400, 25)
        );
    }
}
