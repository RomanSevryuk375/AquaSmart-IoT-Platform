using Mobile.ViewModels;

namespace Mobile.Views;

public partial class DeleteAccountPage : ContentPage
{
    public DeleteAccountPage(DeleteAccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
