# แอปบันทึกค่าใช้จ่าย (Expense Tracker)

โปรเจกต์ .NET MAUI (C#) สำหรับ iOS — เก็บข้อมูล local ด้วย SQLite และ sync ขึ้น Google Sheets
ผ่าน Google Drive ของผู้ใช้เอง

## โครงสร้างโปรเจกต์

```
ExpenseTrackerApp/
├── ExpenseTrackerApp.sln
└── ExpenseTrackerApp/
    ├── Models/              # Category, Tag, Account, InstallmentPlan, Transaction, BudgetPlan
    ├── Services/            # DatabaseService (SQLite), GoogleSheetsService,
    │                        # InterestCalculatorService, ReceiptOcrService
    ├── ViewModels/          # MVVM (CommunityToolkit.Mvvm)
    ├── Views/                # หน้าจอ XAML ทั้งหมด
    ├── Converters/           # InvertedBoolConverter, StringNotEmptyConverter
    ├── Resources/Styles/     # ธีมสี (Colors.xaml, Styles.xaml)
    ├── Platforms/iOS/        # Info.plist, AppDelegate
    ├── App.xaml, AppShell.xaml, MauiProgram.cs
    └── ExpenseTrackerApp.csproj
```

## ฟีเจอร์ที่ทำไว้ในโค้ดชุดนี้

- บันทึกรายรับ/รายจ่าย พร้อมหมวดหมู่ (13 หมวดตั้งต้น เพิ่มเองได้), แท็กติดตาม, แนบรูปสลิป
- **กระเป๋าเงิน 4 ประเภท**: เงินสด, บัญชีธนาคาร, บัตรเครดิต, เงินออม — เลือกไอคอน+สีเองได้
  - บัตรเครดิตตั้งค่าวงเงินคงเหลือ/สูงสุด, เลือกวงเงินแยกหรือร่วมกับบัตรอื่น, วันตัดรอบ/วันครบกำหนด,
    เลือกรูปแบบรอบบิล 2 แบบ
- **ผ่อนชำระ (InstallmentPlan)** — แยกเป็นฟีเจอร์อิสระจากกระเป๋าเงิน:
  - หมวดหมู่: ที่อยู่อาศัย, ยานพาหนะ, บัตรเครดิต, สินค้า/บริการ, ธุรกิจ, การศึกษา
  - เลือกวิธีคิดดอกเบี้ย **คงที่** (fixed) หรือ **ลดต้นลดดอก** (reducing balance, สูตร annuity)
  - ผูกกับบัตรเครดิตได้ พร้อม toggle ตัดวงเงินอัตโนมัติ + ซ่อนจากทรัพย์สินสุทธิ (กันนับซ้ำ)
  - วิธีคำนวณงวดแบบ "งวดนำ ยอดเงินจริง"
- สรุปรายงานตามหมวดหมู่ (กราฟโดนัทวาดด้วย `Microsoft.Maui.Graphics` ไม่พึ่ง library ภายนอก)
- ทรัพย์สินสุทธิ (Net Worth) = ทรัพย์สิน (เงินสด/ธนาคาร/ออม) − หนี้สิน (InstallmentPlan ที่ไม่ถูกซ่อน)
- งบประมาณรายเดือนต่อหมวดหมู่ พร้อม progress bar
- ซิงค์ข้อมูลขึ้น Google Sheets (offline-first: บันทึก local ก่อนเสมอ แล้วค่อย sync)
- โครงสร้างรองรับสแกนสลิปด้วย AI (ต้องเสียบ Vision API เอง ดูหัวข้อด้านล่าง)

## สิ่งที่ต้องทำก่อนใช้งาน

### รันบน Windows ได้เลย ไม่ต้องมี Mac หรือเปิด Visual Studio (ฟีเจอร์เหมือนกันทุกอย่าง)

โปรเจกต์นี้เพิ่ม Windows เป็นอีกแพลตฟอร์มหนึ่งแล้ว (`net10.0-windows10.0.19041.0`) ใช้โค้ด C# ชุดเดียวกันกับ iOS
ทั้งหมด — บันทึกรายจ่าย, บัตรเครดิต, ผ่อนชำระ, งบประมาณ, sync Google Sheets ใช้ได้เหมือนกันทุกอย่าง
รันเป็นโปรแกรม `.exe` ธรรมดาบนเครื่อง Windows ได้เลยผ่าน Command Prompt โดยไม่ต้องเปิด Visual Studio:

```bash
# 1. ติดตั้ง .NET 10 SDK ก่อน (ถ้ายังไม่มี): https://dotnet.microsoft.com/download/dotnet/10.0
# 2. ติดตั้ง MAUI workload (รวม Windows support)
dotnet workload install maui

# 3. เข้าโฟลเดอร์โปรเจกต์
cd ExpenseTrackerApp

# 4. รันแอปได้ทันที (เปิดหน้าต่างโปรแกรมขึ้นมาเลย)
dotnet run --project ExpenseTrackerApp/ExpenseTrackerApp.csproj -f net10.0-windows10.0.19041.0
```

**ถ้าต้องการไฟล์ติดตั้งแบบพกพา (ไม่ต้องรันผ่าน `dotnet run` ทุกครั้ง):**
```bash
dotnet publish ExpenseTrackerApp/ExpenseTrackerApp.csproj ^
  -f net10.0-windows10.0.19041.0 ^
  -c Release ^
  -p:WindowsPackageType=None ^
  -p:SelfContained=true ^
  -p:RuntimeIdentifier=win-x64
```
ไฟล์ `.exe` ที่ได้จะอยู่ที่ `ExpenseTrackerApp\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\`
คัดลอกทั้งโฟลเดอร์ไปเครื่องไหนก็รันได้เลย ไม่ต้องติดตั้งอะไรเพิ่ม (self-contained)

**ข้อควรทราบ:** เวอร์ชัน Windows นี้ใช้สำหรับ**พัฒนาและทดสอบฟีเจอร์**เท่านั้น ไม่ใช่ตัวที่ส่งขึ้น App Store ได้
(App Store รับเฉพาะไฟล์ `.ipa` ที่ build จาก Xcode บน macOS) แต่เขียน/แก้โค้ด ViewModel, Service,
Model ตัวเดียวกันได้เลย ใช้ร่วมกับ iOS 100% ยกเว้นเฉพาะไฟล์ในโฟลเดอร์ `Platforms/iOS` และ `Platforms/Windows`
ที่แยกกันเฉพาะแพลตฟอร์ม

### Build .ipa สำหรับ iOS จริง (ต้องมี Mac หรือ CI)
Apple บังคับให้ compile iOS app ด้วย Xcode ซึ่งรันบน macOS เท่านั้น **แต่ถ้าเครื่องคุณเป็น Windows
ไม่จำเป็นต้องซื้อ Mac** — ใช้วิธีใดวิธีหนึ่งต่อไปนี้แทนได้:

**วิธีที่แนะนำ: GitHub Actions (ฟรี)**
ในโปรเจกต์นี้มีไฟล์ `.github/workflows/build-ios.yml` เตรียมไว้ให้แล้ว วิธีใช้:
1. สร้าง repo บน GitHub แล้ว push โค้ดชุดนี้ขึ้นไป (จากเครื่อง Windows ปกติ)
2. เข้าแท็บ **Actions** ของ repo — ระบบจะ build อัตโนมัติบนเครื่อง macOS ของ GitHub (ฟรี มี free tier ให้ใช้ต่อเดือน)
3. เมื่อ build เสร็จ กด "Artifacts" ท้าย log เพื่อโหลดผลลัพธ์
4. เวอร์ชันที่เตรียมไว้ build แบบ **unsigned สำหรับ Simulator** (ตรวจสอบว่าโค้ด compile ผ่าน) —
   ถ้าต้องการ `.ipa` สำหรับติดตั้งเครื่องจริง ต้องมี Apple Developer Account ก่อน แล้ว uncomment
   job `build-ios-device` ในไฟล์ workflow พร้อมเก็บ certificate/provisioning profile เป็น GitHub Secrets
   (มีคอมเมนต์อธิบายขั้นตอนไว้ในไฟล์แล้ว)

**ทางเลือกอื่น:**
- **เช่า Mac คลาวด์** (MacinCloud, MacStadium) แล้ว Remote Desktop เข้าไปทำงานเหมือนมี Mac จริง — เหมาะถ้าต้อง debug บ่อยๆ
- **Visual Studio 2022 + Pair to Mac** — ถ้ามี Mac อยู่แล้ว (ของเพื่อน/ที่ทำงาน) ต่อ network เดียวกัน เขียนโค้ด/กด Run จาก Windows ได้ โดย VS จะสั่ง build ผ่านเครือข่ายไปที่ Mac เครื่องนั้น

ไม่ว่าวิธีไหน ต้องมี **Apple Developer Account** (ปีละ ~3,500 บาท) เพื่อ sign แอปและติดตั้งบนเครื่องจริงเสมอ
ถ้าแค่ต้องการทดสอบบน Simulator ไม่ต้องมี account ก็ได้

### 2. ตั้งค่า Google Sheets API (สำหรับฟีเจอร์ sync)

**หลักการทำงาน:** แอปใช้ Google OAuth 2.0 ให้ผู้ใช้ล็อกอินด้วย Google Account ของตัวเอง
ขอสิทธิ์แบบจำกัด (`spreadsheets` + `drive.file` — เห็นเฉพาะไฟล์ที่แอปสร้างเอง ไม่ใช่ทั้ง Drive)
แล้วสร้างไฟล์ Google Sheets ชื่อ **"ExpenseTracker Data"** ไว้ใน Drive ของผู้ใช้เอง จากนั้น
ทุกรายการที่บันทึกในแอปจะถูกส่งเข้าชีตอัตโนมัติ (offline-first: บันทึก SQLite ก่อนเสมอ รอ sync ตอนมีเน็ต)

**ขั้นตอนตั้งค่า (ทำครั้งเดียว):**
1. ไปที่ [Google Cloud Console](https://console.cloud.google.com) สร้างโปรเจกต์ใหม่ (ฟรี)
2. เปิดใช้งาน **Google Sheets API** และ **Google Drive API** จากเมนู "APIs & Services"
3. ตั้งค่า **OAuth Consent Screen** — กรอกชื่อแอป โลโก้ อีเมลติดต่อ
   (ถ้าใช้เองคนเดียวไม่ต้องรอ Google ตรวจสอบ ถ้าจะปล่อยให้คนทั่วไปใช้ต้องส่งขอตรวจสอบ)
4. สร้าง **OAuth 2.0 Client ID ประเภท iOS** ใส่ Bundle ID `com.yourcompany.expensetracker`
5. นำ Client ID ที่ได้ไปแทนที่ใน `Services/GoogleSheetsService.cs`:
   ```csharp
   public const string IosClientId = "YOUR_IOS_CLIENT_ID.apps.googleusercontent.com";
   ```
6. แก้ `Platforms/iOS/Info.plist` ส่วน `CFBundleURLSchemes` ให้ตรงกับ Client ID (กลับด้าน reverse-DNS)

**ข้อมูลที่ sync เข้าชีต:** วันที่, ประเภท (รายรับ/รายจ่าย), จำนวนเงิน, หมวดหมู่, บัญชี, หมายเหตุ
ถ้าผู้ใช้ไม่เคยเชื่อมต่อ Google เลย แอปยังใช้งานได้ปกติทุกฟีเจอร์ แค่ไม่มีข้อมูลสำรองบนคลาวด์

### 3. เชื่อมสแกนสลิปด้วย AI (ตัวเลือก)
`Services/ReceiptOcrService.cs` เป็น stub ไว้ก่อน แนะนำ 2 ทางเลือก:
- **Apple Vision Framework** (`VNRecognizeTextRequest`) — ฟรี ทำงาน on-device บน iOS โดยตรง แนะนำเริ่มจากตัวนี้ก่อน
- **Google Cloud Vision API** — แม่นกว่าแต่มีค่าใช้จ่ายตามปริมาณการใช้งาน ต้องขอ API key เพิ่ม

### 4. App Icon และรูปภาพ
มี placeholder SVG ให้แล้ว (`appicon.svg`, `appicon_fg.svg`, `splash.svg` — พื้นหลังครีมกับสัญลักษณ์กระเป๋าเงินสีส้มง่ายๆ) พอให้ build ผ่านได้ทันที แต่แนะนำให้ออกแบบไอคอนจริงแทนก่อนเผยแพร่จริง — ใช้ [Figma](https://figma.com) หรือ SF Symbols ออกแบบให้เข้ากับธีมสีครีม/เหลืองใน `Colors.xaml` แล้วนำมาแทนที่ไฟล์เดิม (ชื่อไฟล์ต้องตรงเดิม)

## หมายเหตุเรื่องธุรกรรมธนาคารอัตโนมัติ

แอปนี้**ไม่ได้**เชื่อมต่อกับธนาคารเพื่อดึงธุรกรรมอัตโนมัติ เพราะธนาคารไทยไม่มี open API
สาธารณะให้แอปบุคคลที่สามใช้งาน ผู้ใช้ต้องบันทึกรายการเอง หรือสแกนสลิปด้วยกล้อง (ตามข้อ 3)
หากต้องการเชื่อมต่อจริงในอนาคต ต้องสมัครเป็น business partner กับธนาคารโดยตรง
(เช่น SCB Open API, Krungthai Open API) ซึ่งมีกระบวนการอนุมัติแยกต่างหาก
