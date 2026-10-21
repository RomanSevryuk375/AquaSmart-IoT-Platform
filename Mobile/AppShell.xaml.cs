using Mobile.ViewModels;
using Mobile.Views;

namespace Mobile;

public partial class AppShell : Shell
{
    private static bool _routesRegistered;

    public AppShell(AppShellViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        RegisterRoutes();
    }

    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        if (BindingContext is AppShellViewModel vm)
        {
            vm.RefreshUserData();
        }
    }

    private static void RegisterRoutes()
    {
        if (_routesRegistered) return;
        _routesRegistered = true;

        // Register secondary / sub-pages that are navigated to dynamically
        // Note: Top-level pages defined in AppShell.xaml (MainPage, DashboardPage, AquariumsPage,
        // KnowledgeBasePage, ProfilePage, ControllersPage) already have shell routes and must not be re-registered.
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));

        Routing.RegisterRoute(nameof(AquariumDetailsPage), typeof(AquariumDetailsPage));
        Routing.RegisterRoute(nameof(AddAquariumPage), typeof(AddAquariumPage));
        Routing.RegisterRoute(nameof(AddSensorPage), typeof(AddSensorPage));
        Routing.RegisterRoute(nameof(AddRelayPage), typeof(AddRelayPage));
        Routing.RegisterRoute(nameof(AddScenarioPage), typeof(AddScenarioPage));

        Routing.RegisterRoute(nameof(AddControllerPage), typeof(AddControllerPage));
        Routing.RegisterRoute(nameof(ControllerDetailsPage), typeof(ControllerDetailsPage));
    }
}
