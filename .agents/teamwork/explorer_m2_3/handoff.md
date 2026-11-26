# Handoff Report: Route & CRUD Authorization Strategy (Milestone 2)

**Agent ID**: `explorer_m2_3`  
**Role**: Route & CRUD Authorization Explorer  
**Milestone**: M2 (Admin Auth UI, Pipeline & Route Protection)  
**Recipient**: Orchestrator (`parent`, ID: `8cd06411-5097-4e95-b532-893861716fb8`)  
**Date**: 2026-11-07  

---

## 1. Observation

### 1.1 All 11 Admin Controllers Under Investigation
Direct inspection of the solution revealed exactly 11 Admin Controllers (9 root controllers and 2 Area controllers):

| # | Controller | Location | Namespace | Line Count | Current `[Authorize]` Status |
|---|------------|----------|-----------|------------|------------------------------|
| 1 | `AdminController` | `WEB_TinhNguyen/BoDieuKhien/AdminController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 13 | ❌ Missing |
| 2 | `AdminTrangChuController` | `WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 320 | ❌ Missing |
| 3 | `AdminChuongTrinhController` | `WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 182 | ❌ Missing |
| 4 | `AdminGayQuyController` | `WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 296 | ❌ Missing |
| 5 | `AdminCauHinhQRController` | `WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 386 | ❌ Missing |
| 6 | `AdminKyUcController` | `WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 173 | ❌ Missing |
| 7 | `AdminBanToChucController` | `WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 243 | ❌ Missing |
| 8 | `AdminTinTucController` | `WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 299 | ❌ Missing |
| 9 | `AdminHieuUngController` | `WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs` | `WEB_TinhNguyen.BoDieuKhien` | 88 | ❌ Missing |
| 10 | `AdminSanPhamController` | `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs` | `WEB_TinhNguyen.Areas.CuaHang.Controllers` | 415 | ❌ Missing |
| 11 | `AdminDonHangController` | `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs` | `WEB_TinhNguyen.Areas.CuaHang.Controllers` | 423 | ❌ Missing |

Non-admin controllers verified as public and untouched:
- `WEB_TinhNguyen/BoDieuKhien/HomeController.cs` (Root public portal)
- `WEB_TinhNguyen/Areas/CuaHang/Controllers/CuaHangController.cs` (Public volunteer shop)
- `WEB_TinhNguyen/Areas/CuaHang/Controllers/GioHangController.cs` (Public volunteer cart & checkout)

---

### 1.2 Catalog of All 21 Data-Modifying CRUD POST Actions
Grepping all `[HttpPost]` action methods in admin controllers yielded exactly 21 actions:

| # | Controller | Action Name | Method Signature | Line | Has `[ValidateAntiForgeryToken]`? | Status / Action Needed |
|---|------------|-------------|------------------|------|-----------------------------------|------------------------|
| 1 | `AdminTrangChuController` | `CreateChienDich` | `Task<IActionResult> CreateChienDich(int Nam, string TieuDe)` | 63 | ✅ Yes | Retain |
| 2 | `AdminTrangChuController` | `ToggleStatus` | `Task<IActionResult> ToggleStatus(int id)` | 91 | ✅ Yes | Retain |
| 3 | `AdminTrangChuController` | `DeleteChienDich` | `Task<IActionResult> DeleteChienDich(int id)` | 122 | ✅ Yes | Retain |
| 4 | `AdminTrangChuController` | `Edit` | `Task<IActionResult> Edit(CauHinhTrangChuVM vm)` | 210 | ✅ Yes | Retain |
| 5 | `AdminTrangChuController` | `UploadHieuUngNhanh` | `Task<IActionResult> UploadHieuUngNhanh(int idChienDich, string tenHieuUng, IFormFile fileJS)` | 283 | ✅ Yes | Retain |
| 6 | `AdminChuongTrinhController` | `Edit` | `Task<IActionResult> Edit(CauHinhChuongTrinhVM vm)` | 104 | ✅ Yes | Retain |
| 7 | `AdminGayQuyController` | `Edit` | `Task<IActionResult> Edit(CauHinhGayQuyVM vm)` | 167 | ✅ Yes | Retain |
| 8 | `AdminCauHinhQRController` | `Create` | `Task<IActionResult> Create(CauHinhThanhToanQR model, IFormFile? FileAnhQR)` | 110 | ✅ Yes | Retain |
| 9 | `AdminCauHinhQRController` | `Edit` | `Task<IActionResult> Edit(int id, CauHinhThanhToanQR model, IFormFile? FileAnhQR)` | 187 | ✅ Yes | Retain |
| 10 | `AdminCauHinhQRController` | `ToggleActive` | `Task<IActionResult> ToggleActive(int id)` | 266 | ✅ Yes | Retain |
| 11 | `AdminCauHinhQRController` | `Delete` | `Task<IActionResult> Delete(int id)` | 321 | ✅ Yes | Retain |
| 12 | `AdminKyUcController` | `Edit` | `Task<IActionResult> Edit(CauHinhKyUcVM vm)` | 103 | ✅ Yes | Retain |
| 13 | `AdminBanToChucController` | `Edit` | `Task<IActionResult> Edit(CauHinhBanToChucVM vm)` | 153 | ✅ Yes | Retain |
| 14 | `AdminTinTucController` | `Edit` | `Task<IActionResult> Edit(CauHinhTinTucVM vm)` | 225 | ✅ Yes | Retain |
| 15 | `AdminHieuUngController` | `Save` | `Task<IActionResult> Save(int IDHieuUng, string TenHieuUng, string MoTa, IFormFile FileJS)` | 33 | ❌ **No** | **ADD `[ValidateAntiForgeryToken]`** |
| 16 | `AdminHieuUngController` | `Delete` | `Task<IActionResult> Delete(int id)` | 68 | ❌ **No** | **ADD `[ValidateAntiForgeryToken]`** |
| 17 | `AdminSanPhamController` | `Create` | `Task<IActionResult> Create(SanPhamCrudViewModel model)` | 100 | ✅ Yes | Retain |
| 18 | `AdminSanPhamController` | `Edit` | `Task<IActionResult> Edit(int id, SanPhamCrudViewModel model)` | 207 | ✅ Yes | Retain |
| 19 | `AdminSanPhamController` | `Delete` | `Task<IActionResult> Delete(int id)` | 295 | ✅ Yes | Retain |
| 20 | `AdminDonHangController` | `CapNhatTrangThai` | `Task<IActionResult> CapNhatTrangThai(int id, string trangThaiDonHang, string? trangThaiThanhToan)` | 197 | ❌ **No** | **ADD `[ValidateAntiForgeryToken]`** |
| 21 | `AdminDonHangController` | `XoaDonHang` | `Task<IActionResult> XoaDonHang(int id)` | 341 | ❌ **No** | **ADD `[ValidateAntiForgeryToken]`** |

**Observation on Anti-Forgery in Views**:
- `WEB_TinhNguyen/GiaoDien/AdminHieuUng/Index.cshtml`:
  - Line 30: `<form asp-action="Delete" asp-route-id="@item.IDHieuUng" method="post" ...>`
  - Line 50: `<form asp-action="Save" method="post" enctype="multipart/form-data" ...>`
  Because ASP.NET Core TagHelpers (`asp-action`) are used on `<form>`, ASP.NET Core automatically generates `__RequestVerificationToken` hidden input fields.
- `WEB_TinhNguyen/Areas/CuaHang/Views/AdminDonHang/ChiTiet.cshtml`:
  - Line 319: `@Html.AntiForgeryToken()` explicitly present inside `CapNhatTrangThai` form.
  - Line 413: `@Html.AntiForgeryToken()` explicitly present inside modal `CapNhatTrangThai` form.
- `WEB_TinhNguyen/Areas/CuaHang/Views/AdminDonHang/Index.cshtml`:
  - Line 441: `@Html.AntiForgeryToken()` explicitly present inside `quickStatusForm` (`CapNhatTrangThai`).
  - Line 502: `@Html.AntiForgeryToken()` explicitly present inside `deleteOrderForm` (`XoaDonHang`).
Adding `[ValidateAntiForgeryToken]` to these 4 endpoints aligns 100% with the Razor views and prevents CSRF vulnerabilities.

---

### 1.3 Inspection of Existing Test Suites & Reflection Assertions
Detailed examination of the test suite uncovered the following critical test contracts:

1. **`Tier1_AdminRoutingTests.cs`**:
   - `F19_T07` (lines 180-203): Confirms the 8 root admin controllers exist, inherit from `Microsoft.AspNetCore.Mvc.Controller`, and define public `Index` action method.
   - `F19_T08` (lines 205-215): Verifies `new AdminController()` parameterless constructor:
     ```csharp
     var controller = new AdminController();
     SetupControllerContext(controller);
     var result = controller.Index();
     Assert.NotNull(result);
     ```
   - `F19_T09` to `F19_T15` (lines 217-326): Instantiates each of the other 7 controllers directly: `new AdminXController(offlineContext, mockEnv)` and calls `await controller.Index()`.
   - `F19_T16` (lines 328-375): Instantiates all 8 admin controllers directly and executes `.Index()`:
     ```csharp
     var admin = new AdminController();
     SetupControllerContext(admin);
     var qr = new AdminCauHinhQRController(offlineContext, mockEnv);
     var trangChu = new AdminTrangChuController(offlineContext, mockEnv);
     var chuongTrinh = new AdminChuongTrinhController(offlineContext, mockEnv);
     var gayQuy = new AdminGayQuyController(offlineContext, mockEnv);
     var kyUc = new AdminKyUcController(offlineContext, mockEnv);
     var btc = new AdminBanToChucController(offlineContext, mockEnv);
     var tinTuc = new AdminTinTucController(offlineContext, mockEnv);
     ...
     Assert.True(r1 is ViewResult, "/Admin must return ViewResult");
     ```

2. **`Tier1_AdminOrderTests.cs`**:
   - `F18_T01` (lines 25-34): Asserts `typeof(AdminDonHangController)` inherits from `Controller` and is decorated with `[Area("CuaHang")]`.
   - `F18_T08` (lines 137-144): Asserts `controllerType.GetMethod("Index", new[] { typeof(string), typeof(string), typeof(string), typeof(string) }) != null`.
   - `F18_T09` (lines 146-153): Asserts `controllerType.GetMethod("ChiTiet", new[] { typeof(int) }) != null`.
   - `F18_T10` (lines 155-165): Asserts `controllerType.GetMethod("CapNhatTrangThai", new[] { typeof(int), typeof(string), typeof(string) }) != null` and has `[HttpPost]`.
   - `F18_T11` (lines 167-177): Asserts `controllerType.GetMethod("XoaDonHang", new[] { typeof(int) }) != null` and has `[HttpPost]`.

3. **`Tier1_AdminTests.cs`**:
   - `F16_T01` (lines 257-263): `ctrlType.GetMethod("Delete")` requires exactly one public method named `Delete` on `AdminSanPhamController`.
   - `F16_T02` (lines 265-272): `parameters?.Length > 0 && parameters[0].Name?.ToLower().Contains("id") == true` requires parameter name `id` on `Delete`.

4. **`Tier5_ChallengerM2Harness.cs`**:
   - `M2_C26` (lines 545-552): Asserts `AdminSanPhamController` has `[Area("CuaHang")]`.
   - `M2_C27` (lines 554-569): Asserts all POST methods on `AdminSanPhamController` have `[ValidateAntiForgeryToken]`.
   - `M2_C28` to `M2_C36`: Instantiates `AdminSanPhamController` and executes `Create`, `Edit`, `Delete` actions directly.

5. **`Tier5_ChallengerM2_2_AdversarialHarness.cs`**:
   - `M2_2_C09` (lines 428-475): Directly invokes `new AdminDonHangController(context).CapNhatTrangThai(1001, "DaXacNhan", "DaThanhToan")` in C# and expects a `RedirectToActionResult`.

6. **`Tier5_ChallengerM1Harness.cs`**:
   - `CH19` (lines 510-538): Verifies empirical pipeline order in `Program.cs`:
     `actualSessionLine > actualRoutingLine` and `actualSessionLine < actualAuthLine`.

---

## 2. Logic Chain

1. **Decorator Protection Mechanism**:
   - In ASP.NET Core MVC, decorating a Controller class with `[Authorize]` enforces authorization on all action methods within that controller unless an action method is explicitly decorated with `[AllowAnonymous]`.
   - For `AdminController`, decorating the class with `[Authorize]` while decorating `Login(GET)` and `Login(POST)` with `[AllowAnonymous]` ensures that `/admin/login` remains open to unauthenticated users, while `/admin`, `/admin/index`, and `/admin/logout` are protected.
   - For the other 10 Admin Controllers, class-level `[Authorize]` automatically protects all GET and POST actions, eliminating any risk of forgotten endpoints.

2. **C# Instantiation & Reflection Compatibility in Unit Tests**:
   - In .NET, attributes (`[Authorize]`, `[ValidateAntiForgeryToken]`) are metadata stored in type assemblies. They are evaluated by ASP.NET Core MVC's pipeline filters (`AuthorizationFilter`, `AntiforgeryFilter`) during HTTP request execution.
   - When a test instantiates a controller via `new AdminTrangChuController(ctx, env)` or `new AdminController()` and calls `controller.Index()` directly in C#, the C# runtime executes the method body directly without triggering MVC pipeline filters.
   - Therefore, adding `[Authorize]` to classes and `[ValidateAntiForgeryToken]` to actions has **zero impact** on direct C# unit test instantiation or execution (such as `F19_T16`, `F19_T08`, `CH2_02`, `M2_2_C09`).
   - Maintaining `public AdminController() : this(null)` guarantees that tests relying on parameterless constructor (`F19_T08`, `F19_T16`) succeed without modification, while `public AdminController(ApplicationDbContext? context)` allows DI to inject `ApplicationDbContext`.
   - Parameter names (e.g. `int id` in `AdminSanPhamController.Delete` and `int id, string trangThaiDonHang, string? trangThaiThanhToan` in `AdminDonHangController.CapNhatTrangThai`) are preserved with exact signatures, preserving 100% compatibility with reflection tests `F16_T01`, `F16_T02`, `F18_T10`, and `F18_T11`.

3. **HTTP 302 Redirection & Cookie Authentication Contract**:
   - Registering Cookie Authentication with `CookieAuthenticationDefaults.AuthenticationScheme`:
     - `LoginPath = "/admin/login"`
     - `LogoutPath = "/admin/logout"`
     - `AccessDeniedPath = "/admin/login"`
     - `Cookie.Name = ".WEB_TinhNguyen.AdminAuth"`
   - Middleware order in `Program.cs`:
     ```csharp
     app.UseHttpsRedirection();
     app.UseRouting();
     app.UseSession();
     app.UseAuthentication();
     app.UseAuthorization();
     app.UseStaticFiles();
     ```
   - When an unauthenticated user sends a GET or POST request to any protected route (`/admin`, `/AdminTrangChu`, `/CuaHang/AdminSanPham`, etc.):
     1. `UseRouting()` matches the endpoint.
     2. `UseAuthentication()` evaluates the `.WEB_TinhNguyen.AdminAuth` cookie; finding none, `HttpContext.User.Identity.IsAuthenticated` remains `false`.
     3. `UseAuthorization()` detects the `[Authorize]` requirement on the endpoint, challenges the authentication scheme.
     4. `CookieAuthenticationHandler` handles the challenge by returning HTTP 302 redirect with header `Location: /admin/login?ReturnUrl=%2F...`.
     5. The action method is never executed, ensuring unauthorized access and unauthorized state mutations are strictly prevented.

4. **ReturnUrl Safe Handling**:
   - In `AdminController.Login(POST)`, validate `Url.IsLocalUrl(returnUrl)`. If valid, redirect to `returnUrl`; if null, empty, or external, redirect to `nameof(Index)`. This satisfies R2 & R3 while thwarting Open Redirect attacks.

---

## 3. Caveats

- **No Caveats on Scope**: All 11 Admin Controllers and 21 CRUD POST actions were cataloged with 100% precision.
- **Views Lock Compliance**: Views under `WEB_TinhNguyen/Areas/CuaHang/Views/` are SHA-256 hash locked and must NOT be modified. Inspection showed that `ChiTiet.cshtml` and `Index.cshtml` already include `@Html.AntiForgeryToken()` and TagHelpers, so backend controller decoration requires zero view changes.
- **Ambiguous Namespace Warning**: When implementing `AdminController`, ensure `BaoMatMatKhau` is imported cleanly from `WEB_TinhNguyen.DuLieu` without collision with `WEB_TinhNguyen.Areas.CuaHang.Helpers`.

---

## 4. Conclusion & Exact Implementation Specifications

### 4.1 Master Decorator Specification Matrix

| Controller File | Class Attribute Specification | Action Attribute Additions / Changes |
|-----------------|-------------------------------|--------------------------------------|
| `WEB_TinhNguyen/BoDieuKhien/AdminController.cs` | `[Authorize]` | `[AllowAnonymous]` on `Login(GET)` and `Login(POST)`; `[ValidateAntiForgeryToken]` on `Login(POST)`; `[HttpGet, HttpPost]` on `Logout()`; `Index()` inherits `[Authorize]` |
| `WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs` | `[Authorize]` | None needed (all 5 POST actions already have `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs` | `[Authorize]` | None needed (`Edit` POST already has `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs` | `[Authorize]` | None needed (`Edit` POST already has `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs` | `[Authorize]` | None needed (all 4 POST actions already have `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs` | `[Authorize]` | None needed (`Edit` POST already has `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs` | `[Authorize]` | None needed (`Edit` POST already has `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs` | `[Authorize]` | None needed (`Edit` POST already has `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs` | `[Authorize]` | Add `[ValidateAntiForgeryToken]` to `Save` (line 33) and `Delete` (line 68) |
| `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs` | `[Area("CuaHang")]`, `[Authorize]` | None needed (all 3 POST actions already have `[ValidateAntiForgeryToken]`) |
| `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs` | `[Area("CuaHang")]`, `[Authorize]` | Add `[ValidateAntiForgeryToken]` to `CapNhatTrangThai` (line 197) and `XoaDonHang` (line 341) |

---

### 4.2 Exact Code Diffs for Implementation Workers

#### Spec 1: `WEB_TinhNguyen/BoDieuKhien/AdminController.cs`
```csharp
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.BoDieuKhien
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext? _context;

        public AdminController() : this(null)
        {
        }

        public AdminController(ApplicationDbContext? context)
        {
            _context = context;
        }

        // =========================================================================
        // 1. ĐĂNG NHẬP QUẢN TRỊ (LOGIN GET & POST)
        // =========================================================================
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ReturnUrl"] = returnUrl;
            var model = new AdminLoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            var effectiveReturnUrl = returnUrl ?? model.ReturnUrl;
            ViewData["ReturnUrl"] = effectiveReturnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isValid = DbInitializer.ValidateAdminCredentials(model.TenDangNhap, model.MatKhau, _context);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            var admin = DbInitializer.GetAdminByUsername(model.TenDangNhap, _context);
            if (admin == null || !admin.TrangThaiHoatDong)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản quản trị đã bị vô hiệu hóa hoặc không tồn tại.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.TenDangNhap),
                new Claim(ClaimTypes.Role, string.IsNullOrEmpty(admin.VaiTro) ? "Admin" : admin.VaiTro),
                new Claim(ClaimTypes.GivenName, admin.HoTen ?? admin.TenDangNhap),
                new Claim("AdminId", admin.IDTaiKhoan.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNhoDangNhap,
                ExpiresUtc = model.GhiNhoDangNhap ? System.DateTimeOffset.UtcNow.AddDays(7) : System.DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(effectiveReturnUrl) && Url.IsLocalUrl(effectiveReturnUrl))
            {
                return Redirect(effectiveReturnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 2. ĐĂNG XUẤT QUẢN TRỊ (LOGOUT)
        // =========================================================================
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // =========================================================================
        // 3. BẢNG ĐIỀU KHIỂN QUẢN TRỊ (INDEX)
        // =========================================================================
        public IActionResult Index()
        {
            return View();
        }
    }
}
```

#### Spec 2: `WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs
@@ -1,4 +1,5 @@
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Mvc;
 using Microsoft.EntityFrameworkCore;
@@ -14,6 +15,7 @@
 namespace WEB_TinhNguyen.BoDieuKhien
 {
+    [Authorize]
     public class AdminTrangChuController : Controller
     {
```

#### Spec 3: `WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs
@@ -1,4 +1,5 @@
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Mvc;
 using Microsoft.EntityFrameworkCore;
@@ -14,6 +15,7 @@
 namespace WEB_TinhNguyen.BoDieuKhien
 {
+    [Authorize]
     public class AdminChuongTrinhController : Controller
     {
```

#### Spec 4: `WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs
@@ -1,4 +1,5 @@
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Hosting;
 using Microsoft.AspNetCore.Http;
@@ -14,6 +15,7 @@
 namespace WEB_TinhNguyen.BoDieuKhien
 {
+    [Authorize]
     public class AdminGayQuyController : Controller
     {
```

#### Spec 5: `WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs
@@ -5,4 +5,5 @@
 using System.Threading.Tasks;
 using Microsoft.AspNetCore.Hosting;
 using Microsoft.AspNetCore.Http;
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Mvc;
@@ -20,6 +21,7 @@
     /// </summary>
+    [Authorize]
     public class AdminCauHinhQRController : Controller
     {
```

#### Spec 6: `WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs
@@ -1,4 +1,5 @@
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Mvc;
 using Microsoft.EntityFrameworkCore;
@@ -14,6 +15,7 @@
 namespace WEB_TinhNguyen.BoDieuKhien
 {
+    [Authorize]
     public class AdminKyUcController : Controller
     {
```

#### Spec 7: `WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs
@@ -1,4 +1,5 @@
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Hosting;
 using Microsoft.AspNetCore.Http;
@@ -14,6 +15,7 @@
 namespace WEB_TinhNguyen.BoDieuKhien
 {
+    [Authorize]
     public class AdminBanToChucController : Controller
     {
```

#### Spec 8: `WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs
@@ -1,4 +1,5 @@
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Hosting;
 using Microsoft.AspNetCore.Http;
@@ -14,6 +15,7 @@
 namespace WEB_TinhNguyen.BoDieuKhien
 {
+    [Authorize]
     public class AdminTinTucController : Controller
     {
```

#### Spec 9: `WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs`
```diff
--- a/WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs
+++ b/WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs
@@ -1,4 +1,5 @@
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Hosting;
 using Microsoft.AspNetCore.Http;
@@ -13,6 +14,7 @@
 namespace WEB_TinhNguyen.BoDieuKhien
 {
+    [Authorize]
     public class AdminHieuUngController : Controller
     {
@@ -32,6 +34,7 @@
         // 2. THÊM / SỬA HIỆU ỨNG (Lưu file JS)
         // ==========================================================
         [HttpPost]
+        [ValidateAntiForgeryToken]
         public async Task<IActionResult> Save(int IDHieuUng, string TenHieuUng, string MoTa, IFormFile FileJS)
@@ -67,6 +70,7 @@
         // 3. XÓA HIỆU ỨNG
         // ==========================================================
         [HttpPost]
+        [ValidateAntiForgeryToken]
         public async Task<IActionResult> Delete(int id)
```

#### Spec 10: `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs`
```diff
--- a/WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs
+++ b/WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs
@@ -6,4 +6,5 @@
 using Microsoft.AspNetCore.Hosting;
 using Microsoft.AspNetCore.Http;
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Mvc;
@@ -20,6 +21,7 @@
     /// </summary>
     [Area("CuaHang")]
+    [Authorize]
     public class AdminSanPhamController : Controller
     {
```

#### Spec 11: `WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs`
```diff
--- a/WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs
+++ b/WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs
@@ -3,4 +3,5 @@
 using System.Linq;
 using System.Threading.Tasks;
+using Microsoft.AspNetCore.Authorization;
 using Microsoft.AspNetCore.Mvc;
 using Microsoft.EntityFrameworkCore;
@@ -18,6 +19,7 @@
     /// </summary>
     [Area("CuaHang")]
+    [Authorize]
     public class AdminDonHangController : Controller
     {
@@ -196,6 +198,7 @@
         // 3. CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG & RESTOCK (CAP NHAT TRANG THAI)
         // =========================================================================
         [HttpPost]
+        [ValidateAntiForgeryToken]
         public async Task<IActionResult> CapNhatTrangThai(int id, string trangThaiDonHang, string? trangThaiThanhToan)
@@ -340,6 +343,7 @@
         // 4. XÓA ĐƠN HÀNG (XOA DON HANG)
         // =========================================================================
         [HttpPost]
+        [ValidateAntiForgeryToken]
         public async Task<IActionResult> XoaDonHang(int id)
```

#### Spec 12: Pipeline Registration in `WEB_TinhNguyen/Program.cs`
```csharp
// Cookie Authentication Registration
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".WEB_TinhNguyen.AdminAuth";
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/login";
        options.ExpireTimeSpan = System.TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// Middleware Execution Ordering (satisfying CH19)
app.UseHttpsRedirection();
app.UseRouting();

// Middleware Session: Dat sau UseRouting va truoc phan quyen
app.UseSession();

// Middleware Authentication: Dat giua UseSession va UseAuthorization
app.UseAuthentication();

app.UseAuthorization();

// Phục vụ tệp tĩnh
app.UseStaticFiles();
```

---

## 5. Verification Method

To independently verify this strategy after implementation:

1. **Compilation Check**:
   ```powershell
   dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx
   ```
   *Expected*: `Build succeeded. 0 Error(s)`.

2. **Full Regression Test Suite Execution**:
   ```powershell
   dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
   ```
   *Expected*: All 477 existing tests pass with 0 failures and 0 skipped.
   Specifically verify:
   - `F19_T07` & `F19_T08`: `AdminController` compiles and executes `Index()`.
   - `F19_T16`: All 8 admin controllers instantiate and execute `Index()`.
   - `F18_T10` & `F18_T11`: `AdminDonHangController` methods maintain exact signatures and `[HttpPost]`.
   - `F16_T01` & `F16_T02`: `AdminSanPhamController.Delete(int id)` reflection check succeeds.
   - `M2_C27`: `AdminSanPhamController` POST actions enforce `[ValidateAntiForgeryToken]`.
   - `CH19`: Pipeline ordering in `Program.cs` satisfies `Routing < Session < Authorization`.

3. **Reflection Verification of Attributes**:
   - Check that all 11 controller types have `controllerType.GetCustomAttribute<AuthorizeAttribute>() != null`.
   - Check that `AdminController.Login` actions have `[AllowAnonymous]`.
   - Check that `AdminHieuUngController.Save`, `AdminHieuUngController.Delete`, `AdminDonHangController.CapNhatTrangThai`, and `AdminDonHangController.XoaDonHang` have `method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null`.

4. **HTTP 302 Redirection Verification**:
   - Send unauthenticated GET to `/admin` -> Returns HTTP 302 with `Location: /admin/login?ReturnUrl=%2Fadmin`.
   - Send unauthenticated GET to `/CuaHang/AdminSanPham` -> Returns HTTP 302 with `Location: /admin/login?ReturnUrl=%2FCuaHang%2FAdminSanPham`.
   - Send unauthenticated POST to `/AdminTrangChu/CreateChienDich` -> Returns HTTP 302 with `Location: /admin/login?ReturnUrl=%2FAdminTrangChu%2FCreateChienDich`.

5. **Invalidation Conditions**:
   - Any compiler error CS0246, CS0104, or CS7036.
   - Removing parameterless constructor on `AdminController` causing `F19_T08` / `F19_T16` to fail.
   - Changing parameter names or method signatures on `Delete(int id)` or `CapNhatTrangThai(...)` breaking reflection tests.
   - Modifying any files under `WEB_TinhNguyen/Areas/CuaHang/Views/`.
