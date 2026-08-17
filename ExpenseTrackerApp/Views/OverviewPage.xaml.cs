using ExpenseTrackerApp.ViewModels;

namespace ExpenseTrackerApp.Views;

public partial class OverviewPage : ContentPage
{
    private readonly OverviewViewModel _vm;

    public OverviewPage(OverviewViewModel vm)
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
