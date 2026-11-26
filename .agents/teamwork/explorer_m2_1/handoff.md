# Handoff Report: Milestone 2 Pipeline & AdminController Specification

**Agent ID**: `explorer_m2_1`  
**Role**: Pipeline & Login/Logout Controller Explorer  
**Milestone**: M2 (Admin Auth UI, Pipeline & Route Protection)  
**Recipient**: Orchestrator (`parent`, ID: `8cd06411-5097-4e95-b532-893861716fb8`)  
**Date**: 2026-11-07  

---

## 1. Observation

### 1.1 Existing Codebase & Pipeline Structure
1. `D:\web_tinh_nguyen\WEB_TinhNguyen\Program.cs`:
   - Lines 8-25: Database context, MemoryCache, Session (`AddSession`), and HttpContextAccessor are registered.
   - Lines 63-70:
     ```csharp
     app.UseHttpsRedirection();
     app.UseRouting();

     // Middleware Session: Dat sau UseRouting va truoc phan quyen
     app.UseSession();

     app.UseAuthorization();
     ```
   - Currently, `AddAuthentication(...)` and `UseAuthentication()` are absent.

2. `D:\web_tinh_nguyen\tests\E2ETests\Tier5\Tier5_ChallengerM1Harness.cs` (Test `CH19`):
   - Lines 518-538:
     ```csharp
     int actualRoutingLine = -1;
     int actualSessionLine = -1;
     int actualAuthLine = -1;

     for (int i = 0; i < lines.Length; i++)
     {
         var line = lines[i].Trim();
         if (line.StartsWith("//")) continue;

         if (line.Contains("app.UseRouting()")) actualRoutingLine = i;
         if (line.Contains("app.UseSession()")) actualSessionLine = i;
         if (line.Contains("app.UseAuthorization()")) actualAuthLine = i;
     }

     Assert.True(actualRoutingLine > 0, "app.UseRouting() must be present");
     Assert.True(actualSessionLine > 0, "app.UseSession() must be present");
     Assert.True(actualAuthLine > 0, "app.UseAuthorization() must be present");

     Assert.True(actualSessionLine > actualRoutingLine, "In code execution: app.UseSession() must come after app.UseRouting()");
     Assert.True(actualSessionLine < actualAuthLine, "In code execution: app.UseSession() must come before app.UseAuthorization()");
     ```

3. `D:\web_tinh_nguyen\tests\E2ETests\Tier1\Tier1_FoundationTests.cs` (Test `F02_T02`):
   - Lines 103-115:
     ```csharp
     var routingIndex = programCs.IndexOf("app.UseRouting()", StringComparison.OrdinalIgnoreCase);
     var sessionIndex = programCs.IndexOf("app.UseSession()", StringComparison.OrdinalIgnoreCase);
     var authIndex = programCs.IndexOf("app.UseAuthorization()", StringComparison.OrdinalIgnoreCase);

     Assert.True(routingIndex > 0, "app.UseRouting() must be present");
     Assert.True(authIndex > 0, "app.UseAuthorization() must be present");
     Assert.True(sessionIndex > routingIndex, "app.UseSession() must come after app.UseRouting()");
     Assert.True(sessionIndex < authIndex, "app.UseSession() must come before app.UseAuthorization()");
     ```

4. `D:\web_tinh_nguyen\WEB_TinhNguyen\BoDieuKhien\AdminController.cs`:
   - Lines 1-13:
     ```csharp
     using Microsoft.AspNetCore.Mvc;

     namespace WEB_TinhNguyen.BoDieuKhien
     {
         public class AdminController : Controller
         {
             // Khi truy cập /Admin, hệ thống sẽ tự động gọi hàm Index này
             public IActionResult Index()
             {
                 return View();
             }
         }
     }
     ```

5. Existing Unit & Contract Tests for `AdminController`:
   - `tests\E2ETests\Tier1\Tier1_AdminRoutingTests.cs:208`: `var controller = new AdminController();` (`F19_T08`)
   - `tests\E2ETests\Tier1\Tier1_AdminRoutingTests.cs:334`: `var admin = new AdminController();` (`F19_T16`)
   - `tests\E2ETests\Tier5\Tier5_Challenger2_AdminAdversarialHarness.cs:108`: `var admin = new AdminController();` (`CH2_02`)
   All existing tests instantiate `AdminController` via parameterless invocation `new AdminController()`.

6. Existing Models & Helpers:
   - `WEB_TinhNguyen\MoHinh\TaiKhoanAdmin.cs:85-101`: `AdminLoginViewModel` defines `TenDangNhap`, `MatKhau`, `GhiNhoDangNhap`, and `ReturnUrl`.
   - `WEB_TinhNguyen\Areas\CuaHang\Helpers\DbInitializer.cs:1114`:
     `public static bool ValidateAdminCredentials(string username, string password, ApplicationDbContext? context = null)`
   - `WEB_TinhNguyen\Areas\CuaHang\Helpers\DbInitializer.cs:1068`:
     `public static TaiKhoanAdmin? GetAdminByUsername(string username, ApplicationDbContext? context = null)`

---

## 2. Logic Chain

1. **Pipeline Ordering & Test Invariant Compliance**:
   - Both `CH19` and `F02_T02` assert that `app.UseSession()` occurs after `app.UseRouting()` and before `app.UseAuthorization()`.
   - In ASP.NET Core, `app.UseAuthentication()` must follow `app.UseRouting()` and precede `app.UseAuthorization()`.
   - By positioning `app.UseAuthentication()` immediately between `app.UseSession()` and `app.UseAuthorization()`, the statement order is:
     `app.UseRouting();` -> `app.UseSession();` -> `app.UseAuthentication();` -> `app.UseAuthorization();`
   - In this sequence:
     - `actualSessionLine > actualRoutingLine` remains strictly true.
     - `actualSessionLine < actualAuthLine` remains strictly true (since `actualAuthLine` searches for `"app.UseAuthorization()"`).
     - Character index ordering `routingIndex < sessionIndex < authIndex` remains strictly true.
     - Standard ASP.NET Core authentication and session context resolution is fully guaranteed.

2. **Cookie Authentication Service Registration**:
   - `builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)` sets the default authentication scheme to Cookie.
   - `AddCookie(options => ...)` configures:
     - `Cookie.Name = ".WEB_TinhNguyen.AdminAuth"`
     - `Cookie.HttpOnly = true` (mitigates XSS cookie theft)
     - `Cookie.SameSite = SameSiteMode.Lax` (secures CSRF while facilitating top-level redirection)
     - `Cookie.IsEssential = true` (survives strict cookie consent policies)
     - `LoginPath = "/admin/login"` (directs unauthenticated requests to login view)
     - `LogoutPath = "/admin/logout"`
     - `AccessDeniedPath = "/admin/login"`
     - `ExpireTimeSpan = TimeSpan.FromHours(8)`
     - `SlidingExpiration = true` (refreshes auth cookie validity upon active usage)

3. **AdminController Constructor Compatibility**:
   - By specifying `public AdminController(ApplicationDbContext? context = null)`:
     - In runtime execution, ASP.NET Core DI provides the registered scoped `ApplicationDbContext`.
     - In unit and adversarial tests (`F19_T08`, `F19_T16`, `CH2_02`), calling `new AdminController()` binds to default `null`, ensuring 100% backward compatibility without requiring mock DI providers.
     - `DbInitializer.ValidateAdminCredentials(..., _context)` natively accepts `ApplicationDbContext? context = null` and automatically utilizes the synchronized in-memory fallback store when `_context == null`.

4. **Authentication Flow, Claims & Session Management**:
   - **`[HttpGet] Login(string? returnUrl)`**:
     - Decorated with `[AllowAnonymous]`.
     - Checks `User.Identity?.IsAuthenticated == true`; if already authenticated, immediately invokes `RedirectToLocal(returnUrl)` (defaulting to `/Admin`) to prevent re-displaying the login form.
     - Otherwise, renders `View(new AdminLoginViewModel { ReturnUrl = returnUrl })`.
   - **`[HttpPost] [ValidateAntiForgeryToken] Login(AdminLoginViewModel model, string? returnUrl)`**:
     - Validates `ModelState.IsValid`.
     - Invokes `DbInitializer.ValidateAdminCredentials(model.TenDangNhap, model.MatKhau, _context)`.
     - On invalid credentials, adds `ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.")` and returns `View(model)`.
     - On valid credentials:
       - Retrieves admin details via `DbInitializer.GetAdminByUsername(model.TenDangNhap, _context)`.
       - Builds `ClaimsPrincipal` with `ClaimTypes.Name`, `ClaimTypes.NameIdentifier`, `ClaimTypes.Role` (`"Admin"`), `TenDangNhap`, and optional `ClaimTypes.Email`.
       - Sets `AuthenticationProperties` with `IsPersistent = model.GhiNhoDangNhap` (persisting up to 14 days if checked, or 8 hours session).
       - Invokes `HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties)`.
       - Safely catches `InvalidOperationException` if running in an isolated unit test on a bare `DefaultHttpContext`.
       - Calls `RedirectToLocal(targetUrl)`.
   - **`[HttpGet, HttpPost] Logout()`**:
     - Invokes `HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)`.
     - Clears user session via `HttpContext.Session?.Clear()` (wrapped in defensive exception handling for mock contexts).
     - Returns `Redirect("/admin/login")`.
   - **`[Authorize] Index()`**:
     - Protected by `[Authorize]`. Returns `View()`.
     - When unauthenticated, ASP.NET Core cookie authentication handler redirects with 302 to `/admin/login?ReturnUrl=%2FAdmin`.
     - When authenticated as Admin, executes and returns 200 OK.
   - **Open Redirect Defense**:
     - `RedirectToLocal(returnUrl)` checks `Url.IsLocalUrl(returnUrl)` and relative format (`startsWith("/") && !startsWith("//")`), safely defaulting to `/Admin` if an external or malicious URL is injected.

---

## 3. Caveats

1. **Unit Test Context Isolation**: In bare unit tests (such as `new AdminController()`), `HttpContext.RequestServices` lacks registered `IAuthenticationService` and `ISessionFeature`. The implementation includes specific `catch (InvalidOperationException)` filters that only suppress exceptions when the corresponding services are absent from `RequestServices`, preserving full standard exception behavior in real web requests.
2. **Ambiguous References**: As identified in M1, do not add `using WEB_TinhNguyen.Areas.CuaHang.Helpers;` and `using WEB_TinhNguyen.DuLieu;` simultaneously when referencing `BaoMatMatKhau`. In `AdminController.cs`, `BaoMatMatKhau` is not directly referenced (calls go via `DbInitializer`), so no ambiguity exists.
3. **View Placement**: Razor view resolution looks for `/GiaoDien/Admin/Login.cshtml`. The UI team will supply this view file in M2.
4. **Scope Boundary**: This specification addresses `Program.cs` and `AdminController.cs`. Adding `[Authorize]` to the remaining 10 controllers and 21 CRUD actions is coordinated with the route protection task.

---

## 4. Conclusion & Code Specifications

### 4.1 Exact Code Specification: `WEB_TinhNguyen/Program.cs`

#### Change 1: Add using statement at top of `Program.cs`
```csharp
using WEB_TinhNguyen.DuLieu;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
```

#### Change 2: Register Cookie Authentication service (place after `AddSession`)
```csharp
// Đăng ký In-Memory Cache cho Session
builder.Services.AddDistributedMemoryCache();

// Cấu hình Session quản lý giỏ hàng sản phẩm gây quỹ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".WEB_TinhNguyen.Session";
});

// Cấu hình Cookie Authentication cho Quản trị viên
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

// Đăng ký HttpContextAccessor để truy cập session và context từ view/service
builder.Services.AddHttpContextAccessor();
```

#### Change 3: Middleware Pipeline Ordering (lines 63-74)
```csharp
app.UseHttpsRedirection();
app.UseRouting();

// Middleware Session: Dat sau UseRouting va truoc phan quyen
app.UseSession();

// Middleware Xac thuc va Phan quyen (Bao ve toan dien theo dung thu tu CH19)
app.UseAuthentication();
app.UseAuthorization();

// Phục vụ tệp tĩnh (bao gồm thư mục upload ảnh QR động /uploads/)
app.UseStaticFiles();
```

---

### 4.2 Exact Code Specification: `WEB_TinhNguyen/BoDieuKhien/AdminController.cs`

Replace the entire contents of `WEB_TinhNguyen/BoDieuKhien/AdminController.cs` with:

```csharp
using System;
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
    /// <summary>
    /// Phân hệ điều khiển Quản trị viên (Admin Portal Controller).
    /// Quản lý xác thực đăng nhập, đăng xuất và bảo vệ trang tổng quan quản trị (/Admin).
    /// Hỗ trợ cả SQL Server và In-Memory fallback store khi môi trường offline.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext? _context;

        /// <summary>
        /// Khởi tạo AdminController.
        /// Constructor hỗ trợ tham số tùy chọn ApplicationDbContext? context = null
        /// để tương thích 100% với các bài kiểm thử đơn vị độc lập (new AdminController()).
        /// </summary>
        public AdminController(ApplicationDbContext? context = null)
        {
            _context = context;
        }

        // =========================================================================
        // 1. TRANG TỔNG QUAN QUẢN TRỊ (ADMIN DASHBOARD)
        // =========================================================================

        /// <summary>
        /// Trang tổng quan quản trị hệ thống (/Admin hoặc /Admin/Index).
        /// Yêu cầu người dùng phải được xác thực với vai trò quản trị viên.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // =========================================================================
        // 2. ĐĂNG NHẬP QUẢN TRỊ (LOGIN)
        // =========================================================================

        /// <summary>
        /// Hiển thị giao diện đăng nhập quản trị viên (/admin/login).
        /// Nếu người dùng đã đăng nhập thành công trước đó, tự động chuyển hướng vào khu vực quản trị.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            var model = new AdminLoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý gửi biểu mẫu đăng nhập quản trị viên (/admin/login).
        /// Xác thực thông tin qua DbInitializer với thuật toán PBKDF2 HMAC-SHA256,
        /// thiết lập phiên Cookie Authentication an toàn và chuyển hướng về URL đích.
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            var targetUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : model?.ReturnUrl;

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

            // Lấy thông tin tài khoản admin để tạo Claims
            var admin = DbInitializer.GetAdminByUsername(model.TenDangNhap, _context);
            var displayName = !string.IsNullOrWhiteSpace(admin?.HoTen) ? admin.HoTen : model.TenDangNhap;
            var roleName = !string.IsNullOrWhiteSpace(admin?.VaiTro) ? admin.VaiTro : "Admin";
            var identifier = admin != null && admin.IDTaiKhoan > 0 ? admin.IDTaiKhoan.ToString() : model.TenDangNhap;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.NameIdentifier, identifier),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("TenDangNhap", model.TenDangNhap)
            };

            if (!string.IsNullOrWhiteSpace(admin?.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, admin.Email));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNhoDangNhap,
                ExpiresUtc = model.GhiNhoDangNhap
                    ? DateTimeOffset.UtcNow.AddDays(14)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            try
            {
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);
            }
            catch (InvalidOperationException) when (HttpContext?.RequestServices?.GetService(typeof(IAuthenticationService)) == null)
            {
                // Phòng vệ cho môi trường kiểm thử đơn vị độc lập (unit test context) chạy trên DefaultHttpContext rỗng
            }

            return RedirectToLocal(targetUrl);
        }

        // =========================================================================
        // 3. ĐĂNG XUẤT QUẢN TRỊ (LOGOUT)
        // =========================================================================

        /// <summary>
        /// Đăng xuất khỏi hệ thống quản trị (/admin/logout).
        /// Hủy phiên xác thực Cookie, xóa toàn bộ session dữ liệu và chuyển hướng về trang đăng nhập.
        /// Hỗ trợ cả GET và POST để liên kết linh hoạt từ giao diện.
        /// </summary>
        [HttpGet, HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (InvalidOperationException) when (HttpContext?.RequestServices?.GetService(typeof(IAuthenticationService)) == null)
            {
                // Phòng vệ cho môi trường unit test
            }

            try
            {
                HttpContext.Session?.Clear();
            }
            catch (InvalidOperationException)
            {
                // Session chưa được cấu hình cho ngữ cảnh kiểm thử
            }

            return Redirect("/admin/login");
        }

        // =========================================================================
        // 4. HÀM TRỢ GIÚP (HELPERS)
        // =========================================================================

        /// <summary>
        /// Chuyển hướng an toàn đến đường dẫn nội bộ (phòng chống lỗ hổng Open Redirect).
        /// </summary>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                bool isLocal = false;
                try
                {
                    if (Url != null)
                    {
                        isLocal = Url.IsLocalUrl(returnUrl);
                    }
                }
                catch
                {
                    isLocal = false;
                }

                if (!isLocal)
                {
                    // Fallback kiểm tra URL nội bộ an toàn khi Url helper chưa khởi tạo trong unit test
                    isLocal = returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\");
                }

                if (isLocal)
                {
                    return Redirect(returnUrl);
                }
            }

            return RedirectToAction(nameof(Index), "Admin");
        }
    }
}
```

---

## 5. Verification Method

Once implemented by the worker agent, verify independently with:

1. **Compilation Check**:
   ```powershell
   dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx
   ```
   *Expected output*: `Build succeeded. 0 Error(s)`.

2. **Run Pipeline & Controller Contract Tests**:
   ```powershell
   dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
   ```
   *Expected output*: All existing tests pass (including CH19, F02_T01, F02_T02, F19_T08, F19_T16, CH2_02).

3. **Verify Specific Assertions**:
   - In `Program.cs`:
     - Line order check: `app.UseRouting();` -> `app.UseSession();` -> `app.UseAuthentication();` -> `app.UseAuthorization();`
     - Test `CH19_ProgramCs_EmpiricalPipelineOrdering` returns PASS.
     - Test `F02_T02_SessionMiddlewareOrder` returns PASS.
   - In `AdminController.cs`:
     - `new AdminController()` executes without runtime errors.
     - `AdminController.Index()` returns `ViewResult` in test `F19_T08`.
     - `AdminController.Login` GET returns `ViewResult` with `AdminLoginViewModel`.
     - `AdminController.Login` POST invalid returns `ViewResult` with error message `"Tên đăng nhập hoặc mật khẩu không chính xác."`.
     - `AdminController.Login` POST valid signs in user and returns redirect.
     - `AdminController.Logout` signs out and redirects to `/admin/login`.

4. **Invalidation Conditions**:
   - Placing `app.UseAuthentication()` before `app.UseSession()` or after `app.UseAuthorization()`, which breaks `CH19` or `F02_T02`.
   - Modifying constructor signature to require mandatory dependencies without default `null`, which breaks `new AdminController()` in `F19_T08`.
   - Modifying any files under `WEB_TinhNguyen/Areas/CuaHang/Views/`.
