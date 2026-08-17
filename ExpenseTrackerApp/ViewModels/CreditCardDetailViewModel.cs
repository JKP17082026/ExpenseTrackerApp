using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

public partial class CreditCardDetailViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private readonly InterestCalculatorService _interest;

    [ObservableProperty] private Account? account;
    [ObservableProperty] private DateTime cycleStart;
    [ObservableProperty] private DateTime cycleEnd;
    [ObservableProperty] private DateTime paymentDueDate;
    [ObservableProperty] private decimal statementBalance;
    [ObservableProperty] private decimal minimumPayment;
    [ObservableProperty] private decimal estimatedInterestIfMinimum;
    [ObservableProperty] private double cycleProgressPercent;

    public CreditCardDetailViewModel(IDatabaseService db, InterestCalculatorService interest)
    {
        _db = db;
        _interest = interest;
        Title = "รายละเอียดบัตรเครดิต";
    }

    [RelayCommand]
    public async Task LoadAsync(int accountId)
    {
        Account = await _db.GetAccountAsync(accountId);
        if (Account is null || !Account.IsCreditCard) return;

        var today = DateTime.Now;
        (CycleStart, CycleEnd) = _interest.GetCurrentBillingCycle(Account, today);
        PaymentDueDate = _interest.GetPaymentDueDate(Account, CycleEnd);

        var allTransactions = await _db.GetTransactionsAsync();
        var accountTransactions = allTransactions.Where(t => t.AccountId == Account.Id).ToList();

        StatementBalance = _interest.GetCycleSpending(accountTransactions, CycleStart, CycleEnd);
        MinimumPayment = _interest.GetMinimumPayment(StatementBalance);

        // หน้าสร้างกระเป๋าบัตรเครดิตไม่ได้เก็บอัตราดอกเบี้ยไว้ (ตามภาพตัวอย่าง) จึงใช้ค่าเฉลี่ยตามกฎหมายไทยเป็นค่าประมาณ
        // TODO: ถ้าต้องการความแม่นยำ ให้เพิ่มฟิลด์ AnnualInterestRate ใน Account สำหรับ CreditCard ในเวอร์ชันถัดไป
        const decimal assumedCreditCardApr = 16m;

        var daysUntilDue = Math.Max((PaymentDueDate - today).Days, 0);
        EstimatedInterestIfMinimum = _interest.EstimateInterestIfMinimumPaid(
            StatementBalance, MinimumPayment, assumedCreditCardApr, daysUntilDue);

        var totalCycleDays = (CycleEnd - CycleStart).Days;
        var elapsedDays = (today - CycleStart).Days;
        CycleProgressPercent = totalCycleDays > 0 ? Math.Clamp((double)elapsedDays / totalCycleDays * 100, 0, 100) : 0;
    }
}
