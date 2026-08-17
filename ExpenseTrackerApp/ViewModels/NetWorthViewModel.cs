using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

// ทรัพย์สินสุทธิ = รวมทรัพย์สิน (เงินสด+ธนาคาร+ออม+ลงทุน+ดิจิทัล) - รวมหนี้สิน (InstallmentPlan ทั้งหมดที่ไม่ถูกซ่อน)
public partial class NetWorthViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;

    [ObservableProperty] private decimal netWorth;
    [ObservableProperty] private decimal totalAssets;
    [ObservableProperty] private decimal totalLiabilities;

    public ObservableCollection<AccountLine> Assets { get; } = new();
    public ObservableCollection<AccountLine> Liabilities { get; } = new();

    public NetWorthViewModel(IDatabaseService db)
    {
        _db = db;
        Title = "ทรัพย์สินสุทธิ";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var accounts = await _db.GetAccountsAsync();
        Assets.Clear();
        Liabilities.Clear();

        foreach (var account in accounts)
        {
            if (account.IsCreditCard)
                continue; // ยอดใช้บัตรเครดิตนับผ่าน InstallmentPlan ที่ผูกไว้ (กันนับซ้ำ)

            var balance = await _db.GetCashAccountBalanceAsync(account.Id);
            Assets.Add(new AccountLine { Name = account.Name, Amount = balance });
        }

        var installments = await _db.GetInstallmentPlansAsync();
        foreach (var plan in installments)
        {
            if (plan.HideFromNetWorth) continue; // ผู้ใช้เลือกซ่อน เช่น กันนับซ้ำกับยอดบัตรเครดิต
            Liabilities.Add(new AccountLine { Name = plan.Name, Amount = plan.RemainingBalance });
        }

        TotalAssets = Assets.Sum(a => a.Amount);
        TotalLiabilities = Liabilities.Sum(l => l.Amount);
        NetWorth = TotalAssets - TotalLiabilities;
    }
}

public class AccountLine
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
