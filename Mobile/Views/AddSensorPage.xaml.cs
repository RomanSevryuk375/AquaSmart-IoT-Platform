using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AddSensorPage : ContentPage
{
    public AddSensorPage(AddSensorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
