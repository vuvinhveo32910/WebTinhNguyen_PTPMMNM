# BÁO CÁO NGHIÊN CỨU & THIẾT KẾ KIẾN TRÚC PHÂN QUYỀN QUẢN TRỊ (R1, R2)
**Người thực hiện**: `teamwork_explorer_survey_2`  
**Ngày thực hiện**: 2026-11-07  
**Phạm vi**: Khảo sát hiện trạng và thiết kế chi tiết Mô hình Dữ liệu (Data Model), Lược đồ Phân quyền (Permissions Schema) và Đường ống Ủy quyền (Authorization Pipeline) cho Hệ thống Quản trị Web Tình Nguyện.

---

## 1. KHẢO SÁT HIỆN TRẠNG HỆ THỐNG XÁC THỰC & DỮ LIỆU HIỆN TẠI

### 1.1. Thực thể `TaiKhoanAdmin` (`WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`)
- **Tình trạng hiện tại**:
  - Lớp `TaiKhoanAdmin` ánh xạ vào bảng SQL Server `[dbo].[TaiKhoanAdmin]` qua thuộc tính `[Table("TaiKhoanAdmin")]` và cấu hình Fluent API trong `ApplicationDbContext.cs`.
  - Có chỉ mục duy nhất: `[Index(nameof(TenDangNhap), IsUnique = true)]`.
  - Các thuộc tính hiện có:
    - `IDTaiKhoan` (`int`, khóa chính identity)
    - `TenDangNhap` (`string`, max 100, required)
    - `MatKhauHash` (`string`, max 500, required - chuỗi Base64 44 ký tự PBKDF2 HMAC-SHA256)
    - `MuoiBaoMat` (`string`, max 100, required - chuỗi Base64 24 ký tự salt 16 bytes)
    - `HoTen` (`string?`, max 150)
    - `Email` (`string?`, max 150)
    - `VaiTro` (`string`, max 50, mặc định `"Admin"`)
    - `TrangThaiHoatDong` (`bool`, mặc định `true`)
    - `NgayTao` (`DateTime`, mặc định `DateTime.Now`)
    - `LanDangNhapCuoi` (`DateTime?`, ghi nhận thời điểm đăng nhập)
    - Phương thức `Clone()`: Hỗ trợ cô lập dữ liệu cho bộ nhớ dự phòng In-Memory store (`DbInitializer`).
- **Khoảng cách cần nâng cấp**:
  - Hiện **chưa có cột/thuộc tính lưu danh sách phân quyền** (`QuyenHan` hoặc `PhanQuyen`).
  - Cần bổ sung cột `QuyenHan` (`string?`, nullable, lưu chuỗi danh sách mã phân quyền dạng phân tách dấu phẩy hoặc JSON).
  - Cần cập nhật hàm `Clone()` để sao chép trường `QuyenHan`, tránh mất dữ liệu khi clone trong in-memory store.

### 1.2. Ánh xạ CSDL `ApplicationDbContext` (`WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`)
- **Tình trạng hiện tại**:
  - Đã khai báo DbSet: `public DbSet<TaiKhoanAdmin> TaiKhoanAdmins { get; set; }` và alias `public DbSet<TaiKhoanAdmin> TaiKhoanAdmin { get => ... }` (dòng 56-57).
  - Cấu hình Fluent API (dòng 227-240):
    ```csharp
    modelBuilder.Entity<TaiKhoanAdmin>(entity =>
    {
        entity.ToTable("TaiKhoanAdmin");
        entity.HasKey(e => e.IDTaiKhoan);
        entity.Property(e => e.IDTaiKhoan).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.TenDangNhap).IsUnique();
        entity.Property(e => e.TenDangNhap).IsRequired().HasMaxLength(100);
        entity.Property(e => e.MatKhauHash).IsRequired().HasMaxLength(500);
        entity.Property(e => e.MuoiBaoMat).IsRequired().HasMaxLength(100);
        entity.Property(e => e.HoTen).HasMaxLength(150);
        entity.Property(e => e.Email).HasMaxLength(150);
        entity.Property(e => e.VaiTro).IsRequired().HasMaxLength(50).HasDefaultValue("Admin");
        entity.Property(e => e.TrangThaiHoatDong).HasDefaultValue(true);
    });
    ```
- **Kế hoạch mở rộng**:
  - Bổ sung cấu hình thuộc tính `QuyenHan`:
    ```csharp
    entity.Property(e => e.QuyenHan).HasMaxLength(4000).IsRequired(false);
    ```
  - Lưu ý an toàn: Trong `OnConfiguring` (dòng 62), DbContext đã bỏ qua cảnh báo `PendingModelChangesWarning` nên việc thêm property vào Model không gây xung đột runtime.

### 1.3. Khởi tạo & Lưu trữ Kép `DbInitializer` (`WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`)
- **Tình trạng hiện tại**:
  - `DbInitializer` cung cấp cơ chế Dual-Mode Persistence:
    - Khi có SQL Server trực tuyến: Thực thi DDL tự động bằng `ExecuteSqlRaw` và seed vào database vật lý.
    - Khi SQL Server ngoại tuyến / môi trường sandbox kiểm thử: Sử dụng `List<TaiKhoanAdmin> _inMemoryAdmins` đồng bộ qua `_syncLock` (dòng 21).
  - Seed admin mặc định trong `GetDefaultAdmins()` (dòng 59-79):
    - Tài khoản: `"admin"` / `"Admin@123"`.
    - Mật khẩu băm an toàn qua `GetOrCreateDefaultAdminCredentials()`.
    - `VaiTro = "Admin"`.
  - Quản trị viên In-Memory CRUD:
    - `GetInMemoryAdmins()` (dòng 1175)
    - `GetInMemoryAdminById(int id)` (dòng 1187)
    - `AddInMemoryAdmin(TaiKhoanAdmin item)` (dòng 1199)
    - `UpdateInMemoryAdmin(TaiKhoanAdmin item)` (dòng 1216)
    - `DeleteInMemoryAdmin(int id)` (dòng 1240)
    - `CloneAdmin(TaiKhoanAdmin a)` (dòng 84-99)
    - `ValidateAdminCredentials(string username, string password, ApplicationDbContext? context)` (dòng 1114)
- **Kế hoạch mở rộng**:
  1. Trong câu lệnh SQL DDL `CREATE TABLE [dbo].[TaiKhoanAdmin]` (dòng 415-427 và dòng 1010-1025): Bổ sung cột `[QuyenHan] NVARCHAR(MAX) NULL`.
  2. Bổ sung script chuyển dịch phi phá hủy (Idempotent ALTER TABLE):
     ```sql
     IF OBJECT_ID(N'[dbo].[TaiKhoanAdmin]', N'U') IS NOT NULL
     BEGIN
         IF COL_LENGTH(N'[dbo].[TaiKhoanAdmin]', N'QuyenHan') IS NULL
         BEGIN
             ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [QuyenHan] NVARCHAR(MAX) NULL;
         END;
     END;
     ```
  3. Trong `GetDefaultAdmins()`: Gán `QuyenHan = "All"` (hoặc `"SuperAdmin"`) cho tài khoản `admin` mặc định.
  4. Nếu tài khoản `admin` đã tồn tại trong CSDL nhưng `QuyenHan` đang null/rỗng: Tự động cập nhật `QuyenHan = "All"` để đảm bảo tài khoản hệ thống không bao giờ bị mất quyền.
  5. Cập nhật `CloneAdmin`, `AddInMemoryAdmin`, `UpdateInMemoryAdmin` để sao chép trường `QuyenHan`.
  6. Bổ sung các phương thức facade cho phân hệ quản lý tài khoản:
     - `GetAdmins(ApplicationDbContext? context)`
     - `GetAdminById(int id, ApplicationDbContext? context)`
     - `AddAdmin(TaiKhoanAdmin item, ApplicationDbContext? context)`
     - `UpdateAdmin(TaiKhoanAdmin item, ApplicationDbContext? context)`
     - `DeleteAdmin(int id, ApplicationDbContext? context)`

### 1.4. Tiện ích Mật mã `BaoMatMatKhau` (`WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs`)
- **Tình trạng hiện tại**:
  - Chuẩn mật mã: PBKDF2 HMAC-SHA256, 100,000 iterations, 16-byte random salt bằng `RandomNumberGenerator`, output 32-byte subkey.
  - So sánh bất biến thời gian: `CryptographicOperations.FixedTimeEquals` chống hoàn toàn Timing Attack.
  - Hỗ trợ đầy đủ các hàm:
    - `TaoMuoiNgauNhien()`: Sinh chuỗi Base64 salt.
    - `BamMatKhau(matKhau, muoiBaoMat)`: Băm mật khẩu.
    - `KiemTraMatKhau(matKhauNhap, matKhauHashDaLuu, muoiBaoMat)`: Xác thực mật khẩu.
    - Contract alias: `HashPassword(password, out salt)` và `VerifyPassword(password, hash, salt)`.
  - Lớp chuyển tiếp `WEB_TinhNguyen.Areas.CuaHang.Helpers.BaoMatMatKhau`: Đã tồn tại để tương thích gọi chéo namespace.
- **Đánh giá**:
  - Tiện ích hoạt động hoàn hảo, đáp ứng 100% tiêu chí chấp nhận R1 ("Mật khẩu được mã hóa/băm bằng thuật toán an toàn trước khi lưu vào CSDL, không lưu plain-text"). Khi tạo tài khoản quản trị mới, chỉ cần gọi `BaoMatMatKhau.HashPassword(model.MatKhau, out salt)`.

### 1.5. Xử lý Đăng nhập & Tạo Claims `AdminController` (`WEB_TinhNguyen/BoDieuKhien/AdminController.cs`)
- **Tình trạng hiện tại**:
  - Hành động `Login` (POST) (dòng 83-142):
    - Xác thực qua `DbInitializer.ValidateAdminCredentials(model.TenDangNhap, model.MatKhau, _context)`.
    - Tạo danh sách `Claim`:
      ```csharp
      var claims = new List<Claim>
      {
          new Claim(ClaimTypes.Name, displayName),
          new Claim(ClaimTypes.NameIdentifier, identifier),
          new Claim(ClaimTypes.Role, roleName),
          new Claim("TenDangNhap", model.TenDangNhap)
      };
      ```
    - Ký cookie qua `HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties)`.
- **Khoảng cách cần nâng cấp**:
  - Khi đăng nhập thành công, cần nạp danh sách quyền từ `admin.QuyenHan` vào `ClaimsPrincipal`.
  - Mỗi phân hệ được cấp quyền sẽ được gán thành một claim riêng: `new Claim("QuyenHan", moduleName)`.
  - Nếu tài khoản có quyền toàn bộ (`"All"` hoặc `"SuperAdmin"` hoặc `admin`), nạp claim `new Claim("QuyenHan", "All")` và nạp đầy đủ 11 claims của tất cả các phân hệ.

### 1.6. Cấu hình Đường ống Middleware `Program.cs` (`WEB_TinhNguyen/Program.cs`)
- **Tình trạng hiện tại**:
  - Thứ tự Middleware bất biến (đã được khóa chặt bởi bài test `M2_3_05_ProgramCsPipelineOrderingInvariants`):
    `UseRouting()` -> `UseSession()` -> `UseAuthentication()` -> `UseAuthorization()` -> `UseStaticFiles()`.
  - Cấu hình Cookie:
    - Scheme: `CookieAuthenticationDefaults.AuthenticationScheme`
    - Cookie.Name: `.WEB_TinhNguyen.AdminAuth`
    - LoginPath: `/admin/login`
    - LogoutPath: `/admin/logout`
    - AccessDeniedPath: `/admin/login` (được assert chặt chẽ bởi bài test `M2_3_04_CookieAuthenticationOptionsRegistration`).
- **Phân tích R2**:
  - Vì bài test `M2_3_04` kiểm tra nghiêm ngặt `AccessDeniedPath == "/admin/login"`, ta **KHÔNG ĐƯỢC PHÉP** thay đổi giá trị này trong `Program.cs`.
  - Để thỏa mãn R2 ("trả về mã trạng thái từ chối truy cập HTTP 403 Forbidden hoặc chuyển hướng kèm thông báo không có quyền"):
    - Bộ lọc ủy quyền tùy chỉnh (`PhanQuyenAdminAttribute`) sẽ trực tiếp thiết lập `context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;` và `context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);` khi phát hiện truy cập trái phép.
    - Điều này đảm bảo trả về chính xác mã HTTP 403 Forbidden mà không làm ảnh hưởng đến cấu hình cookie options trong `Program.cs`.

---

## 2. THIẾT KẾ KIẾN TRÚC PHÂN QUYỀN (PERMISSIONS SCHEMA)

### 2.1. Danh mục 11 Phân hệ Quản trị & Mã Quyền Hạn Chuẩn hóa
Theo đúng yêu cầu tại ORIGINAL_REQUEST §R1 và TASK 2, hệ thống gồm đúng 11 phân hệ chức năng:

| STT | Tên Phân hệ (Module) | Hằng số Mã Quyền (`QuyenAdmin`) | Tên Hiển Thị Giao Diện | Controller Phụ Trách | Tuyến Đường (Route) |
|:---:|:---|:---|:---|:---|:---|
| 1 | Trang chủ | `TrangChu` | Cấu hình Trang chủ | `AdminTrangChuController` | `/AdminTrangChu` |
| 2 | Chương trình | `ChuongTrinh` | Cấu hình Chương trình | `AdminChuongTrinhController` | `/AdminChuongTrinh` |
| 3 | Gây quỹ | `GayQuy` | Cấu hình Đóng góp / Gây quỹ | `AdminGayQuyController` | `/AdminGayQuy` |
| 4 | Cấu hình QR | `CauHinhQR` | Cấu hình Thanh toán QR | `AdminCauHinhQRController` | `/AdminCauHinhQR` |
| 5 | Ký ức | `KyUc` | Cấu hình Ký ức | `AdminKyUcController` | `/AdminKyUc` |
| 6 | Ban tổ chức | `BanToChuc` | Cấu hình Ban Tổ Chức | `AdminBanToChucController` | `/AdminBanToChuc` |
| 7 | Tin tức | `TinTuc` | Cấu hình Tin tức | `AdminTinTucController` | `/AdminTinTuc` |
| 8 | Hiệu ứng | `HieuUng` | Quản lý Hiệu ứng | `AdminHieuUngController` | `/AdminHieuUng` |
| 9 | Quản lý sản phẩm | `SanPham` | Sản phẩm Gây Quỹ | `AdminSanPhamController` | `/CuaHang/AdminSanPham` |
| 10 | Đơn hàng | `DonHang` | Đơn hàng Gây Quỹ | `AdminDonHangController` | `/CuaHang/AdminDonHang` |
| 11 | Quản lý tài khoản | `QuanLyTaiKhoan` | Quản lý Tài khoản Quản trị | `AdminTaiKhoanController` | `/AdminTaiKhoan` |
| * | Toàn quyền Hệ thống | `SuperAdmin` (hoặc `All`) | Toàn bộ Quyền Hệ thống | Toàn bộ Controllers | Toàn bộ các routes |

### 2.2. Lớp Hằng số `QuyenAdmin` (`WEB_TinhNguyen/MoHinh/QuyenAdmin.cs`)
Tạo mới file `WEB_TinhNguyen/MoHinh/QuyenAdmin.cs`:
```csharp
namespace WEB_TinhNguyen.MoHinh
{
    public static class QuyenAdmin
    {
        // Quyền tối cao (SuperAdmin / Toàn quyền)
        public const string SuperAdmin = "SuperAdmin";
        public const string All = "All";

        // 11 Phân hệ chức năng nghiệp vụ
        public const string TrangChu = "TrangChu";
        public const string ChuongTrinh = "ChuongTrinh";
        public const string GayQuy = "GayQuy";
        public const string CauHinhQR = "CauHinhQR";
        public const string KyUc = "KyUc";
        public const string BanToChuc = "BanToChuc";
        public const string TinTuc = "TinTuc";
        public const string HieuUng = "HieuUng";
        public const string SanPham = "SanPham";
        public const string DonHang = "DonHang";
        public const string QuanLyTaiKhoan = "QuanLyTaiKhoan";

        // Danh mục mô tả chi tiết phục vụ hiển thị Checkbox trên giao diện
        public static readonly IReadOnlyDictionary<string, string> DanhSachPhanHe = new Dictionary<string, string>
        {
            { TrangChu, "Cấu hình Trang chủ" },
            { ChuongTrinh, "Cấu hình Chương trình & Lộ trình" },
            { GayQuy, "Cấu hình Đóng góp & Kênh gây quỹ" },
            { CauHinhQR, "Cấu hình Thanh toán & Mã VietQR" },
            { KyUc, "Cấu hình Ký ức & Thư viện ảnh" },
            { BanToChuc, "Cấu hình Ban Tổ Chức & Thành viên" },
            { TinTuc, "Cấu hình Tin tức & Bài viết" },
            { HieuUng, "Quản lý Hiệu ứng & Animation" },
            { SanPham, "Quản lý Sản phẩm Gây quỹ (Cửa hàng)" },
            { DonHang, "Quản lý Đơn hàng Gây quỹ (Cửa hàng)" },
            { QuanLyTaiKhoan, "Quản lý Tài khoản & Phân quyền Quản trị" }
        };

        // Danh sách mảng tất cả 11 module chuẩn
        public static readonly string[] TatCaPhanHe = new[]
        {
            TrangChu, ChuongTrinh, GayQuy, CauHinhQR, KyUc,
            BanToChuc, TinTuc, HieuUng, SanPham, DonHang, QuanLyTaiKhoan
        };
    }
}
```

### 2.3. Lược đồ Lưu trữ Bền vững trong CSDL (Database Schema Design)
- **Cột bổ sung**: `[QuyenHan] NVARCHAR(MAX) NULL` trong bảng `[dbo].[TaiKhoanAdmin]`.
- **Định dạng dữ liệu**:
  - Chuỗi phân tách dấu phẩy các mã quyền (Delimited String): Ví dụ `"TrangChu,ChuongTrinh,TinTuc"`.
  - Đối với tài khoản toàn quyền: Lưu `"All"` hoặc `"SuperAdmin"`.
- **Ưu điểm vượt trội của phương án này**:
  1. *Khả năng chịu lỗi cao (Fault-Tolerant & Resilient)*: Không phát sinh bảng phụ trợ nhiều-nhiều, không gây phức tạp hóa truy vấn JOIN.
  2. *Tương thích hoàn hảo với bộ nhớ In-Memory fallback*: Dễ dàng clone, serialize và quản lý trong bộ nhớ song song `_inMemoryAdmins` mà không bị lỗi đứt gãy quan hệ khóa ngoại khi database offline.
  3. *Tương thích 100% với hệ thống Claims Identity*: Tách chuỗi ra mảng và nạp trực tiếp vào `ClaimsPrincipal`.
  4. *Tính phi phá hủy CSDL*: Bảng cũ chỉ cần `ALTER TABLE ADD QuyenHan` là chạy ngay lập tức, không làm gián đoạn CSDL đang vận hành.

### 2.4. Khởi tạo Dữ liệu Mẫu (Seed Data) & An toàn Tương thích Ngược
- Tài khoản quản trị mặc định:
  - Tên đăng nhập: `admin`
  - Mật khẩu: `Admin@123`
  - Quyền hạn: `QuyenHan = "All"`
  - Trạng thái: Hoạt động (`TrangThaiHoatDong = true`)
- Khi `DbInitializer.SeedAdmin(context)` chạy:
  - Tự động bổ sung cột `QuyenHan` nếu CSDL vật lý chưa có.
  - Kiểm tra xem tài khoản `admin` đã có `QuyenHan` chưa; nếu rỗng/null thì tự động gán `"All"`.
  - Khi tài khoản `admin` đăng nhập, hệ thống cấp quyền toàn bộ trên tất cả 11 phân hệ.

---

## 3. THIẾT KẾ ĐƯỜNG ỐNG KIỂM SOÁT ỦY QUYỀN (AUTHORIZATION ENFORCEMENT - R2)

### 3.1. Quá trình Gán Claims khi Đăng nhập (`AdminController.Login`)
Khi người dùng đăng nhập thành công qua form `/admin/login`, quy trình tạo Claims diễn ra như sau:

```
[Người dùng gửi biểu mẫu đăng nhập: username, password]
                     │
                     ▼
[DbInitializer.ValidateAdminCredentials: xác thực PBKDF2]
                     │
                     ├─ Thất bại ──> Trả về ModelState.AddModelError, render Login view
                     │
                     ▼ Thành công
[DbInitializer.GetAdminByUsername: lấy TaiKhoanAdmin]
                     │
                     ▼
[Tạo ClaimsPrincipal]
 ├─ Name: admin.HoTen
 ├─ NameIdentifier: admin.IDTaiKhoan
 ├─ Role: admin.VaiTro ("Admin" hoặc "SuperAdmin")
 ├─ TenDangNhap: admin.TenDangNhap
 ├─ Email: admin.Email
 └─ QuyenHan Claims:
      ├─ Nếu là "All" / "SuperAdmin" hoặc TenDangNhap == "admin":
      │    ├─ Thêm Claim("QuyenHan", "All")
      │    ├─ Thêm Claim("QuyenHan", "SuperAdmin")
      │    └─ Thêm 11 Claim("QuyenHan", module) cho cả 11 phân hệ
      └─ Nếu là quyền cụ thể:
           └─ Tách chuỗi admin.QuyenHan và thêm từng Claim("QuyenHan", module)
                     │
                     ▼
[HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)]
```

### 3.2. Bộ lọc Ủy quyền Tùy chỉnh: `PhanQuyenAdminAttribute`
Tạo mới tệp `WEB_TinhNguyen/BoDieuKhien/PhanQuyenAdminAttribute.cs`:
```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.BoDieuKhien
{
    /// <summary>
    /// Bộ lọc kiểm soát quyền truy cập chi tiết theo từng phân hệ quản trị (R2 Granular Authorization).
    /// Áp dụng cho Controller hoặc Action method.
    /// Kế thừa AuthorizeAttribute để bảo toàn reflection invariant trên các bài test tự động.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PhanQuyenAdminAttribute : AuthorizeAttribute, IAsyncActionFilter
    {
        public string Module { get; }

        public PhanQuyenAdminAttribute(string module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // 1. Nếu chưa đăng nhập: Nhường lại cho [Authorize] xử lý HTTP 302 Challenge về /admin/login
            if (user?.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            // 2. Kiểm tra quyền hạn chi tiết đối với phân hệ được yêu cầu
            if (!PhanQuyenAdminHelper.CoQuyen(user, Module))
            {
                // Từ chối truy cập: Thiết lập mã HTTP 403 Forbidden
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            // 3. Đã được cấp quyền hợp lệ -> Cho phép tiếp tục thực thi action
            await next();
        }
    }
}
```

### 3.3. Lớp Trợ giúp Kiểm tra Quyền: `PhanQuyenAdminHelper`
Tạo mới file `WEB_TinhNguyen/MoHinh/PhanQuyenAdminHelper.cs`:
```csharp
using System;
using System.Linq;
using System.Security.Claims;

namespace WEB_TinhNguyen.MoHinh
{
    public static class PhanQuyenAdminHelper
    {
        public const string ClaimTypeQuyenHan = "QuyenHan";

        /// <summary>
        /// Kiểm tra xem ClaimsPrincipal hiện tại có quyền thao tác trên phân hệ được chỉ định hay không.
        /// Hỗ trợ cơ chế SuperAdmin Bypass toàn diện.
        /// </summary>
        public static bool CoQuyen(ClaimsPrincipal? user, string moduleYeuCau)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // Phân hệ tổng quan /Admin hoặc Dashboard: mọi admin đăng nhập đều có quyền xem
            if (string.Equals(moduleYeuCau, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(moduleYeuCau, "Dashboard", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 1. SuperAdmin Bypass: Tài khoản root 'admin' hoặc có Role SuperAdmin
            if (user.IsInRole("SuperAdmin") ||
                string.Equals(user.FindFirst("TenDangNhap")?.Value, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 2. SuperAdmin Bypass: Có claim "All", "SuperAdmin" hoặc "*"
            var claimsQuyen = user.FindAll(ClaimTypeQuyenHan).Select(c => c.Value).ToList();
            if (claimsQuyen.Any(q => string.Equals(q, QuyenAdmin.All, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(q, QuyenAdmin.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
                                     q == "*"))
            {
                return true;
            }

            // 3. Kiểm tra mã phân hệ cụ thể
            return claimsQuyen.Any(q => string.Equals(q, moduleYeuCau, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Kiểm tra quyền hạn trực tiếp trên thực thể TaiKhoanAdmin
        /// </summary>
        public static bool CoQuyen(TaiKhoanAdmin? admin, string moduleYeuCau)
        {
            if (admin == null || !admin.TrangThaiHoatDong) return false;

            if (string.Equals(admin.TenDangNhap, "admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(admin.VaiTro, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return admin.HasPermission(moduleYeuCau);
        }
    }

    /// <summary>
    /// Phương thức mở rộng thuận tiện cho ClaimsPrincipal (đặc biệt hữu ích trong Razor View)
    /// </summary>
    public static class PhanQuyenExtensions
    {
        public static bool CoQuyen(this ClaimsPrincipal user, string module)
        {
            return PhanQuyenAdminHelper.CoQuyen(user, module);
        }
    }
}
```

### 3.4. Gắn Thuộc tính Bảo vệ trên Toàn bộ 11 Admin Controllers
Mỗi Controller quản trị được trang trí thuộc tính tương ứng ở cấp độ lớp (Class-level), đảm bảo bảo vệ 100% cả yêu cầu GET và toàn bộ 21+ POST CRUD endpoints:

```csharp
// 1. Cấu hình Trang chủ
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.TrangChu)]
public class AdminTrangChuController : Controller { ... }

// 2. Cấu hình Chương trình
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.ChuongTrinh)]
public class AdminChuongTrinhController : Controller { ... }

// 3. Cấu hình Đóng góp / Gây quỹ
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.GayQuy)]
public class AdminGayQuyController : Controller { ... }

// 4. Cấu hình Thanh toán QR
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.CauHinhQR)]
public class AdminCauHinhQRController : Controller { ... }

// 5. Cấu hình Ký ức
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.KyUc)]
public class AdminKyUcController : Controller { ... }

// 6. Cấu hình Ban Tổ Chức
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.BanToChuc)]
public class AdminBanToChucController : Controller { ... }

// 7. Cấu hình Tin tức
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.TinTuc)]
public class AdminTinTucController : Controller { ... }

// 8. Quản lý Hiệu ứng
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.HieuUng)]
public class AdminHieuUngController : Controller { ... }

// 9. Quản lý Sản phẩm (Cửa hàng)
[Area("CuaHang")]
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.SanPham)]
public class AdminSanPhamController : Controller { ... }

// 10. Quản lý Đơn hàng (Cửa hàng)
[Area("CuaHang")]
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.DonHang)]
public class AdminDonHangController : Controller { ... }

// 11. Quản lý Tài khoản Quản trị
[Authorize]
[PhanQuyenAdmin(QuyenAdmin.QuanLyTaiKhoan)]
public class AdminTaiKhoanController : Controller { ... }

// Dashboard chung: Không gắn PhanQuyenAdmin, mọi admin đã đăng nhập đều vào được
[Authorize]
public class AdminController : Controller { ... }
```

### 3.5. Cơ chế Xử lý khi Không Có Quyền (HTTP 403 vs Redirect)
Theo yêu cầu R2 và tiêu chí chấp nhận:
- Khi người dùng gửi yêu cầu HTTP (GET hoặc POST) đến một phân hệ mà tài khoản chưa được cấp quyền:
  1. `OnActionExecutionAsync` bắt chặn ngay lập tức TRƯỚC KHI action method được kích hoạt.
  2. Không có bất kỳ dòng code xử lý dữ liệu nào (Create, Update, Delete) được thực thi.
  3. Trả về mã phản hồi HTTP `403 Forbidden` (`StatusCodes.Status403Forbidden`).
  4. Đối với giao diện người dùng nếu cần hiển thị trang thông báo thân thiện: Có thể tạo sẵn View `AccessDenied.cshtml` tại `/GiaoDien/Admin/AccessDenied.cshtml` với mã trạng thái 403 để người quản trị biết rõ tài khoản của mình thiếu quyền truy cập.

---

## 4. BẢO TỒN TÍNH TOÀN VẸN CỦA HỆ THỐNG KIỂM THỬ HIỆN CÓ

Qua rà soát chuyên sâu các bộ kiểm thử tự động hiện tại (`518 tests` trong `tests/E2ETests/`), đã phát hiện các điểm ràng buộc kiểm thử then chốt:

1. **Ràng buộc `M2_3_01_AllElevenAdminControllersHaveAuthorizeAttribute` & `M2_2_01`**:
   - Kiểm tra `[Authorize]` trên 11 controller hiện tại qua Reflection.
   - *Giải pháp*: Giữ nguyên `[Authorize]` trên tất cả các controller; thêm `[PhanQuyenAdmin(...)]` song song. Vì `PhanQuyenAdminAttribute` kế thừa `AuthorizeAttribute`, nên cả 2 cách kiểm tra reflection đều hoàn toàn tương thích.
2. **Ràng buộc `M2_3_02_AllAdminCrudActionsHaveAuthorizeProtection` & `M2_2_03`**:
   - Kiểm tra đúng 21 CRUD POST actions trên 10 domain controllers phải có `[ValidateAntiForgeryToken]` và không có `[AllowAnonymous]`.
   - *Giải pháp*: Các controller hiện tại giữ nguyên 100% các attribute cũ.
3. **Ràng buộc `M2_3_04_CookieAuthenticationOptionsRegistration`**:
   - Khẳng định `AccessDeniedPath == "/admin/login"`.
   - *Giải pháp*: Giữ nguyên `options.AccessDeniedPath = "/admin/login"` trong `Program.cs`. Không chỉnh sửa đường dẫn này.
4. **Ràng buộc `M2_3_05_ProgramCsPipelineOrderingInvariants`**:
   - Khẳng định thứ tự: `UseRouting` -> `UseSession` -> `UseAuthentication` -> `UseAuthorization` -> `UseStaticFiles`.
   - *Giải pháp*: Tuyệt đối không thay đổi thứ tự middleware trong `Program.cs`.
5. **Ràng buộc `F19_T01`, `F19_T02`, `F19_T03` (`Tier1_AdminRoutingTests.cs`)**:
   - Kiểm tra chuỗi thô của file `_AdminLayout.cshtml` để tìm các đoạn mã Razor: `"Index", "AdminTrangChu", new { area = "" }`, `"Index", "AdminChuongTrinh", new { area = "" }`, v.v.
   - *Giải pháp*: Khi cập nhật menu động ở R3, các thẻ link được bọc trong `@if (User.CoQuyen(...))` nhưng bên trong vẫn giữ nguyên cấu trúc `Url.Action("Index", "AdminTrangChu", new { area = "" })`, đảm bảo regex và string assert của test luôn PASS.
6. **Ràng buộc SHA-256 Hash Locked**:
   - Thư mục `WEB_TinhNguyen/Areas/CuaHang/Views/` bị khóa băm SHA-256. Không được phép chỉnh sửa các view trong thư mục này.

---

## 5. KẾT LUẬN & ĐỀ XUẤT TRIỂN KHAI CHO CÁC BƯỚC TIẾP THEO

1. **Mô hình Dữ liệu**:
   - Bổ sung thuộc tính `QuyenHan` vào `TaiKhoanAdmin.cs`.
   - Bổ sung cập nhật trong `DbInitializer.cs` (cả DDL SQL Server idempotent và in-memory store).
2. **Lược đồ Phân quyền**:
   - Xây dựng lớp `QuyenAdmin.cs` chứa hằng số 11 phân hệ và `SuperAdmin`.
   - Xây dựng lớp trợ giúp `PhanQuyenAdminHelper.cs`.
3. **Đường ống Ủy quyền**:
   - Xây dựng attribute `PhanQuyenAdminAttribute.cs`.
   - Cập nhật `AdminController.Login` để nạp claims quyền hạn vào `ClaimsPrincipal`.
   - Gắn `[PhanQuyenAdmin(...)]` lên các controller quản trị.
4. **Quản lý Tài khoản (R1 Controller & Views)**:
   - Xây dựng `AdminTaiKhoanController` và ViewModel tạo mới/phân quyền tài khoản.
   - Xây dựng giao diện danh sách và form thêm tài khoản với danh sách checkbox 11 phân hệ.
