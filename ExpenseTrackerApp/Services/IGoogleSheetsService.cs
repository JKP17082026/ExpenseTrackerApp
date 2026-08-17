using ExpenseTrackerApp.Models;

namespace ExpenseTrackerApp.Services;

public interface IGoogleSheetsService
{
    Task<bool> SignInAsync();
    Task SignOutAsync();
    bool IsSignedIn { get; }

    // สร้าง (หรือใช้ที่มีอยู่) Google Sheet ชื่อ "ExpenseTracker Data" ใน Drive ของผู้ใช้
    Task<string> EnsureSpreadsheetExistsAsync();

    Task PushTransactionAsync(Transaction transaction);
    Task PushAccountsSnapshotAsync(List<Account> accounts);
    Task<List<Transaction>> PullTransactionsAsync();

    Task SyncAllAsync();
}
