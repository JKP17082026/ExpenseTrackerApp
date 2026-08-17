using ExpenseTrackerApp.Models;

namespace ExpenseTrackerApp.Services;

// คำนวณทุกอย่างเกี่ยวกับบัตรเครดิต (รอบบิล/ดอกเบี้ย) และรายการผ่อนชำระ (คงที่ / ลดต้นลดดอก)
public class InterestCalculatorService
{
    // ================= บัตรเครดิต =================

    public (DateTime cycleStart, DateTime cycleEnd) GetCurrentBillingCycle(Account account, DateTime today)
    {
        if (account.StatementDay is null)
            throw new InvalidOperationException("บัญชีนี้ไม่ใช่บัตรเครดิต");

        int day = Math.Min(account.StatementDay.Value, DateTime.DaysInMonth(today.Year, today.Month));
        var thisMonthStatement = new DateTime(today.Year, today.Month, day);

        // "วันตัดรอบเป็นวันแรกของรอบใหม่" หรือ "วันสุดท้ายของรอบเดิม" มีผลต่อว่าธุรกรรมวันตัดรอบพอดีนับรอบไหน
        bool statementDayEndsCycle = account.CycleStyle == StatementCycleStyle.StatementDayEndsCurrentCycle;

        DateTime cycleEnd = statementDayEndsCycle
            ? (today <= thisMonthStatement ? thisMonthStatement : thisMonthStatement.AddMonths(1))
            : (today < thisMonthStatement ? thisMonthStatement : thisMonthStatement.AddMonths(1));

        DateTime cycleStart = cycleEnd.AddMonths(-1).AddDays(statementDayEndsCycle ? 1 : 0);
        return (cycleStart, cycleEnd);
    }

    public DateTime GetPaymentDueDate(Account account, DateTime cycleEnd)
    {
        if (account.PaymentDueDay is null)
            throw new InvalidOperationException("บัญชีนี้ไม่ใช่บัตรเครดิต");

        var dueMonth = cycleEnd.AddMonths(1);
        int day = Math.Min(account.PaymentDueDay.Value, DateTime.DaysInMonth(dueMonth.Year, dueMonth.Month));
        return new DateTime(dueMonth.Year, dueMonth.Month, day);
    }

    public decimal GetCycleSpending(List<Transaction> accountTransactions, DateTime cycleStart, DateTime cycleEnd)
    {
        return accountTransactions
            .Where(t => t.Type == CategoryType.Expense && t.Date >= cycleStart && t.Date <= cycleEnd)
            .Sum(t => t.Amount - (t.DiscountAmount ?? 0));
    }

    public decimal GetMinimumPayment(decimal statementBalance, decimal minPercent = 0.10m)
    {
        var min = statementBalance * minPercent;
        return Math.Max(min, 200);
    }

    public decimal EstimateInterestIfMinimumPaid(decimal statementBalance, decimal minimumPaid, decimal annualRatePercent, int daysUntilNextCycle)
    {
        var outstanding = statementBalance - minimumPaid;
        if (outstanding <= 0) return 0;

        var dailyRate = (annualRatePercent / 100m) / 365m;
        return Math.Round(outstanding * dailyRate * daysUntilNextCycle, 2);
    }

    // ================= รายการผ่อนชำระ (InstallmentPlan) =================

    // ค่างวดต่อเดือนโดยประมาณ ใช้แสดงผลก่อนบันทึก (ผู้ใช้กรอกเองได้ถ้าต้องการ override)
    public decimal EstimateMonthlyPayment(InstallmentPlan plan)
    {
        if (plan.TotalInstallments <= 0) return 0;
        var remainingInstallments = Math.Max(plan.TotalInstallments - plan.InstallmentsPaid, 1);

        return plan.InterestMethod switch
        {
            InterestCalculationMethod.Fixed => EstimateFixedInterestPayment(plan, remainingInstallments),
            InterestCalculationMethod.ReducingBalance => EstimateReducingBalancePayment(plan, remainingInstallments),
            _ => plan.RemainingBalance / remainingInstallments
        };
    }

    // ดอกเบี้ยคงที่: คิดดอกเบี้ยจากเงินต้น "เริ่มต้น" ตลอดสัญญา แล้วหารเฉลี่ยเท่ากันทุกงวด
    // สูตร: ดอกเบี้ยรวม = เงินต้น x อัตราดอกเบี้ยต่อปี x จำนวนปีของสัญญา
    //       ค่างวด = (เงินต้น + ดอกเบี้ยรวม) / จำนวนงวดทั้งหมด
    private decimal EstimateFixedInterestPayment(InstallmentPlan plan, int remainingInstallments)
    {
        var years = plan.TotalInstallments / 12m;
        var totalInterest = plan.PrincipalTotal * (plan.AnnualInterestRate / 100m) * years;
        var totalPayable = plan.PrincipalTotal + totalInterest;
        return Math.Round(totalPayable / plan.TotalInstallments, 2);
    }

    // ลดต้นลดดอก (reducing balance / effective interest): ดอกเบี้ยคิดจากเงินต้นคงเหลือจริงในแต่ละงวด
    // ใช้สูตรค่างวดคงที่มาตรฐาน (annuity formula):
    //   ค่างวด = P x r x (1+r)^n / ((1+r)^n - 1)
    //   โดย P = เงินต้นคงเหลือ, r = ดอกเบี้ยต่องวด (ต่อปี/12), n = จำนวนงวดที่เหลือ
    private decimal EstimateReducingBalancePayment(InstallmentPlan plan, int remainingInstallments)
    {
        var monthlyRate = (double)(plan.AnnualInterestRate / 100m) / 12.0;
        var principal = (double)plan.RemainingBalance;

        if (monthlyRate == 0)
            return (decimal)(principal / remainingInstallments);

        var factor = Math.Pow(1 + monthlyRate, remainingInstallments);
        var payment = principal * monthlyRate * factor / (factor - 1);
        return Math.Round((decimal)payment, 2);
    }

    // ตารางผ่อนแบบลดต้นลดดอก คืนรายการ (งวดที่, เงินต้นที่ลด, ดอกเบี้ยงวดนี้, เงินต้นคงเหลือหลังงวดนี้)
    public List<(int installmentNo, decimal principalPortion, decimal interestPortion, decimal remainingAfter)> BuildReducingBalanceSchedule(InstallmentPlan plan)
    {
        var schedule = new List<(int, decimal, decimal, decimal)>();
        var remainingInstallments = Math.Max(plan.TotalInstallments - plan.InstallmentsPaid, 1);
        var monthlyPayment = EstimateReducingBalancePayment(plan, remainingInstallments);
        var monthlyRate = plan.AnnualInterestRate / 100m / 12m;
        var balance = plan.RemainingBalance;

        for (int i = plan.InstallmentsPaid + 1; i <= plan.TotalInstallments; i++)
        {
            var interestPortion = Math.Round(balance * monthlyRate, 2);
            var principalPortion = monthlyPayment - interestPortion;
            balance = Math.Max(balance - principalPortion, 0);
            schedule.Add((i, principalPortion, interestPortion, balance));
        }

        return schedule;
    }

    // ดอกเบี้ยที่จ่ายไปแล้วทั้งหมด (ใช้ได้ทั้ง 2 แบบ เพราะอิงจากยอดจ่ายจริง - เงินต้นที่ลดจริง)
    public decimal GetTotalInterestPaid(InstallmentPlan plan)
    {
        var principalReduced = plan.PrincipalTotal - plan.RemainingBalance;
        return Math.Max(plan.AmountPaidSoFar - principalReduced, 0);
    }

    // ดอกเบี้ยที่ต้องจ่ายทั้งหมดตลอดอายุสัญญาที่เหลือ
    public decimal GetRemainingInterest(InstallmentPlan plan)
    {
        var remainingInstallments = Math.Max(plan.TotalInstallments - plan.InstallmentsPaid, 0);
        if (remainingInstallments == 0) return 0;

        var monthlyPayment = EstimateMonthlyPayment(plan);
        var totalRemainingPayments = monthlyPayment * remainingInstallments;
        return Math.Max(totalRemainingPayments - plan.RemainingBalance, 0);
    }
}
