using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class AddControllerViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private string _controllerName = string.Empty;

    [ObservableProperty]
    private string _modelType = "ESP32-AquaHub v2";

    [ObservableProperty]
    private string _macAddress = string.Empty;

    [ObservableProperty]
    private string _attachedAquarium = string.Empty;

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

        if (!Helpers.ValidationHelper.ValidateMacAddress(MacAddress, out var macError))
        {
            await Shell.Current.DisplayAlert("Внимание", macError, "OK");
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
