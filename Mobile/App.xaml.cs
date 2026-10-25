namespace Mobile;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        MainPage = serviceProvider.GetRequiredService<AppShell>();
    }

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
