# BRIEFING — 2026-11-06T08:15:00Z

## Mission
Implement Milestone 1: Dynamic QR Payment Configuration & VietQR Foundation for WEB_TinhNguyen.

## 🔒 My Identity
- Archetype: worker
- Roles: implementer, qa, specialist
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_worker_m1\
- Original parent: 3ebaa881-f84a-47af-9d40-eda26cf6ddcc
- Milestone: Milestone 1 (QR Payment Configuration & VietQR Foundation)

## 🔒 Key Constraints
- Exclusive write ownership limited to:
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
- MANDATORY INTEGRITY WARNING: DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task.
- Verification: dotnet build WEB_TinhNguyen/WEB_TinhNguyen.csproj must succeed with 0 errors. All 381 E2E tests must pass.
- Never write source code or test code to `.agents/teamwork/`.

## Current Parent
- Conversation ID: 3ebaa881-f84a-47af-9d40-eda26cf6ddcc
- Updated: 2026-11-06T08:15:00Z

## Task Summary
- **What to build**: Dynamic QR payment configuration system with VietQR standard generation, admin CRUD management, fallback in-memory store, idempotent SQL creation, and navigation integration.
- **Success criteria**: 
  1. `CauHinhThanhToanQR` entity with GetQrUrl method.
  2. `ApplicationDbContext` with `CauHinhThanhToanQRs` DbSet.
  3. `DbInitializer` with idempotent table creation, default MBBank seed, thread-safe in-memory store and CRUD helpers.
  4. `VietQrHelper` for compact2/custom VietQR URL generation.
  5. `AdminCauHinhQRController` supporting DB and in-memory dual-mode fallback, image upload.
  6. Admin views (`Index`, `Create`, `Edit`).
  7. Menu link in `_AdminLayout.cshtml`.
  8. Static file support for `/uploads/` in `Program.cs`.
  9. Pass `dotnet build` and 381 E2E tests.

## Key Decisions Made
- `VietQrHelper.cs`: Implemented standard Napas247 VietQR REST API pattern with URL parameter encoding, defaulting to MB / 0924044283 / NGUYEN THAI VINH / compact2, and included PopularBanks dictionary.
- `CauHinhThanhToanQR.cs`: Designed with data annotations, custom image path fallback, and dynamic `GetQrUrl(amount, memo, accountName)`.
- `ApplicationDbContext.cs`: Registered `CauHinhThanhToanQRs` DbSet and configured table mapping to `[dbo].[CauHinhThanhToanQR]`.
- `DbInitializer.cs`: Added idempotent SQL table creation, default MBBank seed, and thread-safe in-memory CRUD methods (`GetInMemoryQrConfigs`, `GetActiveStoreQrConfig`, `GetActiveDonationQrConfigs`, `AddInMemoryQrConfig`, `UpdateInMemoryQrConfig`, `DeleteInMemoryQrConfig`, `ToggleActiveInMemoryQrConfig`).
- `Program.cs`: Registered `app.UseStaticFiles()` right after `app.UseAuthorization()` to guarantee uploaded QR images in `wwwroot/uploads/` are immediately servable.
- `AdminCauHinhQRController.cs`: Dual-mode controller implementing Index, Create, Edit, ToggleActive, Delete actions, and image saving under `/uploads/qr/`.
- Admin Razor Views (`Index.cshtml`, `Create.cshtml`, `Edit.cshtml`): Designed with modern Tailwind CSS matching `_AdminLayout`, including instant live preview, bank quick selector, toggle switches, and modal viewer.
- `_AdminLayout.cshtml`: Added sidebar navigation link with `fa-qrcode` icon and text "Cấu hình Thanh toán QR".

## Artifact Index
- handoff.md — Final handoff report for Milestone 1.
- progress.md — Liveness heartbeat and step-by-step progress.

## Change Tracker
- **Files modified**:
  - `WEB_TinhNguyen/MoHinh/CauHinhThanhToanQR.cs` (new)
  - `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs` (DbSet & OnModelCreating)
  - `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs` (DDL, Seed & In-Memory CRUD)
  - `WEB_TinhNguyen/BoDieuKhien/VietQrHelper.cs` (new)
  - `WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs` (new)
  - `WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/Index.cshtml` (new)
  - `WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/Create.cshtml` (new)
  - `WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/Edit.cshtml` (new)
  - `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` (menu link)
  - `WEB_TinhNguyen/Program.cs` (UseStaticFiles)
- **Build status**: Succeeded with 0 errors.
- **Pending issues**: None.

## Quality Status
- **Build/test result**: Pass (381 Passed | 0 Failed | 1 Skipped for M2).
- **Lint status**: Clean (0 compiler errors).
- **Tests added/modified**: Full suite in `Tier1_DynamicQrPaymentTests.cs` executed.

## Loaded Skills
- None specified.
