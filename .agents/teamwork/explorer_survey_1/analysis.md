# Technical Analysis Report: Admin Navigation & Routing Investigation

**Author**: Survey Explorer 1 (teamwork_explorer)  
**Date**: 2026-11-06  
**Scope**: Project routing, controllers, actions, and navigation links for the 8 Admin pages  
**Working Directory**: `D:\web_tinh_nguyen`  

---

## Executive Summary

When navigating through the Admin area sidebar in `_AdminLayout.cshtml`, clicking 6 of the 8 Admin functions triggers an **HTTP 404 Not Found** error.

The root cause is an incorrect controller naming argument passed to `@Url.Action(action, controller)` in `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`: the developer prefixed the controller name with the view directory name `"GiaoDien/"` (e.g. `@Url.Action("Index", "GiaoDien/AdminTrangChu")`). ASP.NET Core URL helper encodes the slash character as `%2F`, generating non-existent route paths like `/GiaoDien%2FAdminTrangChu`. Because no controller or route matches this pattern, the routing engine returns HTTP 404.

All 8 controllers, actions, and corresponding views already exist in the codebase, compile with 0 errors, and expect matching view models. Fixing the controller arguments in `_AdminLayout.cshtml` (and two active class conditions) will immediately resolve the 404 issue and enable full navigation across all 8 Admin modules.

---

## 1. Routing and Area Configuration

### 1.1 Route Registration (`Program.cs`)
File: `D:\web_tinh_nguyen\WEB_TinhNguyen\Program.cs`  
Lines: 76–86

```csharp
// Route cho Area (Cửa hàng gây quỹ & Admin Cửa hàng): Đặt TRƯỚC route mặc định
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=CuaHang}/{action=Index}/{id?}")
    .WithStaticAssets();

// Route mặc định hệ thống
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
```

Key Findings:
1. **Area Routing**: Only controllers with the `[Area("AreaName")]` attribute match the `"areas"` route pattern. In this project, only controllers under `WEB_TinhNguyen.Areas.CuaHang.Controllers` (`CuaHangController`, `AdminSanPhamController`, `AdminDonHangController`, `GioHangController`) have `[Area("CuaHang")]`.
2. **Root Routing**: None of the 8 general admin controllers have an `[Area]` attribute or `[Route]` prefix. They reside in namespace `WEB_TinhNguyen.BoDieuKhien` and are routed via the default route: `{controller=Home}/{action=Index}/{id?}`.
3. **Endpoint Matching**:
   - `/Admin` or `/Admin/Index` maps to `AdminController.Index`
   - `/AdminTrangChu` or `/AdminTrangChu/Index` maps to `AdminTrangChuController.Index`
   - `/AdminChuongTrinh` or `/AdminChuongTrinh/Index` maps to `AdminChuongTrinhController.Index`
   - `/AdminGayQuy` or `/AdminGayQuy/Index` maps to `AdminGayQuyController.Index`
   - `/AdminCauHinhQR` or `/AdminCauHinhQR/Index` maps to `AdminCauHinhQRController.Index`
   - `/AdminKyUc` or `/AdminKyUc/Index` maps to `AdminKyUcController.Index`
   - `/AdminBanToChuc` or `/AdminBanToChuc/Index` maps to `AdminBanToChucController.Index`
   - `/AdminTinTuc` or `/AdminTinTuc/Index` maps to `AdminTinTucController.Index`

### 1.2 Razor View Engine Configuration
File: `D:\web_tinh_nguyen\WEB_TinhNguyen\Program.cs` (lines 30–51) and `WEB_TinhNguyen\DuLieu\VietnameseViewLocationExpander.cs`

```csharp
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    // Định dạng View cho phân hệ MVC gốc (Root)
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/GiaoDien/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/GiaoDien/Shared/{0}.cshtml");
    options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");       // Fallback an toàn
    options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");   // Fallback an toàn
...
```
Razor resolves non-area controller views via `/GiaoDien/{1}/{0}.cshtml` where `{1}` = Controller name and `{0}` = Action name. For example, `AdminTrangChuController.Index()` resolves to `/GiaoDien/AdminTrangChu/Index.cshtml`.

---

## 2. Investigation of the 8 Admin Features

| # | Admin Feature | Controller Class | Action | View Path | Model Type |
|---|---|---|---|---|---|
| 1 | **AdminTrangChu** | `AdminTrangChuController` | `Index()` | `WEB_TinhNguyen/GiaoDien/AdminTrangChu/Index.cshtml` | `IEnumerable<ChienDich>` |
| 2 | **AdminChuongTrinh** | `AdminChuongTrinhController` | `Index()` | `WEB_TinhNguyen/GiaoDien/AdminChuongTrinh/Index.cshtml` | `IEnumerable<ChienDich>` |
| 3 | **AdminGayQuy** | `AdminGayQuyController` | `Index()` | `WEB_TinhNguyen/GiaoDien/AdminGayQuy/Index.cshtml` | `IEnumerable<ChienDich>` |
| 4 | **AdminCauHinhQR** | `AdminCauHinhQRController` | `Index()` | `WEB_TinhNguyen/GiaoDien/AdminCauHinhQR/Index.cshtml` | `IEnumerable<CauHinhThanhToanQR>` |
| 5 | **AdminKyUc** | `AdminKyUcController` | `Index()` | `WEB_TinhNguyen/GiaoDien/AdminKyUc/Index.cshtml` | `IEnumerable<ChienDich>` |
| 6 | **AdminBanToChuc** | `AdminBanToChucController` | `Index()` | `WEB_TinhNguyen/GiaoDien/AdminBanToChuc/Index.cshtml` | `IEnumerable<ChienDich>` |
| 7 | **AdminTinTuc** | `AdminTinTucController` | `Index()` | `WEB_TinhNguyen/GiaoDien/AdminTinTuc/Index.cshtml` | `IEnumerable<ChienDich>` |
| 8 | **Admin (Bảng điều khiển)** | `AdminController` | `Index()` | `WEB_TinhNguyen/GiaoDien/Admin/Index.cshtml` | None (`ViewData["Title"]`) |

---

## 3. Root Cause of HTTP 404 Errors

File: `D:\web_tinh_nguyen\WEB_TinhNguyen\GiaoDien\Shared\_AdminLayout.cshtml`  
Lines: 241–296

### 3.1 Verbatim Code with Bugs

```html
241:                 <li>
242:                     <a href="@Url.Action("Index", "Admin")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "Admin" ? "active" : "")">
243:                         <i class="fas fa-tachometer-alt"></i> <span class="link-text">Bảng điều khiển</span>
244:                     </a>
245:                 </li>
246: 
247:                 <h4>
248:                     <span>Nội dung</span>
249:                     <div class="menu-separator"></div>
250:                 </h4>
251: 
252:                 <li>
253:                     <a href="@Url.Action("Index", "GiaoDien/AdminTrangChu")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTrangChu" ? "active" : "")">
254:                         <i class="fas fa-home"></i> <span class="link-text">Cấu hình Trang chủ</span>
255:                     </a>
256:                 </li>
257:                 <li>
258:                     <a href="@Url.Action("Index", "GiaoDien/AdminChuongTrinh")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminChuongTrinh" ? "active" : "")">
259:                         <i class="fas fa-calendar-alt"></i> <span class="link-text">Cấu hình Chương trình</span>
260:                     </a>
261:                 </li>
262:                 <li>
263:                     <a href="@Url.Action("Index", "GiaoDien/AdminGayQuy")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminGayQuy" ? "active" : "")">
264:                         <i class="fas fa-hand-holding-heart"></i> <span class="link-text">Cấu hình Đóng Góp</span>
265:                     </a>
266:                 </li>
267:                 <li>
268:                     <a href="/CuaHang/AdminSanPham" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminSanPham" ? "active" : "")">
269:                         <i class="fas fa-boxes"></i> <span class="link-text">Sản phẩm Gây Quỹ</span>
270:                     </a>
271:                 </li>
272:                 <li>
273:                     <a href="/CuaHang/AdminDonHang" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminDonHang" ? "active" : "")">
274:                         <i class="fas fa-receipt"></i> <span class="link-text">Đơn hàng Gây Quỹ</span>
275:                     </a>
276:                 </li>
277:                 <li>
278:                     <a href="@Url.Action("Index", "AdminCauHinhQR")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminCauHinhQR" ? "active" : "")">
279:                         <i class="fas fa-qrcode"></i> <span class="link-text">Cấu hình Thanh toán QR</span>
280:                     </a>
281:                 </li>
282:                 <li>
283:                     <a href="@Url.Action("Index", "GiaoDien/AdminKyUc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminKyUc" ? "active" : "")">
284:                         <i class="fas fa-images"></i> <span class="link-text">Cấu hình Ký ức</span>
285:                     </a>
286:                 </li>
287:                 <li>
288:                     <a href="@Url.Action("Index", "GiaoDien/AdminBanToChuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminBanToChuc" ? "active" : "")">
289:                         <i class="fas fa-sitemap"></i> <span class="link-text">Cấu hình Ban Tổ Chức</span>
290:                     </a>
291:                 </li>
292:                 <li>
293:                     <a href="@Url.Action("Index", "GiaoDien/AdminTinTuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTinTuc" ? "active" : "")">
294:                         <i class="fas fa-newspaper"></i> <span class="link-text">Cấu hình Tin tức</span>
295:                     </a>
296:                 </li>
```

### 3.2 Detailed Defect Breakdown

1. **Incorrect Controller Parameter in `@Url.Action`**:
   The second parameter of `@Url.Action(action, controller)` in ASP.NET Core specifies the controller class prefix (e.g. `"AdminTrangChu"` for `AdminTrangChuController`). When `"GiaoDien/AdminTrangChu"` was provided, ASP.NET Core's URL encoder escaped `/` as `%2F`, generating the href `/GiaoDien%2FAdminTrangChu`. When the browser makes a request to this URL, the route handler cannot find any controller matching `GiaoDien%2FAdminTrangChuController` or any controller `GiaoDienController` with action `AdminTrangChu`, returning **HTTP 404 Not Found**.
   Affected items:
   - Line 253: `"GiaoDien/AdminTrangChu"` -> should be `"AdminTrangChu"`
   - Line 258: `"GiaoDien/AdminChuongTrinh"` -> should be `"AdminChuongTrinh"`
   - Line 263: `"GiaoDien/AdminGayQuy"` -> should be `"AdminGayQuy"`
   - Line 283: `"GiaoDien/AdminKyUc"` -> should be `"AdminKyUc"`
   - Line 288: `"GiaoDien/AdminBanToChuc"` -> should be `"AdminBanToChuc"`
   - Line 293: `"GiaoDien/AdminTinTuc"` -> should be `"AdminTinTuc"`

2. **Active CSS Class Condition Mismatch**:
   - Line 258: `ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminChuongTrinh"`
   - Line 263: `ViewContext.RouteData.Values["Controller"]?.ToString() == "GiaoDien/AdminGayQuy"`
   The RouteData `Controller` value is always `"AdminChuongTrinh"` or `"AdminGayQuy"` without the prefix. These conditions evaluate to `false`, preventing the active menu item styling from highlighting.

3. **Dashboard Shortcut Dead Link**:
   In `WEB_TinhNguyen/GiaoDien/Admin/Index.cshtml`, Line 149:
   `<a href="#" ...> Quản lý Tin tức</a>`
   The news card shortcut points to `#` instead of `@Url.Action("Index", "AdminTinTuc")`.

---

## 4. Controller Action & Runtime Behavior Analysis

### 4.1 Data Loading and Models
- **`AdminController.Index()`**: Returns `View()`. No model is required or used by `/GiaoDien/Admin/Index.cshtml`.
- **`AdminCauHinhQRController.Index()`**: Dual-mode data access. If SQL Server is accessible, queries `_context.CauHinhThanhToanQRs`. If unreachable or empty, automatically falls back to `DbInitializer.GetInMemoryQrConfigs()`. Renders `/GiaoDien/AdminCauHinhQR/Index.cshtml` with model `IEnumerable<CauHinhThanhToanQR>`.
- **`AdminTrangChuController.Index()`**, **`AdminChuongTrinhController.Index()`**, **`AdminGayQuyController.Index()`**, **`AdminKyUcController.Index()`**, **`AdminBanToChucController.Index()`**, **`AdminTinTucController.Index()`**:
  Each queries `_context.ChienDichs.OrderByDescending(c => c.Nam).ToListAsync()` and passes the list to `View(model)`.
  If the `ChienDichs` table contains 0 rows, EF Core returns an empty list `List<ChienDich>`, which each Index view handles gracefully using `@if (!Model.Any())` or empty table body rendering.

### 4.2 Exception & NotFound Analysis
- None of the 8 `Index()` actions return `NotFound()`.
- `NotFound()` is only returned in `Edit(int id)` when an invalid campaign or configuration ID is requested.
- In `_AdminLayout.cshtml`, all assets (Tailwind CDN, FontAwesome, Google Fonts) load externally and do not throw runtime errors.

---

## 5. Proposed Fixes

### 5.1 In `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`

Replace lines 252–266 and 282–296 with:

```html
                <li>
                    <a href="@Url.Action("Index", "AdminTrangChu")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTrangChu" ? "active" : "")">
                        <i class="fas fa-home"></i> <span class="link-text">Cấu hình Trang chủ</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminChuongTrinh")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminChuongTrinh" ? "active" : "")">
                        <i class="fas fa-calendar-alt"></i> <span class="link-text">Cấu hình Chương trình</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminGayQuy")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminGayQuy" ? "active" : "")">
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
                    <a href="@Url.Action("Index", "AdminKyUc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminKyUc" ? "active" : "")">
                        <i class="fas fa-images"></i> <span class="link-text">Cấu hình Ký ức</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminBanToChuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminBanToChuc" ? "active" : "")">
                        <i class="fas fa-sitemap"></i> <span class="link-text">Cấu hình Ban Tổ Chức</span>
                    </a>
                </li>
                <li>
                    <a href="@Url.Action("Index", "AdminTinTuc")" class="@(ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTinTuc" ? "active" : "")">
                        <i class="fas fa-newspaper"></i> <span class="link-text">Cấu hình Tin tức</span>
                    </a>
                </li>
```

### 5.2 In `WEB_TinhNguyen/GiaoDien/Admin/Index.cshtml`

Replace line 149:
```html
<a href="@Url.Action("Index", "AdminTinTuc")" class="bg-white border border-gray-200 rounded-xl p-5 flex items-center gap-4 hover:border-orange-500 hover:shadow-lg transition-all group">
```
instead of `href="#"`.
