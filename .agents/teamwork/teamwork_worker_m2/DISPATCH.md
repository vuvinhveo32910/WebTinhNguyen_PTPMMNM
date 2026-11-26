## 2026-11-06T08:28:17Z
You are teamwork_worker_m2.
Your working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_worker_m2\
Authoritative user request: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
Scope document: D:\web_tinh_nguyen\PROJECT.md (Milestone 2)

Read ORIGINAL_REQUEST.md and PROJECT.md before starting work.
Refer to Milestone 1 deliverables and contracts:
- `WEB_TinhNguyen/MoHinh/CauHinhThanhToanQR.cs`
- `WEB_TinhNguyen/BoDieuKhien/VietQrHelper.cs`
- `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs` (`GetActiveStoreQrConfig()`, `GetActiveDonationQrConfigs()`)

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

EXCLUSIVE WRITE FILE OWNERSHIP:
- WEB_TinhNguyen/Areas/CuaHang/Controllers/GioHangController.cs
- WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs
- WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/DatHangThanhCong.cshtml
- WEB_TinhNguyen/Areas/CuaHang/Views/AdminDonHang/ChiTiet.cshtml
- WEB_TinhNguyen/BoDieuKhien/HomeController.cs
- WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml

TASK REQUIREMENTS (Milestone 2 - Requirement R2):
1. In `WEB_TinhNguyen/Areas/CuaHang/Controllers/GioHangController.cs`:
   - In `DatHangThanhCong(int id)`: query the active Store payment QR config (`ApDungChoCuaHang == true` and `TrangThaiHoatDong == true`, sorted by `ThuTu`, `IDCauHinhQR`). Support dual-mode: check `_context.Database.CanConnect()` or try querying `_context.CauHinhThanhToanQRs`; fallback to `DbInitializer.GetActiveStoreQrConfig()`.
   - Pass the resolved configuration to the view via `ViewBag.CauHinhQR`.
2. In `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs`:
   - In `ChiTiet(int id)`: query the active Store payment QR config via dual-mode and pass to the view via `ViewBag.CauHinhQR`.
3. In `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/DatHangThanhCong.cshtml`:
   - Bind dynamic values from `ViewBag.CauHinhQR as WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR`:
     - Bank Name: `@(qrConfig?.TenNganHang ?? "Quân Đội (MBBank)")`
     - Account Number: `@(qrConfig?.SoTaiKhoan ?? "0924044283")` (both text display and copyToClipboard function call)
     - Account Holder: `@(qrConfig?.TenChuTaiKhoan ?? "NGUYEN THAI VINH")`
     - Instructions text: if `qrConfig?.HuongDanThanhToan` is present, display it; otherwise display standard guide.
     - QR Image: generate dynamic QR URL using `qrConfig.GetQrUrl(cleanAmount, Model.MaDonHang, qrConfig.TenChuTaiKhoan)` or fallback.
   - **CRITICAL BACKWARD COMPATIBILITY**: Retain fallback string tokens `MB-0924044283`, `NGUYEN%20THAI%20VINH`, `img.vietqr.io`, `COD` in the Razor template (e.g. `var defaultQr = $"https://img.vietqr.io/image/MB-0924044283-compact2.png?amount={cleanAmount}&addInfo={cleanMemo}&accountName=NGUYEN%20THAI%20VINH";`) so test assertions in `Tier5_ChallengerM3_2Harness.cs` pass while live rendering binds `qrConfig`.
4. In `WEB_TinhNguyen/Areas/CuaHang/Views/AdminDonHang/ChiTiet.cshtml`:
   - Bind dynamic values from `ViewBag.CauHinhQR as WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR`:
     - Bank Name, Account Number, Account Holder, dynamic QR URL, and clipboard copy scripts.
     - Retain fallback constants `0924044283`, `vietqr`, `NGUYEN THAI VINH` in view markup so `Tier1_AdminOrderTests.cs` passes.
5. In `WEB_TinhNguyen/BoDieuKhien/HomeController.cs`:
   - In `GayQuy(int? year)`: query active Donation payment QR config(s) (`ApDungChoQuyenGop == true` and `TrangThaiHoatDong == true`, sorted by `ThuTu`, `IDCauHinhQR`). Support dual-mode: query DB or fallback to `DbInitializer.GetActiveDonationQrConfigs()`.
   - Pass to view via `ViewBag.CauHinhQRDongGop`.
6. In `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`:
   - Update donation channels and `#qrModal`:
     - When donation payment config(s) exist, display bank name, account number, account holder, transfer memo syntax dynamically.
     - In `#qrModal`, ensure `#qrModalBank`, `#qrModalAccNum`, `#qrModalAccName`, `#qrModalNote`, and `#qrModalImg` are bound dynamically.
     - Support dynamic VietQR generation via `VietQrHelper.GenerateUrl` if channel has no custom image.
7. Verification:
   - Run `dotnet build WEB_TinhNguyen/WEB_TinhNguyen.csproj` -> must succeed with 0 errors.
   - Run `dotnet run --project tests/E2ETests/E2ETests.csproj` -> all tests must pass (including `DynamicQR_F07_T01` and `DynamicQR_F07_T02` with 0 skipped and 0 failed!).
8. Write comprehensive handoff report to `D:\web_tinh_nguyen\.agents\teamwork\teamwork_worker_m2\handoff.md`. Include files modified, build output, test execution summary, and verification commands.
