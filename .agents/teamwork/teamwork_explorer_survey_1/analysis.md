# BÁO CÁO KHẢO SÁT VÀ THIẾT KẾ KIẾN TRÚC PHÂN HỆ QUẢN LÝ TÀI KHOẢN VÀ HỆ THỐNG PHÂN QUYỀN ADMIN (R1, R2, R3)

**Tác giả**: teamwork_explorer_survey_1  
**Mục tiêu**: Khảo sát toàn diện 11 phân hệ chức năng quản trị, cấu trúc điều hướng layout `_AdminLayout.cshtml`, cơ chế xác thực/phân quyền và thiết kế giải pháp phân quyền chi tiết (Granular Admin Permissions) đảm bảo tương thích 100% với toàn bộ 518 kiểm thử tự động hiện có.  
**Ngày thực hiện**: 2026-11-07  

---

## 1. TỔNG QUAN HỆ THỐNG VÀ HIỆN TRẠNG KIẾN TRÚC

### 1.1. Công nghệ nền tảng
- **Framework**: .NET 10.0 (`net10.0`), ASP.NET Core MVC.
- **ORM & Database**: Entity Framework Core 10.0.12 kết nối SQL Server với chuỗi kết nối `DefaultConnection`, kết hợp cơ chế bộ nhớ đệm dự phòng In-Memory (`_inMemoryAdmins`, `_inMemoryProducts`, `_inMemoryOrders`, `_inMemoryQrConfigs`) trong `DbInitializer.cs`.
- **Bảo mật mật khẩu**: `BaoMatMatKhau` sử dụng thuật toán chuẩn PBKDF2 (HMAC-SHA256, 100,000 vòng lặp, muối ngẫu nhiên 16-byte từ CSPRNG, so sánh hằng thời gian `FixedTimeEquals`).
- **Xác thực phiên**: Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`) với cookie name `.WEB_TinhNguyen.AdminAuth`, đường dẫn `LoginPath = "/admin/login"`, `LogoutPath = "/admin/logout"`, `AccessDeniedPath = "/admin/login"`.
- **Pipeline middleware trong `Program.cs`**:
  `app.UseRouting() -> app.UseSession() -> app.UseAuthentication() -> app.UseAuthorization() -> app.UseStaticFiles()` (bắt buộc duy trì nghiêm ngặt theo invariant CH19 & F02_T02).

### 1.2. Hiện trạng phân quyền hiện tại
- Tất cả 11 controller quản trị hiện tại (`AdminController` + 10 controller nghiệp vụ) đều được trang trí thuộc tính `[Authorize]` ở cấp lớp (class-level).
- Khi quản trị viên đăng nhập tại `AdminController.Login`, hệ thống cấp Claims gồm `ClaimTypes.Name`, `ClaimTypes.NameIdentifier`, `ClaimTypes.Role` (mặc định là `"Admin"`), `TenDangNhap`.
- **Hạn chế**: Chưa có cơ chế phân quyền chi tiết theo từng phân hệ chức năng. Bất kỳ tài khoản quản trị viên nào đã đăng nhập đều có thể truy cập và thao tác trên toàn bộ 10 phân hệ.
- **Yêu cầu mới (R1, R2, R3)**: Xây dựng phân hệ thứ 11 (Quản lý tài khoản quản trị) cho phép tạo tài khoản mới và gán quyền theo danh sách 11 phân hệ; thực thi kiểm soát truy cập (chặn HTTP 403 Forbidden hoặc chuyển hướng); và hiển thị động menu sidebar chỉ với các phân hệ được cấp quyền.

---

## 2. DANH MỤC 11 PHÂN HỆ QUẢN TRỊ VÀ ĐẶC TẢ CHI TIẾT ENDPOINTS

Dưới đây là bảng tổng hợp và đặc tả toàn bộ 11 phân hệ chức năng quản trị:

| # | Phân hệ chức năng | Mã quyền (Permission Code) | Controller & Vị trí tệp | Route cơ sở | Số Action POST CRUD | Hiện trạng |
|---|---|---|---|---|---|---|
| 1 | Trang chủ (Dashboard / Home) | `TrangChu` | `AdminController.cs` & `AdminTrangChuController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/Admin`<br>`/AdminTrangChu` | 5 | Đang hoạt động |
| 2 | Chương trình (Programs) | `ChuongTrinh` | `AdminChuongTrinhController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminChuongTrinh` | 1 | Đang hoạt động |
| 3 | Gây quỹ (Fundraising) | `GayQuy` | `AdminGayQuyController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminGayQuy` | 1 | Đang hoạt động |
| 4 | Cấu hình QR (QR Configuration) | `CauHinhQR` | `AdminCauHinhQRController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminCauHinhQR` | 4 | Đang hoạt động |
| 5 | Ký ức (Memories) | `KyUc` | `AdminKyUcController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminKyUc` | 1 | Đang hoạt động |
| 6 | Ban tổ chức (Organizers) | `BanToChuc` | `AdminBanToChucController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminBanToChuc` | 1 | Đang hoạt động |
| 7 | Tin tức (News) | `TinTuc` | `AdminTinTucController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminTinTuc` | 1 | Đang hoạt động |
| 8 | Hiệu ứng (Effects) | `HieuUng` | `AdminHieuUngController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminHieuUng` | 2 | Đang hoạt động |
| 9 | Quản lý sản phẩm (Products) | `SanPham` | `AdminSanPhamController.cs`<br>`WEB_TinhNguyen/Areas/CuaHang/Controllers/` | `/CuaHang/AdminSanPham` | 3 | Đang hoạt động |
| 10 | Đơn hàng (Orders) | `DonHang` | `AdminDonHangController.cs`<br>`WEB_TinhNguyen/Areas/CuaHang/Controllers/` | `/CuaHang/AdminDonHang` | 2 | Đang hoạt động |
| 11 | Quản lý tài khoản quản trị (Admin Accounts) | `TaiKhoan` | `AdminTaiKhoanController.cs`<br>`WEB_TinhNguyen/BoDieuKhien/` | `/AdminTaiKhoan` | 4 | **Triển khai mới** |

---

### Chi tiết Action và Tuyến đường của từng phân hệ:

#### Phân hệ 1: Trang chủ & Bảng điều khiển
- **Controller 1A**: `AdminController` (`WEB_TinhNguyen/BoDieuKhien/AdminController.cs`)
  - `[Authorize] [HttpGet] Index()`: Route `/Admin`, `/Admin/Index`. Trả về Dashboard quản trị tổng quan.
  - `[AllowAnonymous] [HttpGet] Login(string? returnUrl)`: Route `/admin/login`. Hiển thị form đăng nhập.
  - `[AllowAnonymous] [HttpPost] [ValidateAntiForgeryToken] Login(...)`: Route `/admin/login`. Xử lý đăng nhập.
  - `[HttpGet, HttpPost] Logout()`: Route `/admin/logout`. Đăng xuất và hủy phiên Cookie/Session.
- **Controller 1B**: `AdminTrangChuController` (`WEB_TinhNguyen/BoDieuKhien/AdminTrangChuController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminTrangChu` hoặc `/AdminTrangChu/Index`
  - `CreateChienDich(int Nam, string TieuDe)`: POST, Route `/AdminTrangChu/CreateChienDich`, `[ValidateAntiForgeryToken]`
  - `ToggleStatus(int id)`: POST, Route `/AdminTrangChu/ToggleStatus`, `[ValidateAntiForgeryToken]`
  - `DeleteChienDich(int id)`: POST, Route `/AdminTrangChu/DeleteChienDich`, `[ValidateAntiForgeryToken]`
  - `Edit(int id)`: GET, Route `/AdminTrangChu/Edit/{id}`
  - `Edit(CauHinhTrangChuVM vm)`: POST, Route `/AdminTrangChu/Edit`, `[ValidateAntiForgeryToken]`
  - `UploadHieuUngNhanh(...)`: POST, Route `/AdminTrangChu/UploadHieuUngNhanh`, `[ValidateAntiForgeryToken]`

#### Phân hệ 2: Cấu hình Chương trình
- **Controller**: `AdminChuongTrinhController` (`WEB_TinhNguyen/BoDieuKhien/AdminChuongTrinhController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminChuongTrinh`, `/AdminChuongTrinh/Index`
  - `Edit(int id)`: GET, Route `/AdminChuongTrinh/Edit/{id}`
  - `Edit(CauHinhChuongTrinhVM vm)`: POST, Route `/AdminChuongTrinh/Edit`, `[ValidateAntiForgeryToken]`

#### Phân hệ 3: Cấu hình Gây quỹ / Đóng góp
- **Controller**: `AdminGayQuyController` (`WEB_TinhNguyen/BoDieuKhien/AdminGayQuyController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminGayQuy`, `/AdminGayQuy/Index`
  - `Edit(int id)`: GET, Route `/AdminGayQuy/Edit/{id}`
  - `Edit(CauHinhGayQuyVM vm)`: POST, Route `/AdminGayQuy/Edit`, `[ValidateAntiForgeryToken]`

#### Phân hệ 4: Cấu hình Thanh toán QR
- **Controller**: `AdminCauHinhQRController` (`WEB_TinhNguyen/BoDieuKhien/AdminCauHinhQRController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminCauHinhQR`, `/AdminCauHinhQR/Index`
  - `Create()`: GET, Route `/AdminCauHinhQR/Create`
  - `Create(CauHinhThanhToanQR model, IFormFile? FileAnhQR)`: POST, Route `/AdminCauHinhQR/Create`, `[ValidateAntiForgeryToken]`
  - `Edit(int id)`: GET, Route `/AdminCauHinhQR/Edit/{id}`
  - `Edit(int id, CauHinhThanhToanQR model, IFormFile? FileAnhQR)`: POST, Route `/AdminCauHinhQR/Edit/{id}`, `[ValidateAntiForgeryToken]`
  - `ToggleActive(int id)`: POST, Route `/AdminCauHinhQR/ToggleActive/{id}`, `[ValidateAntiForgeryToken]`
  - `Delete(int id)`: POST, Route `/AdminCauHinhQR/Delete/{id}`, `[ValidateAntiForgeryToken]`

#### Phân hệ 5: Cấu hình Ký ức
- **Controller**: `AdminKyUcController` (`WEB_TinhNguyen/BoDieuKhien/AdminKyUcController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminKyUc`, `/AdminKyUc/Index`
  - `Edit(int id)`: GET, Route `/AdminKyUc/Edit/{id}`
  - `Edit(CauHinhKyUcVM vm)`: POST, Route `/AdminKyUc/Edit`, `[ValidateAntiForgeryToken]`

#### Phân hệ 6: Cấu hình Ban Tổ Chức
- **Controller**: `AdminBanToChucController` (`WEB_TinhNguyen/BoDieuKhien/AdminBanToChucController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminBanToChuc`, `/AdminBanToChuc/Index`
  - `Edit(int id)`: GET, Route `/AdminBanToChuc/Edit/{id}`
  - `Edit(CauHinhBanToChucVM vm)`: POST, Route `/AdminBanToChuc/Edit`, `[ValidateAntiForgeryToken]`

#### Phân hệ 7: Cấu hình Tin tức
- **Controller**: `AdminTinTucController` (`WEB_TinhNguyen/BoDieuKhien/AdminTinTucController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminTinTuc`, `/AdminTinTuc/Index`
  - `Edit(int id)`: GET, Route `/AdminTinTuc/Edit/{id}`
  - `Edit(CauHinhTinTucVM vm)`: POST, Route `/AdminTinTuc/Edit`, `[ValidateAntiForgeryToken]`

#### Phân hệ 8: Quản lý Hiệu ứng
- **Controller**: `AdminHieuUngController` (`WEB_TinhNguyen/BoDieuKhien/AdminHieuUngController.cs`)
  - Class attribute: `[Authorize]`
  - `Index()`: GET, Route `/AdminHieuUng`, `/AdminHieuUng/Index`
  - `Save(int IDHieuUng, string TenHieuUng, string MoTa, IFormFile FileJS)`: POST, Route `/AdminHieuUng/Save`, `[ValidateAntiForgeryToken]`
  - `Delete(int id)`: POST, Route `/AdminHieuUng/Delete/{id}`, `[ValidateAntiForgeryToken]`

#### Phân hệ 9: Quản lý Sản phẩm Gây quỹ
- **Controller**: `AdminSanPhamController` (`WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminSanPhamController.cs`)
  - Class attributes: `[Area("CuaHang")]`, `[Authorize]`
  - `Index()`: GET, Route `/CuaHang/AdminSanPham`, `/CuaHang/AdminSanPham/Index`
  - `Create()`: GET, Route `/CuaHang/AdminSanPham/Create`
  - `Create(SanPhamCrudViewModel model)`: POST, Route `/CuaHang/AdminSanPham/Create`, `[ValidateAntiForgeryToken]`
  - `Edit(int id)`: GET, Route `/CuaHang/AdminSanPham/Edit/{id}`
  - `Edit(int id, SanPhamCrudViewModel model)`: POST, Route `/CuaHang/AdminSanPham/Edit/{id}`, `[ValidateAntiForgeryToken]`
  - `Delete(int id)`: POST, Route `/CuaHang/AdminSanPham/Delete/{id}`, `[ValidateAntiForgeryToken]`

#### Phân hệ 10: Quản lý Đơn hàng Gây quỹ
- **Controller**: `AdminDonHangController` (`WEB_TinhNguyen/Areas/CuaHang/Controllers/AdminDonHangController.cs`)
  - Class attributes: `[Area("CuaHang")]`, `[Authorize]`
  - `Index(string? q, string? trangThai, string? thanhToan, string? sort)`: GET, Route `/CuaHang/AdminDonHang`, `/CuaHang/AdminDonHang/Index`
  - `ChiTiet(int id)`: GET, Route `/CuaHang/AdminDonHang/ChiTiet/{id}`
  - `CapNhatTrangThai(int id, string trangThaiDonHang, string? trangThaiThanhToan)`: POST, Route `/CuaHang/AdminDonHang/CapNhatTrangThai`, `[ValidateAntiForgeryToken]`
  - `XoaDonHang(int id)`: POST, Route `/CuaHang/AdminDonHang/XoaDonHang`, `[ValidateAntiForgeryToken]`

> **Lưu ý kiểm thử nghiêm ngặt**: Tổng số 21 Action POST CRUD trên 10 controller nghiệp vụ hiện tại được kiểm tra trực tiếp trong `Tier5_ChallengerM2_2_RouteProtectionHarness` (Test `M2_2_03`) và `Tier5_ChallengerM2_3_AuthPipelineHarness` (Test `M2_3_02`). Cả hai harness đều xác nhận đúng số lượng 21 action và kiểm tra `[Authorize]` + `[ValidateAntiForgeryToken]`. Việc bảo toàn 21 action này là bắt buộc.

---

## 3. THIẾT KẾ PHÂN HỆ THỨ 11: QUẢN LÝ TÀI KHOẢN QUẢN TRỊ (R1)

### 3.1. Vị trí và Tên Controller
- **Tên lớp**: `AdminTaiKhoanController : Controller`
- **Tệp nguồn**: `WEB_TinhNguyen/BoDieuKhien/AdminTaiKhoanController.cs`
- **Namespace**: `WEB_TinhNguyen.BoDieuKhien`
- **Trang trí lớp**: `[Authorize]`, `[PhanQuyenAdmin("TaiKhoan")]`
- **Hỗ trợ Dependency Injection**:
  `public AdminTaiKhoanController(ApplicationDbContext? context = null)` với tham số tùy chọn null để tương thích với các unit test độc lập khởi tạo qua `new AdminTaiKhoanController()`.

### 3.2. Danh sách Action nghiệp vụ của `AdminTaiKhoanController`
1. `[HttpGet] public async Task<IActionResult> Index()`:
   - Route: `/AdminTaiKhoan` hoặc `/AdminTaiKhoan/Index`.
   - Hiển thị danh sách tài khoản quản trị (`List<TaiKhoanAdmin>`).
   - Cung cấp dữ liệu: Tên đăng nhập, Họ tên, Email, Trạng thái (Hoạt động / Tạm khóa), Ngày tạo, Đăng nhập cuối, và nhãn hiển thị các quyền đã cấp.
2. `[HttpGet] public IActionResult Create()`:
   - Route: `/AdminTaiKhoan/Create`.
   - Hiển thị form tạo mới tài khoản quản trị, gồm danh sách 11 checkbox phân quyền và nút chọn nhanh "Tất cả quyền (Toàn quyền)".
3. `[HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> Create(AdminTaiKhoanCreateViewModel model)`:
   - Route: `/AdminTaiKhoan/Create`.
   - Xác thực dữ liệu đầu vào:
     - Tên đăng nhập không được trùng lặp.
     - Mật khẩu tối thiểu 6 ký tự, có xác nhận mật khẩu khớp.
     - Tạo muối ngẫu nhiên bằng `BaoMatMatKhau.TaoMuoiNgauNhien()`.
     - Băm mật khẩu bằng `BaoMatMatKhau.BamMatKhau(model.MatKhau, salt)`.
     - Lưu danh sách quyền hạn dạng chuỗi phân tách dấu phẩy (hoặc `"ALL"` nếu chọn toàn quyền).
     - Lưu vào CSDL vật lý (nếu có kết nối) và đồng bộ vào `DbInitializer.AddInMemoryAdmin()`.
4. `[HttpGet] public async Task<IActionResult> Edit(int id)`:
   - Route: `/AdminTaiKhoan/Edit/{id}`.
   - Hiển thị form cập nhật thông tin tài khoản, danh sách phân quyền, và cho phép đổi mật khẩu mới (nếu nhập).
5. `[HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id, AdminTaiKhoanEditViewModel model)`:
   - Route: `/AdminTaiKhoan/Edit/{id}`.
   - Cập nhật thông tin Họ tên, Email, Trạng thái, Phân quyền. Nếu có nhập mật khẩu mới thì băm và tạo muối mới.
   - Không cho phép khóa trạng thái hoặc tước quyền của tài khoản `admin` mặc định.
6. `[HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> ToggleStatus(int id)`:
   - Route: `/AdminTaiKhoan/ToggleStatus/{id}`.
   - Đảo trạng thái kích hoạt `TrangThaiHoatDong` (bảo vệ: không được khóa tài khoản `admin` gốc).
7. `[HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> Delete(int id)`:
   - Route: `/AdminTaiKhoan/Delete/{id}`.
   - Xóa tài khoản quản trị (bảo vệ: không cho phép xóa tài khoản `admin` mặc định hoặc tài khoản đang đăng nhập).

### 3.3. Cập nhật Model và ViewModel
Trong `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`:
- Bổ sung trường `PhanQuyen` vào thực thể `TaiKhoanAdmin`:
  ```csharp
  [StringLength(1000)]
  [Display(Name = "Danh sách phân quyền")]
  public string? PhanQuyen { get; set; } = "ALL";
  ```
- Định nghĩa ViewModel tạo mới `AdminTaiKhoanCreateViewModel`:
  ```csharp
  public class AdminTaiKhoanCreateViewModel
  {
      [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
      [StringLength(100)]
      public string TenDangNhap { get; set; } = string.Empty;

      [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
      [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
      [DataType(DataType.Password)]
      public string MatKhau { get; set; } = string.Empty;

      [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
      [Compare("MatKhau", ErrorMessage = "Xác nhận mật khẩu không khớp")]
      [DataType(DataType.Password)]
      public string XacNhanMatKhau { get; set; } = string.Empty;

      public string? HoTen { get; set; }
      public string? Email { get; set; }
      public string VaiTro { get; set; } = "Admin";
      public bool TrangThaiHoatDong { get; set; } = true;
      public List<string> SelectedPermissions { get; set; } = new List<string>();
  }
  ```
- Định nghĩa ViewModel chỉnh sửa `AdminTaiKhoanEditViewModel`:
  ```csharp
  public class AdminTaiKhoanEditViewModel
  {
      public int IDTaiKhoan { get; set; }
      public string TenDangNhap { get; set; } = string.Empty;
      public string? MatKhauMoi { get; set; }
      public string? XacNhanMatKhauMoi { get; set; }
      public string? HoTen { get; set; }
      public string? Email { get; set; }
      public string VaiTro { get; set; } = "Admin";
      public bool TrangThaiHoatDong { get; set; } = true;
      public List<string> SelectedPermissions { get; set; } = new List<string>();
  }
  ```

### 3.4. Cập nhật Lưu trữ bền vững Dual-Mode (SQL Server & In-Memory Store)
- **Trong SQL Server (`DbInitializer.cs`)**:
  Bổ sung câu lệnh DDL tự động di trú:
  ```sql
  IF COL_LENGTH(N'[dbo].[TaiKhoanAdmin]', N'PhanQuyen') IS NULL
  BEGIN
      ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [PhanQuyen] NVARCHAR(1000) NULL;
  END;
  ```
- **Trong In-Memory Store (`DbInitializer.cs`)**:
  - `CloneAdmin`: Sao chép `PhanQuyen = a.PhanQuyen`.
  - `GetDefaultAdmins`: Thiết lập tài khoản `admin` mặc định có `PhanQuyen = "ALL"`.
  - `UpdateInMemoryAdmin`: Cập nhật trường `PhanQuyen`.

---

## 4. KIẾN TRÚC THỰC THI KIỂM SOÁT PHÂN QUYỀN (R2)

### 4.1. Cơ chế Claim khi Đăng nhập (`AdminController.Login`)
Khi xác thực thành công thông tin đăng nhập trong `AdminController.Login`:
```csharp
// Đọc thông tin phân quyền của tài khoản
var phanQuyen = admin?.PhanQuyen;
if (string.IsNullOrWhiteSpace(phanQuyen) && (roleName == "Admin" || roleName == "SuperAdmin"))
{
    phanQuyen = "ALL";
}

claims.Add(new Claim("PhanQuyen", phanQuyen ?? "ALL"));

if (phanQuyen == "ALL")
{
    claims.Add(new Claim("Permission", "ALL"));
}
else
{
    var listQuyen = phanQuyen.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    foreach (var q in listQuyen)
    {
        claims.Add(new Claim("Permission", q));
    }
}
```

### 4.2. Bộ lọc kiểm soát phân quyền: `[PhanQuyenAdminAttribute]`
- **Vị trí**: `WEB_TinhNguyen/DuLieu/PhanQuyenAdminAttribute.cs`
- **Kế thừa**: `Attribute, IAsyncActionFilter`
- **Nguyên lý kiểm tra**:
  1. Nếu người dùng chưa xác thực (`!User.Identity.IsAuthenticated`): Để bộ lọc `[Authorize]` xử lý chuyển hướng 302 về `/admin/login`.
  2. Nếu người dùng có quyền toàn phần (`User.IsInRole("Admin")`, hoặc có Claim `Permission == "ALL"`, hoặc `PhanQuyen == "ALL"`): Cho phép thực thi yêu cầu (`await next()`).
  3. Nếu người dùng có Claim `Permission == _requiredPermission`: Cho phép thực thi (`await next()`).
  4. Nếu KHÔNG có quyền:
     - Với yêu cầu AJAX / JSON: Trả về `JsonResult` hoặc `ObjectResult` với mã HTTP `403 Forbidden`.
     - Với yêu cầu Web thông thường: Trả về `ViewResult` hiển thị trang thông báo từ chối truy cập `AccessDenied.cshtml` với HTTP status `403 Forbidden` (hoặc chuyển hướng kèm TempData cảnh báo theo R2).

### 4.3. Extension Method hỗ trợ kiểm tra quyền: `PhanQuyenHelper`
```csharp
public static class PhanQuyenHelper
{
    public static bool CoQuyen(this ClaimsPrincipal user, string permissionCode)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        if (user.IsInRole("Admin") || user.HasClaim("Permission", "ALL") || user.HasClaim("PhanQuyen", "ALL"))
            return true;
        return user.HasClaim("Permission", permissionCode);
    }
}
```

---

## 5. ĐIỀU HƯỚNG GIAO DIỆN VÀ MENU SIDEBAR THEO PHÂN QUYỀN (R3)

### 5.1. Khảo sát hiện trạng `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`
Menu thanh bên (Sidebar) hiện tại được tổ chức thành 3 khối nhóm:

1. **Khối Bảng điều khiển**:
   - Icon: `fas fa-tachometer-alt`
   - Nhãn: `Bảng điều khiển`
   - Liên kết: `@Url.Action("Index", "Admin", new { area = "" })`
   - Active: `ViewContext.RouteData.Values["Controller"]?.ToString() == "Admin"`

2. **Khối Nội dung** (`<h4><span>Nội dung</span>...</h4>`):
   - Cấu hình Trang chủ: `fas fa-home`, `@Url.Action("Index", "AdminTrangChu", new { area = "" })`
   - Cấu hình Chương trình: `fas fa-calendar-alt`, `@Url.Action("Index", "AdminChuongTrinh", new { area = "" })`
   - Cấu hình Đóng Góp: `fas fa-hand-holding-heart`, `@Url.Action("Index", "AdminGayQuy", new { area = "" })`
   - Sản phẩm Gây Quỹ: `fas fa-boxes`, `/CuaHang/AdminSanPham`
   - Đơn hàng Gây Quỹ: `fas fa-receipt`, `/CuaHang/AdminDonHang`
   - Cấu hình Thanh toán QR: `fas fa-qrcode`, `@Url.Action("Index", "AdminCauHinhQR", new { area = "" })`
   - Cấu hình Ký ức: `fas fa-images`, `@Url.Action("Index", "AdminKyUc", new { area = "" })`
   - Cấu hình Ban Tổ Chức: `fas fa-sitemap`, `@Url.Action("Index", "AdminBanToChuc", new { area = "" })`
   - Cấu hình Tin tức: `fas fa-newspaper`, `@Url.Action("Index", "AdminTinTuc", new { area = "" })`

3. **Khối Tương tác** (`<h4><span>Tương tác</span>...</h4>`):
   - Đăng ký TNV: `fas fa-users`, `#`

4. **Khối Hệ thống** (`<h4><span>Hệ thống</span>...</h4>`):
   - Xem trang ngoài: `fas fa-external-link-alt`, `@Url.Action("Index", "Home", new { area = "" })`

### 5.2. Các mục menu cần bổ sung để đủ 11 phân hệ
1. **Cấu hình Hiệu ứng**:
   - Vị trí: Đặt trong khối **Nội dung** sau Cấu hình Tin tức.
   - Liên kết: `@Url.Action("Index", "AdminHieuUng", new { area = "" })`
   - Icon: `fas fa-magic`
   - Nhãn: `Cấu hình Hiệu ứng`
   - Active: `ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminHieuUng"`
2. **Quản lý Tài khoản Quản trị**:
   - Vị trí: Đặt trong khối **Hệ thống** trước "Xem trang ngoài".
   - Liên kết: `@Url.Action("Index", "AdminTaiKhoan", new { area = "" })`
   - Icon: `fas fa-user-shield`
   - Nhãn: `Quản lý Tài khoản`
   - Active: `ViewContext.RouteData.Values["Controller"]?.ToString() == "AdminTaiKhoan"`

### 5.3. Ẩn/Hiện động menu sidebar theo phân quyền (R3)
Bọc từng thẻ `<li>` điều hướng trong khối điều kiện Razor sử dụng `@if (User.CoQuyen("..."))`:
- Bảng điều khiển: `@if (User.CoQuyen("TrangChu")) { ... }`
- Cấu hình Trang chủ: `@if (User.CoQuyen("TrangChu")) { ... }`
- Cấu hình Chương trình: `@if (User.CoQuyen("ChuongTrinh")) { ... }`
- Cấu hình Đóng Góp: `@if (User.CoQuyen("GayQuy")) { ... }`
- Sản phẩm Gây Quỹ: `@if (User.CoQuyen("SanPham")) { ... }`
- Đơn hàng Gây Quỹ: `@if (User.CoQuyen("DonHang")) { ... }`
- Cấu hình Thanh toán QR: `@if (User.CoQuyen("CauHinhQR")) { ... }`
- Cấu hình Ký ức: `@if (User.CoQuyen("KyUc")) { ... }`
- Cấu hình Ban Tổ Chức: `@if (User.CoQuyen("BanToChuc")) { ... }`
- Cấu hình Tin tức: `@if (User.CoQuyen("TinTuc")) { ... }`
- Cấu hình Hiệu ứng: `@if (User.CoQuyen("HieuUng")) { ... }`
- Quản lý Tài khoản: `@if (User.CoQuyen("TaiKhoan")) { ... }`

**Xác minh tính tương thích ngược với các bài kiểm thử Layout hiện tại**:
- `Tier5_Challenger2_AdminAdversarialHarness.CH2_04`: Kiểm tra chuỗi `"Index", "{ctrl}", new { area = "" }` có mặt trong tệp `_AdminLayout.cshtml` bằng `File.ReadAllText`. Khi bọc trong `@if`, các chuỗi này vẫn tồn tại nguyên vẹn trong tệp nguồn -> **PASS**.
- `Tier5_Challenger2_AdminAdversarialHarness.CH2_05`: Kiểm tra tất cả cuộc gọi `@Url.Action` có chứa `area = ""`. Cả hai liên kết mới (`AdminHieuUng` và `AdminTaiKhoan`) đều tuân thủ `new { area = "" }` -> **PASS**.
- `Tier5_Challenger2_AdminAdversarialHarness.CH2_06`: Kiểm tra 10 biểu thức `ViewContext.RouteData.Values["Controller"]?.ToString() == "{ctrl}"` có mặt nguyên văn trong tệp -> **PASS**.
- `Tier1_AdminOrderTests.F18_T02 & F18_T03`: Kiểm tra liên kết `AdminDonHang`, icon `fa-receipt` và văn bản `Đơn hàng Gây Quỹ` -> **PASS**.
- `Tier5_ChallengerM2Harness.M2_C41`: Kiểm tra liên kết `AdminSanPham`, icon `fa-boxes` và văn bản `Sản phẩm` -> **PASS**.

---

## 6. DANH MỤC CÁC RÀNG BUỘC KIẾN TRÚC VÀ TÍNH TƯƠNG THÍCH (CONSTRAINTS & INVARIANTS)

1. **Khóa bất biến thư mục View Cửa hàng (`Areas/CuaHang/Views/`)**:
   - `Tier5_ChallengerM2_2_RouteProtectionHarness.M2_2_06_CuaHangViewsPreserved` giám sát tính nguyên vẹn của 12 tệp view trong `Areas/CuaHang/Views/`.
   - **Ràng buộc tuyệt đối**: KHÔNG ĐƯỢC CHỈNH SỬA bất kỳ tệp nào trong `Areas/CuaHang/Views/`. Mọi phân quyền cho `AdminSanPhamController` và `AdminDonHangController` chỉ được áp dụng ở tầng Controller.

2. **Bảo toàn 21 Action POST CRUD và 11 Controller hiện tại**:
   - `Tier5_ChallengerM2_2_RouteProtectionHarness` (Test `M2_2_01` & `M2_2_03`) và `Tier5_ChallengerM2_3_AuthPipelineHarness` (Test `M2_3_01` & `M2_3_02`) kiểm tra phản chiếu 11 controller hiện có và 21 action POST CRUD.
   - Các controller này phải tiếp tục giữ nguyên thuộc tính `[Authorize]` ở cấp lớp và `[ValidateAntiForgeryToken]` trên các action POST.

3. **Thứ tự Middleware Pipeline trong `Program.cs`**:
   - `app.UseRouting() -> app.UseSession() -> app.UseAuthentication() -> app.UseAuthorization() -> app.UseStaticFiles()`
   - Tuyệt đối không thay đổi thứ tự hay hoán đổi vị trí của các middleware này.

4. **Khởi tạo độc lập `new AdminController()`**:
   - `AdminController` phải duy trì constructor không tham số hoặc tham số tùy chọn `ApplicationDbContext? context = null` để phục vụ các bài kiểm thử F19_T08 & F19_T16.

5. **Quy ước thư mục tiếng Việt cho phân hệ gốc**:
   - Bộ điều khiển đặt trong `BoDieuKhien/`, mô hình đặt trong `MoHinh/`, tầng dữ liệu đặt trong `DuLieu/`, giao diện đặt trong `GiaoDien/`.
   - Tuyệt đối không tạo thư mục `Controllers/`, `Models/`, `Views/` ở thư mục gốc vì sẽ xung đột với cấu hình Razor ViewLocationFormats.

---

## 7. KẾT LUẬN VÀ LỘ TRÌNH TRIỂN KHAI

Kiến trúc phân quyền chi tiết đã được khảo sát kỹ lưỡng và thiết kế theo mô hình tối ưu:
- **R1**: Mở rộng thực thể `TaiKhoanAdmin` với trường `PhanQuyen`, tạo controller `AdminTaiKhoanController` và giao diện quản trị tài khoản tại `GiaoDien/AdminTaiKhoan/`. Mật khẩu được băm an toàn bằng PBKDF2 HMAC-SHA256.
- **R2**: Sử dụng bộ lọc phân quyền `[PhanQuyenAdmin]` kết hợp Claims-based authorization, chặn truy cập và thao tác trái phép (trả về 403 Forbidden).
- **R3**: Thanh menu sidebar tự động lọc động theo phương thức mở rộng `User.CoQuyen(...)`, bảo toàn 100% các điều kiện kiểm thử tĩnh của tệp `_AdminLayout.cshtml`.
- **R4**: Sẵn sàng tích hợp kịch bản kiểm thử tự động xác minh phân quyền (tạo tài khoản, gán quyền, xác thực truy cập thành công 200 OK với phân hệ được cấp quyền và chặn 403 với phân hệ không được cấp quyền).
