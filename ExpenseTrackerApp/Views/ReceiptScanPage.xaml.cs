using ExpenseTrackerApp.Services;

namespace ExpenseTrackerApp.Views;

public partial class ReceiptScanPage : ContentPage
{
    private readonly IReceiptOcrService _ocr;

    public ReceiptScanPage(IReceiptOcrService ocr)
    {
        InitializeComponent();
        _ocr = ocr;
    }

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.Default.CapturePhotoAsync();
        if (photo is null) return;

        var localPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
        using var stream = await photo.OpenReadAsync();
        using var newStream = File.OpenWrite(localPath);
        await stream.CopyToAsync(newStream);

        var result = await _ocr.ScanReceiptAsync(localPath);
        if (result.Success)
        {
            // ส่งค่ากลับไปหน้า AddTransactionPage ผ่าน MessagingCenter หรือ Shell navigation parameter
            await Shell.Current.GoToAsync("..");
        }
    }
}
