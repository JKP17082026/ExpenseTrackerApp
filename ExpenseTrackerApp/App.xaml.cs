namespace ExpenseTrackerApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    // MAUI 9 เลิกใช้ Application.MainPage แล้ว ให้กำหนดหน้าตั้งต้นผ่าน CreateWindow แทน
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
