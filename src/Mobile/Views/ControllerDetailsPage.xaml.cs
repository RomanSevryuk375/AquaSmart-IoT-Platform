using Mobile.ViewModels;

namespace Mobile.Views;

public partial class ControllerDetailsPage : ContentPage
{
    public ControllerDetailsPage(ControllerDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
