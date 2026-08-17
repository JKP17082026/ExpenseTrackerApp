using ExpenseTrackerApp.ViewModels;

namespace ExpenseTrackerApp.Views;

public partial class AddInstallmentPage : ContentPage
{
    private readonly AddInstallmentViewModel _vm;

    public AddInstallmentPage(AddInstallmentViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadOptionsAsync();
    }
}
