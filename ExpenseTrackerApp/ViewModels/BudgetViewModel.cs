using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

public partial class BudgetViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;

    [ObservableProperty] private int year = DateTime.Now.Year;
    [ObservableProperty] private int month = DateTime.Now.Month;

    public ObservableCollection<BudgetLine> Lines { get; } = new();

    public BudgetViewModel(IDatabaseService db)
    {
        _db = db;
        Title = "งบประมาณ";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var plans = await _db.GetBudgetPlansAsync(Year, Month);
        var categories = await _db.GetCategoriesAsync(CategoryType.Expense);
        var from = new DateTime(Year, Month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var transactions = await _db.GetTransactionsAsync(from, to);

        Lines.Clear();
        foreach (var plan in plans)
        {
            var category = categories.FirstOrDefault(c => c.Id == plan.CategoryId);
            if (category is null) continue;

            var spent = transactions.Where(t => t.CategoryId == plan.CategoryId && t.Type == CategoryType.Expense).Sum(t => t.Amount);

            Lines.Add(new BudgetLine
            {
                CategoryName = category.Name,
                Icon = category.Icon,
                Limit = plan.MonthlyLimit,
                Spent = spent,
                PercentUsed = plan.MonthlyLimit > 0 ? Math.Min((double)(spent / plan.MonthlyLimit) * 100, 100) : 0
            });
        }
    }

    [RelayCommand]
    public async Task SetBudgetAsync((int categoryId, decimal limit) input)
    {
        var existing = (await _db.GetBudgetPlansAsync(Year, Month)).FirstOrDefault(p => p.CategoryId == input.categoryId);
        var plan = existing ?? new BudgetPlan { CategoryId = input.categoryId, Year = Year, Month = Month };
        plan.MonthlyLimit = input.limit;
        await _db.SaveBudgetPlanAsync(plan);
        await LoadAsync();
    }
}

public class BudgetLine
{
    public string CategoryName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public double PercentUsed { get; set; }
}
