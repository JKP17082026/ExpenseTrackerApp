using ExpenseTrackerApp.Models;

namespace ExpenseTrackerApp.Services;

public interface IDatabaseService
{
    Task InitializeAsync();

    Task<List<Transaction>> GetTransactionsAsync(DateTime? from = null, DateTime? to = null);
    Task<int> SaveTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(int id);

    Task<List<Account>> GetAccountsAsync();
    Task<Account?> GetAccountAsync(int id);
    Task<int> SaveAccountAsync(Account account);
    Task DeleteAccountAsync(int id);

    Task<List<InstallmentPlan>> GetInstallmentPlansAsync();
    Task<InstallmentPlan?> GetInstallmentPlanAsync(int id);
    Task<int> SaveInstallmentPlanAsync(InstallmentPlan plan);
    Task DeleteInstallmentPlanAsync(int id);

    Task<List<Category>> GetCategoriesAsync(CategoryType? type = null);
    Task<int> SaveCategoryAsync(Category category);

    Task<List<Tag>> GetTagsAsync();
    Task<int> SaveTagAsync(Tag tag);

    Task<List<BudgetPlan>> GetBudgetPlansAsync(int year, int month);
    Task<int> SaveBudgetPlanAsync(BudgetPlan plan);

    Task<decimal> GetCashAccountBalanceAsync(int accountId);
}
