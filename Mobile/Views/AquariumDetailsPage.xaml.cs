using System.ComponentModel;
using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class AquariumDetailsPage : ContentPage
{
    private readonly AquariumDetailsViewModel _viewModel;

    public AquariumDetailsPage(AquariumDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.WhenAll(
            HeaderCard.FadeInUpAsync(300, 15),
            ContentScroll.FadeInUpAsync(350, 20)
        );
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AquariumDetailsViewModel.SelectedTabIndex))
        {
            ContentScroll.Opacity = 0.2;
            ContentScroll.TranslationY = 8;
            await Task.WhenAll(
                ContentScroll.FadeTo(1.0, 180, Easing.CubicOut),
                ContentScroll.TranslateTo(0, 0, 180, Easing.CubicOut)
            );
        }
    }
}
