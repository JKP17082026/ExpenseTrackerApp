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

    protected override void OnStart()
    {
        base.OnStart();

        // Initialize database แบบ async หลังจาก scene ถูกสร้างเสร็จแล้ว
        // ป้องกันปัญหา iOS watchdog timeout / deadlock ที่เคยเกิดตอน initialize
        // แบบ blocking (.GetAwaiter().GetResult()) ใน MauiProgram.cs
        _ = InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            var db = Handler?.MauiContext?.Services.GetService<Services.IDatabaseService>();
            if (db is not null)
            {
                await db.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            // TODO: ใส่ logging หรือแจ้งผู้ใช้ถ้า initialize database ไม่สำเร็จ
            System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex}");
        }
    }
}
