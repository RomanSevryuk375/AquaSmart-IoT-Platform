using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AquariumDetailsPage : ContentPage
{
    private readonly AquariumDetailsViewModel _viewModel;

    public AquariumDetailsPage(AquariumDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
}
