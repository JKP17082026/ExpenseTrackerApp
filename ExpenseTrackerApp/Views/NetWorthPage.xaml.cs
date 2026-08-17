using ExpenseTrackerApp.ViewModels;

namespace ExpenseTrackerApp.Views;

public partial class NetWorthPage : ContentPage
{
    private readonly NetWorthViewModel _vm;

    public NetWorthPage(NetWorthViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
