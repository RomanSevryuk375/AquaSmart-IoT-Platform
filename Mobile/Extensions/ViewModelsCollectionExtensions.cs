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

        return services;
    }
}
