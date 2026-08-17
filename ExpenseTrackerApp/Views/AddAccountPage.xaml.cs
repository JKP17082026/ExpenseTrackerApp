using ExpenseTrackerApp.ViewModels;

namespace ExpenseTrackerApp.Views;

public partial class AddAccountPage : ContentPage
{
    private readonly AddAccountViewModel _vm;

    public AddAccountPage(AddAccountViewModel vm)
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
