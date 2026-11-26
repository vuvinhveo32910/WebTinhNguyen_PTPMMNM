# Handoff Report: Milestone 2 Admin Login View & Layout Architecture

**Agent ID**: `explorer_m2_2`  
**Role**: Login View & Layout Explorer  
**Milestone**: M2 (Admin Auth UI, Pipeline & Route Protection)  
**Recipient**: Orchestrator (`parent`, ID: `8cd06411-5097-4e95-b532-893861716fb8`)  
**Date**: 2026-11-07  

---

## 1. Observation

### 1.1 Existing Model & View Contract Analysis
- **`AdminLoginViewModel` Contract** (`WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs:85-101`):
  ```csharp
  public class AdminLoginViewModel
  {
      [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
      [StringLength(100, ErrorMessage = "Tên đăng nhập không được vượt quá 100 ký tự")]
      [Display(Name = "Tên đăng nhập")]
      public string TenDangNhap { get; set; } = string.Empty;

      [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
      [DataType(DataType.Password)]
      [Display(Name = "Mật khẩu")]
      public string MatKhau { get; set; } = string.Empty;

      [Display(Name = "Ghi nhớ đăng nhập")]
      public bool GhiNhoDangNhap { get; set; } = false;

      public string? ReturnUrl { get; set; }
  }
  ```
  *Key Observations*:
  - The model already provides field-level data annotation error messages in Vietnamese.
  - The model exposes four primary properties: `TenDangNhap`, `MatKhau`, `GhiNhoDangNhap`, and `ReturnUrl`.
  - Global `_ViewImports.cshtml` (`WEB_TinhNguyen/GiaoDien/_ViewImports.cshtml:1-3`) imports `WEB_TinhNguyen.MoHinh` and `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`.

- **Existing Admin Layout Topbar & User Info** (`WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml:326-347`):
  ```html
  <!-- TOPBAR -->
  <header class="h-20 flex items-center justify-between px-4 md:px-8 z-10 shrink-0 mt-4">
      <div class="flex items-center gap-3 text-gray-800 font-extrabold text-xl md:text-2xl tracking-tight">
          @ViewData["Title"]
      </div>

      <div class="flex items-center gap-5">
          <!-- Info user -->
          <div class="hidden sm:flex items-center gap-3 bg-white px-4 py-2 rounded-2xl shadow-sm border border-gray-100">
              <div class="h-8 w-8 rounded-xl bg-[#161a2d] text-white flex items-center justify-center font-bold text-sm shadow-md">
                  A
              </div>
              <div class="flex flex-col">
                  <span class="text-sm font-bold text-gray-800 leading-none">Admin Hệ thống</span>
                  <span class="text-[10px] text-gray-500 font-medium mt-1">Quản trị viên</span>
              </div>
          </div>
          <!-- Logout -->
          <a href="#" title="Đăng xuất" class="w-10 h-10 rounded-full bg-white text-gray-400 hover:text-red-500 hover:bg-red-50 flex items-center justify-center shadow-sm border border-gray-100 transition-all">
              <i class="fas fa-sign-out-alt"></i>
          </a>
      </div>
  </header>
  ```
  *Key Observations*:
  - Line 335: Static avatar initial `'A'`.
  - Line 338: Hardcoded admin name `<span class="text-sm font-bold text-gray-800 leading-none">Admin Hệ thống</span>`.
  - Line 339: Role subtitle `<span class="text-[10px] text-gray-500 font-medium mt-1">Quản trị viên</span>`.
  - Line 343: Dead logout anchor `<a href="#" title="Đăng xuất" ...>`.

- **Design Language & Palette Across Admin Views**:
  - Found in `WEB_TinhNguyen/GiaoDien/Admin/Index.cshtml`, `AdminTrangChu/Index.cshtml`, `AdminCauHinhQR/Index.cshtml`, `_AdminLayout.cshtml`:
    - **Volunteer Green / Emerald Gradient**: `from-[#1b5e20] to-[#4caf50]`, `from-emerald-600 to-green-700`, `hover:from-emerald-700 hover:to-green-800`.
    - **Dark Tech Slate**: `#161a2d` (sidebar background, badge background).
    - **Light Surface**: `#f4f7fe` (body background), `bg-white`, `border-gray-100`, `border-gray-200`.
    - **Typography**: Google Font `Be Vietnam Pro` (400, 500, 600, 700, 800).
    - **Iconography**: FontAwesome 6 (`fas fa-shield-alt`, `fas fa-user`, `fas fa-lock`, `fas fa-eye`, `fas fa-eye-slash`, `fas fa-sign-in-alt`, `fas fa-sign-out-alt`, `fas fa-check-circle`, `fas fa-exclamation-circle`).
    - **Alert Banners**: Red/Error banner style with `rounded-2xl bg-red-50 border border-red-200 text-red-700` and `fa-exclamation-circle`.

- **Test Suite Constraints**:
  - `Tier1_AdminRoutingTests.cs` (F19_T01, F19_T02, F19_T05): Validates that the 8 specific admin index views bind `_AdminLayout.cshtml`. None of these tests require `Login.cshtml` to use `_AdminLayout.cshtml`.
  - `Tier5_Challenger2_AdminAdversarialHarness.cs` (CH2_03, CH2_04, CH2_06): Enforces layout links for the 8 admin index controllers.
  - No existing tests touch or reference `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml` yet.

- **Directory Restriction Compliance**:
  - `WEB_TinhNguyen/Areas/CuaHang/Views/` verified untouched.

---

## 2. Logic Chain

1. **Clean Dedicated Layout for Login (`Layout = null`)**:
   - `_ViewStart.cshtml` in `WEB_TinhNguyen/GiaoDien/` sets the default layout to `_Layout` (the client storefront layout with header navigation, donation banners, cart, and footer).
   - `_AdminLayout.cshtml` renders a full sidebar (`sidebar-container`) and admin topbar for authenticated admins.
   - For a dedicated, distraction-free admin authentication experience, `Login.cshtml` must specify `Layout = null` and declare its own standalone HTML5 document shell. This eliminates unwanted sidebar/navbar clutter while loading `Be Vietnam Pro`, `FontAwesome 6`, and `Tailwind CSS`.

2. **Styling and Theme Alignment**:
   - Consistent with `_AdminLayout.cshtml` and `Admin/Index.cshtml`, the login page adopts the volunteer green / emerald palette:
     - Gradient backdrop with subtle emerald lighting: `bg-gradient-to-br from-slate-900 via-[#0e2717] to-[#161a2d]`.
     - Card container: `bg-white/95 backdrop-blur-xl rounded-3xl shadow-2xl border border-white/20`.
     - Brand badge icon: `bg-gradient-to-tr from-[#1b5e20] to-[#4caf50]` with `<i class="fas fa-shield-alt"></i>`.
     - Action button: `bg-gradient-to-r from-emerald-600 to-green-700 hover:from-emerald-700 hover:to-green-800 text-white font-bold rounded-xl shadow-lg shadow-emerald-700/25`.

3. **Form Security, Binding, and CSRF Protection**:
   - Bound to `@model WEB_TinhNguyen.MoHinh.AdminLoginViewModel`.
   - Incorporates `@Html.AntiForgeryToken()` alongside `<form asp-action="Login" asp-controller="Admin" method="post">` to guarantee CSRF verification by `[ValidateAntiForgeryToken]` in `AdminController`.
   - Binds `asp-for="TenDangNhap"` with an intuitive username icon and autofocus.
   - Binds `asp-for="MatKhau"` with password visibility toggle (`fa-eye` / `fa-eye-slash`) via inline client JavaScript.
   - Binds `asp-for="GhiNhoDangNhap"` with a styled checkbox for persistent authentication cookies.
   - Hidden input `asp-for="ReturnUrl"` preserves post-login redirect targets (e.g., when an admin was redirected from `/admin/cauhinhqr`).

4. **Error Summary Alert Banner**:
   - Rendered when `!ViewData.ModelState.IsValid || TempData["ErrorMessage"] != null`.
   - Formatted with a clean warning icon and bulleted error messages.
   - Displays explicit fallback `"Tên đăng nhập hoặc mật khẩu không chính xác."` when credentials verification fails or when generic model errors occur.
   - Preserves field-specific validation messages (e.g., `"Vui lòng nhập tên đăng nhập"`).

5. **Admin Layout Dynamic Identity & Logout Route**:
   - In `_AdminLayout.cshtml`:
     - Line 335: Dynamic avatar initial `@(User.Identity?.Name?.Length > 0 ? User.Identity.Name.Substring(0, 1).ToUpper() : "A")`.
     - Line 338: Dynamic username badge `@(User.Identity?.Name ?? "Quản trị viên")`.
     - Line 343: Functional logout anchor `<a href="/admin/logout" title="Đăng xuất" ...>` routing directly to `AdminController.Logout()`.

---

## 3. Caveats

1. **TagHelper Resolution**:
   - In Razor views that set `Layout = null`, tag helpers are imported from `_ViewImports.cshtml`. If tag helpers ever fail to expand in custom test harness contexts, using explicit HTML attributes (`name="TenDangNhap"`, `name="MatKhau"`, `name="ReturnUrl"`) along with `asp-for` ensures dual-mode compatibility. Both are supplied in the proposed markup.
2. **Local Redirect URL Sanitization**:
   - The hidden `ReturnUrl` field should be validated in `AdminController.Login(POST)` via `Url.IsLocalUrl(model.ReturnUrl)` to prevent open-redirect vulnerabilities.
3. **No Area Collision**:
   - `AdminController` is in the root namespace (`WEB_TinhNguyen.BoDieuKhien`), and its route is `/admin/login`. No files under `Areas/CuaHang/` are modified.

---

## 4. Conclusion & Proposed Implementation Specifications

### 4.1 Specification 1: `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml`
Worker must create `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Admin\Login.cshtml` with the following complete markup:

```html
@model WEB_TinhNguyen.MoHinh.AdminLoginViewModel
@{
    Layout = null;
    ViewData["Title"] = "Đăng nhập Quản trị Hệ thống";
}
<!DOCTYPE html>
<html lang="vi" class="h-full">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Trăng Sáng Langbiang</title>
    <link rel="shortcut icon" href="/favicon.ico" />

    <!-- Google Font: Be Vietnam Pro -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">

    <!-- FontAwesome 6 -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />

    <!-- Tailwind CSS CDN -->
    <script src="https://cdn.tailwindcss.com"></script>
    <script>
        tailwind.config = {
            theme: {
                extend: {
                    fontFamily: {
                        sans: ['"Be Vietnam Pro"', 'sans-serif'],
                    }
                }
            }
        }
    </script>
    <style>
        body {
            font-family: 'Be Vietnam Pro', sans-serif;
        }
    </style>
</head>
<body class="min-h-screen flex items-center justify-center p-4 sm:p-6 lg:p-8 bg-gradient-to-br from-slate-900 via-[#0e2717] to-[#161a2d] relative overflow-hidden select-none">

    <!-- Ambient Lighting Effects -->
    <div class="absolute -top-32 -left-32 w-96 h-96 bg-emerald-500/20 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-32 -right-32 w-96 h-96 bg-green-600/20 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-80 h-80 bg-yellow-400/10 rounded-full blur-3xl pointer-events-none"></div>

    <!-- Login Card Container -->
    <div class="relative z-10 w-full max-w-md bg-white/95 backdrop-blur-xl rounded-3xl shadow-2xl border border-white/20 p-8 sm:p-10 transition-all">

        <!-- Brand Header -->
        <div class="text-center mb-7">
            <div class="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-tr from-[#1b5e20] to-[#4caf50] text-white text-2xl shadow-lg shadow-emerald-700/30 mb-4 ring-4 ring-emerald-50">
                <i class="fas fa-shield-alt"></i>
            </div>
            <h1 class="text-2xl font-extrabold text-gray-900 tracking-tight">Cổng Quản Trị Hệ Thống</h1>
            <p class="text-xs font-semibold text-emerald-700 mt-1 uppercase tracking-wider">Trăng Sáng Langbiang • Tình Nguyện</p>
        </div>

        <!-- Alert Error Summary Banner -->
        @if (!ViewData.ModelState.IsValid || TempData["ErrorMessage"] != null)
        {
            <div class="mb-6 p-4 rounded-2xl bg-red-50 border border-red-200 text-red-700 text-sm shadow-sm flex items-start gap-3" role="alert">
                <div class="w-8 h-8 rounded-xl bg-red-100 text-red-600 flex items-center justify-center shrink-0 mt-0.5">
                    <i class="fas fa-exclamation-circle text-base"></i>
                </div>
                <div class="flex-1">
                    <p class="font-bold text-red-800">Đăng nhập không thành công</p>
                    <div class="text-xs text-red-600 mt-1 space-y-1">
                        @if (TempData["ErrorMessage"] != null)
                        {
                            <p class="flex items-center gap-1.5"><i class="fas fa-circle text-[5px]"></i> @TempData["ErrorMessage"]</p>
                        }
                        @if (!ViewData.ModelState.IsValid && ViewData.ModelState.ErrorCount > 0)
                        {
                            foreach (var state in ViewData.ModelState.Values)
                            {
                                foreach (var error in state.Errors)
                                {
                                    <p class="flex items-center gap-1.5"><i class="fas fa-circle text-[5px]"></i> @error.ErrorMessage</p>
                                }
                            }
                        }
                        else if (TempData["ErrorMessage"] == null)
                        {
                            <p class="flex items-center gap-1.5"><i class="fas fa-circle text-[5px]"></i> Tên đăng nhập hoặc mật khẩu không chính xác.</p>
                        }
                    </div>
                </div>
            </div>
        }

        <!-- Login Form -->
        <form asp-action="Login" asp-controller="Admin" method="post" class="space-y-5">
            @Html.AntiForgeryToken()
            <input type="hidden" asp-for="ReturnUrl" value="@Model?.ReturnUrl" />

            <!-- Input: Tên đăng nhập -->
            <div>
                <label asp-for="TenDangNhap" class="block text-xs font-bold text-gray-700 uppercase tracking-wider mb-2">
                    Tên đăng nhập
                </label>
                <div class="relative">
                    <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-gray-400">
                        <i class="fas fa-user text-sm"></i>
                    </div>
                    <input asp-for="TenDangNhap"
                           type="text"
                           autocomplete="username"
                           autofocus
                           required
                           placeholder="admin"
                           class="w-full pl-10 pr-4 py-3 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent transition text-gray-800 placeholder-gray-400 bg-gray-50/50 focus:bg-white text-sm" />
                </div>
                <span asp-validation-for="TenDangNhap" class="text-xs text-red-500 mt-1 block"></span>
            </div>

            <!-- Input: Mật khẩu -->
            <div>
                <label asp-for="MatKhau" class="block text-xs font-bold text-gray-700 uppercase tracking-wider mb-2">
                    Mật khẩu
                </label>
                <div class="relative">
                    <div class="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-gray-400">
                        <i class="fas fa-lock text-sm"></i>
                    </div>
                    <input asp-for="MatKhau"
                           type="password"
                           id="matKhauInput"
                           autocomplete="current-password"
                           required
                           placeholder="••••••••••••"
                           class="w-full pl-10 pr-11 py-3 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-transparent transition text-gray-800 placeholder-gray-400 bg-gray-50/50 focus:bg-white text-sm" />
                    <button type="button"
                            id="togglePasswordBtn"
                            onclick="togglePasswordVisibility()"
                            tabindex="-1"
                            class="absolute inset-y-0 right-0 pr-3.5 flex items-center text-gray-400 hover:text-emerald-600 transition"
                            title="Hiện/ẩn mật khẩu">
                        <i class="fas fa-eye text-sm" id="passwordEyeIcon"></i>
                    </button>
                </div>
                <span asp-validation-for="MatKhau" class="text-xs text-red-500 mt-1 block"></span>
            </div>

            <!-- Checkbox: Ghi nhớ đăng nhập & Security info -->
            <div class="flex items-center justify-between pt-1">
                <label class="flex items-center gap-2 cursor-pointer select-none">
                    <input asp-for="GhiNhoDangNhap"
                           type="checkbox"
                           class="w-4 h-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500 cursor-pointer" />
                    <span class="text-xs font-medium text-gray-600">Ghi nhớ đăng nhập</span>
                </label>
                <span class="text-[11px] text-gray-400 flex items-center gap-1">
                    <i class="fas fa-shield-alt text-emerald-600"></i> Bảo mật 256-bit
                </span>
            </div>

            <!-- Submit Button with Volunteer Palette -->
            <button type="submit"
                    class="w-full py-3.5 px-6 rounded-xl font-bold text-white bg-gradient-to-r from-emerald-600 to-green-700 hover:from-emerald-700 hover:to-green-800 shadow-lg shadow-emerald-700/25 hover:shadow-xl transition-all duration-200 flex items-center justify-center gap-2.5 text-sm active:scale-[0.99] mt-2 cursor-pointer">
                <i class="fas fa-sign-in-alt"></i>
                <span>Đăng nhập Quản trị</span>
            </button>
        </form>

        <!-- Back to Website Footer -->
        <div class="mt-8 pt-5 border-t border-gray-100 text-center">
            <a href="/" class="inline-flex items-center gap-2 text-xs font-semibold text-gray-500 hover:text-emerald-600 transition">
                <i class="fas fa-arrow-left"></i> Quay lại Cổng thông tin Trăng Sáng Langbiang
            </a>
        </div>
    </div>

    <!-- Password Visibility Toggle Script -->
    <script>
        function togglePasswordVisibility() {
            var input = document.getElementById('matKhauInput');
            var icon = document.getElementById('passwordEyeIcon');
            if (input && icon) {
                if (input.type === 'password') {
                    input.type = 'text';
                    icon.classList.remove('fa-eye');
                    icon.classList.add('fa-eye-slash');
                } else {
                    input.type = 'password';
                    icon.classList.remove('fa-eye-slash');
                    icon.classList.add('fa-eye');
                }
            }
        }
    </script>
</body>
</html>
```

---

### 4.2 Specification 2: `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` Lines 330-348
Worker must replace lines 331-346 in `_AdminLayout.cshtml`:

#### Target (Current):
```html
            <div class="flex items-center gap-5">
                <!-- Info user -->
                <div class="hidden sm:flex items-center gap-3 bg-white px-4 py-2 rounded-2xl shadow-sm border border-gray-100">
                    <div class="h-8 w-8 rounded-xl bg-[#161a2d] text-white flex items-center justify-center font-bold text-sm shadow-md">
                        A
                    </div>
                    <div class="flex flex-col">
                        <span class="text-sm font-bold text-gray-800 leading-none">Admin Hệ thống</span>
                        <span class="text-[10px] text-gray-500 font-medium mt-1">Quản trị viên</span>
                    </div>
                </div>
                <!-- Logout -->
                <a href="#" title="Đăng xuất" class="w-10 h-10 rounded-full bg-white text-gray-400 hover:text-red-500 hover:bg-red-50 flex items-center justify-center shadow-sm border border-gray-100 transition-all">
                    <i class="fas fa-sign-out-alt"></i>
                </a>
            </div>
```

#### Replacement (Updated):
```html
            <div class="flex items-center gap-5">
                <!-- Info user -->
                <div class="hidden sm:flex items-center gap-3 bg-white px-4 py-2 rounded-2xl shadow-sm border border-gray-100">
                    <div class="h-8 w-8 rounded-xl bg-[#161a2d] text-white flex items-center justify-center font-bold text-sm shadow-md">
                        @(User.Identity?.Name?.Length > 0 ? User.Identity.Name.Substring(0, 1).ToUpper() : "A")
                    </div>
                    <div class="flex flex-col">
                        <span class="text-sm font-bold text-gray-800 leading-none">@(User.Identity?.Name ?? "Quản trị viên")</span>
                        <span class="text-[10px] text-gray-500 font-medium mt-1">Quản trị viên</span>
                    </div>
                </div>
                <!-- Logout -->
                <a href="/admin/logout" title="Đăng xuất" class="w-10 h-10 rounded-full bg-white text-gray-400 hover:text-red-500 hover:bg-red-50 flex items-center justify-center shadow-sm border border-gray-100 transition-all">
                    <i class="fas fa-sign-out-alt"></i>
                </a>
            </div>
```

---

## 5. Verification Method

To independently verify the implementations:

1. **Solution Build**:
   ```powershell
   dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx
   ```
   *Expected Outcome*: `Build succeeded. 0 Error(s)`.

2. **Full Regression Test Suite**:
   ```powershell
   dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
   ```
   *Expected Outcome*: All 477+ tests pass with 0 failures.

3. **Content Verification Checks**:
   - Check `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml` exists:
     ```powershell
     Test-Path "D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Admin\Login.cshtml"
     ```
   - Check `_AdminLayout.cshtml` has updated logout route:
     ```powershell
     Select-String -Path "D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Shared\_AdminLayout.cshtml" -Pattern 'href="/admin/logout"'
     ```
   - Check `_AdminLayout.cshtml` has dynamic user identity:
     ```powershell
     Select-String -Path "D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Shared\_AdminLayout.cshtml" -Pattern '@\(User\.Identity\?\.Name'
     ```
   - Check no files under `WEB_TinhNguyen/Areas/CuaHang/Views/` were touched.

4. **Invalidation Conditions**:
   - Any compilation error in Razor templates.
   - Any broken tests in `Tier1_AdminRoutingTests` or `Tier5_Challenger2_AdminAdversarialHarness`.
   - Missing CSRF token or missing ViewModel binding in `Login.cshtml`.
   - Modifying files inside `WEB_TinhNguyen/Areas/CuaHang/Views/`.
