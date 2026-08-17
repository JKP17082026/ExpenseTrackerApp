using ExpenseTrackerApp.Models;
using SQLite;

namespace ExpenseTrackerApp.Services;

// ใช้ SQLite เก็บข้อมูล local เป็นหลัก แล้ว sync ขึ้น Google Sheets แบบ background
// เหตุผล: แอปต้องใช้งานได้แม้ไม่มีเน็ต (offline-first) แล้วค่อย sync ทีหลัง
public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db is not null) return _db;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "expensetracker.db3");
        _db = new SQLiteAsyncConnection(dbPath);
        return _db;
    }

    public async Task InitializeAsync()
    {
        var db = await GetDbAsync();
        await db.CreateTableAsync<Transaction>();
        await db.CreateTableAsync<Account>();
        await db.CreateTableAsync<InstallmentPlan>();
        await db.CreateTableAsync<Category>();
        await db.CreateTableAsync<Tag>();
        await db.CreateTableAsync<BudgetPlan>();

        await SeedDefaultCategoriesAsync(db);
    }

    private static async Task SeedDefaultCategoriesAsync(SQLiteAsyncConnection db)
    {
        var existing = await db.Table<Category>().CountAsync();
        if (existing > 0) return;

        var defaults = new List<Category>
        {
            new() { Name = "อาหาร", Icon = "🍜", ColorHex = "#FAC775", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 1 },
            new() { Name = "เดินทาง", Icon = "🚗", ColorHex = "#85B7EB", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 2 },
            new() { Name = "ช้อปปิ้ง", Icon = "🛍️", ColorHex = "#ED93B1", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 3 },
            new() { Name = "สุขภาพ", Icon = "💊", ColorHex = "#F09595", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 4 },
            new() { Name = "บันเทิง", Icon = "🎬", ColorHex = "#AFA9EC", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 5 },
            new() { Name = "บิล/สาธารณูปโภค", Icon = "📄", ColorHex = "#97C459", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 6 },
            new() { Name = "การศึกษา", Icon = "📚", ColorHex = "#5DCAA5", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 7 },
            new() { Name = "ที่พัก", Icon = "🏠", ColorHex = "#EF9F27", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 8 },
            new() { Name = "กาแฟ/เครื่องดื่ม", Icon = "☕", ColorHex = "#D85A30", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 9 },
            new() { Name = "สังสรรค์", Icon = "🎁", ColorHex = "#F0997B", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 10 },
            new() { Name = "อื่นๆ", Icon = "✨", ColorHex = "#B4B2A9", Type = CategoryType.Expense, IsBuiltIn = true, SortOrder = 99 },
            new() { Name = "เงินเดือน", Icon = "💰", ColorHex = "#639922", Type = CategoryType.Income, IsBuiltIn = true, SortOrder = 1 },
            new() { Name = "รายได้เสริม", Icon = "🌱", ColorHex = "#9FE1CB", Type = CategoryType.Income, IsBuiltIn = true, SortOrder = 2 },
        };

        await db.InsertAllAsync(defaults);
    }

    public async Task<List<Transaction>> GetTransactionsAsync(DateTime? from = null, DateTime? to = null)
    {
        var db = await GetDbAsync();
        var query = db.Table<Transaction>();
        var list = await query.ToListAsync();

        if (from.HasValue) list = list.Where(t => t.Date >= from.Value).ToList();
        if (to.HasValue) list = list.Where(t => t.Date <= to.Value).ToList();

        return list.OrderByDescending(t => t.Date).ToList();
    }

    public async Task<int> SaveTransactionAsync(Transaction transaction)
    {
        var db = await GetDbAsync();
        if (transaction.Id == 0)
            return await db.InsertAsync(transaction);

        await db.UpdateAsync(transaction);
        return transaction.Id;
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var db = await GetDbAsync();
        await db.DeleteAsync<Transaction>(id);
    }

    public async Task<List<Account>> GetAccountsAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<Account>().ToListAsync();
    }

    public async Task<Account?> GetAccountAsync(int id)
    {
        var db = await GetDbAsync();
        return await db.Table<Account>().Where(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveAccountAsync(Account account)
    {
        var db = await GetDbAsync();
        if (account.Id == 0)
            return await db.InsertAsync(account);

        await db.UpdateAsync(account);
        return account.Id;
    }

    public async Task DeleteAccountAsync(int id)
    {
        var db = await GetDbAsync();
        await db.DeleteAsync<Account>(id);
    }

    public async Task<List<InstallmentPlan>> GetInstallmentPlansAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<InstallmentPlan>().ToListAsync();
    }

    public async Task<InstallmentPlan?> GetInstallmentPlanAsync(int id)
    {
        var db = await GetDbAsync();
        return await db.Table<InstallmentPlan>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveInstallmentPlanAsync(InstallmentPlan plan)
    {
        var db = await GetDbAsync();

        // ถ้าผูกกับบัตรเครดิตและเปิด "ตัดเงินจากบัตรเมื่อสร้างรายการ" ให้หักวงเงินคงเหลือของบัตรทันที (เฉพาะตอนสร้างใหม่)
        if (plan.Id == 0 && plan.LinkedCreditCardAccountId.HasValue && plan.DeductFromCreditCardOnCreate)
        {
            var card = await GetAccountAsync(plan.LinkedCreditCardAccountId.Value);
            if (card is not null && card.IsCreditCard && card.CreditLimitRemaining.HasValue)
            {
                card.CreditLimitRemaining -= plan.PrincipalTotal;
                await SaveAccountAsync(card);
            }
        }

        if (plan.Id == 0)
            return await db.InsertAsync(plan);

        await db.UpdateAsync(plan);
        return plan.Id;
    }

    public async Task DeleteInstallmentPlanAsync(int id)
    {
        var db = await GetDbAsync();
        await db.DeleteAsync<InstallmentPlan>(id);
    }

    public async Task<List<Category>> GetCategoriesAsync(CategoryType? type = null)
    {
        var db = await GetDbAsync();
        var list = await db.Table<Category>().ToListAsync();
        if (type.HasValue) list = list.Where(c => c.Type == type.Value).ToList();
        return list.OrderBy(c => c.SortOrder).ToList();
    }

    public async Task<int> SaveCategoryAsync(Category category)
    {
        var db = await GetDbAsync();
        if (category.Id == 0)
            return await db.InsertAsync(category);

        await db.UpdateAsync(category);
        return category.Id;
    }

    public async Task<List<Tag>> GetTagsAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<Tag>().ToListAsync();
    }

    public async Task<int> SaveTagAsync(Tag tag)
    {
        var db = await GetDbAsync();
        if (tag.Id == 0)
            return await db.InsertAsync(tag);

        await db.UpdateAsync(tag);
        return tag.Id;
    }

    public async Task<List<BudgetPlan>> GetBudgetPlansAsync(int year, int month)
    {
        var db = await GetDbAsync();
        var list = await db.Table<BudgetPlan>().ToListAsync();
        return list.Where(b => b.Year == year && b.Month == month).ToList();
    }

    public async Task<int> SaveBudgetPlanAsync(BudgetPlan plan)
    {
        var db = await GetDbAsync();
        if (plan.Id == 0)
            return await db.InsertAsync(plan);

        await db.UpdateAsync(plan);
        return plan.Id;
    }

    // ยอดคงเหลือบัญชีกระแสเงินสด = ยอดตั้งต้น + รายรับ - รายจ่าย (ไม่รวมบัตรเครดิต/สินเชื่อ)
    public async Task<decimal> GetCashAccountBalanceAsync(int accountId)
    {
        var account = await GetAccountAsync(accountId);
        if (account is null || account.IsCreditCard) return 0;

        var transactions = await GetTransactionsAsync();
        var relevant = transactions.Where(t => t.AccountId == accountId);

        var income = relevant.Where(t => t.Type == CategoryType.Income).Sum(t => t.Amount);
        var expense = relevant.Where(t => t.Type == CategoryType.Expense).Sum(t => t.Amount);

        return account.OpeningBalance + income - expense;
    }
}
