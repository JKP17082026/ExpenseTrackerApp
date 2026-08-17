namespace ExpenseTrackerApp.Services;

public class ReceiptOcrResult
{
    public decimal? Amount { get; set; }
    public string? MerchantName { get; set; }
    public DateTime? Date { get; set; }
    public bool Success { get; set; }
}

public interface IReceiptOcrService
{
    // ถ่าย/เลือกรูปสลิป แล้วอ่านยอดเงิน/ร้านค้า/วันที่ออกมาอัตโนมัติ
    Task<ReceiptOcrResult> ScanReceiptAsync(string imagePath);
}
