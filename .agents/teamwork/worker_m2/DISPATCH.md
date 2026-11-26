## 2026-11-07T03:03:13Z
You are worker_m2 (Role: Admin Auth UI, Pipeline & Route Protection Worker).
Your working directory is: D:\web_tinh_nguyen\.agents\teamwork\worker_m2
Your parent conversation ID is: 8cd06411-5097-4e95-b532-893861716fb8

CRITICAL: You MUST read:
- D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
- D:\web_tinh_nguyen\PROJECT.md
- D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_1\handoff.md
- D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_2\handoff.md
- D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_3\handoff.md

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

File Write Ownership (You exclusively own these files):
1. `D:\web_tinh_nguyen\WEB_TinhNguyen\Program.cs`:
   - Add `using Microsoft.AspNetCore.Authentication.Cookies;`
   - Register Cookie Authentication (`LoginPath = "/admin/login"`, `LogoutPath = "/admin/logout"`, `AccessDeniedPath = "/admin/login"`, `Cookie.Name = ".WEB_TinhNguyen.AdminAuth"`) per explorer_m2_1.
   - Pipeline ordering (MANDATORY for CH19 & F02_T02):
     `app.UseRouting();` -> `app.UseSession();` -> `app.UseAuthentication();` -> `app.UseAuthorization();` -> `app.UseStaticFiles();`
2. `D:\web_tinh_nguyen\WEB_TinhNguyen\BoDieuKhien\AdminController.cs`:
   - Replace with the exact drop-in implementation from explorer_m2_1 handoff.
   - Supports parameterless constructor `public AdminController(ApplicationDbContext? context = null)` for backward test compatibility (`F19_T08`).
   - `[Authorize] Index()`.
   - `[AllowAnonymous] [HttpGet] Login(string? returnUrl)` and `[AllowAnonymous] [HttpPost] [ValidateAntiForgeryToken] Login(AdminLoginViewModel model, string? returnUrl)`.
   - `[HttpGet, HttpPost] Logout()`.
3. `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Admin\Login.cshtml`:
   - Create new Razor view using the complete markup from explorer_m2_2 handoff.
   - Includes form bound to `AdminLoginViewModel`, CSRF token, validation error banner, ReturnUrl hidden field, and volunteer green Tailwind styling.
4. `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Shared\_AdminLayout.cshtml`:
   - Update logout button (line 343) from `href="#"` to `href="/admin/logout"`.
   - Update user badge to bind `@(User.Identity?.Name ?? "Quản trị viên")` per explorer_m2_2 handoff.
5. Apply `[Authorize]` attribute to class level and `[ValidateAntiForgeryToken]` to CRUD actions across the remaining 10 Admin controllers per explorer_m2_3 handoff:
   - `WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs` (`[Authorize]`, plus `[ValidateAntiForgeryToken]` on `Save` and `Delete`)
   - `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs` (`[Authorize]`)
   - `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs` (`[Authorize]`, plus `[ValidateAntiForgeryToken]` on `CapNhatTrangThai` and `XoaDonHang`)

RESTRICTION:
- DO NOT modify ANY files under `WEB_TinhNguyen/Areas/CuaHang/Views/` (SHA-256 locked by test M3Challenger2).

Verification Requirements:
1. Run `dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx` -> must succeed with 0 Errors.
2. Run `dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all` -> must pass 100% of all 500 tests (0 failed, 0 skipped).
3. Verify that `CH19_ProgramCs_EmpiricalPipelineOrdering` and `F02_T02_SessionMiddlewareOrder` pass.
4. Verify that `F19_T08` and `F19_T16` pass.

Outputs:
Write your structured completion report to:
`D:\web_tinh_nguyen\.agents\teamwork\worker_m2\handoff.md`
