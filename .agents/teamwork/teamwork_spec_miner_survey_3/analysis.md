# Specification Mining & Test Architecture Analysis: Granular Admin Authentication & Authorization (R1-R4)

**Agent**: `teamwork_spec_miner_survey_3`  
**Milestone**: Survey & Verification Plan  
**Target Project**: `WEB_TinhNguyen` (.NET 10.0, ASP.NET Core MVC, Entity Framework Core 10.0.12)  
**Date**: 2026-11-07  

---

## 1. Executive Summary & Authoritative Specification Sources

This investigation analyzes the requirements for granular role-based access control and admin account management within the `/admin` area of the **WEB_TinhNguyen** fundraising platform. 

### Authoritative Specification Sources Examined:
1. **`ORIGINAL_REQUEST.md`**: Defines core requirements R1 (Admin Account Creation & Permission Assignment), R2 (Granular Authorization Enforcement on GET/POST), R3 (Dynamic Navigation Menu), and R4 (Automated Verification Suite).
2. **`PROJECT.md`**: Documents system architecture, PBKDF2 cryptography (`BaoMatMatKhau`), dual-mode resilience (SQL Server + synchronized thread-safe In-Memory fallback store in `DbInitializer.cs`), cookie authentication pipeline ordering, and controller contracts.
3. **Existing Source Code**:
   - Models: `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`
   - Data & Helpers: `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`, `WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs`, `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`
   - Controllers: `WEB_TinhNguyen/BoDieuKhien/Admin*.cs`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*.cs`
   - Views: `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`, `WEB_TinhNguyen/GiaoDien/Admin/*.cshtml`
4. **Existing Test Infrastructure**:
   - `tests/E2ETests/` (518 tests across Tiers 1-5, custom reflection runner `TestRunnerEngine.cs`)
   - Standalone Challenger Harnesses: `tests/M2Challenger_Auth` (204 tests), `tests/M1Challenger`, `tests/M3Challenger2`

---

## 2. Existing Automated Test Infrastructure Analysis

### 2.1 Test Projects & Frameworks
- **Primary Test Project**: `tests/E2ETests/E2ETests.csproj`
  - **Framework**: .NET 10.0 (`net10.0`), `OutputType: Exe`.
  - **Dependencies**: References `Microsoft.AspNetCore.App` and `WEB_TinhNguyen.csproj`. Contains zero external xUnit/NUnit/MSTest NuGet package dependencies, eliminating external network restore overhead and package version drift.
  - **Execution Engine**: Custom high-performance reflection test runner `TestRunnerEngine.Run(args)` located in `tests/E2ETests/Framework/TestRunner.cs`. Discovers methods decorated with `[E2ETest(tier, feature, name)]`.
  - **Assertion Library**: Standalone fluent assertions in `tests/E2ETests/Framework/Assertions.cs` (`Assert.True`, `Assert.False`, `Assert.Equal`, `Assert.NotNull`, `Assert.Contains`, `Assert.Matches`, `Assert.RequireFile`, etc.).
  - **Context Helper**: `E2ETestContext.cs` locates project roots, resolves Razor view files, and provides `MockSession`.
- **Standalone Challenger Projects**:
  - `tests/M2Challenger_Auth/M2Challenger_Auth.csproj`: Dedicated pipeline test suite auditing controllers, CSRF tokens, Cookie authentication options, invalid logins, claims resolution, open redirect defenses, and pipeline challenges.
  - `tests/M1Challenger/M1Challenger.csproj` & `tests/M3Challenger2/M3Challenger2.csproj`: Structural and adversarial stress test harnesses.

### 2.2 Test Counts & Current Baseline Execution
Running the unified test suite command:
```powershell
dotnet run --project tests/E2ETests/E2ETests.csproj --no-build -- --all
```
Yields the empirical baseline:
- **Tier 1 (Foundation & Routing)**: 161 Passed / 161 Total
- **Tier 2 (Boundary & Corner Cases)**: 28 Passed / 28 Total
- **Tier 3 (Cross-Feature Combinations)**: 18 Passed / 18 Total
- **Tier 4 (Real-World Scenarios)**: 12 Passed / 12 Total
- **Tier 5 (Adversarial & Stress Harnesses)**: 299 Passed / 299 Total
- **Overall Total**: **518 Tests | 518 Passed | 0 Failed | 0 Skipped** (Execution time: ~18.15s).
- In addition, `tests/M2Challenger_Auth/` executes **204 standalone tests with 100% pass rate** (204 passed, 0 failed).

### 2.3 How Auth Scenarios Are Simulated & Tested in `tests/E2ETests/`
Existing tests in `Tier5_ChallengerM2_3_AuthPipelineHarness.cs` and `Tier1_AdminRoutingTests.cs` simulate authentication via:
1. **Reflection Auditing**: Inspecting type and method attributes (`ctrlType.GetCustomAttribute<AuthorizeAttribute>()`, `method.GetCustomAttribute<HttpPostAttribute>()`, `ValidateAntiForgeryTokenAttribute`).
2. **Controller Mock Setup**:
   - `MockAuthService : IAuthenticationService`: Intercepts `SignInAsync`, `SignOutAsync`, `ChallengeAsync`, and `ForbidAsync`. Tracks issued `ClaimsPrincipal`, authentication properties (`IsPersistent`), and redirect URLs.
   - `MockSession : ISession`: Emulates in-memory session state with `Clear()`, `Set()`, `TryGetValue()`.
   - `MockTempDataProvider : ITempDataProvider`: Stores flash messages between redirects.
   - Initializing `ControllerContext` with `ActionContext(httpContext, routeData, actionDescriptor)` without starting an external web server.
3. **ClaimsPrincipal Generation**: Populating `ClaimsIdentity` with `ClaimTypes.Name`, `ClaimTypes.NameIdentifier`, `ClaimTypes.Role`, and custom claims, attaching it to `DefaultHttpContext.User`.
4. **Offline In-Memory Fallback Testing**:
   - Utilizing `DbInitializer.ValidateAdminCredentials`, `GetAdminByUsername`, and `_inMemoryAdmins` with a dead DbContext (`Server=127.0.0.1,65533;Database=OfflineTestDb...;Connect Timeout=1`).
   - Ensures 100% test pass rate even when SQL Server is offline or inaccessible in sandboxed build environments.

### 2.4 Critical Constraints & Invariants
1. **LOCKED VIEWS CONSTRAINT**:
   - Under NO CIRCUMSTANCES modify any files under `WEB_TinhNguyen/Areas/CuaHang/Views/`. These files are locked and verified by SHA-256 checksums in adversarial harnesses.
2. **EXISTING TEST INTEGRITY (Zero Regressions)**:
   - All 518 existing tests must continue to pass 100%.
   - In particular, `Tier1_AdminRoutingTests` (`F19_T01`, `F19_T02`, `F19_T03`) and `Tier5_Challenger2_AdminAdversarialHarness` (`CH2_04`, `CH2_05`, `CH2_06`) mandate that `_AdminLayout.cshtml` must:
     - Contain zero occurrences of `GiaoDien/` or `%2F`.
     - Explicitly retain verbatim Url.Action pattern: `"Index", "<ControllerName>", new { area = "" }` for all 8 root controllers.
     - Retain exact active tab check: `ViewContext.RouteData.Values["Controller"]?.ToString() == "<ControllerName>"`.
3. **PARAMETERLESS CONSTRUCTORS**:
   - `AdminController` and other controllers must support parameterless instantiation (e.g. `public AdminController(ApplicationDbContext? context = null)`) to maintain compatibility with legacy unit tests (`F19_T08`).
4. **PIPELINE ORDERING**:
   - Middleware order in `Program.cs` (`UseRouting()` -> `UseSession()` -> `UseAuthentication()` -> `UseAuthorization()` -> `UseStaticFiles()`) is strictly audited by test `CH19`.
5. **BUILD ZERO-WARNING/ZERO-ERROR**:
   - `dotnet build` must compile cleanly with 0 errors.

---

## 3. Formal Specifications for Requirements R1 - R4

### 3.1 R1: Quản lý Tài khoản Quản trị và Phân quyền (Admin Account & Role Management)
- **Module Scope**: The system must provide an administrative interface and persistence layer to manage admin accounts and configure their granular module permissions.
- **Taxonomy of 11 Admin Modules**:
  | # | Module Code | Display Name (Vietnamese) | Controller | Route Prefix |
  |---|-------------|---------------------------|------------|--------------|
  | 1 | `TrangChu` | Cấu hình Trang chủ | `AdminTrangChuController` | `/AdminTrangChu` |
  | 2 | `ChuongTrinh` | Cấu hình Chương trình | `AdminChuongTrinhController` | `/AdminChuongTrinh` |
  | 3 | `GayQuy` | Cấu hình Đóng Góp / Gây quỹ | `AdminGayQuyController` | `/AdminGayQuy` |
  | 4 | `CauHinhQR` | Cấu hình Thanh toán QR | `AdminCauHinhQRController` | `/AdminCauHinhQR` |
  | 5 | `KyUc` | Cấu hình Ký ức | `AdminKyUcController` | `/AdminKyUc` |
  | 6 | `BanToChuc` | Cấu hình Ban Tổ Chức | `AdminBanToChucController` | `/AdminBanToChuc` |
  | 7 | `TinTuc` | Cấu hình Tin tức | `AdminTinTucController` | `/AdminTinTuc` |
  | 8 | `HieuUng` | Thư viện Hiệu ứng | `AdminHieuUngController` | `/AdminHieuUng` |
  | 9 | `SanPham` | Quản lý Sản phẩm Gây Quỹ | `AdminSanPhamController` | `/CuaHang/AdminSanPham` |
  | 10 | `DonHang` | Quản lý Đơn hàng Gây Quỹ | `AdminDonHangController` | `/CuaHang/AdminDonHang` |
  | 11 | `TaiKhoan` | Quản lý Tài khoản Quản trị | `AdminTaiKhoanController` | `/AdminTaiKhoan` |
  *(Note: The main dashboard `/Admin` is accessible to all authenticated admins).*

- **Data Model Specifications (`TaiKhoanAdmin`)**:
  - `IDTaiKhoan` (int, PK, identity)
  - `TenDangNhap` (nvarchar(100), unique, required)
  - `MatKhauHash` (nvarchar(500), required)
  - `MuoiBaoMat` (nvarchar(100), required)
  - `HoTen` (nvarchar(150), optional)
  - `Email` (nvarchar(150), optional, valid email format)
  - `VaiTro` (nvarchar(50), required: `"SuperAdmin"` or `"Admin"`)
  - `QuyenHan` (nvarchar(1000), stores serialized list or comma-separated module keys, e.g. `"TrangChu,TinTuc"` or `"ALL"`)
  - `TrangThaiHoatDong` (bit, default 1/true)
  - `NgayTao` (datetime2, default GETDATE())
  - `LanDangNhapCuoi` (datetime2, nullable)
- **Cryptographic Password Security**:
  - Algorithm: PBKDF2 with HMAC-SHA256, 100,000 iterations, 16-byte cryptographically secure random salt generated via `RandomNumberGenerator`.
  - Implemented via `BaoMatMatKhau.TaoMuoiNgauNhien()`, `BaoMatMatKhau.BamMatKhau(password, salt)`, and `BaoMatMatKhau.KiemTraMatKhau(password, hash, salt)` using `CryptographicOperations.FixedTimeEquals` to prevent timing attacks.
  - Under no condition may cleartext passwords be stored in database or memory.
- **Dual-Mode Persistence Specifications**:
  - SQL Server DDL in `DbInitializer.cs`: Table `[dbo].[TaiKhoanAdmin]` schema must include `[QuyenHan] NVARCHAR(1000) NULL`.
  - In-Memory Fallback: `DbInitializer._inMemoryAdmins` must support `QuyenHan` in `CloneAdmin`, `GetDefaultAdmins`, `AddInMemoryAdmin`, and `UpdateInMemoryAdmin`.
  - Default Admin Seeding: Seed account `admin` / `Admin@123` must possess `VaiTro = "SuperAdmin"` and `QuyenHan = "ALL"` (or all 11 modules), ensuring initial bootstrap access.

### 3.2 R2: Kiểm soát Quyền Truy cập và Thực thi theo Phân hệ (Granular Authorization Enforcement)
- **Enforcement Scope**: Every HTTP request (GET, POST, and data mutations) directed at any module controller must verify that the requesting admin account has the required module permission.
- **Authorization Rules**:
  1. **Superadmin / Full Access**: Accounts where `VaiTro == "SuperAdmin"` or `QuyenHan == "ALL"` (or containing all module permissions) have unrestricted access to all 11 modules.
  2. **Authorized Module Request**:
     - When an authenticated admin who has permission for Module `X` sends a request (GET or POST) to Module `X` endpoints, the system returns **HTTP 200 OK** (or standard action result, e.g. 302 redirect on successful form submission).
  3. **Unauthorized Module Request**:
     - When an authenticated admin who does NOT have permission for Module `Y` sends a request (GET or POST) to any endpoint of Module `Y`, the system MUST reject the request.
     - Observable behavior: **HTTP 403 Forbidden** (`ForbidResult` / `StatusCode(403)`) OR **HTTP 302 Redirect** to an Access Denied view or admin dashboard with a clear warning notification (e.g. `TempData["ErrorMessage"] = "Bạn không có quyền truy cập phân hệ này."`).
     - **Critical POST Protection**: No data mutation (create, edit, delete, status toggle) can occur when an unauthorized request is submitted.
- **Mechanisms for Enforcement**:
  - Custom Authorization Filter or Attribute: e.g., `[PhanQuyenAdmin("TrangChu")]` or Policy-based authorization `[Authorize(Policy = "Module_TrangChu")]`.
  - Claims Generation at Login: `AdminController.Login` reads `admin.QuyenHan` and appends permission claims to `ClaimsIdentity` (e.g. `new Claim("Permission", "TrangChu")`).
  - Request Evaluation: The filter checks whether `User.IsInRole("SuperAdmin")` OR `User.HasClaim("Permission", "ALL")` OR `User.HasClaim("Permission", requiredModule)`. If not satisfied, sets `context.Result = new ForbidResult()` or redirects to Access Denied.

### 3.3 R3: Hiển thị Giao diện và Điều hướng theo Phân quyền (Dynamic Navigation Menu)
- **View Location**: `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`.
- **Dynamic Filtering Rules**:
  - The sidebar navigation menu dynamically evaluates the current authenticated user's permissions:
    - If user has permission for Module `X`: Display the navigation link for Module `X`.
    - If user lacks permission for Module `X`: Hide the navigation link for Module `X` completely from the DOM.
  - Superadmin (`SuperAdmin` or `ALL`) sees all 11 module links.
  - Sub-admin with single module permission (e.g., `TinTuc`) sees only Dashboard, `Cấu hình Tin tức`, and `Xem trang ngoài`.
  - Sub-admin with dual permissions (e.g., `SanPham` + `DonHang`) sees only Dashboard, `Sản phẩm Gây Quỹ`, `Đơn hàng Gây Quỹ`, and `Xem trang ngoài`.
- **Preservation of Existing Assertions**:
  - Existing tests in `Tier1_AdminRoutingTests` and `Tier5_Challenger2` verify verbatim patterns in `_AdminLayout.cshtml` (such as `@Url.Action("Index", "AdminTrangChu", new { area = "" })` and `ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTrangChu"`).
  - Therefore, the dynamic visibility checks must wrap the existing link elements (e.g. `@if (UserCanAccess("AdminTrangChu")) { ... }`) so that static file scans continue to find the required controller tokens.

### 3.4 R4: Kịch bản Kiểm thử Tự động Xác minh (Automated Verification Suite)
- **Suite Integration**: Leverage the existing test runner in `tests/E2ETests/` with dedicated classes or fixtures across Tiers 1 to 4.
- **Verification Objectives**:
  - Verify admin model, crypto, and permission storage.
  - Verify granular permission enforcement across all 11 modules (GET and POST).
  - Verify 403 Forbidden / Redirect on unauthorized access.
  - Verify dynamic menu visibility.
  - Verify end-to-end admin creation, permission assignment, login, and access lifecycle.
  - Maintain 100% pass rate on all 518 existing tests.

---

## 4. Features Discovered & Edge Cases

### 4.1 Features Discovered
| # | Category | Feature | Description | Inputs | Outputs | Error Behavior | Discovered Via |
|---|----------|---------|-------------|--------|---------|----------------|----------------|
| 1 | R1: Model & Storage | Admin Account Entity (`TaiKhoanAdmin`) | Entity model mapped to `[dbo].[TaiKhoanAdmin]` with identity, username, password hash, salt, role, permissions, status, and timestamps. | Account data, `QuyenHan` permissions | `TaiKhoanAdmin` instance | Validation errors on empty username/hash/role | `TaiKhoanAdmin.cs`, ORIGINAL_REQUEST §R1 |
| 2 | R1: Security | PBKDF2 Password Hashing | Cryptographic utility generating 16-byte random salt and 100,000-iteration HMAC-SHA256 hash. | Plaintext password, salt | 44-char Base64 hash, 24-char salt | Rejects null/empty inputs; handles Unicode | `BaoMatMatKhau.cs`, PROJECT.md |
| 3 | R1: Storage | Granular Permission Representation | Structured storage of module permissions as comma-separated keys or serialized tokens (`"ALL"` or `"TrangChu,TinTuc"`). | List of module codes | `QuyenHan` string | Defaults to empty string or null (no permissions) | ORIGINAL_REQUEST §R1 |
| 4 | R1: Account Mgmt | Admin Account Creation Action | Form action to create new admin account with designated module permissions. | Username, password, full name, email, selected permissions | Created `TaiKhoanAdmin` in DB / in-memory | Rejects duplicate username, short password, invalid email | ORIGINAL_REQUEST §R1, AC |
| 5 | R1: Persistence | Dual-Mode Resilience Store | Thread-safe in-memory store in `DbInitializer.cs` supporting CRUD operations with `QuyenHan` for offline environments. | `TaiKhoanAdmin` object, ID | Cloned `TaiKhoanAdmin` | Returns false/null on non-existent ID | `DbInitializer.cs`, Survey |
| 6 | R2: Auth Pipeline | Cookie Auth & Claims Generation | `AdminController.Login` signs in user and issues Claims for Name, Role, and granular Permissions. | Valid credentials | Authenticated Cookie with ClaimsPrincipal | Rejects invalid credentials; adds model error | `AdminController.cs`, PROJECT.md |
| 7 | R2: Authorization | Granular Module Authorization Filter | Filter/Attribute inspecting user claims against required module for incoming request. | HttpContext, module code | Allowed execution or block result | Sets HTTP 403 Forbidden or 302 Redirect | ORIGINAL_REQUEST §R2 |
| 8 | R2: Enforcement | Superadmin Unrestricted Access | Bypass rule allowing accounts with `SuperAdmin` role or `ALL` permission to access all 11 modules. | Request from Superadmin | HTTP 200 OK across all modules | Never blocked | ORIGINAL_REQUEST §R2 |
| 9 | R2: Enforcement | GET Endpoint Protection | Blocks unauthorized GET requests to any of the 11 admin module views. | GET request from unauthorized sub-admin | HTTP 403 Forbidden or 302 Redirect | View is not rendered | ORIGINAL_REQUEST §R2 |
| 10 | R2: Enforcement | POST Endpoint Protection | Blocks unauthorized POST requests (create, update, delete) to any admin module. | POST request from unauthorized sub-admin | HTTP 403 Forbidden or 302 Redirect | Data mutation is halted | ORIGINAL_REQUEST §R2 |
| 11 | R3: UI Navigation | Dynamic Sidebar Filtering | Sidebar in `_AdminLayout.cshtml` conditionally renders module links matching user permissions. | Current `User.Claims` | Tailored HTML navigation menu | Unauthorized links hidden | ORIGINAL_REQUEST §R3 |
| 12 | R3: UI Navigation | Dashboard Shortcut Card Filtering | Dashboard quick links in `Admin/Index.cshtml` conditionally display only authorized shortcuts. | Current `User.Claims` | Filtered quick action cards | Unauthorized cards hidden | `Admin/Index.cshtml`, ORIGINAL_REQUEST §R3 |
| 13 | R4: Verification | Unified Test Suite Runner | In-process test runner executing Tiers 1-5 with filtering and detailed execution summary. | CLI flags (`--all`, `--tier`, `--feature`) | Console summary, exit code 0 or 1 | Fails on any assertion failure | `TestRunner.cs` |

### 4.2 Edge Cases
| # | Feature | Input | Observed Behavior |
|---|---------|-------|-------------------|
| 1 | R1: Account Creation | Duplicate username with different case (`ADMIN` vs `admin`) | Must be rejected as duplicate; username check must be case-insensitive. |
| 2 | R1: Account Creation | Username with leading/trailing whitespace (`"  subadmin  "`) | Whitespace trimmed before hashing/saving to prevent login ambiguity. |
| 3 | R1: Password Hashing | 10,000-character password or Unicode characters with diacritics (`MậtKhẩu@2026!🇻🇳`) | PBKDF2 hashes accurately without buffer overflow or character corruption. |
| 4 | R1: Permission Assignment | Empty permissions list (account created with no modules selected) | Account created successfully; user can log in and access Dashboard `/Admin`, but cannot access any of the 11 module endpoints. |
| 5 | R1: Permission Assignment | Invalid or malicious permission tokens (`"TrangChu,DROP_TABLE,<script>"`) | Unknown tokens ignored or rejected by validation; only valid enum/module codes accepted. |
| 6 | R2: Authorization | Sub-admin assigned Module `TinTuc` attempts direct URL traversal to `/CuaHang/AdminSanPham` | Blocked immediately with HTTP 403 Forbidden or 302 Redirect; no product catalog rendered. |
| 7 | R2: Authorization | Sub-admin assigned Module `TrangChu` attempts POST request `POST /CuaHang/AdminSanPham/Delete/1` | Blocked immediately; product is NOT deleted from database or in-memory store. |
| 8 | R2: Authorization | Inactive sub-admin account (`TrangThaiHoatDong = false`) with valid password | Login rejected immediately; no authentication cookie issued. |
| 9 | R2: Authorization | Superadmin account accessing newly created module or all existing 11 modules | All endpoints return HTTP 200 OK; full CRUD execution permitted. |
| 10 | R3: Dynamic Menu | Sub-admin with no module permissions logs in | Sidebar displays only Dashboard link and external site link; all 11 module links hidden. |
| 11 | R3: Dynamic Menu | Sub-admin with exactly 1 permission (`CauHinhQR`) logs in | Sidebar displays only Dashboard, `Cấu hình Thanh toán QR`, and external link. |
| 12 | R4: Regression | Running all existing tests after adding granular authorization | All 518 existing tests must continue to pass 100% with zero regressions. |

---

## 5. Comprehensive Test Architecture & Test Cases (Tiers 1-4)

The verification suite is structured across four rigorous test tiers, integrating directly into `tests/E2ETests/` and executed via `TestRunnerEngine`:

```
tests/E2ETests/
├── Tier1/
│   ├── Tier1_AdminGranularAuthTests.cs       # Tier 1: Unit & Model Verification (F21)
├── Tier2/
│   ├── Tier2_GranularAuthBoundaryTests.cs    # Tier 2: Boundary & Corner Cases (F22)
├── Tier3/
│   ├── Tier3_GranularAuthCrossFeatureTests.cs # Tier 3: Cross-Feature & Endpoints (F23)
├── Tier4/
│   ├── Tier4_GranularAuthE2EScenarioTests.cs  # Tier 4: Real-World E2E Scenarios (F24)
```

### 5.1 Tier 1: Unit, Model & Cryptographic Verification
*Scope: Validate data structures, PBKDF2 hashing, permission parsing/serialization, controller reflection attributes, and in-memory store CRUD.*

- **Scenario T1.1: `TaiKhoanAdmin` Entity & Permission Property Contract**
  - Verify `TaiKhoanAdmin` contains `IDTaiKhoan`, `TenDangNhap`, `MatKhauHash`, `MuoiBaoMat`, `HoTen`, `Email`, `VaiTro`, `QuyenHan`, `TrangThaiHoatDong`, `NgayTao`, `LanDangNhapCuoi`.
  - Verify default values: `TrangThaiHoatDong = true`, `VaiTro = "Admin"`, `QuyenHan` initialized or nullable string.
- **Scenario T1.2: PBKDF2 Cryptographic Hashing for New Admin Passwords**
  - Verify `BaoMatMatKhau.HashPassword` generates unique 16-byte random salts and distinct hashes for newly created admin passwords.
  - Verify `VerifyPassword` returns true for correct password and false for wrong password.
- **Scenario T1.3: Permission Serialization & Deserialization Helper**
  - Verify helper functions parse comma-separated strings (`"TrangChu,TinTuc,SanPham"`) into `HashSet<string>` or list, and serialize back to normalized string.
  - Verify case-insensitive comparison (`"trangchu"` matches `"TrangChu"`).
- **Scenario T1.4: Dual-Mode In-Memory Store Admin CRUD with Permissions**
  - Verify `DbInitializer.AddInMemoryAdmin` stores and retrieves admin with specific `QuyenHan`.
  - Verify `DbInitializer.UpdateInMemoryAdmin` correctly updates permissions.
  - Verify `CloneAdmin` safely clones `QuyenHan` preventing reference pollution.
- **Scenario T1.5: Controller Authorization Attribute Audit (Reflection)**
  - Audit all 11 module controllers (`AdminTrangChu`, `AdminChuongTrinh`, `AdminGayQuy`, `AdminCauHinhQR`, `AdminKyUc`, `AdminBanToChuc`, `AdminTinTuc`, `AdminHieuUng`, `AdminSanPham`, `AdminDonHang`, `AdminTaiKhoan`).
  - Verify each controller or action declares authorization attributes enforcing granular access control.
- **Scenario T1.6: CSRF Token Audit on All Admin POST Actions (Reflection)**
  - Audit all data-modifying POST actions across all 11 controllers; verify every action is decorated with `[ValidateAntiForgeryToken]` and requires authorization.

### 5.2 Tier 2: Boundary, Edge Cases & Adversarial Robustness
*Scope: Test boundary limits, extreme inputs, account state flags, and security hardening.*

- **Scenario T2.1: Empty Permissions Boundary**
  - Admin account with `QuyenHan = ""` or empty string.
  - Verify user is authenticated but unauthorized for all 11 modules.
- **Scenario T2.2: Unknown & Malformed Permission Tokens**
  - Admin account with `QuyenHan = "INVALID_MODULE, <script>alert(1)</script>, --SQLi"`.
  - System safely ignores invalid tokens without crashing; denies access to valid modules.
- **Scenario T2.3: Inactive Admin Account Rejection**
  - Admin with `TrangThaiHoatDong = false` attempts authentication.
  - `DbInitializer.ValidateAdminCredentials` returns false; login fails with generic error.
- **Scenario T2.4: Username Normalization & Case Insensitivity**
  - Account created as `"AdminLangBiang"`. Login with `"adminlangbiang"` and `"  ADMINLANGBIANG  "`.
  - Verified: Credentials validate successfully; whitespace is trimmed.
- **Scenario T2.5: Extreme Password Boundaries**
  - Admin password with 10,000 characters and Vietnamese diacritics.
  - PBKDF2 hashes safely and constant-time comparison prevents timing attacks.
- **Scenario T2.6: Concurrent Permission Updates**
  - 50 parallel threads concurrently updating permissions of different admin accounts in in-memory store.
  - Verified: No data corruption, race conditions, or lock deadlocks.

### 5.3 Tier 3: Cross-Feature Integration & Enforcement (GET & POST)
*Scope: Test complete request-response flow for authorized vs unauthorized users across all 11 modules.*

- **Scenario T3.1: Superadmin Full Access Matrix (All 11 Modules)**
  - Simulate login for Superadmin (`admin` / `Admin@123` with `VaiTro = "SuperAdmin"`).
  - Execute GET request on all 11 module index endpoints:
    - `/AdminTrangChu`, `/AdminChuongTrinh`, `/AdminGayQuy`, `/AdminCauHinhQR`, `/AdminKyUc`, `/AdminBanToChuc`, `/AdminTinTuc`, `/AdminHieuUng`, `/CuaHang/AdminSanPham`, `/CuaHang/AdminDonHang`, `/AdminTaiKhoan`.
  - Result: All return **HTTP 200 OK**.
- **Scenario T3.2: Granular Authorized Sub-Admin (Module X -> 200 OK)**
  - Create Sub-admin with permissions for `["TrangChu", "TinTuc"]`.
  - Send GET request to `/AdminTrangChu` and `/AdminTinTuc`.
  - Result: Returns **HTTP 200 OK**.
- **Scenario T3.3: Granular Unauthorized Sub-Admin GET Blocking (Module Y -> 403/302)**
  - Using same Sub-admin (permissions `["TrangChu", "TinTuc"]`), send GET request to:
    - `/AdminChuongTrinh`, `/AdminGayQuy`, `/AdminCauHinhQR`, `/AdminKyUc`, `/AdminBanToChuc`, `/AdminHieuUng`, `/CuaHang/AdminSanPham`, `/CuaHang/AdminDonHang`, `/AdminTaiKhoan`.
  - Result: Every unauthorized endpoint returns **HTTP 403 Forbidden** or **HTTP 302 Redirect** with denial message.
- **Scenario T3.4: Unauthorized POST Data Modification Interception**
  - Sub-admin without `SanPham` permission sends `POST /CuaHang/AdminSanPham/Create` or `POST /CuaHang/AdminSanPham/Delete/1`.
  - Result: Intercepted and blocked before action execution; database/store state is unmodified.
- **Scenario T3.5: Authorized POST Data Modification Execution**
  - Sub-admin with `TinTuc` permission sends valid `POST /AdminTinTuc/Edit`.
  - Result: Request succeeds; data is updated.
- **Scenario T3.6: Account Management Module Self-Protection**
  - Only admins with `TaiKhoan` permission or `SuperAdmin` can access `/AdminTaiKhoan`.
  - Sub-admins without `TaiKhoan` permission cannot view or create other admin accounts.

### 5.4 Tier 4: Real-World End-to-End Scenarios
*Scope: End-to-end lifecycle from superadmin creation to sub-admin operations and dynamic UI rendering.*

- **Scenario T4.1: End-to-End Multi-Role Admin Creation & Operation Workflow**
  - Step 1: Superadmin logs in, accesses `/AdminTaiKhoan`, and creates `editor_tintuc` with only "Tin tức" permission.
  - Step 2: Superadmin creates `store_manager` with only "Quản lý sản phẩm" and "Đơn hàng" permissions.
  - Step 3: `editor_tintuc` logs in:
    - Layout menu renders only "Cấu hình Tin tức" (all other 10 module links hidden).
    - Can create/edit news articles (200 OK).
    - Attempts to browse `/CuaHang/AdminSanPham` -> Blocked (403/Redirect).
  - Step 4: `store_manager` logs in:
    - Layout menu renders only "Sản phẩm Gây Quỹ" and "Đơn hàng Gây Quỹ" (all other 9 links hidden).
    - Can update order status (200 OK).
    - Attempts to browse `/AdminTinTuc` -> Blocked (403/Redirect).
- **Scenario T4.2: Dynamic Sidebar DOM Audit for Single vs Multi-Permission Users**
  - Parse rendered Razor view of `_AdminLayout.cshtml` under different `ClaimsPrincipal` contexts.
  - Verify exact link elements present and absent based on user claims.
  - Verify layout styling, emerald highlights, and FontAwesome icons remain intact.
- **Scenario T4.3: Full Regression & Invariant Verification**
  - Execute full suite of all 518 existing tests + new Tiers 1-4 tests.
  - Verify 100% pass rate, 0 failures, 0 regressions.

---

## 6. Test Runner Commands, Expected Outputs & Pass Criteria

### 6.1 Test Execution Commands
1. **Compile Entire Solution & Test Suites**:
   ```powershell
   dotnet build tests\E2ETests\E2ETests.csproj --no-restore --verbosity minimal
   ```
   *Expected Output*: `Build succeeded. 0 Warning(s), 0 Error(s)`.

2. **Execute Full Unified Test Suite**:
   ```powershell
   dotnet run --project tests\E2ETests\E2ETests.csproj --no-build -- --all
   ```
   *Expected Output*:
   ```
   ================================================================================
                                 TEST EXECUTION SUMMARY
   ================================================================================
     Tier 1: Total: >161 | Passed: >161 | Failed: 0 | Skipped: 0
     Tier 2: Total:  >28 | Passed:  >28 | Failed: 0 | Skipped: 0
     Tier 3: Total:  >18 | Passed:  >18 | Failed: 0 | Skipped: 0
     Tier 4: Total:  >12 | Passed:  >12 | Failed: 0 | Skipped: 0
     Tier 5: Total:  299 | Passed:  299 | Failed: 0 | Skipped: 0
   --------------------------------------------------------------------------------
     OVERALL: 530+ Tests | 530+ Passed | 0 Failed | 0 Skipped
   ================================================================================
   ```
   *Exit code*: `0`.

3. **Execute Tier-Specific Tests**:
   ```powershell
   dotnet run --project tests\E2ETests\E2ETests.csproj --no-build -- --tier 1
   dotnet run --project tests\E2ETests\E2ETests.csproj --no-build -- --tier 2
   dotnet run --project tests\E2ETests\E2ETests.csproj --no-build -- --tier 3
   dotnet run --project tests\E2ETests\E2ETests.csproj --no-build -- --tier 4
   ```

4. **Execute Standalone Auth Challenger**:
   ```powershell
   dotnet run --project tests\M2Challenger_Auth\M2Challenger_Auth.csproj --no-build
   ```
   *Expected Output*: `RESULTS: 204 PASSED, 0 FAILED`.

### 6.2 Acceptance Pass Criteria
- **Pass Rate**: 100% across all tests (0 failures, 0 broken tests).
- **Zero Regressions**: All 518 existing tests pass cleanly.
- **Zero Compilation Errors**: Clean build without errors.
- **Offline Reliability**: All test cases execute synchronously in-memory without requiring live SQL Server instances.
- **Integrity Compliance**: No modifications to locked views under `WEB_TinhNguyen/Areas/CuaHang/Views/`.
