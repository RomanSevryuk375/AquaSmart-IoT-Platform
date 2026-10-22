using Mobile.ViewModels;

namespace Mobile.Views;

public partial class EditProfilePage : ContentPage
{
    private readonly EditProfileViewModel _viewModel;

    public EditProfilePage(EditProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
