using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

public partial class InstallmentDetailViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private readonly InterestCalculatorService _interest;

    [ObservableProperty] private InstallmentPlan? plan;
    [ObservableProperty] private decimal totalInterestPaid;
    [ObservableProperty] private decimal remainingInterest;
    [ObservableProperty] private decimal estimatedMonthlyPayment;

    public InstallmentDetailViewModel(IDatabaseService db, InterestCalculatorService interest)
    {
        _db = db;
        _interest = interest;
        Title = "รายละเอียดผ่อนชำระ";
    }

    [RelayCommand]
    public async Task LoadAsync(int planId)
    {
        Plan = await _db.GetInstallmentPlanAsync(planId);
        if (Plan is null) return;

        TotalInterestPaid = _interest.GetTotalInterestPaid(Plan);
        RemainingInterest = _interest.GetRemainingInterest(Plan);
        EstimatedMonthlyPayment = _interest.EstimateMonthlyPayment(Plan);
    }

    [RelayCommand]
    public async Task MarkInstallmentPaidAsync()
    {
        if (Plan is null) return;

        Plan.InstallmentsPaid += 1;
        Plan.AmountPaidSoFar += EstimatedMonthlyPayment;
        await _db.SaveInstallmentPlanAsync(Plan);
        await LoadAsync(Plan.Id);
    }
}
