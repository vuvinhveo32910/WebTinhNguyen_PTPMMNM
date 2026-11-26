# Detailed Analysis Report: Views, Layouts, Navigation & Client-Side Assets for 8 Admin Pages

**Investigator**: Teamwork Explorer (Survey Explorer 2)  
**Date**: 2026-11-06  
**Scope**: 8 Admin Management Pages (AdminTrangChu, AdminChuongTrinh, AdminGayQuy, AdminCauHinhQR, AdminKyUc, AdminBanToChuc, AdminTinTuc, Admin Dashboard)  

---

## 1. Executive Summary

This investigation analyzed the presentation layer of all 8 Admin modules in the `WEB_TinhNguyen` solution. The root cause of the HTTP 404 errors when navigating admin links is an **incorrect controller name specification in `_AdminLayout.cshtml`**, where directory paths (`"GiaoDien/AdminTrangChu"`) were mistakenly supplied as controller names to `@Url.Action(...)`. This caused ASP.NET Core MVC routing to URL-encode the path separator as `%2F`, resulting in malformed target routes like `/GiaoDien%2FAdminTrangChu` that match no controller action descriptor.

Furthermore, a secondary critical routing bug was uncovered: `_AdminLayout.cshtml` is shared with Area `CuaHang` (used by `AdminSanPham` and `AdminDonHang`), but links in the layout do not specify `new { area = "" }`. When an admin is in the `CuaHang` area, ambient route values bleed into the URL generator, converting links like `@Url.Action("Index", "Admin")` into `/CuaHang/Admin` (HTTP 404).

All 8 admin views exist in `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\` and declare `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";`.

---

## 2. Admin Layout & Component Architecture

### 2.1 File Location & Inclusion
- **Primary Layout File**: `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Shared\_AdminLayout.cshtml` (357 lines).
- **Partials**: `_AdminLayout.cshtml` does not reference any external partial view (`<partial ...>` or `@Html.Partial(...)`). The entire desktop sidebar, expandable hover interaction, topbar header, user profile widget, and `@RenderBody()` main section are defined inline in a single layout file.
- **Related ViewStarts**:
  - `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\_ViewStart.cshtml`:
    ```cshtml
    @{
        Layout = "_Layout";
    }
    ```
  - `D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\CuaHang\Views\_ViewStart.cshtml`:
    ```cshtml
    @{
        var controller = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
        var viewPath = ViewContext.View?.Path ?? "";

        if (controller.StartsWith("Admin", StringComparison.OrdinalIgnoreCase) ||
            viewPath.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";
        }
        else
        {
            Layout = "~/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml";
        }
    }
    ```
    *Note*: This dynamic layout switch confirms that Store Admin views (`AdminSanPham` and `AdminDonHang`) in `Areas/CuaHang` execute within `_AdminLayout.cshtml`.

---

## 3. Sidebar Navigation Link Inspection & Root Cause Analysis

### 3.1 Line-by-Line Inspection of `_AdminLayout.cshtml`
Location: `WEB_TinhNguyen\GiaoDien\Shared\_AdminLayout.cshtml` (Lines 240-318)

| Item # | Label | Existing Code (Lines 241-296, 313) | Evaluation / Status | Issue Description |
|---|---|---|---|---|
| 1 | Bảng điều khiển | Line 242: `<a href="@Url.Action("Index", "Admin")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "Admin" ? "active" : "")">` | ⚠️ Missing Area Clear | Generates `/Admin` from root, but when in `CuaHang` area generates `/CuaHang/Admin` (404). |
| 2 | Cấu hình Trang chủ | Line 253: `<a href="@Url.Action("Index", "GiaoDien/AdminTrangChu")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTrangChu" ? "active" : "")">` | ❌ CRITICAL 404 BUG | Controller passed as `"GiaoDien/AdminTrangChu"`. Generates `/GiaoDien%2FAdminTrangChu` -> 404! |
| 3 | Cấu hình Chương trình | Line 258: `<a href="@Url.Action("Index", "GiaoDien/AdminChuongTrinh")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminChuongTrinh" ? "active" : "")">` | ❌ CRITICAL 404 BUG + Active Bug | Generates `/GiaoDien%2FAdminChuongTrinh` -> 404! Active condition also checks `"GiaoDien/AdminChuongTrinh"` so never activates. |
| 4 | Cấu hình Đóng Góp | Line 263: `<a href="@Url.Action("Index", "GiaoDien/AdminGayQuy")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminGayQuy" ? "active" : "")">` | ❌ CRITICAL 404 BUG + Active Bug | Generates `/GiaoDien%2FAdminGayQuy` -> 404! Active condition also checks `"GiaoDien/AdminGayQuy"`. |
| 5 | Sản phẩm Gây Quỹ | Line 268: `<a href="/CuaHang/AdminSanPham" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminSanPham" ? "active" : "")">` | ⚠️ Hardcoded URL | Works, but better as `@Url.Action("Index", "AdminSanPham", new { area = "CuaHang" })`. |
| 6 | Đơn hàng Gây Quỹ | Line 273: `<a href="/CuaHang/AdminDonHang" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminDonHang" ? "active" : "")">` | ⚠️ Hardcoded URL | Works, but better as `@Url.Action("Index", "AdminDonHang", new { area = "CuaHang" })`. |
| 7 | Cấu hình Thanh toán QR | Line 278: `<a href="@Url.Action("Index", "AdminCauHinhQR")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminCauHinhQR" ? "active" : "")">` | ⚠️ Missing Area Clear | Controller name is correct, but lacks `new { area = "" }`. Fails when accessed from `CuaHang` area. |
| 8 | Cấu hình Ký ức | Line 283: `<a href="@Url.Action("Index", "GiaoDien/AdminKyUc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminKyUc" ? "active" : "")">` | ❌ CRITICAL 404 BUG | Controller passed as `"GiaoDien/AdminKyUc"`. Generates `/GiaoDien%2FAdminKyUc` -> 404! |
| 9 | Cấu hình Ban Tổ Chức | Line 288: `<a href="@Url.Action("Index", "GiaoDien/AdminBanToChuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminBanToChuc" ? "active" : "")">` | ❌ CRITICAL 404 BUG | Controller passed as `"GiaoDien/AdminBanToChuc"`. Generates `/GiaoDien%2FAdminBanToChuc` -> 404! |
| 10 | Cấu hình Tin tức | Line 293: `<a href="@Url.Action("Index", "GiaoDien/AdminTinTuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTinTuc" ? "active" : "")">` | ❌ CRITICAL 404 BUG | Controller passed as `"GiaoDien/AdminTinTuc"`. Generates `/GiaoDien%2FAdminTinTuc` -> 404! |
| 11 | Xem trang ngoài | Line 313: `<a href="@Url.Action("Index", "Home")" target="_blank" class="btn-highlight">` | ⚠️ Missing Area Clear | Lacks `new { area = "" }`. From `CuaHang` area generates `/CuaHang/Home` (404). |

### 3.2 Mechanism of URL Malformation
In ASP.NET Core:
1. `IUrlHelper.Action(string? action, string? controller, object? values)` populates route data dictionary `{ controller = "GiaoDien/AdminTrangChu", action = "Index" }`.
2. When evaluating the default route pattern `{controller}/{action}/{id?}`, the router encodes path-breaking characters (`/`) into percent-encoded entities (`%2F`) to preserve single-segment route token semantics.
3. The resulting rendered HTML tag is `<a href="/GiaoDien%2FAdminTrangChu">`.
4. When clicked, the HTTP GET request is received by Kestrel/ASP.NET Core routing. The routing middleware decodes the token into controller `"GiaoDien/AdminTrangChu"`.
5. Since all registered controllers in `WEB_TinhNguyen.BoDieuKhien` have simple un-prefixed names (`AdminTrangChuController` -> controller name `"AdminTrangChu"`), the framework finds 0 matching actions and returns **HTTP 404 Not Found**.

### 3.3 Dashboard View Links (`GiaoDien/Admin/Index.cshtml`)
In `GiaoDien\Admin\Index.cshtml`:
- Line 107: `<a href="@Url.Action("Index", "AdminTrangChu")" ...>` -> Correctly uses `"AdminTrangChu"`.
- Line 139: `<a href="@Url.Action("Index", "AdminKyUc")" ...>` -> Correctly uses `"AdminKyUc"`.
- Line 149: `<a href="#" class="...">` -> **Quản lý Tin tức link is dead (`#`)**! Should be `@Url.Action("Index", "AdminTinTuc", new { area = "" })`.
- Line 159: `<a href="#" class="...">` -> Duyệt Lời chúc is placeholder `#`.

---

## 4. Comprehensive Inventory of the 8 Admin Views

All 8 admin views exist in `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\`.

| # | Admin Page | Physical View Paths | Controller & Actions | Declared Model | Declared Layout |
|---|---|---|---|---|---|
| 1 | **AdminTrangChu** | `GiaoDien/AdminTrangChu/Index.cshtml`<br>`GiaoDien/AdminTrangChu/Edit.cshtml` | `AdminTrangChuController`<br>- `Index()`<br>- `ToggleStatus(int id)`<br>- `CreateChienDich(...)`<br>- `DeleteChienDich(int id)`<br>- `Edit(int id)` | Index: `@model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>`<br>Edit: `@model WEB_TinhNguyen.MoHinh.CauHinhTrangChuVM` | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |
| 2 | **AdminChuongTrinh** | `GiaoDien/AdminChuongTrinh/Index.cshtml`<br>`GiaoDien/AdminChuongTrinh/Edit.cshtml` | `AdminChuongTrinhController`<br>- `Index()`<br>- `Edit(int id)` [GET/POST] | Index: `@model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>`<br>Edit: `@model WEB_TinhNguyen.MoHinh.CauHinhChuongTrinhVM` | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |
| 3 | **AdminGayQuy** | `GiaoDien/AdminGayQuy/Index.cshtml`<br>`GiaoDien/AdminGayQuy/Edit.cshtml` | `AdminGayQuyController`<br>- `Index()`<br>- `Edit(int id)` [GET/POST]<br>- `XoaKenhDongGop(...)`<br>- `XoaNhaTaiTro(...)` | Index: `@model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>`<br>Edit: `@model WEB_TinhNguyen.MoHinh.CauHinhGayQuyVM` | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |
| 4 | **AdminCauHinhQR** | `GiaoDien/AdminCauHinhQR/Index.cshtml`<br>`GiaoDien/AdminCauHinhQR/Create.cshtml`<br>`GiaoDien/AdminCauHinhQR/Edit.cshtml` | `AdminCauHinhQRController`<br>- `Index()`<br>- `Create()` [GET/POST]<br>- `Edit(int id)` [GET/POST]<br>- `Delete(int id)` [POST]<br>- `ToggleStatus(int id)` [POST]<br>- `SetDefault(int id)` [POST]<br>- `PreviewVietQr(...)` [POST] | Index: `@model IEnumerable<WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR>`<br>Create: `@model WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR`<br>Edit: `@model WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR` | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |
| 5 | **AdminKyUc** | `GiaoDien/AdminKyUc/Index.cshtml`<br>`GiaoDien/AdminKyUc/Edit.cshtml` | `AdminKyUcController`<br>- `Index()`<br>- `Edit(int id)` [GET/POST]<br>- `XoaAnhKhoanhKhac(...)` | Index: `@model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>`<br>Edit: `@model WEB_TinhNguyen.MoHinh.CauHinhKyUcVM` | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |
| 6 | **AdminBanToChuc** | `GiaoDien/AdminBanToChuc/Index.cshtml`<br>`GiaoDien/AdminBanToChuc/Edit.cshtml` | `AdminBanToChucController`<br>- `Index()`<br>- `Edit(int id)` [GET/POST]<br>- `XoaThanhVien(...)` | Index: `@model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>`<br>Edit: `@model WEB_TinhNguyen.MoHinh.CauHinhBanToChucVM` | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |
| 7 | **AdminTinTuc** | `GiaoDien/AdminTinTuc/Index.cshtml`<br>`GiaoDien/AdminTinTuc/Edit.cshtml` | `AdminTinTucController`<br>- `Index()`<br>- `Edit(int id)` [GET/POST]<br>- `XoaBaiViet(...)` | Index: `@model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>`<br>Edit: `@model WEB_TinhNguyen.MoHinh.CauHinhTinTucVM` | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |
| 8 | **Admin (Dashboard)** | `GiaoDien/Admin/Index.cshtml` | `AdminController`<br>- `Index()` | *None (Untyped)* | `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` |

---

## 5. Static Assets Verification & Integrity Check

### 5.1 Asset References in `_AdminLayout.cshtml`
- **Tailwind CSS**: `<script src="https://cdn.tailwindcss.com"></script>` -> Active, loaded via CDN.
- **FontAwesome 6.0.0**: `<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">` -> Active, CDN.
- **Google Fonts (Be Vietnam Pro)**: `<link href="https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@400;500;600;700&display=swap" rel="stylesheet">` -> Active, Google Fonts.
- **Local CSS / JS in Layout**: `_AdminLayout.cshtml` does **not** link to `~/css/site.css` or `~/js/site.js`. All layout styles (sidebar hover expansion, colors, transitions) are declared directly in an inline `<style>` block (lines 15-225). This insulates `_AdminLayout` from potential CSS clashes in `site.css`.

### 5.2 Asset References in Admin Views
- **TinyMCE CDN**:
  - `AdminTinTuc/Edit.cshtml` (line 276) and `AdminTrangChu/Edit.cshtml` (line 607) reference:
    `<script src="https://cdnjs.cloudflare.com/ajax/libs/tinymce/6.8.2/tinymce.min.js" referrerpolicy="origin"></script>`
    Resolves properly without requiring an API key.
- **Validation Scripts**:
  - `_ValidationScriptsPartial.cshtml` references `~/lib/jquery-validation/dist/jquery.validate.min.js` and `~/lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js`. Both files physically exist in `WEB_TinhNguyen/wwwroot/lib/`.
- **Uploaded Images**:
  - `wwwroot/uploads/qr/`, `wwwroot/uploads/gayquy/`, `wwwroot/uploads/bantochuc/`, `wwwroot/gallery/` are all present on disk and served by `app.UseStaticFiles()` in `Program.cs`.
  - Missing asset detected: `AdminGayQuyController.cs` line 50 defaults `Hero_LinkAnh` to `"/gallery/2025/bro02087.jpg"`, but `wwwroot/gallery/2025/` does not exist on disk. While not causing a 404 on page navigation, it produces a broken image thumbnail on initial creation if no image is uploaded.

---

## 6. UI Inconsistencies & Layout Mismatches Across Admin Views

### 6.1 Outer Wrapper Inconsistency
- In `_AdminLayout.cshtml`, the main viewport is structured as:
  ```html
  <main class="flex-1 overflow-y-auto p-4 md:p-8 pt-4">
      @RenderBody()
  </main>
  ```
  The body has background `#f4f7fe` and `<main>` already provides generous padding (`p-4 md:p-8 pt-4`).
- **Inconsistency**:
  - `AdminChuongTrinh` (`Index.cshtml` & `Edit.cshtml`, line 7) and `AdminKyUc` (`Index.cshtml` & `Edit.cshtml`, line 7) wrap their entire markup in `<div class="p-6 bg-gray-50 min-h-screen">`.
  - **Resulting UI defects**:
    1. A noticeable nested rectangular box with mismatched background (`bg-gray-50` vs body `#f4f7fe`).
    2. Double horizontal and vertical padding (`p-4 md:p-8` + `p-6`).
    3. Nested vertical scrolling artifacts caused by `min-h-screen` within `overflow-y-auto`.
  - In contrast, `AdminTrangChu`, `AdminGayQuy`, `AdminCauHinhQR`, `AdminBanToChuc`, and `AdminTinTuc` use standard container wrappers like `max-w-7xl mx-auto space-y-6` without `min-h-screen`.

### 6.2 Table Header & Design Divergence
- **Style 1 (Modern Forest Green / Slate)** (`AdminTrangChu`, `AdminCauHinhQR`):
  - Card: `bg-white rounded-[20px] shadow-sm border border-gray-100`
  - Headers: `text-[11px] font-bold text-gray-400 uppercase tracking-widest`
  - Action buttons: `rounded-xl` with dedicated SVG/FontAwesome icons and subtle tint backgrounds.
- **Style 2 (Emerald / Tabs Modern)** (`AdminGayQuy`, `AdminBanToChuc`, `AdminTinTuc`):
  - Card: `rounded-2xl shadow-sm border border-gray-200`
  - Badges: `bg-emerald-50 text-emerald-700`
- **Style 3 (Legacy Minimalist)** (`AdminChuongTrinh`, `AdminKyUc`):
  - Table: `thead` has basic `bg-gray-100`, plain `text-xs text-gray-500 font-medium`.
  - Action buttons: `rounded-md` with emoji `⚙ Cấu hình...`.
  - Back links: Plain unicode characters `⬅ Quay lại danh sách` instead of FontAwesome icons `<i class="fas fa-arrow-left"></i>`.

---

## 7. Proposed Code Changes

### 7.1 Fix Navigation Links in `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`

**Target File**: `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Shared\_AdminLayout.cshtml`  
**Lines 240-318**:

```cshtml
<<<< BEFORE
            <ul class="sidebar-links">
                <li>
                    <a href="@Url.Action("Index", "Admin")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "Admin" ? "active" : "")">
                        <i class="fas fa-tachometer-alt"></i> <span class="link-text">Bảng điều khiển</span>
                    </a>
                </li>

                <h4>
                    <span>Nội dung</span>
                    <div class="menu-separator"></div>
                </h4>

                <li>
                    <a href="@Url.Action("Index", "GiaoDien/AdminTrangChu")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTrangChu" ? "active" : "")">
                        <i class="fas fa-home"></i> <span class="link-text">Cấu hình Trang chủ</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "GiaoDien/AdminChuongTrinh")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminChuongTrinh" ? "active" : "")">
                        <i class="fas fa-calendar-alt"></i> <span class="link-text">Cấu hình Chương trình</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "GiaoDien/AdminGayQuy")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminGayQuy" ? "active" : "")">
                        <i class="fas fa-hand-holding-heart"></i> <span class="link-text">Cấu hình Đóng Góp</span>
                    </a>
                </li>
                <li>
                    <a href="/CuaHang/AdminSanPham" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminSanPham" ? "active" : "")">
                        <i class="fas fa-boxes"></i> <span class="link-text">Sản phẩm Gây Quỹ</span>
                    </a>
                </li>
                <li>
                    <a href="/CuaHang/AdminDonHang" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminDonHang" ? "active" : "")">
                        <i class="fas fa-receipt"></i> <span class="link-text">Đơn hàng Gây Quỹ</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminCauHinhQR")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminCauHinhQR" ? "active" : "")">
                        <i class="fas fa-qrcode"></i> <span class="link-text">Cấu hình Thanh toán QR</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "GiaoDien/AdminKyUc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminKyUc" ? "active" : "")">
                        <i class="fas fa-images"></i> <span class="link-text">Cấu hình Ký ức</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "GiaoDien/AdminBanToChuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminBanToChuc" ? "active" : "")">
                        <i class="fas fa-sitemap"></i> <span class="link-text">Cấu hình Ban Tổ Chức</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "GiaoDien/AdminTinTuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTinTuc" ? "active" : "")">
                        <i class="fas fa-newspaper"></i> <span class="link-text">Cấu hình Tin tức</span>
                    </a>
                </li>

                <h4>
                    <span>Tương tác</span>
                    <div class="menu-separator"></div>
                </h4>
                <li>
                    <a href="#">
                        <i class="fas fa-users"></i> <span class="link-text">Đăng ký TNV</span>
                    </a>
                </li>

                <h4>
                    <span>Hệ thống</span>
                    <div class="menu-separator"></div>
                </h4>
                <li>
                    <a href="@Url.Action("Index", "Home")" target="_blank" class="btn-highlight">
                        <i class="fas fa-external-link-alt"></i> <span class="link-text">Xem trang ngoài</span>
                    </a>
                </li>
            </ul>
==== AFTER
            <ul class="sidebar-links">
                <li>
                    <a href="@Url.Action("Index", "Admin", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "Admin" ? "active" : "")">
                        <i class="fas fa-tachometer-alt"></i> <span class="link-text">Bảng điều khiển</span>
                    </a>
                </li>

                <h4>
                    <span>Nội dung</span>
                    <div class="menu-separator"></div>
                </h4>

                <li>
                    <a href="@Url.Action("Index", "AdminTrangChu", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTrangChu" ? "active" : "")">
                        <i class="fas fa-home"></i> <span class="link-text">Cấu hình Trang chủ</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminChuongTrinh", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminChuongTrinh" ? "active" : "")">
                        <i class="fas fa-calendar-alt"></i> <span class="link-text">Cấu hình Chương trình</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminGayQuy", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminGayQuy" ? "active" : "")">
                        <i class="fas fa-hand-holding-heart"></i> <span class="link-text">Cấu hình Đóng Góp</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminSanPham", new { area = "CuaHang" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminSanPham" ? "active" : "")">
                        <i class="fas fa-boxes"></i> <span class="link-text">Sản phẩm Gây Quỹ</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminDonHang", new { area = "CuaHang" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminDonHang" ? "active" : "")">
                        <i class="fas fa-receipt"></i> <span class="link-text">Đơn hàng Gây Quỹ</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminCauHinhQR", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminCauHinhQR" ? "active" : "")">
                        <i class="fas fa-qrcode"></i> <span class="link-text">Cấu hình Thanh toán QR</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminKyUc", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminKyUc" ? "active" : "")">
                        <i class="fas fa-images"></i> <span class="link-text">Cấu hình Ký ức</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminBanToChuc", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminBanToChuc" ? "active" : "")">
                        <i class="fas fa-sitemap"></i> <span class="link-text">Cấu hình Ban Tổ Chức</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminTinTuc", new { area = "" })" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTinTuc" ? "active" : "")">
                        <i class="fas fa-newspaper"></i> <span class="link-text">Cấu hình Tin tức</span>
                    </a>
                </li>

                <h4>
                    <span>Tương tác</span>
                    <div class="menu-separator"></div>
                </h4>
                <li>
                    <a href="#">
                        <i class="fas fa-users"></i> <span class="link-text">Đăng ký TNV</span>
                    </a>
                </li>

                <h4>
                    <span>Hệ thống</span>
                    <div class="menu-separator"></div>
                </h4>
                <li>
                    <a href="@Url.Action("Index", "Home", new { area = "" })" target="_blank" class="btn-highlight">
                        <i class="fas fa-external-link-alt"></i> <span class="link-text">Xem trang ngoài</span>
                    </a>
                </li>
            </ul>
>>>>
```

### 7.2 Fix Shortcut Card in `WEB_TinhNguyen/GiaoDien/Admin/Index.cshtml`
Line 149 currently points to `#`:
```cshtml
<<<< BEFORE
        <a href="#" class="bg-white border border-gray-200 rounded-xl p-5 flex items-center gap-4 hover:border-orange-500 hover:shadow-lg transition-all group">
            <div class="w-12 h-12 rounded-xl bg-orange-50 text-orange-600 flex items-center justify-center group-hover:bg-orange-600 group-hover:text-white transition-colors">
                <i class="fas fa-newspaper text-lg"></i>
            </div>
            <div>
                <p class="font-bold text-gray-800 text-sm mb-0.5 group-hover:text-orange-700 transition-colors">Quản lý Tin tức</p>
                <p class="text-xs text-gray-500">Đăng bài viết mới</p>
            </div>
        </a>
==== AFTER
        <a href="@Url.Action("Index", "AdminTinTuc", new { area = "" })" class="bg-white border border-gray-200 rounded-xl p-5 flex items-center gap-4 hover:border-orange-500 hover:shadow-lg transition-all group">
            <div class="w-12 h-12 rounded-xl bg-orange-50 text-orange-600 flex items-center justify-center group-hover:bg-orange-600 group-hover:text-white transition-colors">
                <i class="fas fa-newspaper text-lg"></i>
            </div>
            <div>
                <p class="font-bold text-gray-800 text-sm mb-0.5 group-hover:text-orange-700 transition-colors">Quản lý Tin tức</p>
                <p class="text-xs text-gray-500">Đăng bài viết mới</p>
            </div>
        </a>
>>>>
```

### 7.3 Harmonize Layout Wrapper in `AdminChuongTrinh` and `AdminKyUc`
Remove `<div class="p-6 bg-gray-50 min-h-screen">` and replace with `<div class="max-w-7xl mx-auto space-y-6">` matching `AdminTrangChu` and `_AdminLayout`.
