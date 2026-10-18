using Mobile.ViewModels;
using Mobile.Views;

namespace Mobile;

public partial class AppShell : Shell
{
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

    private void RegisterRoutes()
    {
        Routing.RegisterRoute("MainPage", typeof(MainPage));
        Routing.RegisterRoute("OnboardingPage", typeof(OnboardingPage));
        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("RegistrationPage", typeof(RegistrationPage));
        Routing.RegisterRoute("ForgotPasswordPage", typeof(ForgotPasswordPage));

        Routing.RegisterRoute("DashboardPage", typeof(DashboardPage));
        Routing.RegisterRoute("AquariumsPage", typeof(AquariumsPage));
        Routing.RegisterRoute("AquariumDetailsPage", typeof(AquariumDetailsPage));

        Routing.RegisterRoute("AddAquariumPage", typeof(AddAquariumPage));
        Routing.RegisterRoute("AddSensorPage", typeof(AddSensorPage));
        Routing.RegisterRoute("AddRelayPage", typeof(AddRelayPage));
        Routing.RegisterRoute("AddScenarioPage", typeof(AddScenarioPage));

        Routing.RegisterRoute("KnowledgeBasePage", typeof(KnowledgeBasePage));
        Routing.RegisterRoute("ProfilePage", typeof(ProfilePage));
    }
}
