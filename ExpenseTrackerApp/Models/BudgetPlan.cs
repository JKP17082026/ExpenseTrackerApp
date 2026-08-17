using SQLite;

namespace ExpenseTrackerApp.Models;

// งบประมาณต่อหมวดหมู่ต่อเดือน
public class BudgetPlan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }   // 1-12
}
