using SQLite;

namespace ExpenseTrackerApp.Models;

// บัญชี/กระเป๋าเงิน — ครอบคลุมเงินสด, ธนาคาร, บัตรเครดิต, เงินออม, เงินลงทุน, เงินดิจิทัล
// หมายเหตุ: "ผ่อนชำระ" (สินเชื่อ/หนี้สิน) ไม่ใช่ Account อีกต่อไป แยกไปเป็น InstallmentPlan
// เพราะในแอปจริงผู้ใช้สร้างรายการผ่อนได้โดยไม่ต้องผูกกับกระเป๋าเงินเสมอไป
public class Account
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; } = AccountType.Cash;
    public string IconEmoji { get; set; } = "👛";      // ไอคอนกระเป๋า เช่น 💵 🏦 💳 🐷 📈
    public string? ImagePath { get; set; }              // รูปที่ผู้ใช้อัปโหลดเอง (แทนไอคอน)
    public string ColorHex { get; set; } = "#FAE0A0";

    // ----- เงินสด / ธนาคาร / เงินออม / เงินลงทุน / เงินดิจิทัล -----
    public decimal OpeningBalance { get; set; } = 0;

    // ----- บัตรเครดิตเท่านั้น -----
    public decimal? CreditLimitRemaining { get; set; }     // วงเงินคงเหลือ
    public decimal? CreditLimitMax { get; set; }             // วงเงินสูงสุด
    public bool UseSharedCreditLimit { get; set; } = false;   // true = ใช้วงเงินร่วมกับบัตรอื่น
    public int? SharedCreditLimitGroupId { get; set; }         // กลุ่มบัตรที่ใช้วงเงินร่วมกัน (ผูกกับ Account.Id ของบัตรหลัก)
    public int? StatementDay { get; set; }                       // วันตัดรอบบัตร (1-31)
    public int? PaymentDueDay { get; set; }                       // วันครบกำหนดชำระ (1-31)
    public StatementCycleStyle CycleStyle { get; set; } = StatementCycleStyle.StatementDayStartsNewCycle;

    public bool IsCreditCard => Type == AccountType.CreditCard;
}
