using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class AddAquariumViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _volumeString = "200";

    [ObservableProperty]
    private int _waterTypeIndex = 0; // 0: Freshwater, 1: Marine, 2: Brackish

    [ObservableProperty]
    private string _species = string.Empty;

    [ObservableProperty]
    private string _dimensions = string.Empty;

    public AddAquariumViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Внимание", "Укажите название ёмкости", "OK");
            return;
        }

        if (!Helpers.ValidationHelper.ValidateAquariumVolume(VolumeString, out var volume, out var volError))
        {
            await Shell.Current.DisplayAlert("Внимание", volError, "OK");
            return;
        }

        var model = new AquariumModel
        {
            Name = Name,
            VolumeLiters = volume,
            WaterType = WaterTypeIndex switch
            {
                1 => WaterType.Marine,
                2 => WaterType.Brackish,
                _ => WaterType.Freshwater
            },
            SpeciesInfo = Species,
            Dimensions = string.IsNullOrWhiteSpace(Dimensions) ? "Стандартная" : Dimensions
        };

        await _aquariumService.AddAquariumAsync(model);
        await Shell.Current.DisplayAlert("Успешно", $"Ёмкость «{Name}» добавлена в систему!", "OK");
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
