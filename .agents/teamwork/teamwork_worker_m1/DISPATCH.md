# Dispatch Task for Worker M1 (Area & Data Foundation Implementation)

## Objective
Implement Milestone 1 (Area & Data Foundation) for the volunteer fundraising store system according to the architectural blueprints provided by Explorers 1, 2, and 3.

## Exclusive Write Ownership
You exclusively own and may create/modify only the following files:
1. `WEB_TinhNguyen/Program.cs`
2. `WEB_TinhNguyen/Data/ApplicationDbContext.cs`
3. `tailwind.config.js`
4. `WEB_TinhNguyen/Areas/CuaHang/Models/SanPhamGayQuy.cs`
5. `WEB_TinhNguyen/Areas/CuaHang/Models/DonHangGayQuy.cs`
6. `WEB_TinhNguyen/Areas/CuaHang/Models/ChiTietDonHangGayQuy.cs`
7. `WEB_TinhNguyen/Areas/CuaHang/Models/CartItem.cs`
8. `WEB_TinhNguyen/Areas/CuaHang/Models/CartViewModel.cs`
9. `WEB_TinhNguyen/Areas/CuaHang/Models/CheckoutViewModel.cs`
10. `WEB_TinhNguyen/Areas/CuaHang/Models/SanPhamCrudViewModel.cs`
11. `WEB_TinhNguyen/Areas/CuaHang/Helpers/SessionExtensions.cs`
12. `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`
13. `WEB_TinhNguyen/Areas/CuaHang/Views/_ViewImports.cshtml`
14. `WEB_TinhNguyen/Areas/CuaHang/Views/_ViewStart.cshtml`

## Input Blueprints & Reports
- ORIGINAL_REQUEST.md: `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/ORIGINAL_REQUEST.md`
- Master PROJECT.md: `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/orchestrator/PROJECT.md`
- Explorer 1 Report (Program.cs, ApplicationDbContext.cs, tailwind.config.js): `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_m1_1/handoff.md`
- Explorer 2 Report (Entities & ViewModels): `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_m1_2/handoff.md`
- Explorer 3 Report (SessionExtensions.cs, DbInitializer.cs, Views setup): `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_m1_3/handoff.md`

## Verification Requirements
You MUST run the build command:
`XDG_RUNTIME_DIR=/tmp dotnet build /home/hutech/Vinh_Offline/WEB_TinhNguyen/WEB_TinhNguyen.slnx`
and document that it compiles with 0 errors (`Build succeeded. 0 Error(s)`).

## Mandatory Integrity Warning
> DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

## Completion Output
Write `handoff.md` in `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_worker_m1/handoff.md` following the Handoff Protocol with verified build results and layout compliance.
Send a message to the orchestrator (Recipient: 108b9d81-1dd6-495f-84eb-99dbb197aebe) when done.


## 2026-11-03T12:51:38Z
You are teamwork_worker_m1.
Your working directory is: /home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_worker_m1
Your parent is the Project Orchestrator (Conversation ID: 108b9d81-1dd6-495f-84eb-99dbb197aebe).

MANDATORY INSTRUCTIONS:
1. First read the original user request at: /home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/ORIGINAL_REQUEST.md
2. Read the master project document at: /home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/orchestrator/PROJECT.md
3. Read your specific dispatch instructions at: /home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_worker_m1/DISPATCH.md
4. Read the 3 Explorer handoff reports for Milestone 1:
   - Explorer 1: /home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_m1_1/handoff.md
   - Explorer 2: /home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_m1_2/handoff.md
   - Explorer 3: /home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_m1_3/handoff.md
5. Update your progress.md regularly with "Last visited: [timestamp]" as your heartbeat.
6. Implement all required files per your exclusive write ownership:
   - Program.cs, ApplicationDbContext.cs, tailwind.config.js
   - Areas/CuaHang/Models/SanPhamGayQuy.cs, DonHangGayQuy.cs, ChiTietDonHangGayQuy.cs, CartItem.cs, CartViewModel.cs, CheckoutViewModel.cs, SanPhamCrudViewModel.cs
   - Areas/CuaHang/Helpers/SessionExtensions.cs, DbInitializer.cs
   - Areas/CuaHang/Views/_ViewImports.cshtml, _ViewStart.cshtml
7. Verify compilation: run `XDG_RUNTIME_DIR=/tmp dotnet build /home/hutech/Vinh_Offline/WEB_TinhNguyen/WEB_TinhNguyen.slnx` and confirm 0 errors.

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

8. Write handoff.md following the Handoff Protocol and send a message to the orchestrator (Recipient: 108b9d81-1dd6-495f-84eb-99dbb197aebe).


## 2026-11-06T07:50:10Z
You are teamwork_worker_m1.
Your working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_worker_m1\
Authoritative user request: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
Scope document: D:\web_tinh_nguyen\PROJECT.md (Milestone 1)

Read ORIGINAL_REQUEST.md and PROJECT.md before starting work.
Refer to explorer survey findings:
- D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1\handoff.md
- D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_2\handoff.md
- D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_3\handoff.md

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

EXCLUSIVE WRITE FILE OWNERSHIP:
- WEB_TinhNguyen/MoHinh/CauHinhThanhToanQR.cs (new)
- WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs (register DbSet)
- WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs (idempotent table creation + In-Memory store & CRUD methods)
- WEB_TinhNguyen/BoDieuKhien/VietQrHelper.cs (new)
- WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs (new)
- WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/Index.cshtml (new)
- WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/Create.cshtml (new)
- WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/Edit.cshtml (new)
- WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml (add menu link)
- WEB_TinhNguyen/Program.cs (ensure UseStaticFiles for uploads)

TASK REQUIREMENTS (Milestone 1):
1. Create `CauHinhThanhToanQR` entity in `WEB_TinhNguyen/MoHinh/CauHinhThanhToanQR.cs` with:
   - `IDCauHinhQR` (int PK identity)
   - `TenCauHinh` (string, required, max 150)
   - `MaNganHang` (string, required, max 50, e.g. "MB", "VCB", "TCB", "ICB", "ACB", "BIDV")
   - `TenNganHang` (string, required, max 150)
   - `SoTaiKhoan` (string, required, max 50)
   - `TenChuTaiKhoan` (string, required, max 150)
   - `CuPhapChuyenKhoan` (string?, max 255)
   - `MauVietQR` (string, max 50, default "compact2")
   - `DuongDanAnhQRTuyChon` (string? - path to uploaded custom QR or external URL)
   - `SuDungVietQR` (bool, default true)
   - `HuongDanThanhToan` (string? - guidance text)
   - `GhiChuThem` (string?)
   - `ApDungChoCuaHang` (bool, default true)
   - `ApDungChoQuyenGop` (bool, default true)
   - `TrangThaiHoatDong` (bool, default true)
   - `ThuTu` (int, default 1)
   - `NgayTao` (DateTime, default DateTime.Now), `NgayCapNhat` (DateTime?)
   - Method `GetQrUrl(decimal? amount, string? memo, string? accountName)`
2. Add `DbSet<CauHinhThanhToanQR> CauHinhThanhToanQRs { get; set; }` in `ApplicationDbContext.cs`.
3. In `DbInitializer.cs`:
   - Add idempotent table creation SQL: `IF OBJECT_ID(N'[dbo].[CauHinhThanhToanQR]', N'U') IS NULL CREATE TABLE ...`
   - Seed default record: MBBank, `0924044283`, `NGUYEN THAI VINH`, `Quân Đội (MBBank)`.
   - Implement thread-safe in-memory store collection and CRUD helpers: `GetInMemoryQrConfigs()`, `GetActiveStoreQrConfig()`, `GetActiveDonationQrConfigs()`, `AddInMemoryQrConfig()`, `UpdateInMemoryQrConfig()`, `DeleteInMemoryQrConfig()`, `ToggleActiveInMemoryQrConfig()` so the app operates flawlessly in offline/in-memory mode.
4. Implement `VietQrHelper` in `WEB_TinhNguyen/BoDieuKhien/VietQrHelper.cs`:
   - `GenerateUrl(string maNganHang, string soTaiKhoan, decimal? amount, string? memo, string? tenChuTaiKhoan, string template = "compact2")`
5. Implement `AdminCauHinhQRController` in `WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs`:
   - `Index` (GET): list QR configs, table with toggle active, edit, delete.
   - `Create` (GET & POST): form with all fields + file upload for QR image (`IFormFile FileAnhQR` -> `/uploads/qr/`).
   - `Edit` (GET & POST): edit fields, replace/keep QR image.
   - `Delete` (POST): delete config.
   - `ToggleActive` (POST): toggle active flag.
   - Support both DbContext (when DB is connected) and DbInitializer fallback (when offline).
6. Create Razor views in `WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/`:
   - `Index.cshtml`, `Create.cshtml`, `Edit.cshtml` following Tailwind CSS styling of `_AdminLayout`.
7. Add menu link in `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` with `fa-qrcode` icon and text "Cấu hình Thanh toán QR".
8. Verify compilation and test suite:
   - Run `dotnet build WEB_TinhNguyen/WEB_TinhNguyen.csproj` -> must succeed with 0 errors.
   - Run `dotnet run --project tests/E2ETests/E2ETests.csproj` -> all 337 tests must pass.
9. Write handoff report to `D:\web_tinh_nguyen\.agents\teamwork\teamwork_worker_m1\handoff.md`. Include commands executed, build output, test results, and files modified.
