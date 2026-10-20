using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

[QueryProperty(nameof(ControllerIdString), "controllerId")]
public partial class ControllerDetailsViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private string? _controllerIdString;

    [ObservableProperty]
    private ControllerModel? _controller;

    public ControllerDetailsViewModel(IControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    async partial void OnControllerIdStringChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
        {
            Controller = await _controllerService.GetControllerByIdAsync(id);
        }
    }

    [RelayCommand]
    private async Task RebootAsync()
    {
        if (Controller == null) return;
        var confirm = await Shell.Current.DisplayAlert("Перезагрузка", $"Перезагрузить контроллер «{Controller.Name}»?", "Да", "Отмена");
        if (confirm)
        {
            await _controllerService.RebootControllerAsync(Controller.Id);
            await Shell.Current.DisplayAlert("Готово", "Команда перезагрузки отправлена на контроллер", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Controller == null) return;
        var confirm = await Shell.Current.DisplayAlert("Удаление", $"Удалить контроллер «{Controller.Name}» из системы?", "Удалить", "Отмена");
        if (confirm)
        {
            await _controllerService.DeleteControllerAsync(Controller.Id);
            await Shell.Current.GoToAsync("..");
        }
    }
}
