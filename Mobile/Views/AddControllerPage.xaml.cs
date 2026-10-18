using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AddControllerPage : ContentPage
{
    public AddControllerPage(AddControllerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.WhenAll(
            HeaderCard.FadeInUpAsync(300, 15),
            FormCard.FadeInUpAsync(350, 20)
        );
    }
}
