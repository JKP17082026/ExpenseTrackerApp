using ExpenseTrackerApp.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace ExpenseTrackerApp.Services;

// เชื่อมกับ Google Sheets ผ่าน Google Drive ของผู้ใช้เอง (OAuth 2.0)
//
// วิธีตั้งค่าก่อนใช้งานจริง (ทำครั้งเดียว):
// 1. ไปที่ https://console.cloud.google.com สร้างโปรเจกต์ใหม่
// 2. เปิดใช้งาน "Google Sheets API" และ "Google Drive API"
// 3. สร้าง OAuth 2.0 Client ID ประเภท "iOS" ใส่ Bundle ID ให้ตรงกับแอป
// 4. ดาวน์โหลดค่า ClientId มาใส่ในไฟล์ GoogleAuthConfig.cs (ห้าม commit ค่าจริงขึ้น git)
public class GoogleSheetsService : IGoogleSheetsService
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets, "https://www.googleapis.com/auth/drive.file" };
    private const string SpreadsheetTitle = "ExpenseTracker Data";
    private const string SheetName = "Transactions";

    private UserCredential? _credential;
    private SheetsService? _sheetsService;
    private string? _spreadsheetId;
    private readonly IDatabaseService _db;

    public bool IsSignedIn => _credential is not null;

    public GoogleSheetsService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task<bool> SignInAsync()
    {
        try
        {
            var secrets = new ClientSecrets
            {
                ClientId = GoogleAuthConfig.IosClientId,
            };

            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("ExpenseTrackerApp.GoogleAuth"));

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential,
                ApplicationName = "ExpenseTrackerApp"
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task SignOutAsync()
    {
        _credential = null;
        _sheetsService = null;
        _spreadsheetId = null;
        return Task.CompletedTask;
    }

    public async Task<string> EnsureSpreadsheetExistsAsync()
    {
        if (_spreadsheetId is not null) return _spreadsheetId;
        if (_sheetsService is null) throw new InvalidOperationException("ยังไม่ได้ SignIn");

        var spreadsheet = new Spreadsheet
        {
            Properties = new SpreadsheetProperties { Title = SpreadsheetTitle },
            Sheets = new List<Sheet>
            {
                new() { Properties = new SheetProperties { Title = SheetName } }
            }
        };

        var created = await _sheetsService.Spreadsheets.Create(spreadsheet).ExecuteAsync();
        _spreadsheetId = created.SpreadsheetId;

        // ใส่หัวตาราง
        var header = new List<IList<object>>
        {
            new List<object> { "วันที่", "ประเภท", "จำนวนเงิน", "หมวดหมู่", "บัญชี", "แท็ก", "หมายเหตุ" }
        };
        var range = $"{SheetName}!A1";
        var valueRange = new ValueRange { Values = header };
        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        updateRequest.ValueInputOption = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();

        return _spreadsheetId;
    }

    public async Task PushTransactionAsync(Transaction transaction)
    {
        if (_sheetsService is null) return;
        var spreadsheetId = await EnsureSpreadsheetExistsAsync();

        var accounts = await _db.GetAccountsAsync();
        var categories = await _db.GetCategoriesAsync();
        var accountName = accounts.FirstOrDefault(a => a.Id == transaction.AccountId)?.Name ?? "";
        var categoryName = categories.FirstOrDefault(c => c.Id == transaction.CategoryId)?.Name ?? "";

        var row = new List<object>
        {
            transaction.Date.ToString("yyyy-MM-dd HH:mm"),
            transaction.Type.ToString(),
            transaction.Amount,
            categoryName,
            accountName,
            transaction.Note ?? ""
        };

        var valueRange = new ValueRange { Values = new List<IList<object>> { row } };
        var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, $"{SheetName}!A:G");
        appendRequest.ValueInputOption = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        await appendRequest.ExecuteAsync();

        transaction.IsSyncedToSheets = true;
        await _db.SaveTransactionAsync(transaction);
    }

    public async Task PushAccountsSnapshotAsync(List<Account> accounts)
    {
        if (_sheetsService is null) return;
        var spreadsheetId = await EnsureSpreadsheetExistsAsync();

        var rows = accounts.Select(a => (IList<object>)new List<object>
        {
            a.Name, a.Type.ToString(), a.OpeningBalance, a.CreditLimitRemaining ?? 0
        }).ToList();

        var valueRange = new ValueRange { Values = rows };
        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, "Accounts!A2");
        updateRequest.ValueInputOption = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        await updateRequest.ExecuteAsync();
    }

    public async Task<List<Transaction>> PullTransactionsAsync()
    {
        // สำหรับ merge ข้อมูลตอนเปิดแอปบนเครื่องใหม่ (restore จาก Sheets)
        // ทำเป็น stub ไว้ก่อน — เพิ่ม logic parse แถวกลับเป็น Transaction ตามจริงภายหลัง
        return await Task.FromResult(new List<Transaction>());
    }

    public async Task SyncAllAsync()
    {
        if (!IsSignedIn) return;

        var pending = (await _db.GetTransactionsAsync()).Where(t => !t.IsSyncedToSheets);
        foreach (var t in pending)
            await PushTransactionAsync(t);

        var accounts = await _db.GetAccountsAsync();
        await PushAccountsSnapshotAsync(accounts);
    }
}

// ใส่ Client ID จริงจาก Google Cloud Console ตรงนี้ (ห้าม commit ค่าจริงขึ้น public repo)
public static class GoogleAuthConfig
{
    public const string IosClientId = "YOUR_IOS_CLIENT_ID.apps.googleusercontent.com";
}
