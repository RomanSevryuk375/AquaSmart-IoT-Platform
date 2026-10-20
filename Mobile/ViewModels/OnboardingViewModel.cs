using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    [ObservableProperty]
    private int _currentPosition = 0;

    [ObservableProperty]
    private List<OnboardingSlideModel> _slides = new();

    public OnboardingViewModel()
    {
        Slides = new List<OnboardingSlideModel>
        {
            new OnboardingSlideModel
            {
                Title = "Ваш аквариум всегда под контролем",
                Description = "Мониторинг параметров, управление оборудованием и уведомления — все в одном месте",
                BadgeText = "🛡️",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = "Знайте, что происходит в любой момент",
                Description = "Датчики отслеживают температуру, pH и другие параметры в реальном времени. Вы получаете алерт, если что-то пойдет не так",
                BadgeText = "🌡️",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = "Настройте один раз — забудьте о рутине",
                Description = "Расписание освещения, режим отпуска и автоматические сценарии возьмут на себя ежедневные задачи",
                BadgeText = "⚙️",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = "Уезжайте в отпуск или командировку спокойно",
                Description = "Режим отпуска автоматически настроит оборудование, а удалённый мониторинг покажет статус аквариума в реальном времени",
                BadgeText = "🤖",
                AccentColor = "#4152B0"
            },
            new OnboardingSlideModel
            {
                Title = "Время подключить первое устройство. Готовы начать?",
                Description = "Добавьте свой первый аквариум и подключите датчики — это займёт не более 5 минут",
                BadgeText = "▶️",
                AccentColor = "#4152B0"
            }
        };
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    [RelayCommand]
    private async Task NextOrFinishAsync()
    {
        if (CurrentPosition < Slides.Count - 1)
        {
            CurrentPosition++;
        }
        else
        {
            await Shell.Current.GoToAsync("LoginPage");
        }
    }
}
