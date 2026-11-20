using Mobile.ViewModels;

namespace Mobile.Views;

public partial class HelpSupportPage : ContentPage
{
    public HelpSupportPage(HelpSupportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
