namespace ExpenseTrackerApp.Models;

// หมวดหมู่ของรายการผ่อนชำระ (หนี้สิน) แยกจากหมวดหมู่รายจ่ายทั่วไป
public enum InstallmentCategory
{
    Housing,      // ที่อยู่อาศัย
    Vehicle,      // ยานพาหนะ
    CreditCard,   // บัตรเครดิต (ยอดผ่อนสินค้าบนบัตร)
    Product,      // สินค้า/บริการ
    Business,     // ธุรกิจ
    Education     // การศึกษา
}
