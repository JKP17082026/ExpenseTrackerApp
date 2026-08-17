using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

public partial class AccountsViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;

    public ObservableCollection<Account> CashAccounts { get; } = new();
    public ObservableCollection<Account> CreditAccounts { get; } = new();
    public ObservableCollection<Models.InstallmentPlan> InstallmentPlans { get; } = new();

    public AccountsViewModel(IDatabaseService db)
    {
        _db = db;
        Title = "กระเป๋าเงิน";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var accounts = await _db.GetAccountsAsync();
        CashAccounts.Clear();
        CreditAccounts.Clear();

        foreach (var a in accounts)
        {
            if (a.IsCreditCard) CreditAccounts.Add(a);
            else CashAccounts.Add(a);
        }

        InstallmentPlans.Clear();
        foreach (var p in await _db.GetInstallmentPlansAsync())
            InstallmentPlans.Add(p);
    }
}
