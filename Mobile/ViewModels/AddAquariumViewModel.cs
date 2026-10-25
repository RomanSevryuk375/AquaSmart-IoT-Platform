using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services.Interfaces;
using Mobile.Views;

namespace Mobile.ViewModels;

public partial class AddAquariumViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private int _currentStep = 1;

    // Step 1: Tank Type
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TankTypeName))]
    [NotifyPropertyChangedFor(nameof(TankTypeIcon))]
    [NotifyPropertyChangedFor(nameof(SpeciesInfo))]
    [NotifyPropertyChangedFor(nameof(IsAquariumSelected))]
    [NotifyPropertyChangedFor(nameof(IsTerrariumSelected))]
    [NotifyPropertyChangedFor(nameof(IsPaludariumSelected))]
    [NotifyPropertyChangedFor(nameof(IsGrowboxSelected))]
    [NotifyPropertyChangedFor(nameof(IsFlorariumSelected))]
    [NotifyPropertyChangedFor(nameof(IsFormicariumSelected))]
    private int _selectedTankType = 0; // 0: Aquarium, 1: Terrarium, 2: Paludarium, 3: Growbox, 4: Florarium, 5: Formicarium

    public bool IsAquariumSelected => SelectedTankType == 0;
    public bool IsTerrariumSelected => SelectedTankType == 1;
    public bool IsPaludariumSelected => SelectedTankType == 2;
    public bool IsGrowboxSelected => SelectedTankType == 3;
    public bool IsFlorariumSelected => SelectedTankType == 4;
    public bool IsFormicariumSelected => SelectedTankType == 5;

    // Step 2: Name
    [ObservableProperty]
    private string _name = "Мой первый аквариум";

    // Step 3: Volume
    [ObservableProperty]
    private int _volume = 20;

    // Step 4: Inhabitants
    public ObservableCollection<InhabitantChipModel> InhabitantsList { get; } = new()
    {
        new("Пресноводные рыбы", true),
        new("Морские рыбы", false),
        new("Водные растения", true),
        new("Черепахи", false),
        new("Ящерицы и змеи", false),
        new("Лягушки", false),
        new("Растения", false),
        new("Муравьи", false),
        new("Другие", false)
    };

    // Step 5: Launch Date
    [ObservableProperty]
    private string _launchDate = "11.06.2026";

    [ObservableProperty]
    private bool _isCalendarOpen;

    // Step 6: Controller
    [ObservableProperty]
    private string _selectedControllerName = "Контроллер №123";

    [ObservableProperty]
    private bool _isControllerPickerOpen;

    // Step 7: Color theme
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTheme0Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme1Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme2Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme3Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme4Selected))]
    [NotifyPropertyChangedFor(nameof(IsTheme5Selected))]
    [NotifyPropertyChangedFor(nameof(PreviewThemeColor))]
    [NotifyPropertyChangedFor(nameof(PreviewBorderColor))]
    private int _selectedThemeIndex = 0; // 0: Blue, 1: Cyan, 2: Green, 3: Yellow, 4: Orange, 5: Gray

    public bool IsTheme0Selected => SelectedThemeIndex == 0;
    public bool IsTheme1Selected => SelectedThemeIndex == 1;
    public bool IsTheme2Selected => SelectedThemeIndex == 2;
    public bool IsTheme3Selected => SelectedThemeIndex == 3;
    public bool IsTheme4Selected => SelectedThemeIndex == 4;
    public bool IsTheme5Selected => SelectedThemeIndex == 5;

    // Overlays
    [ObservableProperty]
    private bool _isCancelDialogOpen;

    [ObservableProperty]
    private bool _isSuccessOpen;

    [ObservableProperty]
    private Guid _createdAquariumId;

    public string TankTypeName => SelectedTankType switch
    {
        1 => "Террариум",
        2 => "Палюдариум",
        3 => "Гроубокс",
        4 => "Флорариум",
        5 => "Формикарий",
        _ => "Аквариум"
    };

    public string TankTypeIcon => SelectedTankType switch
    {
        1 => "🦎",
        2 => "🐢",
        3 => "🌱",
        4 => "🌸",
        5 => "🐜",
        _ => "🐟"
    };

    public string PreviewThemeColor => SelectedThemeIndex switch
    {
        1 => "#E0F7FA",
        2 => "#EDF7EE",
        3 => "#FFF9E6",
        4 => "#FFF3E0",
        5 => "#F5F5F5",
        _ => "#F0F4FF"
    };

    public string PreviewBorderColor => SelectedThemeIndex switch
    {
        1 => "#80DEEA",
        2 => "#C3E6C7",
        3 => "#FFE082",
        4 => "#FFCC80",
        5 => "#E0E0E0",
        _ => "#C5D4F5"
    };

    public string SpeciesInfo => $"{TankTypeName} • {Volume}л • {SelectedControllerName}";

    public AddAquariumViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private void SelectTankType(int typeIndex)
    {
        SelectedTankType = typeIndex;
        OnPropertyChanged(nameof(TankTypeName));
        OnPropertyChanged(nameof(TankTypeIcon));
    }

    [RelayCommand]
    private void IncrementVolume()
    {
        Volume += 5;
    }

    [RelayCommand]
    private void DecrementVolume()
    {
        if (Volume > 1) Volume -= (Volume > 5 ? 5 : 1);
    }

    [RelayCommand]
    private void ToggleInhabitant(InhabitantChipModel item)
    {
        item.IsSelected = !item.IsSelected;
    }

    [RelayCommand]
    private void OpenCalendar()
    {
        IsCalendarOpen = true;
    }

    [RelayCommand]
    private void CloseCalendar(string? selectedDate = null)
    {
        if (!string.IsNullOrEmpty(selectedDate))
        {
            LaunchDate = selectedDate;
        }
        IsCalendarOpen = false;
    }

    [RelayCommand]
    private void OpenControllerPicker()
    {
        IsControllerPickerOpen = true;
    }

    [RelayCommand]
    private void SelectController(string name)
    {
        SelectedControllerName = name;
        IsControllerPickerOpen = false;
    }

    [RelayCommand]
    private void SelectTheme(int index)
    {
        SelectedThemeIndex = index;
        OnPropertyChanged(nameof(PreviewThemeColor));
        OnPropertyChanged(nameof(PreviewBorderColor));
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep == 2 && string.IsNullOrWhiteSpace(Name))
        {
            Name = $"{TankTypeName} #1";
        }

        if (CurrentStep < 7)
        {
            CurrentStep++;
        }
        else
        {
            _ = SaveAsync();
        }
    }

    [RelayCommand]
    private void PrevStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
        }
        else
        {
            IsCancelDialogOpen = true;
        }
    }

    [RelayCommand]
    private void PromptCancel()
    {
        IsCancelDialogOpen = true;
    }

    [RelayCommand]
    private void ResumeEditing()
    {
        IsCancelDialogOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmCancelAsync()
    {
        IsCancelDialogOpen = false;
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var selectedInhabitants = InhabitantsList
            .Where(x => x.IsSelected)
            .Select(x => x.Title)
            .ToList();

        var speciesInfo = selectedInhabitants.Count > 0
            ? string.Join(", ", selectedInhabitants)
            : "Без обитателей";

        var model = new AquariumModel
        {
            Name = string.IsNullOrWhiteSpace(Name) ? "Мой первый аквариум" : Name.Trim(),
            VolumeLiters = Volume,
            WaterType = SelectedTankType == 1 ? WaterType.Marine : WaterType.Freshwater,
            SpeciesInfo = $"{TankTypeName} • {Volume}л • {SelectedControllerName}",
            ControllerName = SelectedControllerName,
            CardThemeColor = PreviewThemeColor,
            CardBorderColor = PreviewBorderColor
        };

        var created = await _aquariumService.AddAquariumAsync(model);
        CreatedAquariumId = created.Id;
        IsSuccessOpen = true;
    }

    [RelayCommand]
    private async Task GoToEquipmentAsync()
    {
        IsSuccessOpen = false;
        try
        {
            await Shell.Current.GoToAsync($"../{nameof(AquariumDetailsPage)}?aquariumId={CreatedAquariumId}");
        }
        catch
        {
            try
            {
                await Shell.Current.GoToAsync($"///{nameof(AquariumDetailsPage)}?aquariumId={CreatedAquariumId}");
            }
            catch
            {
                await Shell.Current.GoToAsync($"{nameof(AquariumDetailsPage)}?aquariumId={CreatedAquariumId}");
            }
        }
    }

    [RelayCommand]
    private async Task GoToMainAsync()
    {
        IsSuccessOpen = false;
        await Shell.Current.GoToAsync("//MainTabs/DashboardPage");
    }
}

public partial class InhabitantChipModel : ObservableObject
{
    public string Title { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    public InhabitantChipModel(string title, bool isSelected = false)
    {
        Title = title;
        _isSelected = isSelected;
    }
}
