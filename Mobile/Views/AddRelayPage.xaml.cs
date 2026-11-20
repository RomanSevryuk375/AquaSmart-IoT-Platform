using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AddRelayPage : ContentPage
{
    public AddRelayPage(AddRelayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}
