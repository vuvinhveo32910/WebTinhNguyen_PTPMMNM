# BRIEFING — 2026-11-06T08:41:00Z

## Mission
Implement dynamic QR resolution in controllers and dynamic view binding across Store Checkout, Admin Order Details, and Donation pages for Milestone 2 (R2).

## 🔒 My Identity
- Archetype: implementer
- Roles: implementer, qa
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_worker_m2
- Original parent: 3ebaa881-f84a-47af-9d40-eda26cf6ddcc
- Milestone: Milestone 2

## 🔒 Key Constraints
- EXCLUSIVE WRITE FILE OWNERSHIP:
  - WEB_TinhNguyen/Areas/CuaHang/Controllers/GioHangController.cs
  - WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs
  - WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/DatHangThanhCong.cshtml
  - WEB_TinhNguyen/Areas/CuaHang/Views/AdminDonHang/ChiTiet.cshtml
  - WEB_TinhNguyen/BoDieuKhien/HomeController.cs
  - WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml
- Mandatory integrity: genuine dynamic resolution and binding, no hardcoded cheating.
- Backward compatibility: Retain fallback tokens in Razor templates (`MB-0924044283`, `NGUYEN%20THAI%20VINH`, `img.vietqr.io`, `COD`, `0924044283`, `vietqr`, `NGUYEN THAI VINH`).

## Current Parent
- Conversation ID: 3ebaa881-f84a-47af-9d40-eda26cf6ddcc
- Updated: 2026-11-06T08:28:17Z

## Task Summary
- **What to build**: Dynamic QR configuration resolution in GioHangController, AdminDonHangController, HomeController; dynamic bindings in DatHangThanhCong.cshtml, ChiTiet.cshtml, and GayQuy.cshtml.
- **Success criteria**: `dotnet build WEB_TinhNguyen/WEB_TinhNguyen.csproj` 0 errors; `dotnet run --project tests/E2ETests/E2ETests.csproj` 100% pass (DynamicQR_F07_T01 and DynamicQR_F07_T02 pass, 0 skipped, 0 failed).
- **Interface contracts**: PROJECT.md, CauHinhThanhToanQR.cs, VietQrHelper.cs, DbInitializer.cs
- **Code layout**: ASP.NET Core MVC

## Key Decisions Made
- Dual-mode queries in `GioHangController.DatHangThanhCong` and `AdminDonHangController.ChiTiet`: query `_context.CauHinhThanhToanQRs.Where(q => q.ApDungChoCuaHang && q.TrangThaiHoatDong)` with fallback to `DbInitializer.GetActiveStoreQrConfig()`.
- Dual-mode queries in `HomeController.GayQuy`: query `_context.CauHinhThanhToanQRs.Where(q => q.ApDungChoQuyenGop && q.TrangThaiHoatDong)` with fallback to `DbInitializer.GetActiveDonationQrConfigs()`.
- Maintained fallback tokens `MB-0924044283`, `NGUYEN%20THAI%20VINH`, `img.vietqr.io`, `COD` in `DatHangThanhCong.cshtml`, and `0924044283`, `vietqr`, `NGUYEN THAI VINH` in `ChiTiet.cshtml` for backward test harness compatibility.

## Artifact Index
- `.agents/teamwork/teamwork_worker_m2/handoff.md` — Final 5-component handoff report

## Change Tracker
- **Files modified**:
  - `WEB_TinhNguyen/Areas/CuaHang/Controllers/GioHangController.cs`: Dual-mode QR query in `DatHangThanhCong`, passes `ViewBag.CauHinhQR`.
  - `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs`: Dual-mode QR query in `ChiTiet`, passes `ViewBag.CauHinhQR`.
  - `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/DatHangThanhCong.cshtml`: Dynamic QR rendering and copy script bindings from `ViewBag.CauHinhQR`.
  - `WEB_TinhNguyen/Areas/CuaHang/Views/AdminDonHang/ChiTiet.cshtml`: Dynamic QR rendering and copy script bindings from `ViewBag.CauHinhQR`.
  - `WEB_TinhNguyen/BoDieuKhien/HomeController.cs`: Dual-mode QR query in `GayQuy`, passes `ViewBag.CauHinhQRDongGop`.
  - `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`: Dynamic donation channel cards and `#qrModal` dynamic binding with VietQR fallback.
- **Build status**: PASS (0 errors, 47 pre-existing warnings)
- **Pending issues**: None

## Quality Status
- **Build/test result**: PASS. All 419 E2E tests passed (100% pass, 0 failed, 0 skipped).
- **Lint status**: 0 compile errors.
- **Tests added/modified**: Unlocked and passed `DynamicQR_F07_T01` and `DynamicQR_F07_T02` with 0 skipped.

## Loaded Skills
- none
