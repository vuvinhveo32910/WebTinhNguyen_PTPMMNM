# Forensic Audit Report: Milestone 2 Integrity Audit

**Auditor**: `auditor_m2_1`  
**Role**: Forensic Integrity Auditor  
**Milestone**: M2 (Admin Authentication, Route Protection & Login/Logout)  
**Profile**: General Project  
**Integrity Mode**: Development (also verified compliant under Demo and Benchmark criteria)  
**Verdict**: **CLEAN**  

---

## 1. Observation

### 1.1 Scope and Static Code Analysis
Audited all 14 files modified or created for Milestone 2 by `worker_m2`:

1. **`WEB_TinhNguyen/Program.cs`**:
   - Lines 26–38: Cookie Authentication configuration:
     ```csharp
     builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
         .AddCookie(options =>
         {
             options.Cookie.Name = ".WEB_TinhNguyen.AdminAuth";
             options.Cookie.HttpOnly = true;
             options.Cookie.SameSite = SameSiteMode.Lax;
             options.Cookie.IsEssential = true;
             options.LoginPath = "/admin/login";
             options.LogoutPath = "/admin/logout";
             options.AccessDeniedPath = "/admin/login";
             options.ExpireTimeSpan = TimeSpan.FromHours(8);
             options.SlidingExpiration = true;
         });
     ```
   - Lines 80–90: Middleware Pipeline execution sequence:
     ```csharp
     app.UseRouting();
     app.UseSession();
     app.UseAuthentication();
     app.UseAuthorization();
     app.UseStaticFiles();
     ```
   - Measured character indices in `Program.cs`:
     `UseRouting` = 3432 < `UseSession` = 3516 < `UseAuthentication` = 3616 < `UseAuthorization` = 3642 < `UseStaticFiles` = 3754.
     Satisfies test assertions `CH19` and `F02_T02`.

2. **`WEB_TinhNguyen/BoDieuKhien/AdminController.cs`**:
   - Lines 29–32: Constructor `public AdminController(ApplicationDbContext? context = null)` preserves backward compatibility for parameterless instantiations (`new AdminController()`) required by unit test `F19_T08`.
   - Lines 42–47: `[Authorize]` attribute applied to `Index()`.
   - Lines 57–72: `[AllowAnonymous]` applied to `Login(string? returnUrl = null)` (GET). If `User.Identity.IsAuthenticated == true`, redirects to return URL or `/Admin`.
   - Lines 79–82: `[AllowAnonymous]`, `[HttpPost]`, `[ValidateAntiForgeryToken]` applied to `Login(AdminLoginViewModel model, string? returnUrl = null)`.
   - Line 91: Authenticates credentials strictly via `DbInitializer.ValidateAdminCredentials(model.TenDangNhap, model.MatKhau, _context)`. No hardcoded username/password comparison exists.
   - Lines 104–126: Establishes claims (`ClaimTypes.Name`, `ClaimTypes.NameIdentifier`, `ClaimTypes.Role`, `"TenDangNhap"`, `ClaimTypes.Email`) and creates cookie via `HttpContext.SignInAsync`.
   - Lines 152–174: `[HttpGet, HttpPost] public async Task<IActionResult> Logout()` clears cookie via `HttpContext.SignOutAsync`, clears session via `HttpContext.Session?.Clear()`, and redirects to `/admin/login`.
   - Lines 183–213: `RedirectToLocal()` verifies `Url.IsLocalUrl(...)` and safe relative path bounds (`/` not followed by `/` or `\`) to prevent Open Redirect attacks.

3. **`WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml`**:
   - Razor model binding: `@model WEB_TinhNguyen.MoHinh.AdminLoginViewModel`.
   - CSRF protection: `@Html.AntiForgeryToken()`.
   - Form attributes: `<form asp-action="Login" asp-controller="Admin" method="post" class="space-y-5">`.
   - Model properties bound: `asp-for="TenDangNhap"`, `asp-for="MatKhau"`, `asp-for="GhiNhoDangNhap"`, `asp-for="ReturnUrl"`.
   - Error banner: lines 61–91 displays ModelState errors and `TempData["ErrorMessage"]`.
   - Password reveal script: lines 176–192 toggle password input type between `password` and `text`.
   - Modern Tailwind styling with volunteer green theme.

4. **`WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`**:
   - Line 343: Genuine logout link:
     ```html
     <a href="/admin/logout" title="Đăng xuất" class="w-10 h-10 rounded-full bg-white text-gray-400 hover:text-red-500 hover:bg-red-50 flex items-center justify-center shadow-sm border border-gray-100 transition-all">
         <i class="fas fa-sign-out-alt"></i>
     </a>
     ```
     Verified: Not a dummy `#` link.
   - Line 338: Dynamic identity display:
     `@(User.Identity?.Name ?? "Quản trị viên")`.

5. **Route Protection on 10 Admin Controllers**:
   Class-level `[Authorize]` attributes verified directly on class definitions:
   - `WEB_TinhNguyen\BoDieuKhien\AdminBanToChucController.cs`: Line 16 `[Authorize]`
   - `WEB_TinhNguyen\BoDieuKhien\AdminCauHinhQRController.cs`: Line 22 `[Authorize]`
   - `WEB_TinhNguyen\BoDieuKhien\AdminChuongTrinhController.cs`: Line 16 `[Authorize]`
   - `WEB_TinhNguyen\BoDieuKhien\AdminGayQuyController.cs`: Line 16 `[Authorize]`
   - `WEB_TinhNguyen\BoDieuKhien\AdminHieuUngController.cs`: Line 15 `[Authorize]`
   - `WEB_TinhNguyen\BoDieuKhien\AdminKyUcController.cs`: Line 16 `[Authorize]`
   - `WEB_TinhNguyen\BoDieuKhien\AdminTinTucController.cs`: Line 16 `[Authorize]`
   - `WEB_TinhNguyen\BoDieuKhien\AdminTrangChuController.cs`: Line 16 `[Authorize]`
   - `WEB_TinhNguyen\Areas\CuaHang\Controllers\AdminDonHangController.cs`: Line 21 `[Authorize]`
   - `WEB_TinhNguyen\Areas\CuaHang\Controllers\AdminSanPhamController.cs`: Line 23 `[Authorize]`

6. **Anti-Forgery Token Verification on 21 CRUD POST Endpoints**:
   - `AdminBanToChucController.LuuThongTin` (line 155): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminCauHinhQRController.LuuThongTin` (line 112): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminCauHinhQRController.CapNhatTrangThai` (line 189): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminCauHinhQRController.Xoa` (line 268): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminCauHinhQRController.DatMacDinh` (line 323): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminChuongTrinhController.LuuThongTin` (line 106): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminController.Login` (line 80): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminGayQuyController.LuuThongTin` (line 169): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminHieuUngController.Save` (line 35): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminHieuUngController.Delete` (line 71): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminKyUcController.LuuThongTin` (line 105): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminTinTucController.LuuThongTin` (line 227): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminTrangChuController.CapNhatBanner` (line 65): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminTrangChuController.ThemThongSo` (line 93): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminTrangChuController.XoaThongSo` (line 124): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminTrangChuController.CapNhatGiaTriCotLoi` (line 212): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminTrangChuController.CapNhatChienDichTieuBieu` (line 285): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminDonHangController.CapNhatTrangThai` (line 199): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminDonHangController.XoaDonHang` (line 344): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminSanPhamController.ThemMoi` (line 102): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminSanPhamController.ChinhSua` (line 209): `[HttpPost] [ValidateAntiForgeryToken]`
   - `AdminSanPhamController.Xoa` (line 297): `[HttpPost] [ValidateAntiForgeryToken]`

### 1.2 Zero-Bypass / Cheat Detection
- **No hardcoded credentials shortcut**: Searched all source code for hardcoded password strings or bypass logic like `if (username == "admin" && password == "...")`. Zero bypasses found.
- **Genuine PBKDF2 calculation**: `AdminController.Login` delegates to `DbInitializer.ValidateAdminCredentials`, which invokes `BaoMatMatKhau.KiemTraMatKhau`. This in turn executes .NET BCL `Rfc2898DeriveBytes.Pbkdf2` with HMAC-SHA256, 100,000 iterations, 16-byte random salt, and constant-time comparison via `CryptographicOperations.FixedTimeEquals`.
- **No hardcoded test result strings**: Searched for `PASS`, `FAIL`, or pre-fabricated test result strings in source code. None found.
- **No pre-populated test artifacts**: Searched workspace for pre-populated `.log` or result artifacts predating test runs. Only standard node_modules PostCSS files found.

### 1.3 View Immutability Audit
- Inspected directory `WEB_TinhNguyen/Areas/CuaHang/Views/`:
  - Exactly 0 files touched today (2026-11-07).
  - All files in this directory retain original timestamps (11/5/2026 and 11/6/2026).
  - Challenger test `M3Challenger2` SHA-256 view invariants are 100% intact.

### 1.4 Independent Build and Test Execution

1. **Solution Build**:
   - Command: `dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx`
   - Verbatim Output:
     ```
     Build succeeded.
         2 Warning(s)
         0 Error(s)
     Time Elapsed 00:00:05.11
     ```

2. **Full Automated E2E Regression Suite**:
   - Command: `dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all`
   - Verbatim Output:
     ```
     ================================================================================
                                   TEST EXECUTION SUMMARY
     ================================================================================
       Tier 1: Total: 161 | Passed: 161 | Failed:   0 | Skipped:   0
       Tier 2: Total:  28 | Passed:  28 | Failed:   0 | Skipped:   0
       Tier 3: Total:  18 | Passed:  18 | Failed:   0 | Skipped:   0
       Tier 4: Total:  12 | Passed:  12 | Failed:   0 | Skipped:   0
       Tier 5: Total: 281 | Passed: 281 | Failed:   0 | Skipped:   0
     --------------------------------------------------------------------------------
       OVERALL: 500 Tests | 500 Passed | 0 Failed | 0 Skipped in 13.21s
     ================================================================================
     ```

3. **Critical Invariant Test Execution**:
   - `--filter CH19`: 1/1 Passed (Pipeline ordering verified)
   - `--filter F02_T02`: 2/2 Passed (Session middleware position verified)
   - `--filter F19`: 16/16 Passed (All admin routes and controllers execute and return ViewResult)
   - `--filter CH2_`: 8/8 Passed (Admin adversarial controller and view bindings verified)
   - `--filter M2`: 85/85 Passed (Admin CRUD & validation stress tests verified)

---

## 2. Logic Chain

1. **Authentication Architecture**:
   - Registering cookie authentication with `LoginPath = "/admin/login"` and `LogoutPath = "/admin/logout"` ensures ASP.NET Core automatically redirects unauthenticated requests (HTTP 302) to the login view when hitting protected routes.
   - Placing `app.UseSession()` before `app.UseAuthentication()` and `app.UseAuthorization()` ensures session state is accessible to authentication handlers, while strictly maintaining the sequential position expected by legacy test `CH19`.

2. **Controller Protection**:
   - Class-level `[Authorize]` on all 10 Admin controllers guarantees that every action (GET and POST) inherits route authorization without exception.
   - In `AdminController`, `Index()` is explicitly `[Authorize]`, preventing unauthenticated access to the main admin portal dashboard.
   - `[AllowAnonymous]` is strictly restricted to `Login` (GET/POST), preventing infinite redirect loops while ensuring that no other admin action can be accessed anonymously.

3. **Cryptographic Integrity & Anti-Bypass**:
   - Credential verification in `AdminController.Login` directly calls `DbInitializer.ValidateAdminCredentials`.
   - `ValidateAdminCredentials` looks up the entity and calls `BaoMatMatKhau.KiemTraMatKhau`, which recomputes PBKDF2 HMAC-SHA256 with 100,000 iterations using the admin's unique salt and verifies the result with `CryptographicOperations.FixedTimeEquals`.
   - No short-circuit or bypass strings exist. Incorrect passwords, nonexistent users, and inactive accounts are cleanly rejected.

4. **UI & CSRF Hardening**:
   - `Login.cshtml` generates a valid anti-forgery token via `@Html.AntiForgeryToken()` and binds cleanly to `AdminLoginViewModel`.
   - `_AdminLayout.cshtml` routes the logout anchor directly to `/admin/logout` (which clears both cookie and session) instead of a dead `#` link.
   - All 21 data-modifying CRUD POST endpoints across all admin controllers are protected by `[ValidateAntiForgeryToken]`.

5. **Zero-Side-Effect Verification**:
   - `Areas/CuaHang/Views/` remained untouched, guaranteeing test `M3Challenger2` hash integrity.
   - The solution compiles with 0 errors and all 500 regression tests pass with 100% success rate.

---

## 3. Caveats

- **SQL Server Network Environment**: The local development machine runs without an active SQL Server daemon on port 1433. The test suite uses the synchronized in-memory fallback store (`_inMemoryAdmins`) in `DbInitializer.cs`. The SQL Server DDL and seeding mechanisms were verified via static analysis and offline resilience tests.
- **No caveats regarding code integrity or functionality**: All 14 files under M2 ownership are fully compliant.

---

## 4. Conclusion

### Forensic Audit Report
**Work Product**: Milestone 2 Implementation by `worker_m2`  
**Profile**: General Project  
**Integrity Mode**: Development (also compliant with Demo and Benchmark standards)  
**Verdict**: **CLEAN**  

#### Phase Results:
- **Phase 1: Source Code Analysis**:
  - Hardcoded output detection: **PASS** (Zero backdoor strings, genuine PBKDF2 verification)
  - Facade detection: **PASS** (Genuine Cookie Authentication, ClaimsPrincipal generation, real controller actions)
  - Pre-populated artifact detection: **PASS** (Zero pre-existing verification logs or result artifacts)
  - Layout compliance: **PASS** (`.agents/teamwork/` contains only metadata `.md` files)
- **Phase 2: Behavioral Verification**:
  - Solution build: **PASS** (`dotnet build` succeeds with 0 errors)
  - Full test execution: **PASS** (500/500 passed in 13.21s, 100% pass rate)
  - Zero-Bypass check: **PASS** (`[Authorize]` active across all 11 admin controllers, no password shortcuts)
  - View immutability check: **PASS** (Zero files in `WEB_TinhNguyen/Areas/CuaHang/Views/` touched)
  - CSRF protection: **PASS** (All 21 CRUD POST endpoints have `[ValidateAntiForgeryToken]`)
  - Middleware order check: **PASS** (UseRouting -> UseSession -> UseAuthentication -> UseAuthorization -> UseStaticFiles)

The Milestone 2 work product is **VERIFIED CLEAN** and cleared for Milestone 3.

---

## 5. Verification Method

To independently reproduce the forensic verification:

1. **Verify Middleware Pipeline Ordering**:
   ```powershell
   $prog = Get-Content 'WEB_TinhNguyen/Program.cs' -Raw
   $idxRouting = $prog.IndexOf('app.UseRouting()')
   $idxSession = $prog.IndexOf('app.UseSession()')
   $idxAuthn = $prog.IndexOf('app.UseAuthentication()')
   $idxAuthz = $prog.IndexOf('app.UseAuthorization()')
   $idxStatic = $prog.IndexOf('app.UseStaticFiles()')
   Write-Host "Routing: $idxRouting, Session: $idxSession, Authn: $idxAuthn, Authz: $idxAuthz, Static: $idxStatic"
   # Expected: $idxRouting < $idxSession < $idxAuthn < $idxAuthz < $idxStatic
   ```

2. **Verify View Immutability**:
   ```powershell
   Get-ChildItem -Recurse 'WEB_TinhNguyen/Areas/CuaHang/Views' | Where-Object { $_.LastWriteTime -ge (Get-Date '2026-11-07 00:00:00') }
   # Expected: Empty (0 files)
   ```

3. **Verify Layout Logout Anchor**:
   ```powershell
   Select-String -Path 'WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml' -Pattern 'href="/admin/logout"'
   # Expected: Line 343 matches
   ```

4. **Verify Controller [Authorize] Attributes**:
   ```powershell
   Get-ChildItem WEB_TinhNguyen/BoDieuKhien/Admin*.cs, WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*.cs | ForEach-Object { Select-String -Path $_.FullName -Pattern 'class |\[Authorize\]' }
   # Expected: All 11 controllers display [Authorize]
   ```

5. **Build and Test Execution**:
   ```powershell
   dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx
   # Expected: 0 Error(s)
   dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
   # Expected: 500 Tests | 500 Passed | 0 Failed | 0 Skipped
   ```
