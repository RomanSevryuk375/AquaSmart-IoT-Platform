using Mobile.ViewModels;

namespace Mobile.Views;

public partial class ChangeAvatarPage : ContentPage
{
    private readonly ChangeAvatarViewModel _viewModel;

    public ChangeAvatarPage(ChangeAvatarViewModel viewModel)
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
