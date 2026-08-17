using ExpenseTrackerApp.Services;
using ExpenseTrackerApp.ViewModels;
using ExpenseTrackerApp.Views;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ----- Services (Singleton: ใช้ instance เดียวทั้งแอป) -----
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IGoogleSheetsService, GoogleSheetsService>();
        builder.Services.AddSingleton<IReceiptOcrService, ReceiptOcrService>();
        builder.Services.AddSingleton<InterestCalculatorService>();

        // ----- ViewModels (Transient: สร้างใหม่ทุกครั้งที่เปิดหน้า) -----
        builder.Services.AddTransient<OverviewViewModel>();
        builder.Services.AddTransient<AddTransactionViewModel>();
        builder.Services.AddTransient<AccountsViewModel>();
        builder.Services.AddTransient<AddAccountViewModel>();
        builder.Services.AddTransient<CreditCardDetailViewModel>();
        builder.Services.AddTransient<AddInstallmentViewModel>();
        builder.Services.AddTransient<InstallmentDetailViewModel>();
        builder.Services.AddTransient<NetWorthViewModel>();
        builder.Services.AddTransient<BudgetViewModel>();

        // ----- Pages -----
        builder.Services.AddTransient<OverviewPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<AddTransactionPage>();
        builder.Services.AddTransient<AccountsPage>();
        builder.Services.AddTransient<AddAccountPage>();
        builder.Services.AddTransient<CreditCardDetailPage>();
        builder.Services.AddTransient<AddInstallmentPage>();
        builder.Services.AddTransient<InstallmentDetailPage>();
        builder.Services.AddTransient<BudgetPage>();
        builder.Services.AddTransient<NetWorthPage>();
        builder.Services.AddTransient<CategoryPickerPage>();
        builder.Services.AddTransient<ReceiptScanPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // เตรียมฐานข้อมูลตอนเปิดแอปครั้งแรก
        var db = app.Services.GetRequiredService<IDatabaseService>();
        db.InitializeAsync().GetAwaiter().GetResult();

        return app;
    }
}
