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
        await BottomContainer.FadeInUpAsync(400, 20);
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OnboardingViewModel.CurrentPosition))
        {
            UpdateDashes(_viewModel.CurrentPosition);

            if (_viewModel.CurrentPosition == _viewModel.Slides.Count - 1)
            {
                NextButton.Text = "Начать";
                await NextButton.PulseAsync(1.06, 140);
            }
            else
            {
                NextButton.Text = "Далее";
            }
        }
    }

    private void UpdateDashes(int position)
    {
        var dashes = new[] { Dash0, Dash1, Dash2, Dash3, Dash4 };
        var activeColor = Color.FromArgb("#4152B0");
        var inactiveColor = Color.FromArgb("#DCE2F9");

        for (int i = 0; i < dashes.Length; i++)
        {
            dashes[i].BackgroundColor = (i == position) ? activeColor : inactiveColor;
            dashes[i].WidthRequest = (i == position) ? 44 : 36;
        }
    }
}
