using SQLite;

namespace ExpenseTrackerApp.Models;

public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public decimal Amount { get; set; }
    public CategoryType Type { get; set; } = CategoryType.Expense;
    public DateTime Date { get; set; } = DateTime.Now;
    public string? Note { get; set; }

    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public int? TagId { get; set; }
    public int? TransferToAccountId { get; set; }   // ใช้เมื่อ Type = Transfer

    public string? ReceiptImagePath { get; set; }     // path รูปสลิปที่สแกน/แนบ
    public decimal? DiscountAmount { get; set; }        // ส่วนลดที่หักจากยอดเต็ม
    public bool IsSyncedToSheets { get; set; } = false;
    public string? GoogleSheetsRowId { get; set; }      // ใช้ตอน update/delete แถวใน Sheets
}
