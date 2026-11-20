using System.ComponentModel;

namespace Mobile.Services.Interfaces;

public interface ILocalizationService : INotifyPropertyChanged
{
    string CurrentLanguage { get; }
    bool IsEnglish { get; }
    string this[string key] { get; }
    void SetLanguage(string language);
    string GetString(string key);
    event EventHandler? LanguageChanged;
}
