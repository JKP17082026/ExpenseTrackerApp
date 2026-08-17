using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

public partial class AddTransactionViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;
    private readonly IGoogleSheetsService _sheets;
    private readonly IReceiptOcrService _ocr;

    [ObservableProperty] private CategoryType type = CategoryType.Expense;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private decimal? discountAmount;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private Account? selectedAccount;
    [ObservableProperty] private Tag? selectedTag;
    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private string? note;
    [ObservableProperty] private string? receiptImagePath;

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Account> Accounts { get; } = new();

    public AddTransactionViewModel(IDatabaseService db, IGoogleSheetsService sheets, IReceiptOcrService ocr)
    {
        _db = db;
        _sheets = sheets;
        _ocr = ocr;
        Title = "รายการใหม่";
    }

    [RelayCommand]
    public async Task LoadOptionsAsync()
    {
        Categories.Clear();
        foreach (var c in await _db.GetCategoriesAsync(Type))
            Categories.Add(c);

        Accounts.Clear();
        foreach (var a in await _db.GetAccountsAsync())
            Accounts.Add(a);
    }

    [RelayCommand]
    public async Task ScanReceiptAsync(string imagePath)
    {
        var result = await _ocr.ScanReceiptAsync(imagePath);
        if (!result.Success) return;

        if (result.Amount.HasValue) Amount = result.Amount.Value;
        if (result.Date.HasValue) Date = result.Date.Value;
        ReceiptImagePath = imagePath;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (SelectedCategory is null || SelectedAccount is null || Amount <= 0)
            return; // TODO: แสดง validation error ในหน้า UI จริง

        var transaction = new Transaction
        {
            Amount = Amount,
            DiscountAmount = DiscountAmount,
            Type = Type,
            CategoryId = SelectedCategory.Id,
            AccountId = SelectedAccount.Id,
            TagId = SelectedTag?.Id,
            Date = Date,
            Note = Note,
            ReceiptImagePath = ReceiptImagePath
        };

        await _db.SaveTransactionAsync(transaction);

        if (_sheets.IsSignedIn)
            await _sheets.PushTransactionAsync(transaction);
    }
}
