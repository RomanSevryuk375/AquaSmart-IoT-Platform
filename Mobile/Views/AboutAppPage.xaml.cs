using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AboutAppPage : ContentPage
{
    public AboutAppPage(AboutAppViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
