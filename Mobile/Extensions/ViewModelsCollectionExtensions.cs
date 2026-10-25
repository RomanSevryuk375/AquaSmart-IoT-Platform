using Mobile.ViewModels;

namespace Mobile.Extensions;

public static class ViewModelsCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<AppShellViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<OnboardingViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AquariumsViewModel>();
        services.AddTransient<AquariumDetailsViewModel>();

        services.AddTransient<AddAquariumViewModel>();
        services.AddTransient<AddSensorViewModel>();
        services.AddTransient<AddRelayViewModel>();
        services.AddTransient<AddScenarioViewModel>();

        services.AddTransient<KnowledgeBaseViewModel>();
        services.AddTransient<ProfileViewModel>();

        services.AddTransient<ControllersViewModel>();
        services.AddTransient<AddControllerViewModel>();
        services.AddTransient<ControllerDetailsViewModel>();

        services.AddTransient<EditProfileViewModel>();
        services.AddTransient<ChangeAvatarViewModel>();
        services.AddTransient<AppSettingsViewModel>();
        services.AddTransient<DataHistoryViewModel>();
        services.AddTransient<SecuritySettingsViewModel>();
        services.AddTransient<HelpSupportViewModel>();
        services.AddTransient<AboutAppViewModel>();
        services.AddTransient<DeleteAccountViewModel>();
        services.AddTransient<ArticleDetailsViewModel>();
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<EditAquariumViewModel>();

        return services;
    }
}
