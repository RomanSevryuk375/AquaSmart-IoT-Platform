using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;
using Mobile.Services.Interfaces;

namespace Mobile.ViewModels;

[QueryProperty(nameof(AquariumIdString), "aquariumId")]
public partial class EditAquariumViewModel : ObservableObject, IQueryAttributable
{
    private readonly IAquariumService _aquariumService;
    private readonly IControllerService _controllerService;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("aquariumId", out var val) && val != null)
        {
            var strVal = val.ToString();
            if (Guid.TryParse(strVal, out var id))
            {
                AquariumIdString = strVal;
                _ = LoadAsync(id);
            }
        }
    }

    [ObservableProperty]
    private string? _aquariumIdString;

    [ObservableProperty]
    private AquariumModel? _aquarium;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeciesInfo))]
    private string _volume = "30";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TankTypeName))]
    [NotifyPropertyChangedFor(nameof(TankTypeIcon))]
    [NotifyPropertyChangedFor(nameof(SpeciesInfo))]
    [NotifyPropertyChangedFor(nameof(IsAquariumSelected))]
    [NotifyPropertyChangedFor(nameof(IsTerrariumSelected))]
    [NotifyPropertyChangedFor(nameof(IsGrowboxSelected))]
    [NotifyPropertyChangedFor(nameof(IsFlorariumSelected))]
    [NotifyPropertyChangedFor(nameof(IsFormicariumSelected))]
    private int _selectedTankType = 0; // 0..4 (1..5 matching backend EcosystemType)

    public bool IsAquariumSelected => SelectedTankType == 0;
    public bool IsTerrariumSelected => SelectedTankType == 1;
    public bool IsGrowboxSelected => SelectedTankType == 2;
    public bool IsFlorariumSelected => SelectedTankType == 3;
    public bool IsFormicariumSelected => SelectedTankType == 4;

    [ObservableProperty]
    private int _selectedWaterType = 0; // 0: Freshwater, 1: Marine

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CardThemeColor))]
    [NotifyPropertyChangedFor(nameof(CardBorderColor))]
    [NotifyPropertyChangedFor(nameof(IsTheme0Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme1Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme2Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme3Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme4Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme5Selected))]
    private int _selectedThemeIndex = 0;

    public bool IsTheme0Selected => SelectedThemeIndex == 0;
    public bool IsTheme1Selected => SelectedThemeIndex == 1;
    public bool IsTheme2Selected => SelectedThemeIndex == 2;
    public bool IsTheme3Selected => SelectedThemeIndex == 3;
    public bool IsTheme4Selected => SelectedThemeIndex == 4;
    public bool IsTheme5Selected => SelectedThemeIndex == 5;

    [ObservableProperty]
    private ObservableCollection<ControllerModel> _availableControllers = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedControllerName))]
    [NotifyPropertyChangedFor(nameof(SpeciesInfo))]
    private ControllerModel? _selectedController;

    public string SelectedControllerName => SelectedController?.Name ?? Aquarium?.ControllerName ?? (LocalizationService.Instance.IsEnglish ? "Hub-02" : "Хаб-02");

    public string TankTypeName => SelectedTankType switch
    {
        1 => LocalizationService.Instance["TankType_Terrarium"],
        2 => LocalizationService.Instance["TankType_Growbox"],
        3 => LocalizationService.Instance["TankType_Florarium"],
        4 => LocalizationService.Instance["TankType_Formicarium"],
        _ => LocalizationService.Instance["TankType_Aquarium"]
    };

    public string TankTypeIcon => SelectedTankType switch
    {
        1 => "🦎",
        2 => "🌱",
        3 => "🌸",
        4 => "🐜",
        _ => "🐟"
    };

    public string CardThemeColor => SelectedThemeIndex switch
    {
        1 => "#E0F7FA",
        2 => "#EDF7EE",
        3 => "#FFF9E6",
        4 => "#FFF3E0",
        5 => "#F5F5F5",
        _ => "#F0F4FF"
    };

    public string CardBorderColor => SelectedThemeIndex switch
    {
        1 => "#80DEEA",
        2 => "#C3E6C7",
        3 => "#FFE082",
        4 => "#FFCC80",
        5 => "#E0E0E0",
        _ => "#C5D4F5"
    };

    public string SpeciesInfo => $"{TankTypeName} • {Volume}{(LocalizationService.Instance.IsEnglish ? "L" : "л")} • {SelectedControllerName}";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    public EditAquariumViewModel(IAquariumService aquariumService, IControllerService controllerService)
    {
        _aquariumService = aquariumService;
        _controllerService = controllerService;
    }

    async partial void OnAquariumIdStringChanged(string? value)
    {
        if (Guid.TryParse(value, out var id))
        {
            await LoadAsync(id);
        }
    }

    public async Task LoadAsync(Guid id)
    {
        IsBusy = true;
        try
        {
            Aquarium = await _aquariumService.GetAquariumByIdAsync(id);
            if (Aquarium != null)
            {
                Name = Aquarium.Name;
                Volume = Aquarium.VolumeLiters.ToString("0.#");
                SelectedWaterType = Aquarium.WaterType == WaterType.Marine ? 1 : 0;

                // Match tank type
                var typeStr = (!string.IsNullOrWhiteSpace(Aquarium.TankTypeSubtitle) && Aquarium.TankTypeSubtitle != "Ёмкость" && Aquarium.TankTypeSubtitle != "Tank")
                    ? Aquarium.TankTypeSubtitle
                    : (Aquarium.SpeciesInfo ?? string.Empty);

                SelectedTankType = Aquarium.EcosystemType > 0 
                    ? Math.Clamp(Aquarium.EcosystemType - 1, 0, 4)
                    : (typeStr switch
                    {
                        var s when s.Contains("Террариум", StringComparison.OrdinalIgnoreCase) || s.Contains("Terrarium", StringComparison.OrdinalIgnoreCase) => 1,
                        var s when s.Contains("Гроубокс", StringComparison.OrdinalIgnoreCase) || s.Contains("Growbox", StringComparison.OrdinalIgnoreCase) => 2,
                        var s when s.Contains("Флорариум", StringComparison.OrdinalIgnoreCase) || s.Contains("Florarium", StringComparison.OrdinalIgnoreCase) => 3,
                        var s when s.Contains("Формикарий", StringComparison.OrdinalIgnoreCase) || s.Contains("Formicarium", StringComparison.OrdinalIgnoreCase) => 4,
                        _ => 0
                    });

                // If the name was default "Мой первый аквариум" / "My First Aquarium" but this container is a Terrarium/etc., adjust it
                if ((Name.Equals("Мой первый аквариум", StringComparison.OrdinalIgnoreCase) || Name.Equals("My First Aquarium", StringComparison.OrdinalIgnoreCase)) && SelectedTankType > 0)
                {
                    Name = LocalizationService.Instance.IsEnglish
                        ? $"My First {TankTypeName}"
                        : $"Мой первый {TankTypeName.ToLowerInvariant()}";
                }

                // Match theme index
                SelectedThemeIndex = Aquarium.CardThemeColor switch
                {
                    "#E0F7FA" => 1,
                    "#EDF7EE" => 2,
                    "#FFF9E6" => 3,
                    "#FFF3E0" => 4,
                    "#F5F5F5" => 5,
                    _ => 0
                };
            }

            // Load controllers
            var controllers = await _controllerService.GetControllersAsync();
            AvailableControllers.Clear();
            foreach (var c in controllers)
            {
                AvailableControllers.Add(c);
            }

            if (Aquarium != null && !string.IsNullOrWhiteSpace(Aquarium.ControllerName))
            {
                SelectedController = AvailableControllers.FirstOrDefault(c => c.Name == Aquarium.ControllerName)
                                     ?? AvailableControllers.FirstOrDefault();
            }
        }
        catch
        {
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectTankType(object? parameter)
    {
        if (parameter is int idx)
        {
            SelectedTankType = idx;
        }
        else if (parameter is string s && int.TryParse(s, out var parsed))
        {
            SelectedTankType = parsed;
        }

        if (string.IsNullOrWhiteSpace(Name) || Name.StartsWith("Мой первый", StringComparison.OrdinalIgnoreCase))
        {
            Name = $"Мой первый {TankTypeName.ToLowerInvariant()}";
        }
    }

    [RelayCommand]
    private void SelectWaterType(object? parameter)
    {
        if (parameter is int idx)
        {
            SelectedWaterType = idx;
        }
        else if (parameter is string s && int.TryParse(s, out var parsed))
        {
            SelectedWaterType = parsed;
        }
    }

    [RelayCommand]
    private void SelectTheme(object? parameter)
    {
        if (parameter is int idx)
        {
            SelectedThemeIndex = idx;
        }
        else if (parameter is string s && int.TryParse(s, out var parsed))
        {
            SelectedThemeIndex = parsed;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var isEn = LocalizationService.Instance.IsEnglish;
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = isEn ? "Please enter tank name" : "Введите название ёмкости";
            HasError = true;
            return;
        }

        if (!double.TryParse(Volume, out var parsedVol) || parsedVol <= 0)
        {
            ErrorMessage = isEn ? "Please enter valid tank volume" : "Укажите корректный объём ёмкости";
            HasError = true;
            return;
        }

        HasError = false;
        ErrorMessage = null;
        IsBusy = true;

        try
        {
            if (Aquarium != null)
            {
                Aquarium.Name = Name.Trim();
                Aquarium.VolumeLiters = parsedVol;
                Aquarium.WaterType = SelectedWaterType == 1 ? WaterType.Marine : WaterType.Freshwater;
                Aquarium.TankTypeSubtitle = TankTypeName;
                Aquarium.Icon = TankTypeIcon;
                Aquarium.CardThemeColor = CardThemeColor;
                Aquarium.CardBorderColor = CardBorderColor;
                Aquarium.ControllerName = SelectedControllerName;
                Aquarium.SpeciesInfo = SpeciesInfo;
                Aquarium.EcosystemType = SelectedTankType + 1;

                await _aquariumService.UpdateAquariumAsync(Aquarium);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = isEn ? $"Failed to save: {ex.Message}" : $"Не удалось сохранить: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Aquarium == null) return;

        var loc = Services.LocalizationService.Instance;
        var title = loc.GetString("EditTank_DeleteConfirmTitle");
        var msg = string.Format(loc.GetString("EditTank_DeleteConfirmMsg"), Aquarium.Name);
        var deleteText = loc.GetString("Common_Delete");
        var cancelText = loc.GetString("Common_Cancel");

        bool confirm = await Shell.Current.DisplayAlert(title, msg, deleteText, cancelText);
        if (!confirm) return;

        IsBusy = true;
        try
        {
            await _aquariumService.DeleteAquariumAsync(Aquarium.Id);
            await Shell.Current.GoToAsync("//MainTabs/AquariumsPage");
        }
        catch (Exception ex)
        {
            ErrorMessage = loc.IsEnglish ? $"Failed to delete: {ex.Message}" : $"Не удалось удалить: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
