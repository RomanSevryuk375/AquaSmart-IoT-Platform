using System.ComponentModel;
using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class RegistrationPage : ContentPage
{
    private readonly RegistrationViewModel _viewModel;

    public RegistrationPage(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RegistrationContainer.FadeInUpAsync(350, 20);
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RegistrationViewModel.ErrorMessage) && !string.IsNullOrEmpty(_viewModel.ErrorMessage))
        {
            await RegistrationContainer.ShakeAsync();
        }
    }
}
