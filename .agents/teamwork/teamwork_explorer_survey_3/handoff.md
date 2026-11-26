# Handoff Report: Comprehensive Survey of Donation / Fundraising / Campaign Subsystem

**Author**: `teamwork_explorer_survey_3`  
**Date**: 2026-11-06  
**Milestone**: `survey_donation_subsystem`  
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_3`  
**Target Solution**: `D:\web_tinh_nguyen\WEB_TinhNguyen`  

---

## 1. Observation

### 1.1 Architecture & Component Mapping
The donation / fundraising / campaign landscape at `D:\web_tinh_nguyen` consists of two distinct user-facing touchpoints and two administrative controllers:

| Component Type | File Path | Primary Function |
|---|---|---|
| **Public Controller** | `WEB_TinhNguyen\BoDieuKhien\HomeController.cs` (lines 71–86) | Action `GayQuy(int? year)` queries active or specified `ChienDich`, loads `CauHinhGayQuy` with `KenhDongGops` and `NhaTaiTroGayQuys` via `AsSplitQuery()`. |
| **Public View** | `WEB_TinhNguyen\GiaoDien\Home\GayQuy.cshtml` (310 lines) | Main public donation page (`/Home/GayQuy`) with Hero banner, cards for `KenhDongGops`, and modal `#qrModal` (lines 240–255). |
| **Admin Controller** | `WEB_TinhNguyen\BoDieuKhien\AdminGayQuyController.cs` (288 lines) | Action `Index()` lists campaigns by year; `Edit(int id)` [GET/POST] edits `CauHinhGayQuy` and its child repeater items (`KenhDongGop` and `NhaTaiTroGayQuy`). |
| **Admin View** | `WEB_TinhNguyen\GiaoDien\AdminGayQuy\Edit.cshtml` (531 lines) | Tab 3 (`#tabChannels`, lines 151–273) allows configuring donation channels (`KenhDongGops`), bank account, account name, transfer memo, and file upload (`FileAnhQR`). |
| **Admin List View** | `WEB_TinhNguyen\GiaoDien\AdminGayQuy\Index.cshtml` (70 lines) | Campaign list table with direct links to `AdminGayQuy/Edit/{id}`. |
| **Store Controller** | `WEB_TinhNguyen\Areas\CuaHang\Controllers\GioHangController.cs` (lines 426–460) | Action `DatHangThanhCong(int id)` renders order confirmation and VietQR payment screen. |
| **Store Payment View**| `WEB_TinhNguyen\Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` (lines 1–155) | Order success view; displays dynamic VietQR via `img.vietqr.io` and payment guide table. |
| **Admin Order Controller** | `WEB_TinhNguyen\Areas\CuaHang\Controllers\AdminDonHangController.cs` (lines 1–100) | Manages orders and opens detail view `ChiTiet(int id)`. |
| **Admin Order Detail View** | `WEB_TinhNguyen\Areas\CuaHang\Views\AdminDonHang\ChiTiet.cshtml` (lines 351–396) | Right-hand column contains "BOX VIETQR / THANH TOÁN" with VietQR image and bank details. |
| **Entities & Models** | `WEB_TinhNguyen\MoHinh\Entities.cs` (lines 387–507) | `CauHinhGayQuy` and `KenhDongGop` tables. |
| **ViewModels** | `WEB_TinhNguyen\MoHinh\CauHinhTrangChuVM.cs` (lines 241–320) | `CauHinhGayQuyVM` and `KenhDongGopVM`. |
| **Store Models** | `WEB_TinhNguyen\Areas\CuaHang\Models\DonHangGayQuy.cs` | Stores order totals (`TongThanhToan`, `TienUngHoThem`) and order code (`MaDonHang`). |

---

### 1.2 Exact Locations of QR Codes, Bank Accounts, and Payment Instructions

#### A. Public Donation Landing Page (`/Home/GayQuy`)
- **File**: `WEB_TinhNguyen\GiaoDien\Home\GayQuy.cshtml`
- **Trigger**: Section 2 button for channels with `LoaiHanhDong == "qr"`:
  ```html
  <!-- Lines 137-145 -->
  <button type="button"
          onclick="openQrModal('@Html.Raw(kenh.TenKenh)', '@Html.Raw(kenh.LinkAnhQR)', '@Html.Raw(kenh.TenNganHang)', '@Html.Raw(kenh.SoTaiKhoan)', '@Html.Raw(kenh.TenChuTaiKhoan)', '@Html.Raw(kenh.NoiDungChuyenKhoan)')"
          class="inline-flex items-center justify-center gap-2 ...">
      <i class="fas fa-qrcode"></i>
      <span>@kenh.TextNut</span>
  </button>
  ```
- **Modal Popup (`#qrModal`)**:
  ```html
  <!-- Lines 240-255 -->
  <div id="qrModal" class="fixed inset-0 z-50 hidden items-center justify-center bg-black/60 ...">
      <div class="...">
          <h3 id="qrModalTitle" class="text-xl font-black text-emerald-800 ...">Thông tin Quyên góp</h3>
          <div class="...">
              <img id="qrModalImg" src="" alt="QR Code" class="w-56 h-56 object-contain mx-auto rounded-lg" />
          </div>
          <div class="...">
              <p><span>Ngân hàng:</span> <strong id="qrModalBank"></strong></p>
              <p><span>Số tài khoản:</span> <strong id="qrModalAccNum"></strong></p>
              <p><span>Chủ tài khoản:</span> <strong id="qrModalAccName"></strong></p>
              <p><span>Nội dung CK:</span> <strong id="qrModalNote"></strong></p>
          </div>
      </div>
  </div>
  ```

#### B. Store Order Confirmation & Payment Page (`/CuaHang/GioHang/DatHangThanhCong/{id}`)
- **File**: `WEB_TinhNguyen\Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml`
- **QR Generation & Display**:
  ```csharp
  // Lines 4-8
  var isBankTransfer = Model.PhuongThucThanhToan != "COD";
  var cleanAmount = (long)Model.TongThanhToan;
  var cleanMemo = Uri.EscapeDataString(Model.MaDonHang);
  var qrUrl = $"https://img.vietqr.io/image/MB-0924044283-compact2.png?amount={cleanAmount}&addInfo={cleanMemo}&accountName=NGUYEN%20THAI%20VINH";
  ```
- **Displayed Information (Lines 110–153)**:
  - QR Image: `<img src="@qrUrl" alt="VietQR Quét mã thanh toán đơn hàng @Model.MaDonHang" />`
  - Subtitle: `VIETQR • MBBANK CHUẨN NAPAS247`
  - Ngân hàng: `Quân Đội (MBBank)`
  - Số tài khoản: `0924044283` (with copy-to-clipboard button)
  - Chủ tài khoản: `NGUYEN THAI VINH`
  - Số tiền: `@Model.TongThanhToan.ToString("N0") VNĐ`
  - Nội dung CK: `@Model.MaDonHang` (with copy-to-clipboard button)

#### C. Store Admin Order Detail View (`/CuaHang/AdminDonHang/ChiTiet/{id}`)
- **File**: `WEB_TinhNguyen\Areas\CuaHang\Views\AdminDonHang\ChiTiet.cshtml`
- **QR Generation & Display (Lines 7–9, 362–398)**:
  - Exact duplicate of the VietQR URL:
    `var qrUrl = $"https://img.vietqr.io/image/MB-0924044283-compact2.png?amount={cleanAmount}&addInfo={cleanMemo}&accountName=NGUYEN%20THAI%20VINH";`
  - Displays identical bank details: MBBank, `0924044283`, `NGUYEN THAI VINH`, amount, and order memo.

---

### 1.3 Catalog of Every Single Hardcoded Value in the Subsystem

| File Path | Line(s) | Hardcoded Item | Verbatim Value / Code |
|---|---|---|---|
| `BoDieuKhien\AdminGayQuyController.cs` | 67 | Bank Name | `TenNganHang = "MBBank (Ngân hàng Quân Đội)",` |
| `BoDieuKhien\AdminGayQuyController.cs` | 68 | Account Number | `SoTaiKhoan = "0924044283",` |
| `BoDieuKhien\AdminGayQuyController.cs` | 69 | Account Holder | `TenChuTaiKhoan = "NGUYỄN ANH HÀO",` |
| `BoDieuKhien\AdminGayQuyController.cs` | 70 | Transfer Memo | `NoiDungChuyenKhoan = "[Họ và tên]_[Đơn vị]_gopLangbiang",` |
| `BoDieuKhien\AdminGayQuyController.cs` | 71 | Static QR Image Link | `LinkAnhQR = "https://ik.imagekit.io/ij9ocagrc/kho/QRCK-A.Ha-o-636KHTNdal43FQMQOVKE0khAuEiX4h_OMj08ZkCK.webp",` |
| `BoDieuKhien\AdminGayQuyController.cs` | 78 | Gift Donation Contact | `MoTa = "Quà Trung thu, nhu yếu phẩm,... Vui lòng liên hệ Zalo 0827.564.956 (Anh Phạm Minh Phát - đại diện Ban sáng lập Dự án) hoặc Fanpage",` |
| `BoDieuKhien\AdminGayQuyController.cs` | 82 | External Link | `LinkNgoai = "https://www.facebook.com/profile.php?id=61580211752903",` |
| `GiaoDien\AdminGayQuy\Edit.cshtml` | 252 | Form Placeholder | `placeholder="VD: MB Bank"` |
| `GiaoDien\AdminGayQuy\Edit.cshtml` | 256 | Form Placeholder | `placeholder="VD: 0924044283"` |
| `GiaoDien\AdminGayQuy\Edit.cshtml` | 260 | Form Placeholder | `placeholder="VD: NGUYEN ANH HAO"` |
| `GiaoDien\AdminGayQuy\Edit.cshtml` | 264 | Form Placeholder | `placeholder="VD: Langbiang - Nguyen Van A"` |
| `Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` | 7 | VietQR Endpoint | `https://img.vietqr.io/image/MB-0924044283-compact2.png?...&accountName=NGUYEN%20THAI%20VINH` |
| `Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` | 114 | Gateway Badge | `VIETQR • MBBANK CHUẨN NAPAS247` |
| `Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` | 124 | Bank Name | `Quân Đội (MBBank)` |
| `Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` | 129, 130 | Account Number | `0924044283` |
| `Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` | 138 | Account Holder | `NGUYEN THAI VINH` |
| `Areas\CuaHang\Views\AdminDonHang\ChiTiet.cshtml` | 9 | VietQR Endpoint | `https://img.vietqr.io/image/MB-0924044283-compact2.png?...&accountName=NGUYEN%20THAI%20VINH` |
| `Areas\CuaHang\Views\AdminDonHang\ChiTiet.cshtml` | 373 | Bank Name | `Quân Đội (MBBank)` |
| `Areas\CuaHang\Views\AdminDonHang\ChiTiet.cshtml` | 378, 379 | Account Number | `0924044283` |
| `Areas\CuaHang\Views\AdminDonHang\ChiTiet.cshtml` | 387 | Account Holder | `NGUYEN THAI VINH` |

---

### 1.4 How Campaign Details & General Donation Options Flow to the Views
1. **Public Campaign & Donation Flow (`/Home/GayQuy`)**:
   - `HomeController.GayQuy(int? year)` retrieves the active campaign (`ChienDich.TrangThaiHoatDong == true`) or queries by year.
   - It queries `CauHinhGayQuys` including `.Include(c => c.KenhDongGops).Include(c => c.NhaTaiTroGayQuys)`.
   - `GayQuy.cshtml` receives `CauHinhGayQuy` model:
     - Hero section displays `Model.Hero_TieuDe`, `Model.Hero_MoTa`.
     - Ways to contribute loops over `Model.KenhDongGops.OrderBy(k => k.ThuTu)`.
     - Clicking the QR button executes inline JS with values directly passed as arguments into `openQrModal()`.
2. **Fundraising Store Order & Payment Flow (`/CuaHang/GioHang`)**:
   - In `GioHang\Index.cshtml`, customers can select voluntary extra donations: `model.TienUngHoThem` (+20k, +50k, +100k, +200k).
   - In `GioHangController.DatHang`, total calculation is: `TongTienHang + TienUngHoThem + PhiVanChuyen`.
   - An order is persisted to `DonHangGayQuy` with code format `GQyyyyMMdd-XXXX`.
   - `DatHangThanhCong.cshtml` receives `DonHangGayQuy` model, but currently **does NOT receive any bank configuration model**; all bank and QR info is hardcoded in the `.cshtml` view itself.

---

### 1.5 VietQR Generation vs. Static Images
1. **Donation Subsystem (`/Home/GayQuy`)**:
   - Currently uses a **static image URL** (`kenh.LinkAnhQR`).
   - Admin uploads an image file (`FileAnhQR` -> `uploads/gayquy_qr/`) or defaults to the hardcoded ImageKit link.
   - **Gap**: Cannot dynamically generate VietQR on the fly for arbitrary banks, amounts, or custom campaign memos; requires manual graphic creation.
2. **Store Subsystem (`DatHangThanhCong.cshtml`)**:
   - Uses the **dynamic VietQR API** (`https://img.vietqr.io/image/{BANK}-{ACC}-{TEMPLATE}.png?amount={AMOUNT}&addInfo={MEMO}&accountName={NAME}`).
   - Dynamic parameters today: `amount` (from `Model.TongThanhToan`) and `addInfo` (from `Model.MaDonHang`).
   - Hardcoded parameters today: Bank (`MB`), Account (`0924044283`), Template (`compact2`), and Account Name (`NGUYEN THAI VINH`).
3. **Integration Potential**:
   - Both subsystems can fully utilize dynamic VietQR generation using the standard `img.vietqr.io` specification.
   - Parameters to parameterize dynamically:
     - Bank BIN or Bank Acronym (e.g., `MB`, `VCB`, `ICB`, `TCB`, `ACB`, `BIDV`).
     - Account Number.
     - Account Holder Name (URL-encoded).
     - Template type (`compact2`, `compact`, `qr_only`, `print`).
     - Transfer Memo / Syntax template (e.g. `[HoVaTen]_[DonVi]_gopLangbiang` for general donation, or `[MaDonHang]` for store orders).
     - Custom QR Image URL (as fallback/override if the admin explicitly uploads a branded custom poster or MoMo/ZaloPay QR).

---

### 1.6 Verification of Build & Existing Test Suite
- **Build Verification**:
  ```powershell
  dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen\WEB_TinhNguyen.csproj
  ```
  **Result**: Build succeeded, 0 errors, 4 NU1902/NU1903 transitive package warnings. Tailwind CSS generated in 1.5s.
- **E2E Test Suite Execution**:
  ```powershell
  dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
  ```
  **Result**: 337 Tests executed across Tiers 1–5: **337 Passed, 0 Failed, 0 Skipped** in 4.76s.

---

## 2. Logic Chain

1. **Premise 1**: The user request (R1, R2) demands a dynamic administration interface in Admin allowing administrators to Create, Read, Update, Delete (CRUD) QR configurations, bank accounts, recipient names, transfer syntax, QR images/VietQR parameters, and payment instructions.
2. **Premise 2**: Observation 1.1–1.3 demonstrates that although `AdminGayQuyController` has an existing repeater for `KenhDongGop` with bank fields, it is siloed exclusively to `/Home/GayQuy` under `CauHinhGayQuy`, while `/CuaHang` (`DatHangThanhCong.cshtml` and `AdminDonHang\ChiTiet.cshtml`) completely bypasses the database and hardcodes `MB-0924044283-compact2.png` and `NGUYEN THAI VINH`.
3. **Premise 3**: Observation 1.3 reveals a critical domain inconsistency: `/Home/GayQuy` default recipient is **"NGUYỄN ANH HÀO"**, while the store recipient is **"NGUYEN THAI VINH"**, both using the same phone/account number `0924044283`.
4. **Premise 4**: Observation 1.5 confirms that `img.vietqr.io` allows full dynamic URL interpolation for bank, account, template, name, and memo, and can also fall back to custom uploaded QR images when specified.
5. **Deduction**:
   - To satisfy R1 and R2 cleanly without breaking separation of concerns or duplicating data, the system needs either:
     - **Option A (Recommended)**: A unified entity `CauHinhThanhToanQR` (or `CauHinhQR`) with dedicated Admin CRUD, activation flags (`ApDungChoCuaHang`, `ApDungChoQuyenGop`, `TrangThaiHoatDong`), and a shared service (`IQrConfigService` / `ThanhToanQRService`) consumed by both CuaHang and GayQuy.
     - **Option B**: Expose the primary active `KenhDongGop` (or primary QR channel) from `CauHinhGayQuy` to CuaHang. However, Option B creates tight coupling between the independent `Areas/CuaHang` and the yearly `ChienDich` configuration.
   - Option A is significantly cleaner, extensible, and aligns directly with R1 ("Cung cấp giao diện quản trị Admin cho phép thêm, sửa, xóa các cấu hình mã QR thanh toán/ủng hộ").

---

## 3. Caveats

1. **Existing Test Suite String Assertions (`Tier1_AdminOrderTests.cs`)**:
   - In `tests\E2ETests\Tier1\Tier1_AdminOrderTests.cs` (lines 232–247):
     ```csharp
     [E2ETest(1, 18, "F18_T15: AdminDonHang/ChiTiet.cshtml displays VietQR transfer instructions and bank account")]
     public void F18_T15_AdminDonHangChiTietDisplaysVietQrAndAccount()
     {
         var detailPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "AdminDonHang", "ChiTiet.cshtml");
         var content = File.ReadAllText(detailPath);

         Assert.True(content.Contains("0924044283"), "ChiTiet.cshtml must display charity bank account 0924044283");
         Assert.True(content.Contains("vietqr") || content.Contains("VietQR"), "ChiTiet.cshtml must embed VietQR payment integration");
         Assert.True(content.Contains("NGUYEN THAI VINH") || content.Contains("MBBank") || content.Contains("Quân Đội"), "ChiTiet.cshtml must display bank transfer recipient information");
     }
     ```
   - *Impact*: Test `F18_T15` specifically inspects the file text of `ChiTiet.cshtml` for `"0924044283"` and `"MBBank"` / `"NGUYEN THAI VINH"`.
   - *Mitigation during implementation*: When switching `ChiTiet.cshtml` to dynamic values, the Razor view or default fallback model must retain `"0924044283"` as default fallback value or comment reference, or test `F18_T15` must be updated to inspect the dynamic service oracle.
2. **Offline Resilience & In-Memory Fallback Requirement**:
   - The application supports both SQL Server database mode and In-Memory fallback mode via `DbInitializer` (when database is offline).
   - Any new QR configuration entity and service **MUST** include in-memory fallback defaults in `DbInitializer` so that the site continues to run without SQL Server.
3. **ImageKit vs. Local Uploads**:
   - Seed data currently references external ImageKit URLs (`https://ik.imagekit.io/...`). Admin file uploads are stored locally in `/uploads/gayquy_qr/` or `/uploads/qr/`. Both formats must be supported.

---

## 4. Conclusion & Actionable Blueprint

### 4.1 Recommended Implementation Blueprint

#### Step 1: Entity & Database Modeling
Create `MoHinh\CauHinhThanhToanQR.cs` (or `CauHinhQR.cs`):
```csharp
[Table("CauHinhThanhToanQR")]
public class CauHinhThanhToanQR
{
    [Key]
    public int IDCauHinhQR { get; set; }
    [Required, StringLength(150)]
    public string TenCauHinh { get; set; } = "Tài khoản mặc định";
    [Required, StringLength(50)]
    public string MaNganHang { get; set; } = "MB"; // MB, VCB, TCB, ACB, ICB, etc.
    [Required, StringLength(150)]
    public string TenNganHang { get; set; } = "Quân Đội (MBBank)";
    [Required, StringLength(50)]
    public string SoTaiKhoan { get; set; } = "0924044283";
    [Required, StringLength(150)]
    public string TenChuTaiKhoan { get; set; } = "NGUYỄN ANH HÀO";
    [StringLength(255)]
    public string CuPhapChuyenKhoan { get; set; } = "{MADON}";
    [StringLength(50)]
    public string MauGiaoDienVietQR { get; set; } = "compact2";
    public string? DuongDanAnhQRTuyChon { get; set; }
    public string? HuongDanThanhToan { get; set; }
    public string? GhiChuChanTrang { get; set; }
    public bool ApDungChoCuaHang { get; set; } = true;
    public bool ApDungChoQuyenGop { get; set; } = true;
    public bool TrangThaiHoatDong { get; set; } = true;
    public int ThuTu { get; set; } = 1;
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime? NgayCapNhat { get; set; }
}
```
Add `DbSet<CauHinhThanhToanQR> CauHinhThanhToanQRs { get; set; }` to `ApplicationDbContext.cs`.

#### Step 2: In-Memory Seed & Fallback in `DbInitializer.cs`
- Store `_inMemoryQrConfigs` list seeded with the active default MBBank `0924044283` account.
- Add CRUD helper methods: `GetInMemoryQrConfigs()`, `GetActiveStoreQrConfig()`, `GetActiveDonationQrConfigs()`, `AddInMemoryQrConfig()`, `UpdateInMemoryQrConfig()`, `DeleteInMemoryQrConfig()`.

#### Step 3: Admin Management Module (`AdminCauHinhQRController`)
- **Controller**: `BoDieuKhien\AdminCauHinhQRController.cs`
  - `Index`: Table of QR configs with active toggles and actions (Create, Edit, Delete, SetDefault).
  - `Create` [GET/POST]: Form to add bank, account, holder, memo, upload custom QR, toggle store/donation application.
  - `Edit` [GET/POST]: Modify settings.
  - `Delete` [POST]: Safely delete config.
  - `ToggleActive` [POST]: Quick toggle status.
- **Views**:
  - `GiaoDien\AdminCauHinhQR\Index.cshtml`
  - `GiaoDien\AdminCauHinhQR\Create.cshtml`
  - `GiaoDien\AdminCauHinhQR\Edit.cshtml`
- **Navigation Integration**:
  - Add link to `GiaoDien\Shared\_AdminLayout.cshtml` sidebar under "Nội dung" or "Cài đặt".
  - Add shortcut card to `GiaoDien\Admin\Index.cshtml` dashboard.

#### Step 4: VietQR Dynamic Helper / Service
Create `Helpers\VietQrHelper.cs` (or service):
```csharp
public static class VietQrHelper
{
    public static string GenerateUrl(string bankCode, string accountNumber, decimal? amount, string? memo, string? accountName, string template = "compact2")
    {
        var cleanBank = string.IsNullOrWhiteSpace(bankCode) ? "MB" : bankCode.Trim();
        var cleanAcc = string.IsNullOrWhiteSpace(accountNumber) ? "0924044283" : accountNumber.Trim();
        var cleanTemplate = string.IsNullOrWhiteSpace(template) ? "compact2" : template.Trim();
        
        var query = new List<string>();
        if (amount.HasValue && amount.Value > 0) query.Add($"amount={(long)amount.Value}");
        if (!string.IsNullOrWhiteSpace(memo)) query.Add($"addInfo={Uri.EscapeDataString(memo.Trim())}");
        if (!string.IsNullOrWhiteSpace(accountName)) query.Add($"accountName={Uri.EscapeDataString(accountName.Trim())}");
        
        var queryString = query.Any() ? "?" + string.Join("&", query) : "";
        return $"https://img.vietqr.io/image/{cleanBank}-{cleanAcc}-{cleanTemplate}.png{queryString}";
    }
}
```

#### Step 5: Updating User-Facing Views
1. **`DatHangThanhCong.cshtml` & `AdminDonHang\ChiTiet.cshtml`**:
   - Query the active store QR configuration dynamically.
   - Generate VietQR URL dynamically via `VietQrHelper.GenerateUrl` using `qrConfig.MaNganHang`, `qrConfig.SoTaiKhoan`, `Model.TongThanhToan`, `Model.MaDonHang`, and `qrConfig.TenChuTaiKhoan`.
   - Render the table rows for Bank Name, Account Number, Account Name, and instructions from `qrConfig`.
2. **`GayQuy.cshtml`**:
   - For donation channels, pull the active donation QR configuration.
   - In `#qrModal`, if a custom static QR image is not uploaded, automatically render the dynamic VietQR URL generated with the campaign transfer memo syntax.

---

## 5. Verification Method

### Independent Verification Commands
1. **Compilation Test**:
   ```powershell
   dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen\WEB_TinhNguyen.csproj
   ```
   *Expected*: Build succeeds with 0 errors.
2. **Regression & E2E Test Suite**:
   ```powershell
   dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
   ```
   *Expected*: All 337 existing tests continue to pass.
3. **Files to Inspect**:
   - `WEB_TinhNguyen\BoDieuKhien\AdminGayQuyController.cs` (lines 58–86)
   - `WEB_TinhNguyen\GiaoDien\Home\GayQuy.cshtml` (lines 137–155, 240–288)
   - `WEB_TinhNguyen\Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` (lines 4–155)
   - `WEB_TinhNguyen\Areas\CuaHang\Views\AdminDonHang\ChiTiet.cshtml` (lines 7–10, 351–398)
   - `WEB_TinhNguyen\tests\E2ETests\Tier1\Tier1_AdminOrderTests.cs` (lines 232–247)

### Invalidation Conditions
- Any change that hardcodes alternative bank credentials into views or controllers.
- Any implementation that breaks compatibility with `tests\E2ETests` or fails `dotnet build`.
- Missing in-memory fallback when SQL Server is offline.
