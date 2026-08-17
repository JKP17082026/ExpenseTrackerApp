namespace ExpenseTrackerApp.Models;

// วิธีคิดดอกเบี้ยของการผ่อนชำระ
public enum InterestCalculationMethod
{
    Fixed,           // คงที่ — ดอกเบี้ยคำนวณจากเงินต้นเริ่มต้นตลอดสัญญา ไม่ลดตามที่ผ่อน
    ReducingBalance  // ลดต้นลดดอก — ดอกเบี้ยคำนวณจากเงินต้นคงเหลือจริงในแต่ละงวด (ถูกกว่าแบบคงที่)
}

// วิธีนับงวดเมื่อจ่ายไม่ตรงยอด
public enum InstallmentCountingMethod
{
    InstallmentLeadsActualAmount, // งวดนำ ยอดเงินจริง — จ่ายเท่าไหร่ก็ปิดงวดนั้นได้เลย (ยอดจริงของงวด = ที่จ่ายจริง)
    FixedAmountPerInstallment      // ยอดคงที่ต่องวด — ต้องจ่ายเต็มจำนวนงวดถึงจะนับว่าปิดงวด
}
