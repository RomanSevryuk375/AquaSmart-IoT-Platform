using Mobile.ViewModels;

namespace Mobile.Views;

public partial class EditAquariumPage : ContentPage
{
    private readonly EditAquariumViewModel _viewModel;

    public EditAquariumPage(EditAquariumViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
}
