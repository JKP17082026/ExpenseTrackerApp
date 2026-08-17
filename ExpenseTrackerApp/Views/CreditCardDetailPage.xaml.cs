using ExpenseTrackerApp.ViewModels;

namespace ExpenseTrackerApp.Views;

[QueryProperty(nameof(AccountId), "accountId")]
public partial class CreditCardDetailPage : ContentPage
{
    private readonly CreditCardDetailViewModel _vm;
    public int AccountId { get; set; }

    public CreditCardDetailPage(CreditCardDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync(AccountId);
    }
}
