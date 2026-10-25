using System.ComponentModel;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public class LocalizationService : ILocalizationService
{
    private const string LanguagePrefKey = "setting_language";
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    private string _currentLanguage;

    public string CurrentLanguage => _currentLanguage;
    public bool IsEnglish => _currentLanguage.Equals("English", StringComparison.OrdinalIgnoreCase) ||
                             _currentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

    public LocalizationService()
    {
        _instance = this;
        _currentLanguage = Preferences.Default.Get(LanguagePrefKey, "Русский");
    }

    public string this[string key] => GetString(key);

    public void SetLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language)) return;

        var normalized = language.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                         language.Equals("English", StringComparison.OrdinalIgnoreCase)
            ? "English"
            : "Русский";

        if (_currentLanguage != normalized)
        {
            _currentLanguage = normalized;
            Preferences.Default.Set(LanguagePrefKey, normalized);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;

        if (IsEnglish && EnglishDictionary.TryGetValue(key, out var enVal))
        {
            return enVal;
        }

        if (RussianDictionary.TryGetValue(key, out var ruVal))
        {
            return ruVal;
        }

        return key;
    }

    private static readonly Dictionary<string, string> RussianDictionary = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common
        ["Common_Cancel"] = "Отмена",
        ["Common_Save"] = "Сохранить",
        ["Common_Delete"] = "Удалить",
        ["Common_Ok"] = "OK",
        ["Common_Error"] = "Ошибка",
        ["Common_Success"] = "Успешно",
        ["Common_Done"] = "Готово",
        ["Common_Next"] = "Далее",
        ["Common_Back"] = "Назад",
        ["Common_Edit"] = "Редактировать",
        ["Common_Add"] = "+ Добавить",

        // Themes
        ["Theme_Light"] = "Светлая",
        ["Theme_Dark"] = "Тёмная",
        ["Theme_System"] = "Системная",

        // Navigation
        ["Nav_Home"] = "Главная",
        ["Nav_Tanks"] = "Ёмкости",
        ["Nav_Knowledge"] = "База знаний",
        ["Nav_Profile"] = "Профиль",
        ["Nav_Controllers"] = "Контроллеры",

        // App Settings
        ["Settings_Title"] = "Настройки приложения",
        ["Settings_Appearance"] = "Внешний вид",
        ["Settings_Theme"] = "Тема интерфейса",
        ["Settings_Language"] = "Язык приложения",
        ["Settings_Notifications"] = "Уведомления",
        ["Settings_EmergencyAlerts"] = "Аварийные алерты",
        ["Settings_EmergencyAlertsSub"] = "Критические оповещения о параметрах воды (температура, pH, протечки)",
        ["Settings_ConnectionLoss"] = "Потеря связи с хабом",
        ["Settings_ConnectionLossSub"] = "Уведомления, если контроллер потерял связь с сетью или снова вернулся в сеть",
        ["Settings_ScenarioAlerts"] = "Запуск сценариев",
        ["Settings_ScenarioAlertsSub"] = "Короткие пуши о том, что реле успешно включилось по таймеру или датчику",
        ["Settings_TelegramAlerts"] = "Оповещения в Telegram",
        ["Settings_TelegramAlertsSub"] = "Дублирование важных алертов и задач в Telegram-бот",
        ["Settings_TelegramBotBtn"] = "Бот",
        ["Settings_Data"] = "Данные",
        ["Settings_RefreshInterval"] = "Обновление экранов",
        ["Settings_TrafficSaving"] = "Экономия трафика (Wi-Fi)",
        ["Settings_LocalCache"] = "Локальный кэш",
        ["Settings_Clear"] = "Очистить",

        // Security Settings
        ["Security_Title"] = "Безопасность",
        ["Security_AppLogin"] = "Вход в приложение",
        ["Security_FaceId"] = "Использовать Face ID",
        ["Security_Pin"] = "Код-пароль (ПИН)",
        ["Security_AutoLock"] = "Автоблокировка",
        ["Security_Account"] = "Аккаунт",
        ["Security_ChangePassword"] = "Изменить пароль",
        ["Security_2FA"] = "Двухфакторный вход (2FA)",
        ["Security_Management"] = "Управление",
        ["Security_PinForRelay"] = "Запрос PIN при пуске реле",

        // Profile
        ["Profile_Title"] = "Профиль",
        ["Profile_TelegramTitle"] = "Telegram-бот",
        ["Profile_TelegramSub"] = "Уведомления об авариях и напоминания в чат",
        ["Profile_TelegramLogin"] = "Войти",
        ["Profile_MyControllers"] = "Мои контроллеры",
        ["Profile_AppSettings"] = "Настройки приложения",
        ["Profile_DataHistory"] = "Данные и история",
        ["Profile_Security"] = "Безопасность",
        ["Profile_HelpSupport"] = "Помощь и поддержка",
        ["Profile_AboutApp"] = "О приложении",
        ["Profile_Logout"] = "Выйти из приложения",
        ["Profile_DeleteAccount"] = "Удалить аккаунт",

        ["Nav_Login"] = "Вход",

        // Dashboard
        ["Dash_Greeting"] = "Добро пожаловать!",
        ["Dash_AddFirstController"] = "Добавьте первый контроллер",
        ["Dash_ZeroStateDesc"] = "Контроллер соединяет приложение с вашей ёмкостью — добавьте контроллер или создайте ёмкость, чтобы начать",
        ["Dash_ConnectController"] = "Подключить контроллер",
        ["Dash_AddTank"] = "Добавить ёмкость",
        ["Dash_ExploreKb"] = "Изучить базу знаний",
        ["Dash_AllTanks"] = "Все ёмкости",
        ["Dash_Add"] = "+ Добавить",
        ["Dash_RecentEvents"] = "Последние события",
        ["Dash_DeviceControl"] = "Управление устройствами",
        ["Dash_AddRelay"] = "+ Реле",

        // Tanks List
        ["Tanks_Title"] = "Мои ёмкости",
        ["Tanks_EmptyTitle"] = "Ёмкостей пока нет",
        ["Tanks_EmptyDesc"] = "Добавьте ваш аквариум, террариум или флорариум, чтобы отслеживать показатели и автоматизировать уход.",
        ["Tanks_AddFirst"] = "+ Добавить первую ёмкость",
        ["Tanks_Details"] = "Подробнее →",

        // Knowledge Base
        ["Kb_Title"] = "База знаний",
        ["Kb_SearchPlaceholder"] = "Поиск статей",

        // Tank Details
        ["Tank_Sensors"] = "Датчики",
        ["Tank_Relays"] = "Реле",
        ["Tank_Scenarios"] = "Сценарии",
        ["Tank_Journal"] = "Журнал",
        ["Tank_VacationMode"] = "Режим отпуска",
        ["Tank_Configure"] = "Настроить",
        ["Tank_TimersAndSchedules"] = "Таймеры и расписания реле",
        ["Tank_AddTimer"] = "+ Таймер",
        ["Tank_MaintenanceLog"] = "Журнал обслуживания",
        ["Tank_AddRecord"] = "+ Запись",
        ["Tank_WaterChange"] = "💧 + Подмена воды",
        ["Tank_FilterClean"] = "🫧 + Чистка фильтра",
        ["Tank_WaterTests"] = "🧪 + Тесты воды",
        ["Tank_AddFirstSensor"] = "Добавьте первый датчик",
        ["Tank_FirstSensorDesc"] = "Здесь будут отображаться ваши устройства. Добавьте первый датчик, чтобы контролировать параметры системы",
        ["Tank_AddFirstRelay"] = "Добавьте первое реле",
        ["Tank_FirstRelayDesc"] = "Здесь будет список ваших исполнительных устройств. Добавьте первое реле, чтобы взять контроль над оборудованием",
        ["Tank_AddFirstScenario"] = "Добавьте первый сценарий",
        ["Tank_FirstScenarioDesc"] = "Свяжите датчики и реле между собой, чтобы система сама управляла приборами по расписанию или при изменении показателей",
        ["Tank_JournalEmpty"] = "Журнал пуст",
        ["Tank_JournalEmptyDesc"] = "Фиксируйте регулярное обслуживание: подмены воды, чистку фильтров и замеры параметров, чтобы следить за здоровьем экосистемы.",

        // Edit Tank
        ["EditTank_Title"] = "Редактирование ёмкости",
        ["EditTank_Name"] = "Название ёмкости *",
        ["EditTank_NamePlaceholder"] = "Например, Мой аквариум",
        ["EditTank_Volume"] = "Объём (литров) *",
        ["EditTank_VolumePlaceholder"] = "30",
        ["EditTank_Type"] = "Тип ёмкости",
        ["EditTank_Aquarium"] = "Аквариум",
        ["EditTank_Terrarium"] = "Террариум",
        ["EditTank_Growbox"] = "Гроубокс",
        ["EditTank_Florarium"] = "Флорариум",
        ["EditTank_Formicarium"] = "Формикарий",
        ["EditTank_Theme"] = "Цветовая тема карточки",
        ["EditTank_Controller"] = "Привязанный контроллер",
        ["EditTank_SelectController"] = "Выберите контроллер",
        ["EditTank_SaveChanges"] = "Сохранить изменения",
        ["EditTank_DeleteTank"] = "Удалить ёмкость",
        ["EditTank_DeleteConfirmTitle"] = "Удаление ёмкости",
        ["EditTank_DeleteConfirmMsg"] = "Вы уверены, что хотите удалить «{0}»? Это действие нельзя отменить."
    };

    private static readonly Dictionary<string, string> EnglishDictionary = new(StringComparer.OrdinalIgnoreCase)
    {
        // Common
        ["Common_Cancel"] = "Cancel",
        ["Common_Save"] = "Save",
        ["Common_Delete"] = "Delete",
        ["Common_Ok"] = "OK",
        ["Common_Error"] = "Error",
        ["Common_Success"] = "Success",
        ["Common_Done"] = "Done",
        ["Common_Next"] = "Next",
        ["Common_Back"] = "Back",
        ["Common_Edit"] = "Edit",
        ["Common_Add"] = "+ Add",

        // Themes
        ["Theme_Light"] = "Light",
        ["Theme_Dark"] = "Dark",
        ["Theme_System"] = "System",

        // Navigation
        ["Nav_Home"] = "Home",
        ["Nav_Tanks"] = "Tanks",
        ["Nav_Knowledge"] = "Knowledge",
        ["Nav_Profile"] = "Profile",
        ["Nav_Controllers"] = "Controllers",

        // App Settings
        ["Settings_Title"] = "App Settings",
        ["Settings_Appearance"] = "Appearance",
        ["Settings_Theme"] = "Interface Theme",
        ["Settings_Language"] = "App Language",
        ["Settings_Notifications"] = "Notifications",
        ["Settings_EmergencyAlerts"] = "Emergency Alerts",
        ["Settings_EmergencyAlertsSub"] = "Critical alerts for water metrics (temperature, pH, leaks)",
        ["Settings_ConnectionLoss"] = "Hub Connection Loss",
        ["Settings_ConnectionLossSub"] = "Alerts if a controller loses network connection or reconnects",
        ["Settings_ScenarioAlerts"] = "Scenario Triggers",
        ["Settings_ScenarioAlertsSub"] = "Short push alerts when relays trigger via schedule or sensor",
        ["Settings_TelegramAlerts"] = "Telegram Alerts",
        ["Settings_TelegramAlertsSub"] = "Forward critical alerts and tasks to your Telegram bot",
        ["Settings_TelegramBotBtn"] = "Bot",
        ["Settings_Data"] = "Data & Sync",
        ["Settings_RefreshInterval"] = "Screen Auto-Refresh",
        ["Settings_TrafficSaving"] = "Data Saver (Wi-Fi)",
        ["Settings_LocalCache"] = "Local Cache",
        ["Settings_Clear"] = "Clear",

        // Security Settings
        ["Security_Title"] = "Security",
        ["Security_AppLogin"] = "App Login",
        ["Security_FaceId"] = "Use Face ID / Touch ID",
        ["Security_Pin"] = "Passcode (PIN)",
        ["Security_AutoLock"] = "Auto-Lock",
        ["Security_Account"] = "Account",
        ["Security_ChangePassword"] = "Change Password",
        ["Security_2FA"] = "Two-Factor Auth (2FA)",
        ["Security_Management"] = "Control",
        ["Security_PinForRelay"] = "Require PIN to switch relay",

        // Profile
        ["Profile_Title"] = "Profile",
        ["Profile_TelegramTitle"] = "Telegram Bot",
        ["Profile_TelegramSub"] = "Emergency alerts & task reminders in chat",
        ["Profile_TelegramLogin"] = "Connect",
        ["Profile_MyControllers"] = "My Controllers",
        ["Profile_AppSettings"] = "App Settings",
        ["Profile_DataHistory"] = "Data & History",
        ["Profile_Security"] = "Security",
        ["Profile_HelpSupport"] = "Help & Support",
        ["Profile_AboutApp"] = "About App",
        ["Profile_Logout"] = "Log Out",
        ["Profile_DeleteAccount"] = "Delete Account",

        ["Nav_Login"] = "Sign In",

        // Dashboard
        ["Dash_Greeting"] = "Welcome!",
        ["Dash_AddFirstController"] = "Add your first controller",
        ["Dash_ZeroStateDesc"] = "The controller connects the app to your tank — add a controller or create a tank to get started",
        ["Dash_ConnectController"] = "Connect Controller",
        ["Dash_AddTank"] = "Add Tank",
        ["Dash_ExploreKb"] = "Explore Knowledge Base",
        ["Dash_AllTanks"] = "All Tanks",
        ["Dash_Add"] = "+ Add",
        ["Dash_RecentEvents"] = "Recent Events",
        ["Dash_DeviceControl"] = "Device Controls",
        ["Dash_AddRelay"] = "+ Relay",

        // Tanks List
        ["Tanks_Title"] = "My Tanks",
        ["Tanks_EmptyTitle"] = "No tanks yet",
        ["Tanks_EmptyDesc"] = "Add your aquarium, terrarium, or florarium to track metrics and automate care.",
        ["Tanks_AddFirst"] = "+ Add first tank",
        ["Tanks_Details"] = "Details →",

        // Knowledge Base
        ["Kb_Title"] = "Knowledge Base",
        ["Kb_SearchPlaceholder"] = "Search articles",

        // Tank Details
        ["Tank_Sensors"] = "Sensors",
        ["Tank_Relays"] = "Relays",
        ["Tank_Scenarios"] = "Scenarios",
        ["Tank_Journal"] = "Logbook",
        ["Tank_VacationMode"] = "Vacation Mode",
        ["Tank_Configure"] = "Configure",
        ["Tank_TimersAndSchedules"] = "Relay Timers & Schedules",
        ["Tank_AddTimer"] = "+ Timer",
        ["Tank_MaintenanceLog"] = "Maintenance Logbook",
        ["Tank_AddRecord"] = "+ Log Entry",
        ["Tank_WaterChange"] = "💧 + Water Change",
        ["Tank_FilterClean"] = "🫧 + Clean Filter",
        ["Tank_WaterTests"] = "🧪 + Water Tests",
        ["Tank_AddFirstSensor"] = "Add first sensor",
        ["Tank_FirstSensorDesc"] = "Your devices will appear here. Add your first sensor to monitor water parameters",
        ["Tank_AddFirstRelay"] = "Add first relay",
        ["Tank_FirstRelayDesc"] = "Your actuators will appear here. Add your first relay to take control of your equipment",
        ["Tank_AddFirstScenario"] = "Add first scenario",
        ["Tank_FirstScenarioDesc"] = "Link sensors and relays together so the system automatically triggers devices on schedule or thresholds",
        ["Tank_JournalEmpty"] = "Logbook is empty",
        ["Tank_JournalEmptyDesc"] = "Log regular maintenance: water changes, filter cleaning, and chemical tests to keep your ecosystem healthy.",

        // Edit Tank
        ["EditTank_Title"] = "Edit Tank",
        ["EditTank_Name"] = "Tank name *",
        ["EditTank_NamePlaceholder"] = "e.g., My Aquarium",
        ["EditTank_Volume"] = "Volume (liters) *",
        ["EditTank_VolumePlaceholder"] = "30",
        ["EditTank_Type"] = "Tank type",
        ["EditTank_Aquarium"] = "Aquarium",
        ["EditTank_Terrarium"] = "Terrarium",
        ["EditTank_Growbox"] = "Growbox",
        ["EditTank_Florarium"] = "Florarium",
        ["EditTank_Formicarium"] = "Formicarium",
        ["EditTank_Theme"] = "Card color theme",
        ["EditTank_Controller"] = "Assigned controller",
        ["EditTank_SelectController"] = "Select controller",
        ["EditTank_SaveChanges"] = "Save changes",
        ["EditTank_DeleteTank"] = "Delete tank",
        ["EditTank_DeleteConfirmTitle"] = "Delete Tank",
        ["EditTank_DeleteConfirmMsg"] = "Are you sure you want to delete \"{0}\"? This action cannot be undone."
    };
}
