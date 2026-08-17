using SQLite;

namespace ExpenseTrackerApp.Models;

// แท็กติดตาม ใช้จัดกลุ่มรายการข้ามหมวดหมู่ เช่น "ทริปเชียงใหม่"
public class Tag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏷️";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
