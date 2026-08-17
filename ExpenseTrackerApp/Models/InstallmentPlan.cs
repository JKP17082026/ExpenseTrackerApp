using SQLite;

namespace ExpenseTrackerApp.Models;

// รายการผ่อนชำระ/หนี้สิน แยกออกจาก Account ตามที่แอปอ้างอิงออกแบบไว้
// รองรับทั้งดอกเบี้ยคงที่และแบบลดต้นลดดอก และเลือกผูกกับบัตรเครดิตได้
public class InstallmentPlan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string IconEmoji { get; set; } = "🏠";
    public string? ImagePath { get; set; }

    public InstallmentCategory Category { get; set; } = InstallmentCategory.Housing;
    public string Name { get; set; } = string.Empty;       // ชื่อรายการ เช่น "ผ่อนบ้าน"
    public string CreditorName { get; set; } = string.Empty; // เจ้าหนี้ เช่น "ธนาคาร ก."

    public InterestCalculationMethod InterestMethod { get; set; } = InterestCalculationMethod.Fixed;

    public decimal PrincipalTotal { get; set; }              // เงินต้นทั้งหมด
    public decimal AmountPaidSoFar { get; set; }               // ผ่อนชำระไปแล้ว
    public int TotalInstallments { get; set; }                  // จำนวนงวดทั้งหมด
    public int InstallmentsPaid { get; set; }                     // จำนวนงวดที่จ่ายไปแล้ว
    public decimal AnnualInterestRate { get; set; }                // อัตราดอกเบี้ย (% ต่อปี)

    public DateTime StartDate { get; set; } = DateTime.Now;         // วันที่เริ่มผ่อน
    public DateTime? ExpectedPayoffDate { get; set; }                 // วันที่คาดว่าจะผ่อนหมด (ไม่บังคับ)
    public int PaymentDueDayOfMonth { get; set; } = 5;                  // วันที่ครบกำหนดชำระของแต่ละเดือน

    public InstallmentCountingMethod CountingMethod { get; set; } = InstallmentCountingMethod.InstallmentLeadsActualAmount;

    // ----- ตัวเลือกเมื่อเจ้าหนี้คือบัตรเครดิต -----
    public int? LinkedCreditCardAccountId { get; set; }               // ผูกกับกระเป๋าบัตรเครดิตใบไหน
    public bool DeductFromCreditCardOnCreate { get; set; } = true;      // ตัดวงเงินคงเหลือของบัตรทันทีที่สร้างรายการ
    public bool HideFromNetWorth { get; set; } = false;                  // ซ่อนจากหน้าทรัพย์สินสุทธิ (กันนับซ้ำกับยอดบัตร)

    public string? MemoNote { get; set; }                                 // บันทึกช่วยจำ เช่น "ผ่อนของขวัญให้แฟน"

    public decimal RemainingBalance => Math.Max(PrincipalTotal - AmountPaidSoFar, 0);
    public double ProgressPercent => TotalInstallments > 0 ? (double)InstallmentsPaid / TotalInstallments * 100 : 0;
}
