# Comprehensive Codebase Survey Report: Admin Routes, Controllers, CRUD Operations & Authorization Architecture

## 1. Observation

### 1.1 Project Structure & Areas Layout
A thorough survey of directory hierarchy at `D:\web_tinh_nguyen` revealed the following structural facts:
- **No `Areas/Admin` Directory**: Directory inspection of `WEB_TinhNguyen/Areas` confirms that only `Areas/CuaHang` exists. There is **no** `Areas/Admin` directory.
- **Vietnamese Directory Convention for MVC Root**:
  - `WEB_TinhNguyen/BoDieuKhien/` holds standard MVC controllers.
  - `WEB_TinhNguyen/GiaoDien/` holds Razor views (`/GiaoDien/{controller}/{action}.cshtml`).
  - `WEB_TinhNguyen/DuLieu/` holds Entity Framework Core database context (`ApplicationDbContext.cs`), view location expander (`VietnameseViewLocationExpander.cs`), and compatibility classes.
  - `WEB_TinhNguyen/MoHinh/` holds domain entities (`Entities.cs`) and ViewModels.
- **Area Directory Convention**:
  - `WEB_TinhNguyen/Areas/CuaHang/` contains `Controllers/`, `Helpers/`, `Models/`, and `Views/`.

### 1.2 Inventory of Admin Controllers & Endpoints

A total of **11 controllers** govern administrative features across the system (9 in `BoDieuKhien` and 2 in `Areas/CuaHang/Controllers`).

#### A. Root Administrative Controllers (`WEB_TinhNguyen/BoDieuKhien/`)

| # | Controller File & Namespace | Action Method | HTTP Verb | Route URL | View Path / Output | Purpose |
|---|---|---|---|---|---|---|
| 1 | `AdminController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/Admin`<br>`/Admin/Index` | `~/GiaoDien/Admin/Index.cshtml` | Main Admin Dashboard with KPI counters & quick feature links. |
| 2 | `AdminTrangChuController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminTrangChu`<br>`/AdminTrangChu/Index` | `~/GiaoDien/AdminTrangChu/Index.cshtml` | List of annual campaigns (`ChienDich`). |
| | | `CreateChienDich(int Nam, string TieuDe)` | POST | `/AdminTrangChu/CreateChienDich` | Redirects to `Index` | Creates new annual campaign record. (`[ValidateAntiForgeryToken]`) |
| | | `ToggleStatus(int id)` | POST | `/AdminTrangChu/ToggleStatus` | Redirects to `Index` | Toggles campaign active status. (`[ValidateAntiForgeryToken]`) |
| | | `DeleteChienDich(int id)` | POST | `/AdminTrangChu/DeleteChienDich` | Redirects to `Index` | Permanently deletes a campaign. (`[ValidateAntiForgeryToken]`) |
| | | `Edit(int id)` | GET | `/AdminTrangChu/Edit/{id}` | `~/GiaoDien/AdminTrangChu/Edit.cshtml` | Form to edit banners, countdowns, floating images, gallery, news, sponsors, FAQs. |
| | | `Edit(CauHinhTrangChuVM vm)` | POST | `/AdminTrangChu/Edit` | Redirects to `Index` / `View(vm)` | Persists full homepage configuration. (`[ValidateAntiForgeryToken]`) |
| | | `UploadHieuUngNhanh(int idChienDich, string tenHieuUng, IFormFile fileJS)` | POST | `/AdminTrangChu/UploadHieuUngNhanh` | Redirects to `Edit` | Uploads JS visual effect to `/effects/` and saves to DB. (`[ValidateAntiForgeryToken]`) |
| 3 | `AdminChuongTrinhController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminChuongTrinh`<br>`/AdminChuongTrinh/Index` | `~/GiaoDien/AdminChuongTrinh/Index.cshtml` | Campaign selection table for program itinerary editing. |
| | | `Edit(int id)` | GET | `/AdminChuongTrinh/Edit/{id}` | `~/GiaoDien/AdminChuongTrinh/Edit.cshtml` | Itinerary & stage editor form. |
| | | `Edit(CauHinhChuongTrinhVM vm)` | POST | `/AdminChuongTrinh/Edit` | Redirects to `Index` | Persists program stages (`ChangChuongTrinh`) and timeline events (`ChiTietLoTrinh`). (`[ValidateAntiForgeryToken]`) |
| 4 | `AdminGayQuyController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminGayQuy`<br>`/AdminGayQuy/Index` | `~/GiaoDien/AdminGayQuy/Index.cshtml` | Campaign selection table for fundraising config. |
| | | `Edit(int id)` | GET | `/AdminGayQuy/Edit/{id}` | `~/GiaoDien/AdminGayQuy/Edit.cshtml` | Donation channel and sponsor configuration editor. |
| | | `Edit(CauHinhGayQuyVM vm)` | POST | `/AdminGayQuy/Edit` | Redirects to `Index` | Persists donation channels (`KenhDongGop`) and sponsors (`NhaTaiTroGayQuy`). (`[ValidateAntiForgeryToken]`) |
| 5 | `AdminCauHinhQRController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminCauHinhQR`<br>`/AdminCauHinhQR/Index` | `~/GiaoDien/AdminCauHinhQR/Index.cshtml` | VietQR configurations management table with active toggles. |
| | | `Create()` | GET | `/AdminCauHinhQR/Create` | `~/GiaoDien/AdminCauHinhQR/Create.cshtml` | Form for new QR configuration with live VietQR preview. |
| | | `Create(CauHinhThanhToanQR model, IFormFile? FileAnhQR)` | POST | `/AdminCauHinhQR/Create` | Redirects to `Index` | Creates new QR configuration. (`[ValidateAntiForgeryToken]`) |
| | | `Edit(int id)` | GET | `/AdminCauHinhQR/Edit/{id}` | `~/GiaoDien/AdminCauHinhQR/Edit.cshtml` | Edit existing QR configuration. |
| | | `Edit(int id, CauHinhThanhToanQR model, IFormFile? FileAnhQR)` | POST | `/AdminCauHinhQR/Edit/{id}` | Redirects to `Index` | Updates QR configuration. (`[ValidateAntiForgeryToken]`) |
| | | `ToggleActive(int id)` | POST | `/AdminCauHinhQR/ToggleActive` | JSON or Redirects to `Index` | Toggles QR active flag. Supports AJAX. (`[ValidateAntiForgeryToken]`) |
| | | `Delete(int id)` | POST | `/AdminCauHinhQR/Delete` | Redirects to `Index` | Deletes QR configuration. (`[ValidateAntiForgeryToken]`) |
| 6 | `AdminKyUcController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminKyUc`<br>`/AdminKyUc/Index` | `~/GiaoDien/AdminKyUc/Index.cshtml` | Campaign selection table for memory gallery. |
| | | `Edit(int id)` | GET | `/AdminKyUc/Edit/{id}` | `~/GiaoDien/AdminKyUc/Edit.cshtml` | Form to edit memorial photo gallery and sponsors. |
| | | `Edit(CauHinhKyUcVM vm)` | POST | `/AdminKyUc/Edit` | Redirects to `Index` | Persists gallery photos (`AnhKyUc`) and sponsors (`NhaTaiTroKyUc`). (`[ValidateAntiForgeryToken]`) |
| 7 | `AdminBanToChucController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminBanToChuc`<br>`/AdminBanToChuc/Index` | `~/GiaoDien/AdminBanToChuc/Index.cshtml` | Campaign selection table for organizing committee. |
| | | `Edit(int id)` | GET | `/AdminBanToChuc/Edit/{id}` | `~/GiaoDien/AdminBanToChuc/Edit.cshtml` | Form to edit committee tree chart and members. |
| | | `Edit(CauHinhBanToChucVM vm)` | POST | `/AdminBanToChuc/Edit` | Redirects to `Index` | Persists committee members (`ThanhVienBanToChuc`). (`[ValidateAntiForgeryToken]`) |
| 8 | `AdminTinTucController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminTinTuc`<br>`/AdminTinTuc/Index` | `~/GiaoDien/AdminTinTuc/Index.cshtml` | Campaign selection table for news/articles. |
| | | `Edit(int id)` | GET | `/AdminTinTuc/Edit/{id}` | `~/GiaoDien/AdminTinTuc/Edit.cshtml` | Form to edit news stories and announcements. |
| | | `Edit(CauHinhTinTucVM vm)` | POST | `/AdminTinTuc/Edit` | Redirects to `Index` | Persists news articles (`BaiVietTinTuc`). (`[ValidateAntiForgeryToken]`) |
| 9 | `AdminHieuUngController.cs`<br>`WEB_TinhNguyen.BoDieuKhien` | `Index()` | GET | `/AdminHieuUng`<br>`/AdminHieuUng/Index` | `~/GiaoDien/AdminHieuUng/Index.cshtml` | List of JS visual animation effects. |
| | | `Save(int IDHieuUng, string TenHieuUng, string MoTa, IFormFile FileJS)` | POST | `/AdminHieuUng/Save` | Redirects to `Index` | Creates or updates animation JS effect in `/effects/`. ⚠️ Lacks `[ValidateAntiForgeryToken]`. |
| | | `Delete(int id)` | POST | `/AdminHieuUng/Delete` | Redirects to `Index` | Deletes effect record and deletes file from `/effects/`. ⚠️ Lacks `[ValidateAntiForgeryToken]`. |

#### B. Store Area Administrative Controllers (`WEB_TinhNguyen/Areas/CuaHang/Controllers/`)

| # | Controller File & Namespace | Action Method | HTTP Verb | Route URL | View Path / Output | Purpose |
|---|---|---|---|---|---|---|
| 10 | `AdminSanPhamController.cs`<br>`[Area("CuaHang")]`<br>`WEB_TinhNguyen.Areas.CuaHang.Controllers` | `Index()` | GET | `/CuaHang/AdminSanPham`<br>`/CuaHang/AdminSanPham/Index` | `~/Areas/CuaHang/Views/AdminSanPham/Index.cshtml` | Product catalog management table with search, stock, pricing, and actions. |
| | | `Create()` | GET | `/CuaHang/AdminSanPham/Create` | `~/Areas/CuaHang/Views/AdminSanPham/Create.cshtml` | New product form with image upload. |
| | | `Create(SanPhamCrudViewModel model)` | POST | `/CuaHang/AdminSanPham/Create` | Redirects to `Index` | Saves new product (`SanPhamGayQuy`). (`[ValidateAntiForgeryToken]`) |
| | | `Edit(int id)` | GET | `/CuaHang/AdminSanPham/Edit/{id}` | `~/Areas/CuaHang/Views/AdminSanPham/Edit.cshtml` | Edit existing product details. |
| | | `Edit(int id, SanPhamCrudViewModel model)` | POST | `/CuaHang/AdminSanPham/Edit/{id}` | Redirects to `Index` | Updates product entity. (`[ValidateAntiForgeryToken]`) |
| | | `Delete(int id)` | POST | `/CuaHang/AdminSanPham/Delete` | Redirects to `Index` | Deletes product or sets `TrangThai = false` if order history exists. (`[ValidateAntiForgeryToken]`) |
| 11 | `AdminDonHangController.cs`<br>`[Area("CuaHang")]`<br>`WEB_TinhNguyen.Areas.CuaHang.Controllers` | `Index(string? q, string? trangThai, string? thanhToan, string? sort)` | GET | `/CuaHang/AdminDonHang`<br>`/CuaHang/AdminDonHang/Index` | `~/Areas/CuaHang/Views/AdminDonHang/Index.cshtml` | Order management dashboard with KPI cards, search, status filtering. |
| | | `ChiTiet(int id)` | GET | `/CuaHang/AdminDonHang/ChiTiet/{id}` | `~/Areas/CuaHang/Views/AdminDonHang/ChiTiet.cshtml` | Order inspection view with customer information, ordered items, VietQR verification, status updates. |
| | | `CapNhatTrangThai(int id, string trangThaiDonHang, string? trangThaiThanhToan)` | POST | `/CuaHang/AdminDonHang/CapNhatTrangThai` | JSON or Redirects to `ChiTiet` | Updates order & payment status. Restocks inventory if cancelled. Supports AJAX. ⚠️ Lacks `[ValidateAntiForgeryToken]`. |
| | | `XoaDonHang(int id)` | POST | `/CuaHang/AdminDonHang/XoaDonHang` | JSON or Redirects to `Index` | Permanently removes order and restocks inventory. Supports AJAX. ⚠️ Lacks `[ValidateAntiForgeryToken]`. |

### 1.3 Complete CRUD Inventory (Data Mutation Endpoints)

Summary of all 21 data-modifying actions across administrative controllers:
1. `AdminTrangChuController.CreateChienDich` (POST) — **C**reate
2. `AdminTrangChuController.ToggleStatus` (POST) — **U**pdate
3. `AdminTrangChuController.DeleteChienDich` (POST) — **D**elete
4. `AdminTrangChuController.Edit` (POST) — **U**pdate
5. `AdminTrangChuController.UploadHieuUngNhanh` (POST) — **C**reate
6. `AdminChuongTrinhController.Edit` (POST) — **U**pdate / **C**reate child items / **D**elete old child items
7. `AdminGayQuyController.Edit` (POST) — **U**pdate / **C**reate child items / **D**elete old child items
8. `AdminCauHinhQRController.Create` (POST) — **C**reate
9. `AdminCauHinhQRController.Edit` (POST) — **U**pdate
10. `AdminCauHinhQRController.ToggleActive` (POST) — **U**pdate
11. `AdminCauHinhQRController.Delete` (POST) — **D**elete
12. `AdminKyUcController.Edit` (POST) — **U**pdate / **C**reate child items / **D**elete old child items
13. `AdminBanToChucController.Edit` (POST) — **U**pdate / **C**reate child items / **D**elete old child items
14. `AdminTinTucController.Edit` (POST) — **U**pdate / **C**reate child items / **D**elete old child items
15. `AdminHieuUngController.Save` (POST) — **C**reate / **U**pdate
16. `AdminHieuUngController.Delete` (POST) — **D**elete
17. `AdminSanPhamController.Create` (POST) — **C**reate
18. `AdminSanPhamController.Edit` (POST) — **U**pdate
19. `AdminSanPhamController.Delete` (POST) — **D**elete / Soft-delete
20. `AdminDonHangController.CapNhatTrangThai` (POST) — **U**pdate & inventory restock
21. `AdminDonHangController.XoaDonHang` (POST) — **D**elete & inventory restock

*Note on non-admin POST endpoints*:
- `HomeController.LikeBaiViet(int id)`: Public increment counter for news post likes.
- `GioHangController`: Public customer actions (`ThemVaoGio`, `CapNhatSoLuong`, `XoaKhoiGio`, `XoaGioHang`, `DatHang`).

### 1.4 Existing Authorization & Authentication Mechanism Assessment
- **Zero `[Authorize]` Attributes**: An exhaustive grep for `Authorize` across all `.cs` files in `WEB_TinhNguyen` returned 0 occurrences on classes or methods.
- **Zero `[AllowAnonymous]` Attributes**: No bypass filters are present.
- **No Authentication Schemes Registered**: In `Program.cs`, only `app.UseAuthorization();` is invoked (line 69), but `builder.Services.AddAuthentication(...)` is completely absent. No authentication handler or cookie scheme is registered.
- **No `app.UseAuthentication()` Middleware**: The middleware pipeline in `Program.cs` lacks `app.UseAuthentication()`, rendering `app.UseAuthorization()` a passive no-op.
- **No User Entity or Table**: Neither `Entities.cs` nor `ApplicationDbContext.cs` defines any `User`, `Account`, `AdminUser`, or `TaiKhoan` entity/DbSet.
- **No Login / Logout Endpoints or Views**:
  - Requesting `/admin/login` triggers default routing `{controller=Admin}/{action=Login}`, which returns HTTP 404 because `AdminController` has only `Index()`.
  - In `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` (line 343):
    ```html
    <a href="#" title="Đăng xuất" class="w-10 h-10 rounded-full bg-white text-gray-400 hover:text-red-500 hover:bg-red-50 flex items-center justify-center shadow-sm border border-gray-100 transition-all">
        <i class="fas fa-sign-out-alt"></i>
    </a>
    ```
    The logout button is a non-functional `#` anchor.
  - User identity display in `_AdminLayout.cshtml` (lines 333-341) is hardcoded HTML ("Admin Hệ thống", "Quản trị viên", avatar letter "A").

---

## 2. Logic Chain

1. **Vulnerability from Absence of Auth Pipeline**:
   - Because `Program.cs` lacks `AddAuthentication(...)` and `app.UseAuthentication()`, and no controller carries an `[Authorize]` attribute, all 11 administrative controllers (encompassing 29 GET and POST actions) are completely public. Any anonymous web user can navigate to `/Admin`, view donor records at `/CuaHang/AdminDonHang`, delete products at `/CuaHang/AdminSanPham/Delete`, alter bank account configuration at `/AdminCauHinhQR/Edit`, or overwrite homepage content at `/AdminTrangChu/Edit`.

2. **Route Alignment with User Requirement R3**:
   - The user request explicitly demands:
     > "Áp dụng cơ chế kiểm soát truy cập (Authorization) cho toàn bộ các route thuộc khu vực quản trị `/admin` và tất cả các action/endpoint thực hiện thao tác thêm, xóa, sửa dữ liệu. Khi người dùng chưa đăng nhập hoặc không có vai trò quản trị truy cập vào bất kỳ trang nào thuộc `/admin` hoặc gửi yêu cầu thêm/xóa/sửa, hệ thống phải chặn truy cập và tự động chuyển hướng về trang đăng nhập `/admin/login` (kèm thông báo yêu cầu đăng nhập)."
   - Route mapping in `Program.cs` currently routes `/admin` to `AdminController.Index()`. Adding `Login(string? returnUrl)` and `Logout()` actions to `AdminController` creates a natural, direct mapping where `/admin/login` invokes `AdminController.Login` and resolves view `/GiaoDien/Admin/Login.cshtml` under the existing Vietnamese Razor ViewEngine setup (`/GiaoDien/{1}/{0}.cshtml`).

3. **Separation of Concerns: Public vs Admin Controllers**:
   - Public controllers:
     - `HomeController`: Landing page, Program, Donation channels, About, Committee, News, Memories. Must remain accessible without authentication.
     - `CuaHangController`: Storefront catalog and product detail. Must remain public.
     - `GioHangController`: Public shopping cart and checkout. Must remain public.
   - Admin controllers:
     - `AdminController`, `AdminTrangChuController`, `AdminChuongTrinhController`, `AdminGayQuyController`, `AdminCauHinhQRController`, `AdminKyUcController`, `AdminBanToChucController`, `AdminTinTucController`, `AdminHieuUngController`, `AdminSanPhamController`, `AdminDonHangController`.
     - All of these 11 controllers must require administrative authentication.

4. **Integration with Existing Test Oracle Contracts**:
   - In `tests/E2ETests/Tier1/Tier1_AdminRoutingTests.cs`:
     - Test `F19_T08` instantiates `new AdminController()` with its default parameterless constructor and calls `.Index()`.
     - Test `F19_T16` instantiates all 8 admin controllers directly and verifies `.Index()`.
     - Therefore, when updating `AdminController` to support database queries or services for login authentication, `AdminController` must maintain either a parameterless constructor or optional constructor arguments (e.g. `public AdminController(ApplicationDbContext? context = null)`) so existing tests in `Tier1_AdminRoutingTests.cs` continue to compile and pass without modification.
   - Test `F16_T01` in `Tier1_AdminTests.cs` checks `typeof(AdminSanPhamController).GetMethod("Delete")` and requires an `id` parameter. The action method name `Delete` and parameter `int id` must remain intact.

---

## 3. Caveats

1. **Database Online vs Offline Fallback**:
   - The test harness frequently runs in sandbox environments where SQL Server is not reachable (e.g., using `CreateOfflineDbContext()` pointing to `Server=127.0.0.1,65534`).
   - The auth implementation must support both SQL Server online (reading hashed credentials from SQL Server) and a safe fallback to the default admin credentials (`admin` / `Admin@123`) when database connectivity is unavailable, consistent with the existing dual-mode pattern in `DbInitializer.cs`.
2. **AJAX Handling for Endpoints**:
   - Actions like `AdminCauHinhQRController.ToggleActive`, `AdminDonHangController.CapNhatTrangThai`, and `AdminDonHangController.XoaDonHang` support both standard Form POST and AJAX (`XMLHttpRequest`).
   - Standard cookie authentication automatically issues an HTTP 302 redirect. Acceptance Criteria AC 37 states:
     > "Khi chưa đăng nhập tài khoản admin, mọi yêu cầu truy cập (GET/POST) đến bất kỳ route nào thuộc `/admin` hoặc các endpoint thêm, xóa, sửa đều bị từ chối và tự động chuyển hướng (HTTP redirect 302) về trang đăng nhập `/admin/login`."
   - Hence, unauthenticated AJAX requests should redirect (302) to `/admin/login`, matching the explicit requirement.
3. **Session vs Cookie Persistence**:
   - The user request mentions `phiên đăng nhập an toàn (cookie/session)`. Using ASP.NET Core Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`) with HTTP-only cookies, strict SameSite, and essential cookie flags is the industry standard for ASP.NET Core MVC, fully satisfying R2 and R3.
4. **Scope Boundaries**:
   - This survey is strictly read-only. No source files were modified during this investigation.

---

## 4. Conclusion & Recommended Route & Authorization Strategy

### 4.1 Recommended Route Architecture

1. **Admin Login & Logout Endpoints**:
   - Define in `WEB_TinhNguyen/BoDieuKhien/AdminController.cs`:
     - `[HttpGet] [AllowAnonymous]` `public IActionResult Login(string? returnUrl = null)`
     - `[HttpPost] [AllowAnonymous] [ValidateAntiForgeryToken]` `public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)`
     - `[HttpGet, HttpPost]` `public async Task<IActionResult> Logout()`
   - Map explicit routes or utilize default routing `{controller=Admin}/{action=Login}` so `/admin/login` and `/admin/logout` resolve cleanly.
   - Render login UI at `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml`.

2. **Admin Controller Protection Strategy**:
   - Decorate all 11 admin controllers with `[Authorize]`:
     - `AdminController` (with `[AllowAnonymous]` on `Login` GET & POST)
     - `AdminTrangChuController`
     - `AdminChuongTrinhController`
     - `AdminGayQuyController`
     - `AdminCauHinhQRController`
     - `AdminKyUcController`
     - `AdminBanToChucController`
     - `AdminTinTucController`
     - `AdminHieuUngController`
     - `AdminSanPhamController`
     - `AdminDonHangController`
   - Alternatively, implement an `[AdminAuthorize]` authorization filter attribute or base controller. Applying `[Authorize]` at class level directly guarantees that any future action added to these controllers is protected by default.

3. **Pipeline Configuration (`Program.cs`)**:
   - Add Cookie Authentication:
     ```csharp
     builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
         .AddCookie(options =>
         {
             options.LoginPath = "/admin/login";
             options.LogoutPath = "/admin/logout";
             options.AccessDeniedPath = "/admin/login";
             options.Cookie.Name = ".WEB_TinhNguyen.AdminAuth";
             options.Cookie.HttpOnly = true;
             options.Cookie.SameSite = SameSiteMode.Lax;
             options.ExpireTimeSpan = TimeSpan.FromHours(8);
             options.SlidingExpiration = true;
         });
     ```
   - Register `app.UseAuthentication();` in the middleware pipeline immediately **before** `app.UseAuthorization();` and after `app.UseSession();`.

4. **Security Enhancements for CRUD Actions**:
   - Add `[ValidateAntiForgeryToken]` to:
     - `AdminHieuUngController.Save`
     - `AdminHieuUngController.Delete`
     - `AdminDonHangController.CapNhatTrangThai`
     - `AdminDonHangController.XoaDonHang`
   - Validate `returnUrl` with `Url.IsLocalUrl(returnUrl)` before redirecting post-login to eliminate open redirect vulnerabilities.

5. **Layout Updates (`_AdminLayout.cshtml`)**:
   - Connect the logout button (line 343) to `@Url.Action("Logout", "Admin", new { area = "" })` or `/admin/logout`.
   - Bind current username dynamically from `User.Identity?.Name` or default display name.

---

## 5. Verification Method

To independently verify the observations and conclusions in this report:

1. **Verify No `Areas/Admin` Exists**:
   ```powershell
   Test-Path "D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\Admin"
   # Output: False
   ```

2. **Verify All 11 Admin Controllers and Their Exact File Locations**:
   ```powershell
   Get-ChildItem -Recurse -Filter "*Admin*.cs" "D:\web_tinh_nguyen\WEB_TinhNguyen" | Select-Object FullName
   ```

3. **Verify Zero Existing `[Authorize]` Attributes**:
   ```powershell
   Get-ChildItem -Recurse -Filter "*.cs" "D:\web_tinh_nguyen\WEB_TinhNguyen" | Select-String -Pattern "\[Authorize"
   # Output: Empty (0 matches)
   ```

4. **Execute Current E2E Test Suite (Baseline Stability Check)**:
   ```powershell
   dotnet run --project "D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj" -- --all
   # Output: OVERALL: 477 Tests | 477 Passed | 0 Failed | 0 Skipped
   ```

5. **Verify Solution Build (0 Errors)**:
   ```powershell
   dotnet build "D:\web_tinh_nguyen\WEB_TinhNguyen\WEB_TinhNguyen.csproj"
   # Output: Build succeeded. 0 Error(s).
   ```
