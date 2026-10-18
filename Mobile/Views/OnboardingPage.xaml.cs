using System.ComponentModel;
using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class OnboardingPage : ContentPage
{
    private readonly OnboardingViewModel _viewModel;

    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.WhenAll(
            Carousel.FadeInUpAsync(350, 15),
            BottomContainer.FadeInUpAsync(400, 20)
        );
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OnboardingViewModel.CurrentPosition))
        {
            if (_viewModel.CurrentPosition == _viewModel.Slides.Count - 1)
            {
                NextButton.Text = "Начать работу";
                await NextButton.PulseAsync(1.06, 140);
            }
            else
            {
                NextButton.Text = "Продолжить";
            }
        }
    }
}
