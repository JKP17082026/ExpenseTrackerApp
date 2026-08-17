namespace ExpenseTrackerApp.Models;

// ประเภทกระเป๋า/บัญชี
public enum AccountType
{
    Cash,        // เงินสด
    Bank,        // บัญชีธนาคาร
    CreditCard,  // บัตรเครดิต
    Savings      // เงินออม
}

// รูปแบบรอบบิลบัตรเครดิต — มีผลต่อว่า "วันตัดรอบ" นับเข้ารอบไหน
public enum StatementCycleStyle
{
    StatementDayStartsNewCycle,   // วันตัดรอบเป็นวันแรกของรอบใหม่
    StatementDayEndsCurrentCycle  // วันตัดรอบเป็นวันสุดท้ายของรอบเดิม
}
