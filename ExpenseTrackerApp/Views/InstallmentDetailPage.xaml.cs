using ExpenseTrackerApp.ViewModels;

namespace ExpenseTrackerApp.Views;

[QueryProperty(nameof(PlanId), "planId")]
public partial class InstallmentDetailPage : ContentPage
{
    private readonly InstallmentDetailViewModel _vm;
    public int PlanId { get; set; }

    public InstallmentDetailPage(InstallmentDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync(PlanId);
    }
}
