using System.ComponentModel;
using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.WhenAll(
            HeaderContainer.FadeInUpAsync(300, 15),
            FormContainer.FadeInUpAsync(400, 25)
        );
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginViewModel.ErrorMessage) && !string.IsNullOrEmpty(_viewModel.ErrorMessage))
        {
            await FormContainer.ShakeAsync();
        }
    }
}
