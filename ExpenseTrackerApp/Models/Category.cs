using SQLite;

namespace ExpenseTrackerApp.Models;

// หมวดหมู่รายรับ-รายจ่าย เช่น อาหาร, เดินทาง, บิล ฯลฯ
public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;      // ชื่อหมวดหมู่ เช่น "อาหาร"
    public string Icon { get; set; } = "🍜";               // อีโมจิ/สัญลักษณ์ประจำหมวดหมู่
    public string ColorHex { get; set; } = "#FAC775";      // สีประจำหมวดหมู่ (ใช้ในกราฟ)
    public CategoryType Type { get; set; } = CategoryType.Expense;
    public bool IsBuiltIn { get; set; } = false;           // true = มากับแอป, false = ผู้ใช้สร้างเอง
    public int SortOrder { get; set; } = 0;
}

public enum CategoryType
{
    Expense,   // รายจ่าย
    Income,    // รายรับ
    Transfer   // โอนเงินระหว่างบัญชี
}
