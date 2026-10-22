using Mobile.ViewModels;

namespace Mobile.Views;

public partial class SecuritySettingsPage : ContentPage
{
    public SecuritySettingsPage(SecuritySettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
