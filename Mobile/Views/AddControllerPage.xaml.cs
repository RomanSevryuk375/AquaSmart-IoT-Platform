using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AddControllerPage : ContentPage
{
    public AddControllerPage(AddControllerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}
