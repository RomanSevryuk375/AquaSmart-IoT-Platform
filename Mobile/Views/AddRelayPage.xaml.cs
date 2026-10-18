using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AddRelayPage : ContentPage
{
    public AddRelayPage(AddRelayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
