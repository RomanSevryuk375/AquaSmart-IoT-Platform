using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AddAquariumPage : ContentPage
{
    public AddAquariumPage(AddAquariumViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
