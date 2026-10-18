namespace Mobile;

public partial class App : Application
{
    public App(AppShell appShell)
    {
        InitializeComponent();
        MainPage = appShell;
    }

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
