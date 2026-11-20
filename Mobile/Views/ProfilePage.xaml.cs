using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
        await Task.WhenAll(
            ProfileHeaderCard.FadeInUpAsync(300, 15),
            SettingsCard.FadeInUpAsync(350, 20)
        );
    }
}
