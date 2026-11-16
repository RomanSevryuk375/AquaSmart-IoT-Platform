using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AppSettingsPage : ContentPage
{
    public AppSettingsPage(AppSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
