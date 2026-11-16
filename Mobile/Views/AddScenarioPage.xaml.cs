using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AddScenarioPage : ContentPage
{
    public AddScenarioPage(AddScenarioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}
