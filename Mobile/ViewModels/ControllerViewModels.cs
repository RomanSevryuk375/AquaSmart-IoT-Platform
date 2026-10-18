using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class ControllersViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private ObservableCollection<ControllerModel> _controllers = new();

    [ObservableProperty]
    private bool _hasNoControllers;

    [ObservableProperty]
    private bool _isBusy;

    public ControllersViewModel(IControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var list = await _controllerService.GetControllersAsync();
        Controllers.Clear();
        foreach (var c in list) Controllers.Add(c);
        HasNoControllers = Controllers.Count == 0;
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SelectControllerAsync(ControllerModel controller)
    {
        await Shell.Current.GoToAsync($"ControllerDetailsPage?controllerId={controller.Id}");
    }

    [RelayCommand]
    private async Task AddControllerAsync()
    {
        await Shell.Current.GoToAsync("AddControllerPage");
    }
}

public partial class AddControllerViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private string _controllerName = string.Empty;

    [ObservableProperty]
    private string _modelType = "ESP32-AquaHub v2";

    [ObservableProperty]
    private string _macAddress = "24:6F:28:B4:72:01";

    [ObservableProperty]
    private string _attachedAquarium = "Биотоп Танганьика №1";

    public AddControllerViewModel(IControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ControllerName))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите имя контроллера", "OK");
            return;
        }

        var controller = new ControllerModel
        {
            Name = ControllerName,
            ModelType = ModelType,
            MacAddress = MacAddress,
            IpAddress = "192.168.1." + new Random().Next(100, 200),
            FirmwareVersion = "v2.4.1 (Latest)",
            IsOnline = true,
            LastSeen = DateTime.UtcNow,
            AttachedAquariumName = AttachedAquarium
        };

        await _controllerService.AddControllerAsync(controller);
        await Shell.Current.DisplayAlert("Успешно", $"Контроллер «{ControllerName}» сопряжён и активен!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

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
