using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

[QueryProperty(nameof(ControllerIdString), "controllerId")]
public partial class ControllerDetailsViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private string? _controllerIdString;

    [ObservableProperty]
    private ControllerModel? _controller;

    [ObservableProperty]
    private string _controllerCode = "020623465";

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _macAddress = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isDeleteDialogVisible;

    public ControllerDetailsViewModel(IControllerService controllerService)
    {
        _controllerService = controllerService;
    }

    async partial void OnControllerIdStringChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
        {
            Controller = await _controllerService.GetControllerByIdAsync(id);
            if (Controller != null)
            {
                Name = Controller.Name;
                MacAddress = Controller.MacAddress;
                IsEnabled = Controller.IsOnline;
            }
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Controller != null)
        {
            Controller.Name = Name;
            Controller.MacAddress = MacAddress;
            Controller.IsOnline = IsEnabled;
        }
        await Shell.Current.DisplayAlert("Успех", "Изменения контроллера сохранены!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void PromptDelete()
    {
        IsDeleteDialogVisible = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteDialogVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmDeleteAsync()
    {
        IsDeleteDialogVisible = false;
        if (Controller != null)
        {
            await _controllerService.DeleteControllerAsync(Controller.Id);
        }
        await Shell.Current.GoToAsync("..");
    }
}
