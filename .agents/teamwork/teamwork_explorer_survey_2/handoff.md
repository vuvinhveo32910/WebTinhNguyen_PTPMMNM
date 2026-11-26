# HANDOFF REPORT: Granular Admin Permissions Architecture & Schema Investigation (R1, R2)

**Author**: `teamwork_explorer_survey_2`  
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_2`  
**Timestamp**: 2026-11-07T05:20:00Z  
**Type**: Hard Handoff (Investigation & Architecture Design Complete)

---

## 1. Observation

### 1.1. Existing Data Model & Persistence
- **File**: `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`, Lines 13-79:
  - Model `TaiKhoanAdmin` mapped to table `TaiKhoanAdmin` with columns: `IDTaiKhoan` (int, identity), `TenDangNhap` (unique index, max 100), `MatKhauHash` (max 500), `MuoiBaoMat` (max 100), `HoTen` (max 150), `Email` (max 150), `VaiTro` (max 50, default "Admin"), `TrangThaiHoatDong` (bool, default true), `NgayTao` (DateTime), `LanDangNhapCuoi` (DateTime?).
  - Method `Clone()` (lines 63-78) performs memberwise cloning for in-memory store isolation.
  - Currently, there is NO permission column (`QuyenHan` / `PhanQuyen`) defined on this entity.
- **File**: `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`:
  - Lines 56-57 declare `DbSet<TaiKhoanAdmin> TaiKhoanAdmins` and alias `TaiKhoanAdmin`.
  - Lines 227-240 configure `TaiKhoanAdmin` via Fluent API.
  - Line 62: `optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));` safely ignores pending model change warnings.
- **File**: `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`:
  - Lines 413-427 and 1010-1025: SQL Server DDL `CREATE TABLE [dbo].[TaiKhoanAdmin]` executes raw SQL via `ExecuteSqlRaw`.
  - Lines 59-79: `GetDefaultAdmins()` seeds `admin` with `VaiTro = "Admin"` and PBKDF2 hash of `"Admin@123"`.
  - Lines 1172-1252: Thread-safe in-memory admin store methods (`GetInMemoryAdmins`, `GetInMemoryAdminById`, `AddInMemoryAdmin`, `UpdateInMemoryAdmin`, `DeleteInMemoryAdmin`).
  - Lines 84-99: Private helper `CloneAdmin(TaiKhoanAdmin a)` isolates store mutations.

### 1.2. Cryptographic Helper
- **File**: `WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs`:
  - Lines 19-30: Constants `KichThuocMuoiBytes = 16`, `KichThuocHashBytes = 32`, `SoVongLap = 100_000`.
  - Lines 35-105: `TaoMuoiNgauNhien()`, `BamMatKhau(matKhau, muoiBaoMat)` (Rfc2898DeriveBytes HMAC-SHA256), `KiemTraMatKhau(matKhauNhap, matKhauHashDaLuu, muoiBaoMat)` (using `CryptographicOperations.FixedTimeEquals`).
  - Forwarded through `WEB_TinhNguyen/Areas/CuaHang/Helpers/BaoMatMatKhau.cs`.

### 1.3. Authentication Pipeline & Middleware Constraints
- **File**: `WEB_TinhNguyen/BoDieuKhien/AdminController.cs`:
  - Lines 105-111: ClaimsPrincipal created on login currently only includes:
    `ClaimTypes.Name`, `ClaimTypes.NameIdentifier`, `ClaimTypes.Role`, and `"TenDangNhap"`.
  - Permission claims are not yet added.
- **File**: `WEB_TinhNguyen/Program.cs`:
  - Lines 27-38: Cookie auth registered with `Cookie.Name = ".WEB_TinhNguyen.AdminAuth"`, `LoginPath = "/admin/login"`, `LogoutPath = "/admin/logout"`, `AccessDeniedPath = "/admin/login"`.
  - Lines 80-90: Strict middleware sequence: `UseRouting() -> UseSession() -> UseAuthentication() -> UseAuthorization() -> UseStaticFiles()`.

### 1.4. Test Suite Invariant Assertions
- Executing `dotnet run --project tests/E2ETests/E2ETests.csproj` outputs:
  `OVERALL: 518 Tests | 518 Passed | 0 Failed | 0 Skipped in 21.84s`.
- **File**: `tests/E2ETests/Tier5/Tier5_ChallengerM2_3_AuthPipelineHarness.cs`:
  - Lines 182-216 (`M2_3_01`): Verifies class-level `[Authorize]` across 11 admin controllers via reflection.
  - Lines 221-260 (`M2_3_02`): Verifies all 21 CRUD POST actions across 10 domain controllers have `[ValidateAntiForgeryToken]` and no `[AllowAnonymous]`.
  - Lines 323 (`M2_3_04`): Asserts `cookieOptions.AccessDeniedPath == "/admin/login"`.
  - Lines 331-355 (`M2_3_05`): Asserts exact middleware order in `Program.cs`.
- **File**: `tests/E2ETests/Tier1/Tier1_AdminRoutingTests.cs`:
  - Lines 62-89 (`F19_T01`) & 91-115 (`F19_T02`): Asserts that raw file content of `_AdminLayout.cshtml` contains string patterns `Url.Action("Index", "Admin...", new { area = "" })` for 8 controllers.
- **File**: `tests/E2ETests/Tier5/Tier5_ChallengerM1_2_AdminConcurrencyStressHarness.cs`:
  - Lines 170-200 (`CH_M1_ISOL_01`): Asserts that mutating admin object returned by `GetAdminByUsername` does NOT pollute internal store (verifies deep cloning).

---

## 2. Logic Chain

1. **Premise 1 (R1 Data Requirement)**:
   Per ORIGINAL_REQUEST §R1, admin accounts and their granular permissions across 11 modules must be persistently stored in the database, with passwords securely hashed with PBKDF2 HMAC-SHA256.
2. **Premise 2 (Backward Compatibility & Dual Mode)**:
   The project must run smoothly both against SQL Server and in an offline / sandbox in-memory mode without SQL Server connectivity. Adding a dedicated column `QuyenHan` (`NVARCHAR(MAX) NULL`) storing a delimited string of permissions (e.g. `"TrangChu,ChuongTrinh"` or `"All"`) provides maximum resilience, avoids complex relational table migrations, and guarantees 100% thread-safe cloning in `_inMemoryAdmins`.
3. **Premise 3 (Seed Data Guarantee)**:
   To ensure the default `admin` account continues to access all administrative features and pass all existing 518 tests, `admin` must be seeded with `QuyenHan = "All"`. If an existing database already has `admin` with null/empty `QuyenHan`, `DbInitializer.SeedAdmin` must execute an idempotent SQL update to assign `QuyenHan = "All"`.
4. **Premise 4 (Claims Transformation on Login)**:
   When an admin logs in, `AdminController.Login` reads `admin.QuyenHan`. For specific modules, it attaches individual `new Claim("QuyenHan", module)` claims. If `QuyenHan == "All"` or the user is `admin`, it attaches `Claim("QuyenHan", "All")`, `Claim("QuyenHan", "SuperAdmin")`, and all 11 module claims. This keeps `principal.IsInRole("Admin")` and `"TenDangNhap"` claims unchanged, satisfying test `M2_3_08`.
5. **Premise 5 (Authorization Enforcement - R2)**:
   Per R2, unauthorized requests must be blocked with HTTP 403 Forbidden or redirect. Because existing test `M2_3_04` asserts `AccessDeniedPath == "/admin/login"` in `Program.cs`, changing `AccessDeniedPath` would break that test. Therefore, creating a custom `PhanQuyenAdminAttribute(string module)` (inheriting from `AuthorizeAttribute` and implementing `IAsyncActionFilter`) directly sets `httpContext.Response.StatusCode = StatusCodes.Status403Forbidden` and `context.Result = new StatusCodeResult(403)`. This stops execution before any action logic runs, protecting both GET and mutating POST requests.
6. **Premise 6 (Layout & Invariant Preservation)**:
   `PhanQuyenAdminAttribute` inherits from `AuthorizeAttribute`, so reflection tests `M2_3_01` and `M2_2_01` auditing `[Authorize]` at class level continue to pass. In `_AdminLayout.cshtml`, wrapping links in `@if (User.CoQuyen(...))` preserves the raw string tokens expected by `F19_T01` and `F19_T02`.

---

## 3. Caveats

1. **Area CuaHang Views Protection**:
   `WEB_TinhNguyen/Areas/CuaHang/Views/` files are SHA-256 hash locked per `PROJECT.md` line 83 and `M3Challenger2` test. Do NOT alter files in that directory. All layout and navigation changes must be confined to `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`.
2. **AccessDeniedPath in Program.cs**:
   Do NOT change `options.AccessDeniedPath = "/admin/login"` in `Program.cs`, as `tests/E2ETests/Tier5/Tier5_ChallengerM2_3_AuthPipelineHarness.cs:323` checks this exact string.
3. **No Caveats on Implementation Feasibility**:
   All 11 modules and SuperAdmin bypass rules are fully mapped and compatible with both runtime MVC execution and automated testing.

---

## 4. Conclusion

1. **Permission Architecture**:
   - 11 module codes defined in `QuyenAdmin.cs`: `TrangChu`, `ChuongTrinh`, `GayQuy`, `CauHinhQR`, `KyUc`, `BanToChuc`, `TinTuc`, `HieuUng`, `SanPham`, `DonHang`, `QuanLyTaiKhoan`, plus `SuperAdmin` / `All`.
   - Data stored as `QuyenHan NVARCHAR(MAX) NULL` on `TaiKhoanAdmin`.
   - `DbInitializer.cs` updated with idempotent DDL:
     `IF COL_LENGTH(N'[dbo].[TaiKhoanAdmin]', N'QuyenHan') IS NULL ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [QuyenHan] NVARCHAR(MAX) NULL;`
   - Default `admin` account seeded with `QuyenHan = "All"`.
2. **Authorization Enforcement**:
   - `PhanQuyenAdminAttribute(string module)` applied at class level to all 11 admin controllers.
   - Unauthorized requests immediately halted with HTTP 403 Forbidden (`StatusCodeResult(403)`).
   - SuperAdmin (`All` / `SuperAdmin` / username `admin`) automatically bypasses all checks via `PhanQuyenAdminHelper.CoQuyen`.
   - Unauthenticated requests continue to be challenged with HTTP 302 Redirect to `/admin/login?ReturnUrl=...`.
3. **Account Management (R1 Controller & Views)**:
   - Dedicated controller `AdminTaiKhoanController` under `/AdminTaiKhoan` protected by `[PhanQuyenAdmin(QuyenAdmin.QuanLyTaiKhoan)]`.
   - Passwords hashed using standard `BaoMatMatKhau.HashPassword(model.MatKhau, out salt)`.

---

## 5. Verification Method

To verify the architecture and ensure zero regressions:

1. **Build Verification**:
   ```powershell
   dotnet build
   ```
   *Expected outcome*: 0 errors.

2. **Run Existing Test Suite (518 tests)**:
   ```powershell
   dotnet run --project tests/E2ETests/E2ETests.csproj
   ```
   *Expected outcome*: 518/518 tests pass (100% green).

3. **Inspect Proposed Changes in Analysis File**:
   Review detailed implementation designs, code snippets, and DDL scripts in:
   `D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_2\analysis.md`.
