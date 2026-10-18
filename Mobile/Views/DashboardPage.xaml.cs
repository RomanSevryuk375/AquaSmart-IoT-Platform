using Mobile.Extensions;
using Mobile.ViewModels;

namespace Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private bool _hasAnimated;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();

        if (!_hasAnimated)
        {
            _hasAnimated = true;
            await new VisualElement[]
            {
                HeaderCard,
                AlertCard,
                AquariumsSection,
                TelemetrySection,
                RelaysSection
            }.CascadeEntranceAsync(280, 60);

            if (_viewModel.HasProblems)
            {
                await AlertCard.PulseAsync(1.04, 220);
            }
        }
    }
}
