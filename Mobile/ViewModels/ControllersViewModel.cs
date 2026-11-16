using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class ControllersViewModel : ObservableObject
{
    private readonly IControllerService _controllerService;

    [ObservableProperty]
    private ObservableCollection<ControllerModel> _controllers = new();

    [ObservableProperty]
    private bool _hasNoControllers;

    public bool HasControllers => !HasNoControllers;

    partial void OnHasNoControllersChanged(bool value)
    {
        OnPropertyChanged(nameof(HasControllers));
    }

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
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task OpenKnowledgeBaseAsync()
    {
        await Shell.Current.GoToAsync("//MainTabs/KnowledgeBasePage");
    }

    [RelayCommand]
    private async Task OpenNotificationsAsync()
    {
        await Shell.Current.GoToAsync("NotificationsPage");
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
