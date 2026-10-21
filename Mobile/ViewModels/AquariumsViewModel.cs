using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mobile.ViewModels;

public partial class AquariumsViewModel : ObservableObject
{
    private readonly IAquariumService _aquariumService;

    [ObservableProperty]
    private ObservableCollection<AquariumModel> _aquariums = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAquariums))]
    private bool _hasNoAquariums;

    public bool HasAquariums => !HasNoAquariums;

    [ObservableProperty]
    private bool _isBusy;

    public AquariumsViewModel(IAquariumService aquariumService)
    {
        _aquariumService = aquariumService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        var list = await _aquariumService.GetAquariumsAsync();
        Aquariums.Clear();
        foreach (var a in list) Aquariums.Add(a);
        HasNoAquariums = Aquariums.Count == 0;
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SelectAquariumAsync(AquariumModel aquarium)
    {
        await Shell.Current.GoToAsync($"AquariumDetailsPage?aquariumId={aquarium.Id}");
    }

    [RelayCommand]
    private async Task AddAquariumAsync()
    {
        await Shell.Current.GoToAsync("AddAquariumPage");
    }
}
