namespace ExpenseTrackerApp.Services;

// สแกนสลิปด้วย AI — ตัวอย่างนี้เตรียม interface ไว้ให้เรียกใช้ Vision API ภายนอก
// (เช่น Google Cloud Vision, Apple Vision Framework บน iOS, หรือ Claude API แบบ multimodal)
// ตอนนี้ยังเป็น stub ที่ต้องเสียบ API key จริงก่อนใช้งาน
public class ReceiptOcrService : IReceiptOcrService
{
    public async Task<ReceiptOcrResult> ScanReceiptAsync(string imagePath)
    {
        // TODO: เรียก Vision API จริงตรงนี้ เช่น
        // 1) Apple's VNRecognizeTextRequest (ฟรี ทำงาน on-device บน iOS) — แนะนำเริ่มจากตัวนี้ก่อน
        // 2) Google Cloud Vision API (ต้องมี API key + เสียค่าใช้จ่ายตามปริมาณ)
        // แล้ว parse ข้อความที่ได้เพื่อดึงยอดเงิน/วันที่/ชื่อร้าน ด้วย regex หรือส่งต่อให้ LLM ช่วยตีความ

        await Task.Delay(50); // placeholder

        return new ReceiptOcrResult
        {
            Success = false,
            Amount = null,
            MerchantName = null,
            Date = null
        };
    }
}
