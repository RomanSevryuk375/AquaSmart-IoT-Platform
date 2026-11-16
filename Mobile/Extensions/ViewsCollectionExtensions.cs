using Mobile.Views;

namespace Mobile.Extensions;

public static class ViewsCollectionExtensions
{
    public static IServiceCollection AddViews(this IServiceCollection services)
    {
        services.AddTransient<MainPage>();
        services.AddTransient<OnboardingPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<RegistrationPage>();
        services.AddTransient<ForgotPasswordPage>();

        services.AddTransient<DashboardPage>();
        services.AddTransient<AquariumsPage>();
        services.AddTransient<AquariumDetailsPage>();

        services.AddTransient<AddAquariumPage>();
        services.AddTransient<AddSensorPage>();
        services.AddTransient<AddRelayPage>();
        services.AddTransient<AddScenarioPage>();

        services.AddTransient<KnowledgeBasePage>();
        services.AddTransient<ProfilePage>();

        services.AddTransient<ControllersPage>();
        services.AddTransient<AddControllerPage>();
        services.AddTransient<ControllerDetailsPage>();

        services.AddTransient<EditProfilePage>();
        services.AddTransient<ChangeAvatarPage>();
        services.AddTransient<AppSettingsPage>();
        services.AddTransient<DataHistoryPage>();
        services.AddTransient<SecuritySettingsPage>();
        services.AddTransient<HelpSupportPage>();
        services.AddTransient<AboutAppPage>();
        services.AddTransient<DeleteAccountPage>();
        services.AddTransient<ArticleDetailsPage>();
        services.AddTransient<NotificationsPage>();
        services.AddTransient<EditAquariumPage>();

        return services;
    }
}
