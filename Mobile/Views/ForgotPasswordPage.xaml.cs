using System.ComponentModel;
using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ForgotPasswordViewModel _viewModel;

    public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ForgotPasswordContainer.FadeInUpAsync(350, 20);
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ForgotPasswordViewModel.ErrorMessage) && !string.IsNullOrEmpty(_viewModel.ErrorMessage))
        {
            await ForgotPasswordContainer.ShakeAsync();
        }
    }
}
