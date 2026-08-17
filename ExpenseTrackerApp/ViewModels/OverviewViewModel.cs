using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

// หน้าสรุปรายงาน: กราฟโดนัทแยกตามหมวดหมู่ + ตารางสัดส่วน
public partial class OverviewViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;

    [ObservableProperty] private decimal totalExpense;
    [ObservableProperty] private decimal totalIncome;
    [ObservableProperty] private decimal totalTransfer;
    [ObservableProperty] private DateTime selectedMonth = DateTime.Now;

    public ObservableCollection<CategorySummary> CategorySummaries { get; } = new();

    public OverviewViewModel(IDatabaseService db)
    {
        _db = db;
        Title = "สรุปรายงาน";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var from = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var to = from.AddMonths(1).AddDays(-1);

            var transactions = await _db.GetTransactionsAsync(from, to);
            var categories = await _db.GetCategoriesAsync();

            TotalExpense = transactions.Where(t => t.Type == CategoryType.Expense).Sum(t => t.Amount);
            TotalIncome = transactions.Where(t => t.Type == CategoryType.Income).Sum(t => t.Amount);
            TotalTransfer = transactions.Where(t => t.Type == CategoryType.Transfer).Sum(t => t.Amount);

            CategorySummaries.Clear();
            var grouped = transactions
                .Where(t => t.Type == CategoryType.Expense)
                .GroupBy(t => t.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(g => g.Amount);

            foreach (var g in grouped)
            {
                var category = categories.FirstOrDefault(c => c.Id == g.CategoryId);
                if (category is null) continue;

                CategorySummaries.Add(new CategorySummary
                {
                    Name = category.Name,
                    Icon = category.Icon,
                    ColorHex = category.ColorHex,
                    Amount = g.Amount,
                    Percent = TotalExpense > 0 ? Math.Round((double)(g.Amount / TotalExpense) * 100, 0) : 0
                });
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class CategorySummary
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percent { get; set; }
}
