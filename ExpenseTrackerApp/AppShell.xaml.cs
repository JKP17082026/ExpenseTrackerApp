namespace ExpenseTrackerApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Views.AddAccountPage), typeof(Views.AddAccountPage));
        Routing.RegisterRoute(nameof(Views.CreditCardDetailPage), typeof(Views.CreditCardDetailPage));
        Routing.RegisterRoute(nameof(Views.AddInstallmentPage), typeof(Views.AddInstallmentPage));
        Routing.RegisterRoute(nameof(Views.InstallmentDetailPage), typeof(Views.InstallmentDetailPage));
        Routing.RegisterRoute(nameof(Views.CategoryPickerPage), typeof(Views.CategoryPickerPage));
        Routing.RegisterRoute(nameof(Views.ReceiptScanPage), typeof(Views.ReceiptScanPage));
        Routing.RegisterRoute(nameof(Views.NetWorthPage), typeof(Views.NetWorthPage));
    }
}
