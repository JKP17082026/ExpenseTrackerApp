using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

// หน้า "เพิ่มผ่อนชำระ" — ตรงกับภาพตัวอย่างที่มีหมวดหมู่, เลือกดอกเบี้ยคงที่/ลดต้นลดดอก,
// และตัวเลือกพิเศษเมื่อเจ้าหนี้เป็นบัตรเครดิต
public partial class AddInstallmentViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private readonly InterestCalculatorService _interest;

    [ObservableProperty] private string iconEmoji = "🏠";
    [ObservableProperty] private InstallmentCategory selectedCategory = InstallmentCategory.Housing;

    [ObservableProperty] private string name = string.Empty;         // ชื่อรายการ
    [ObservableProperty] private string creditorName = string.Empty;  // เจ้าหนี้

    [ObservableProperty] private InterestCalculationMethod interestMethod = InterestCalculationMethod.Fixed;

    [ObservableProperty] private decimal principalTotal;
    [ObservableProperty] private decimal amountPaidSoFar;
    [ObservableProperty] private int totalInstallments;
    [ObservableProperty] private int installmentsPaid;
    [ObservableProperty] private decimal annualInterestRate;

    [ObservableProperty] private DateTime startDate = DateTime.Now;
    [ObservableProperty] private DateTime? expectedPayoffDate;
    [ObservableProperty] private int paymentDueDayOfMonth = 5;

    [ObservableProperty] private InstallmentCountingMethod countingMethod = InstallmentCountingMethod.InstallmentLeadsActualAmount;

    // ตัวเลือกพิเศษเมื่อ SelectedCategory == CreditCard
    [ObservableProperty] private Account? linkedCreditCard;
    [ObservableProperty] private bool deductFromCreditCardOnCreate = true;
    [ObservableProperty] private bool hideFromNetWorth = true;

    [ObservableProperty] private string? memoNote;

    [ObservableProperty] private decimal estimatedMonthlyPayment;

    public ObservableCollection<Account> CreditCardAccounts { get; } = new();

    public bool ShowCreditCardOptions => SelectedCategory == InstallmentCategory.CreditCard;

    public AddInstallmentViewModel(IDatabaseService db, InterestCalculatorService interest)
    {
        _db = db;
        _interest = interest;
        Title = "เพิ่มผ่อนชำระ";
    }

    [RelayCommand]
    public async Task LoadOptionsAsync()
    {
        CreditCardAccounts.Clear();
        var accounts = await _db.GetAccountsAsync();
        foreach (var a in accounts.Where(a => a.IsCreditCard))
            CreditCardAccounts.Add(a);
    }

    partial void OnSelectedCategoryChanged(InstallmentCategory value) => OnPropertyChanged(nameof(ShowCreditCardOptions));

    [RelayCommand]
    public void RecalculateEstimate()
    {
        var draft = BuildPlan();
        EstimatedMonthlyPayment = _interest.EstimateMonthlyPayment(draft);
    }

    private InstallmentPlan BuildPlan() => new()
    {
        IconEmoji = IconEmoji,
        Category = SelectedCategory,
        Name = Name,
        CreditorName = CreditorName,
        InterestMethod = InterestMethod,
        PrincipalTotal = PrincipalTotal,
        AmountPaidSoFar = AmountPaidSoFar,
        TotalInstallments = TotalInstallments,
        InstallmentsPaid = InstallmentsPaid,
        AnnualInterestRate = AnnualInterestRate,
        StartDate = StartDate,
        ExpectedPayoffDate = ExpectedPayoffDate,
        PaymentDueDayOfMonth = PaymentDueDayOfMonth,
        CountingMethod = CountingMethod,
        LinkedCreditCardAccountId = ShowCreditCardOptions ? LinkedCreditCard?.Id : null,
        DeductFromCreditCardOnCreate = ShowCreditCardOptions && DeductFromCreditCardOnCreate,
        HideFromNetWorth = ShowCreditCardOptions && HideFromNetWorth,
        MemoNote = MemoNote
    };

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || PrincipalTotal <= 0 || TotalInstallments <= 0)
            return; // TODO: แสดง validation error ในหน้า UI จริง

        var plan = BuildPlan();
        await _db.SaveInstallmentPlanAsync(plan);
    }
}
