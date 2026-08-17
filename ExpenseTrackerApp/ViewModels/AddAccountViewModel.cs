using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTrackerApp.Models;
using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.ViewModels;

// หน้า "สร้างกระเป๋ากันเลย" — ตรงกับภาพตัวอย่าง: เลือกไอคอน, สี, ประเภท 6 แบบ
// บัตรเครดิตมีตัวเลือกวงเงินร่วม/แยก และรูปแบบรอบบิล
public partial class AddAccountViewModel : BaseViewModel
{
    private readonly IDatabaseService _db;

    // ไอคอนที่มากับแอปให้เลือกตามประเภท (ผู้ใช้อัปโหลดรูปเองแทนได้ผ่าน ImagePath)
    public static readonly Dictionary<AccountType, string> DefaultIcons = new()
    {
        { AccountType.Cash, "💵" },
        { AccountType.Bank, "🏦" },
        { AccountType.CreditCard, "💳" },
        { AccountType.Savings, "🐷" },
    };

    public static readonly List<string> PaletteColors = new()
    {
        "#FAE0A0", "#F4A0A8", "#8FBEEA", "#A8D6A0", "#9A9AA5", "#F4E29A", "#D6B8EA", "#9EDCB0", "#C6E0A0", "#9CD9D0"
    };

    [ObservableProperty] private AccountType selectedType = AccountType.Cash;
    [ObservableProperty] private string name = "เงินสด";
    [ObservableProperty] private string iconEmoji = "💵";
    [ObservableProperty] private string? imagePath;
    [ObservableProperty] private string colorHex = "#FAE0A0";

    // เงินสด / ธนาคาร / เงินออม / เงินลงทุน / เงินดิจิทัล
    [ObservableProperty] private decimal openingBalance;

    // บัตรเครดิตเท่านั้น
    [ObservableProperty] private decimal? creditLimitRemaining;
    [ObservableProperty] private decimal? creditLimitMax;
    [ObservableProperty] private bool useSharedCreditLimit;
    [ObservableProperty] private Account? sharedCreditLimitGroupAccount;
    [ObservableProperty] private int statementDay = 5;
    [ObservableProperty] private int paymentDueDay = 20;
    [ObservableProperty] private StatementCycleStyle cycleStyle = StatementCycleStyle.StatementDayStartsNewCycle;

    public ObservableCollection<Account> ExistingCreditCards { get; } = new();

    public bool IsCreditCardType => SelectedType == AccountType.CreditCard;

    public AddAccountViewModel(IDatabaseService db)
    {
        _db = db;
        Title = "สร้างกระเป๋ากันเลย";
    }

    [RelayCommand]
    public async Task LoadOptionsAsync()
    {
        ExistingCreditCards.Clear();
        var accounts = await _db.GetAccountsAsync();
        foreach (var a in accounts.Where(a => a.IsCreditCard))
            ExistingCreditCards.Add(a);
    }

    partial void OnSelectedTypeChanged(AccountType value)
    {
        IconEmoji = DefaultIcons.TryGetValue(value, out var icon) ? icon : "👛";
        OnPropertyChanged(nameof(IsCreditCardType));
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return; // TODO: แสดง validation error ในหน้า UI จริง

        var account = new Account
        {
            Name = Name,
            Type = SelectedType,
            IconEmoji = IconEmoji,
            ImagePath = ImagePath,
            ColorHex = ColorHex
        };

        if (SelectedType == AccountType.CreditCard)
        {
            account.CreditLimitRemaining = CreditLimitRemaining;
            account.CreditLimitMax = CreditLimitMax;
            account.UseSharedCreditLimit = UseSharedCreditLimit;
            account.SharedCreditLimitGroupId = UseSharedCreditLimit ? SharedCreditLimitGroupAccount?.Id : null;
            account.StatementDay = StatementDay;
            account.PaymentDueDay = PaymentDueDay;
            account.CycleStyle = CycleStyle;
        }
        else
        {
            account.OpeningBalance = OpeningBalance;
        }

        await _db.SaveAccountAsync(account);
    }
}
