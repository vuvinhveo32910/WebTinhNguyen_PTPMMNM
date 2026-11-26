# Phân Tích & Thiết Kế Kỹ Thuật: Milestone 1 - Permissions Schema & Model

## 1. Tổng Quan & Bối Cảnh Nghiên Cứu

Chiến dịch phát triển chức năng Quản lý Tài khoản Quản trị và Phân quyền Chi tiết (`/admin`) yêu cầu thiết lập nền tảng mô hình dữ liệu (Data & Security Model) vững chắc tại Milestone 1. Nền tảng này làm điểm tựa cho toàn bộ động cơ kiểm soát quyền truy cập (Milestone 2), giao diện quản lý tài khoản quản trị (Milestone 3), điều hướng động thanh menu sidebar (Milestone 4) và bộ kịch bản kiểm thử tích hợp E2E (Milestone 5).

Nhiệm vụ của `teamwork_explorer_m1_1` là khảo sát toàn diện hiện trạng mã nguồn, kiểm tra 518 bài test hiện có, phân tích yêu cầu từ `ORIGINAL_REQUEST.md` và `PROJECT.md`, từ đó đề xuất thiết kế chi tiết, chính xác từng dòng mã cho 3 thành phần cốt lõi:
1. `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`: Bổ sung thuộc tính `PhanQuyen`, bảo toàn 100% các trường dữ liệu hiện hữu.
2. `WEB_TinhNguyen/MoHinh/QuyenAdmin.cs`: Tạo mới lớp hằng số quyền chuẩn hóa cho 11 phân hệ và quyền toàn quyền hệ thống (`ALL` / `SuperAdmin`), kèm theo các hàm trợ giúp danh sách quyền và mô tả nhãn UI.
3. `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`: Xác minh và cấu hình ánh xạ EF Core cho thuộc tính `PhanQuyen`.
4. Khảo sát liên đới: `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs` phục vụ cơ chế dual-mode persistence (SQL Server và In-Memory fallback store).

---

## 2. Khảo Sát Hiện Trạng Codebase

### 2.1. Thực thể `TaiKhoanAdmin.cs`
- **Đường dẫn**: `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`
- **Tình trạng hiện tại**:
  - Đang quản lý 10 thuộc tính: `IDTaiKhoan` (int, PK Identity), `TenDangNhap` (string, max 100, unique index), `MatKhauHash` (string, max 500), `MuoiBaoMat` (string, max 100), `HoTen` (string?, max 150), `Email` (string?, max 150), `VaiTro` (string, default "Admin"), `TrangThaiHoatDong` (bool, default true), `NgayTao` (DateTime), `LanDangNhapCuoi` (DateTime?).
  - Phương thức `Clone()` tạo bản sao đối tượng để cô lập dữ liệu bộ nhớ.
  - Lớp `AdminLoginViewModel` nằm cùng file, hỗ trợ binding dữ liệu form đăng nhập.
- **Điểm khuyết**: Chưa có thuộc tính `PhanQuyen` để lưu trữ chuỗi mã quyền phân hệ. Phương thức `Clone()` chưa sao chép trường này.

### 2.2. Lớp hằng số `QuyenAdmin.cs`
- **Đường dẫn**: `WEB_TinhNguyen/MoHinh/QuyenAdmin.cs`
- **Tình trạng hiện tại**: File chưa tồn tại.
- **Yêu cầu cần đáp ứng**:
  - Khai báo hằng số cho toàn bộ 11 phân hệ quản trị:
    1. `TrangChu`: Quản lý cấu hình Trang chủ (`AdminTrangChuController`)
    2. `ChuongTrinh`: Quản lý Chương trình (`AdminChuongTrinhController`)
    3. `GayQuy`: Quản lý Gây quỹ (`AdminGayQuyController`)
    4. `CauHinhQR`: Quản lý Cấu hình QR (`AdminCauHinhQRController`)
    5. `KyUc`: Quản lý Ký ức (`AdminKyUcController`)
    6. `BanToChuc`: Quản lý Ban tổ chức (`AdminBanToChucController`)
    7. `TinTuc`: Quản lý Tin tức (`AdminTinTucController`)
    8. `HieuUng`: Quản lý Hiệu ứng (`AdminHieuUngController`)
    9. `SanPham`: Quản lý Sản phẩm Cửa hàng (`Areas/CuaHang/Controllers/AdminSanPhamController`)
    10. `DonHang`: Quản lý Đơn hàng Cửa hàng (`Areas/CuaHang/Controllers/AdminDonHangController`)
    11. `QuanLyTaiKhoan`: Quản lý Tài khoản Quản trị (`AdminTaiKhoanController` - M3)
  - Quyền toàn hệ thống: `ALL` (hoặc `All`) và `SuperAdmin`.
  - Hàm trợ giúp: `TatCaQuyen()`, `DanhSachQuyen()`, lấy danh sách chi tiết kèm icon, tên hiển thị, mô tả phân nhóm cho giao diện phân quyền.
  - Hàm thẩm định quyền: `KiemTraCoQuyen(string? chuoiPhanQuyen, string? maPhanHe)` và các tiện ích chuẩn hóa chuỗi.

### 2.3. Cấu hình DbContext `ApplicationDbContext.cs`
- **Đường dẫn**: `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`
- **Tình trạng hiện tại**:
  - Dòng 56-57: Khai báo `DbSet<TaiKhoanAdmin> TaiKhoanAdmins` và alias `TaiKhoanAdmin`.
  - Dòng 227-240: Cấu hình `OnModelCreating` ánh xạ bảng `TaiKhoanAdmin`, thiết lập các giới hạn độ dài, khóa chính tự tăng, chỉ mục duy nhất và giá trị mặc định cho các cột hiện tại.
- **Điểm cần bổ sung**: Ánh xạ cho cột `PhanQuyen` (`HasColumnType("nvarchar(max)")` hoặc không giới hạn độ dài, giá trị mặc định `"ALL"`).

### 2.4. Lưu trữ Dual-Mode trong `DbInitializer.cs`
- **Đường dẫn**: `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`
- **Quan sát**:
  - `GetDefaultAdmins()` (dòng 59): Khởi tạo tài khoản `admin` mặc định. Cần gán `PhanQuyen = "ALL"`.
  - `CloneAdmin()` (dòng 84): Nhân bản thực thể `TaiKhoanAdmin`. Cần gán `PhanQuyen = a.PhanQuyen`.
  - `UpdateInMemoryAdmin()` (dòng 1215): Cập nhật thực thể admin trong in-memory store. Cần cập nhật `existing.PhanQuyen = item.PhanQuyen`.
  - `SeedAdmin()` (dòng 1010) và `Seed()` (dòng 413): Chứa câu lệnh DDL `CREATE TABLE [dbo].[TaiKhoanAdmin]`. Cần bổ sung cột `[PhanQuyen] NVARCHAR(MAX) NULL` và câu lệnh `IF COL_LENGTH(N'[dbo].[TaiKhoanAdmin]', N'PhanQuyen') IS NULL ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [PhanQuyen] NVARCHAR(MAX) NULL;` để đảm bảo tương thích ngược (idempotent schema migration).

### 2.5. Hiện trạng Kiểm thử Tự động
- Chạy toàn bộ test suite dự án bằng lệnh `dotnet run --project tests/E2ETests`:
  - **Kết quả**: 518/518 bài test vượt qua (100% Passed, 0 Failed, 0 Skipped).
  - Không có bất kỳ tham chiếu nào xung đột với tên `QuyenAdmin` hoặc thuộc tính `PhanQuyen`.

---

## 3. Thiết Kế Chi Tiết & Mã Đề Xuất

### 3.1. Thiết kế `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`

#### Nguyên tắc bảo toàn:
- Giữ nguyên 100% các thuộc tính hiện tại: `IDTaiKhoan`, `TenDangNhap`, `MatKhauHash`, `MuoiBaoMat`, `HoTen`, `Email`, `VaiTro`, `TrangThaiHoatDong`, `NgayTao`, `LanDangNhapCuoi` cùng toàn bộ DataAnnotations (`[Required]`, `[StringLength]`, `[Display]`, `[Index]`, `[Table]`).
- Bổ sung thuộc tính `PhanQuyen` kiểu `string?` với giá trị khởi tạo mặc định là `"ALL"`.
- Bổ sung `PhanQuyen = this.PhanQuyen` vào phương thức `Clone()`.
- Cung cấp các phương thức tiện ích: `CoQuyen(string? maPhanHe)` và `LayDanhSachQuyen()` để thuận tiện truy vấn trực tiếp trên thực thể.

#### Mã nguồn chi tiết đề xuất cho `TaiKhoanAdmin.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WEB_TinhNguyen.MoHinh
{
    /// <summary>
    /// Thực thể quản lý tài khoản quản trị viên hệ thống.
    /// Lưu trữ thông tin định danh, thông tin bảo mật mật khẩu (được băm bằng PBKDF2),
    /// phân quyền và trạng thái hoạt động.
    /// </summary>
    [Table("TaiKhoanAdmin")]
    [Index(nameof(TenDangNhap), IsUnique = true)]
    public class TaiKhoanAdmin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã tài khoản")]
        public int IDTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, ErrorMessage = "Tên đăng nhập không được vượt quá 100 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu băm không được để trống")]
        [StringLength(500, ErrorMessage = "Mật khẩu băm không được vượt quá 500 ký tự")]
        [Display(Name = "Mật khẩu băm")]
        public string MatKhauHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Muối bảo mật không được để trống")]
        [StringLength(100, ErrorMessage = "Muối bảo mật không được vượt quá 100 ký tự")]
        [Display(Name = "Muối bảo mật")]
        public string MuoiBaoMat { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Họ và tên không được vượt quá 150 ký tự")]
        [Display(Name = "Họ và tên")]
        public string? HoTen { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Vai trò không được vượt quá 50 ký tự")]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "Admin";

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThaiHoatDong { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Lần đăng nhập cuối")]
        public DateTime? LanDangNhapCuoi { get; set; }

        /// <summary>
        /// Danh sách mã phân hệ được phân quyền cho tài khoản, phân tách bởi dấu phẩy (vd: "TrangChu,ChuongTrinh,GayQuy"),
        /// hoặc "ALL" / "SuperAdmin" đại diện cho toàn quyền quản trị tất cả phân hệ.
        /// </summary>
        [Display(Name = "Phân quyền quản trị")]
        public string? PhanQuyen { get; set; } = "ALL";

        /// <summary>
        /// Kiểm tra nhanh tài khoản hiện tại có quyền truy cập phân hệ chỉ định hay không.
        /// </summary>
        public bool CoQuyen(string? maPhanHe)
        {
            return QuyenAdmin.KiemTraCoQuyen(this.PhanQuyen, maPhanHe);
        }

        /// <summary>
        /// Lấy danh sách các mã quyền phân hệ dưới dạng danh sách chuỗi riêng lẻ.
        /// </summary>
        public List<string> LayDanhSachQuyen()
        {
            return QuyenAdmin.TachChuoiPhanQuyen(this.PhanQuyen);
        }

        /// <summary>
        /// Tạo bản sao dữ liệu phục vụ bộ nhớ dự phòng In-Memory store trong DbInitializer.
        /// </summary>
        public TaiKhoanAdmin Clone()
        {
            return new TaiKhoanAdmin
            {
                IDTaiKhoan = this.IDTaiKhoan,
                TenDangNhap = this.TenDangNhap,
                MatKhauHash = this.MatKhauHash,
                MuoiBaoMat = this.MuoiBaoMat,
                HoTen = this.HoTen,
                Email = this.Email,
                VaiTro = this.VaiTro,
                TrangThaiHoatDong = this.TrangThaiHoatDong,
                NgayTao = this.NgayTao,
                LanDangNhapCuoi = this.LanDangNhapCuoi,
                PhanQuyen = this.PhanQuyen
            };
        }
    }

    /// <summary>
    /// ViewModel dùng cho form đăng nhập quản trị viên (/admin/login).
    /// Hỗ trợ validation và binding tự động.
    /// </summary>
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
}
```

---

### 3.2. Thiết kế `WEB_TinhNguyen/MoHinh/QuyenAdmin.cs`

#### Nguyên tắc thiết kế:
1. **Hằng số chuẩn xác (Canonical Constants)**: Khai báo đúng 11 mã phân hệ và 2 hằng số toàn quyền:
   - `TrangChu = "TrangChu"`
   - `ChuongTrinh = "ChuongTrinh"`
   - `GayQuy = "GayQuy"`
   - `CauHinhQR = "CauHinhQR"`
   - `KyUc = "KyUc"`
   - `BanToChuc = "BanToChuc"`
   - `TinTuc = "TinTuc"`
   - `HieuUng = "HieuUng"`
   - `SanPham = "SanPham"`
   - `DonHang = "DonHang"`
   - `QuanLyTaiKhoan = "QuanLyTaiKhoan"`
   - `All = "ALL"` và `ALL = "ALL"` (hỗ trợ cả 2 phong cách viết hoa)
   - `SuperAdmin = "SuperAdmin"`
2. **Phương thức trợ giúp (Helper Methods)**:
   - `TatCaQuyen()`: Trả về mảng danh sách 11 mã quyền.
   - `DanhSachQuyen()`: Bí danh của `TatCaQuyen()` theo đúng yêu cầu dispatch.
   - `LayDanhSachThongTinQuyen()`: Trả về danh sách đối tượng `ThongTinPhanQuyen` có tên hiển thị thân thiện, icon FontAwesome, mô tả chi tiết và nhóm phân hệ (Nội dung / Quyên góp - Cửa hàng / Quản trị hệ thống).
   - `LayTenHienThi(string? maQuyen)`: Chuyển đổi mã quyền sang tên tiếng Việt hiển thị trên giao diện.
   - `KiemTraCoQuyen(string? chuoiPhanQuyen, string? maPhanHe)`: Thuật toán kiểm tra quyền mạnh mẽ, hỗ trợ toàn quyền ("ALL", "SuperAdmin"), danh sách phân tách bởi dấu phẩy hoặc chấm phẩy, tự động bỏ qua khoảng trắng và không phân biệt chữ hoa/thường.
   - `TachChuoiPhanQuyen(string? chuoiPhanQuyen)`: Tách chuỗi thành `List<string>`.
   - `ChuanHoaChuoiPhanQuyen(IEnumerable<string>? danhSachQuyen)`: Gom danh sách quyền thành chuỗi chuẩn hóa, tự động nhận diện nếu có "ALL" thì trả về "ALL".

#### Mã nguồn chi tiết đề xuất cho `QuyenAdmin.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace WEB_TinhNguyen.MoHinh
{
    /// <summary>
    /// Định nghĩa các hằng số phân quyền quản trị chuẩn hóa và các phương thức trợ giúp
    /// phân quyền chi tiết cho khu vực quản trị (/admin).
    /// Bao gồm 11 phân hệ chức năng và quyền toàn hệ thống (ALL / SuperAdmin).
    /// </summary>
    public static class QuyenAdmin
    {
        // =========================================================================
        // HẰNG SỐ MÃ PHÂN HỆ QUẢN TRỊ (11 MODULES)
        // =========================================================================
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

        // =========================================================================
        // HẰNG SỐ TOÀN QUYỀN HỆ THỐNG (SUPERADMIN / FULL ACCESS)
        // =========================================================================
        public const string All = "ALL";
        public const string ALL = "ALL";
        public const string SuperAdmin = "SuperAdmin";

        /// <summary>
        /// Mảng danh sách chuẩn hóa toàn bộ 11 mã phân hệ quản trị trong hệ thống.
        /// </summary>
        private static readonly string[] _tatCaMaQuyen = new[]
        {
            TrangChu,
            ChuongTrinh,
            GayQuy,
            CauHinhQR,
            KyUc,
            BanToChuc,
            TinTuc,
            HieuUng,
            SanPham,
            DonHang,
            QuanLyTaiKhoan
        };

        /// <summary>
        /// Trả về danh sách tất cả 11 mã phân hệ quản trị.
        /// </summary>
        public static IReadOnlyList<string> TatCaQuyen() => _tatCaMaQuyen;

        /// <summary>
        /// Bí danh của TatCaQuyen() phục vụ tính tương thích cao trong các ngữ cảnh gọi khác nhau.
        /// </summary>
        public static IReadOnlyList<string> DanhSachQuyen() => _tatCaMaQuyen;

        /// <summary>
        /// Danh sách metadata phân quyền chi tiết phục vụ hiển thị form checkbox gán quyền trên UI.
        /// </summary>
        public static IReadOnlyList<ThongTinPhanQuyen> LayDanhSachThongTinQuyen()
        {
            return new List<ThongTinPhanQuyen>
            {
                new ThongTinPhanQuyen
                {
                    MaQuyen = TrangChu,
                    TenHienThi = "Cấu hình Trang chủ",
                    MoTa = "Quản lý banner, câu khẩu hiệu, đồng hồ đếm ngược và nội dung giới thiệu trang chủ",
                    Icon = "fa-home",
                    NhomPhanHe = "Nội dung & Truyền thông"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = ChuongTrinh,
                    TenHienThi = "Chương trình thiện nguyện",
                    MoTa = "Quản lý các chặng hành trình thiện nguyện, lịch trình chi tiết và hoạt động",
                    Icon = "fa-calendar-alt",
                    NhomPhanHe = "Hoạt động Chiến dịch"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = GayQuy,
                    TenHienThi = "Chiến dịch Gây quỹ",
                    MoTa = "Quản lý chỉ tiêu gây quỹ, các kênh tiếp nhận đóng góp và danh sách nhà tài trợ",
                    Icon = "fa-hand-holding-heart",
                    NhomPhanHe = "Tài chính & Quyên góp"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = CauHinhQR,
                    TenHienThi = "Cấu hình Thanh toán QR",
                    MoTa = "Quản lý thông tin tài khoản ngân hàng và mã VietQR thanh toán/quyên góp tự động",
                    Icon = "fa-qrcode",
                    NhomPhanHe = "Tài chính & Quyên góp"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = KyUc,
                    TenHienThi = "Ký ức & Hoạt động",
                    MoTa = "Quản lý thư viện hình ảnh kỷ niệm, video hành trình và câu chuyện thiện nguyện",
                    Icon = "fa-camera-retro",
                    NhomPhanHe = "Nội dung & Truyền thông"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = BanToChuc,
                    TenHienThi = "Ban tổ chức & Đội ngũ",
                    MoTa = "Quản lý thông tin thành viên ban tổ chức, điều phối viên và đơn vị đồng hành",
                    Icon = "fa-users",
                    NhomPhanHe = "Hoạt động Chiến dịch"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = TinTuc,
                    TenHienThi = "Tin tức & Bài viết",
                    MoTa = "Quản lý các bài viết tin tức, thông cáo báo chí và bài viết cập nhật chiến dịch",
                    Icon = "fa-newspaper",
                    NhomPhanHe = "Nội dung & Truyền thông"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = HieuUng,
                    TenHienThi = "Hiệu ứng & Giao diện",
                    MoTa = "Quản lý hiệu ứng trang trí hình ảnh, khoảnh khắc nổi bật và animation",
                    Icon = "fa-magic",
                    NhomPhanHe = "Nội dung & Truyền thông"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = SanPham,
                    TenHienThi = "Sản phẩm Gây quỹ",
                    MoTa = "Quản lý danh mục sản phẩm, đơn giá, tồn kho và tỷ lệ đóng góp gây quỹ",
                    Icon = "fa-box-open",
                    NhomPhanHe = "Cửa hàng Gây quỹ"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = DonHang,
                    TenHienThi = "Đơn hàng Cửa hàng",
                    MoTa = "Quản lý danh sách đơn đặt hàng gây quỹ, xác nhận thanh toán và cập nhật giao hàng",
                    Icon = "fa-shopping-cart",
                    NhomPhanHe = "Cửa hàng Gây quỹ"
                },
                new ThongTinPhanQuyen
                {
                    MaQuyen = QuanLyTaiKhoan,
                    TenHienThi = "Tài khoản Quản trị",
                    MoTa = "Quản lý danh sách tài khoản quản trị viên, tạo mới tài khoản và phân quyền chi tiết",
                    Icon = "fa-user-shield",
                    NhomPhanHe = "Quản trị Hệ thống"
                }
            };
        }

        /// <summary>
        /// Lấy tên hiển thị thân thiện tiếng Việt theo mã phân hệ.
        /// Nếu mã không xác định, trả về chính chuỗi mã đó.
        /// </summary>
        public static string LayTenHienThi(string? maQuyen)
        {
            if (string.IsNullOrWhiteSpace(maQuyen)) return string.Empty;
            if (string.Equals(maQuyen, All, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(maQuyen, SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return "Toàn quyền Hệ thống";
            }

            var item = LayDanhSachThongTinQuyen().FirstOrDefault(q => string.Equals(q.MaQuyen, maQuyen, StringComparison.OrdinalIgnoreCase));
            return item != null ? item.TenHienThi : maQuyen;
        }

        /// <summary>
        /// Kiểm tra một mã quyền có phải là phân hệ hợp lệ hoặc quyền SuperAdmin không.
        /// </summary>
        public static bool KiemTraHopLe(string? maQuyen)
        {
            if (string.IsNullOrWhiteSpace(maQuyen)) return false;
            if (string.Equals(maQuyen, All, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(maQuyen, SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return _tatCaMaQuyen.Any(m => string.Equals(m, maQuyen.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Thuật toán kiểm tra chuỗi phân quyền của người dùng có chứa quyền truy cập phân hệ chỉ định hay không.
        /// Quy tắc đánh giá:
        /// 1. Nếu maPhanHe null hoặc rỗng -> Cho phép (không yêu cầu phân hệ cụ thể).
        /// 2. Nếu chuoiPhanQuyen null hoặc rỗng -> Từ chối (không có bất kỳ quyền nào).
        /// 3. Nếu chuoiPhanQuyen chứa "ALL" hoặc "SuperAdmin" -> Cho phép toàn bộ phân hệ.
        /// 4. Tách chuoiPhanQuyen bằng dấu phẩy (,) hoặc chấm phẩy (;), so sánh không phân biệt hoa thường với maPhanHe.
        /// </summary>
        public static bool KiemTraCoQuyen(string? chuoiPhanQuyen, string? maPhanHe)
        {
            if (string.IsNullOrWhiteSpace(maPhanHe))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(chuoiPhanQuyen))
            {
                return false;
            }

            var trimmedNorm = chuoiPhanQuyen.Trim();

            // Kiểm tra trường hợp SuperAdmin / ALL tuyệt đối
            if (string.Equals(trimmedNorm, All, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmedNorm, SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Tách các quyền
            var danhSach = trimmedNorm.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return danhSach.Any(q =>
                string.Equals(q, All, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(q, SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(q, maPhanHe.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Phân rã chuỗi phân quyền (dấu phẩy hoặc chấm phẩy) thành danh sách các mã quyền riêng biệt,
        /// đã loại bỏ khoảng trắng thừa và trùng lặp.
        /// </summary>
        public static List<string> TachChuoiPhanQuyen(string? chuoiPhanQuyen)
        {
            if (string.IsNullOrWhiteSpace(chuoiPhanQuyen))
            {
                return new List<string>();
            }

            return chuoiPhanQuyen
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Gom danh sách mã quyền được chọn thành chuỗi lưu trữ CSDL.
        /// Nếu danh sách chứa "ALL" hoặc chứa đủ cả 11 phân hệ, trả về "ALL".
        /// </summary>
        public static string ChuanHoaChuoiPhanQuyen(IEnumerable<string>? danhSachQuyen)
        {
            if (danhSachQuyen == null)
            {
                return string.Empty;
            }

            var cleanList = danhSachQuyen
                .Where(q => !string.IsNullOrWhiteSpace(q))
                .Select(q => q.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleanList.Any(q => string.Equals(q, All, StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(q, SuperAdmin, StringComparison.OrdinalIgnoreCase)))
            {
                return All;
            }

            // Nếu người dùng tick chọn đủ toàn bộ 11 quyền, tự động quy về ALL
            if (_tatCaMaQuyen.All(m => cleanList.Any(c => string.Equals(c, m, StringComparison.OrdinalIgnoreCase))))
            {
                return All;
            }

            return string.Join(",", cleanList);
        }
    }

    /// <summary>
    /// Lớp đối tượng mô tả thông tin chi tiết của từng quyền phân hệ để bind dữ liệu lên UI.
    /// </summary>
    public class ThongTinPhanQuyen
    {
        public string MaQuyen { get; set; } = string.Empty;
        public string TenHienThi { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string NhomPhanHe { get; set; } = string.Empty;
    }
}
```

---

### 3.3. Thiết kế `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`

#### Phân tích & Đề xuất chỉnh sửa:
Trong phương thức `OnModelCreating(ModelBuilder modelBuilder)` tại dòng 227-240:
```csharp
            // Cấu hình bảng TaiKhoanAdmin
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
                // BỔ SUNG CẤU HÌNH PHANQUYEN:
                entity.Property(e => e.PhanQuyen).HasColumnType("nvarchar(max)").HasDefaultValue("ALL");
            });
```
- Việc cấu hình `.HasColumnType("nvarchar(max)").HasDefaultValue("ALL")` hoàn toàn đồng nhất với:
  - Kiểu dữ liệu `NVARCHAR(MAX)` trong SQL Server DDL (`DbInitializer.cs`).
  - Giá trị mặc định `"ALL"` trong thuộc tính C# (`public string? PhanQuyen { get; set; } = "ALL";`).
  - Cho phép lưu trữ chuỗi danh sách mã phân hệ tùy ý không bị cắt bớt hoặc giới hạn chặt.

---

### 3.4. Thiết kế Đồng Bộ Dual-Mode Persistence (`DbInitializer.cs`)

Để đảm bảo Milestone 1 hoàn chỉnh và hoạt động trơn tru trong cả hai môi trường (SQL Server thật và Demo In-Memory offline), cần đồng bộ các vị trí sau trong `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`:

1. **`GetDefaultAdmins()` (Dòng 63-78)**:
   ```csharp
   new TaiKhoanAdmin
   {
       IDTaiKhoan = 1,
       TenDangNhap = "admin",
       MatKhauHash = hash,
       MuoiBaoMat = salt,
       HoTen = "Quản trị viên Hệ thống",
       Email = "admin@tinhnguyen.vn",
       VaiTro = "Admin",
       TrangThaiHoatDong = true,
       NgayTao = DateTime.Now,
       LanDangNhapCuoi = null,
       PhanQuyen = "ALL" // Đảm bảo tài khoản mặc định có toàn quyền hệ thống
   }
   ```

2. **`CloneAdmin(TaiKhoanAdmin a)` (Dòng 84-99)**:
   ```csharp
   return new TaiKhoanAdmin
   {
       IDTaiKhoan = a.IDTaiKhoan,
       TenDangNhap = a.TenDangNhap,
       MatKhauHash = a.MatKhauHash,
       MuoiBaoMat = a.MuoiBaoMat,
       HoTen = a.HoTen,
       Email = a.Email,
       VaiTro = a.VaiTro,
       TrangThaiHoatDong = a.TrangThaiHoatDong,
       NgayTao = a.NgayTao,
       LanDangNhapCuoi = a.LanDangNhapCuoi,
       PhanQuyen = a.PhanQuyen // Sao chép trường phân quyền để cách ly bộ nhớ
   };
   ```

3. **`UpdateInMemoryAdmin(TaiKhoanAdmin item)` (Dòng 1215-1238)**:
   ```csharp
   existing.TenDangNhap = item.TenDangNhap;
   if (!string.IsNullOrEmpty(item.MatKhauHash))
   {
       existing.MatKhauHash = item.MatKhauHash;
   }
   if (!string.IsNullOrEmpty(item.MuoiBaoMat))
   {
       existing.MuoiBaoMat = item.MuoiBaoMat;
   }
   existing.HoTen = item.HoTen;
   existing.Email = item.Email;
   existing.VaiTro = item.VaiTro;
   existing.TrangThaiHoatDong = item.TrangThaiHoatDong;
   existing.LanDangNhapCuoi = item.LanDangNhapCuoi;
   existing.PhanQuyen = item.PhanQuyen; // Cập nhật phân quyền khi admin chỉnh sửa
   return true;
   ```

4. **Câu lệnh DDL SQL Idempotent trong `Seed(ApplicationDbContext context)` và `SeedAdmin(ApplicationDbContext? context)`**:
   Trong khối `CREATE TABLE [dbo].[TaiKhoanAdmin]`:
   ```sql
   [PhanQuyen] NVARCHAR(MAX) NULL
   ```
   Kèm theo kiểm tra cột tồn tại để nâng cấp bảng hiện hữu mà không mất dữ liệu:
   ```sql
   IF COL_LENGTH(N'[dbo].[TaiKhoanAdmin]', N'PhanQuyen') IS NULL
   BEGIN
       ALTER TABLE [dbo].[TaiKhoanAdmin] ADD [PhanQuyen] NVARCHAR(MAX) NULL;
   END;
   ```

---

## 4. Đánh Giá Rủi Ro & Khả Năng Tương Thích Ngược

| Rủi Ro Tiềm Ẩn | Mức Độ | Biện Pháp Kiểm Soát & Hóa Giải |
|---|---|---|
| Phá vỡ bài test cũ kiểm tra `Clone()` hoặc `TaiKhoanAdmin` | Cực Thấp | Đã kiểm tra 518 bài test: Không bài test nào so sánh số lượng trường cố định hoặc kiểm tra strict reflection trên các properties trừ các thuộc tính cũ bắt buộc. Bổ sung `PhanQuyen` có giá trị mặc định `"ALL"` giúp mọi bài test cũ tiếp tục pass 100%. |
| Lỗi migration cơ sở dữ liệu nếu SQL Server đã có bảng `TaiKhoanAdmin` cũ | Thấp | Sử dụng câu lệnh DDL an toàn `IF COL_LENGTH(...) IS NULL ALTER TABLE ... ADD [PhanQuyen] NVARCHAR(MAX) NULL;` đảm bảo tính lũy thừa (idempotency). |
| Trùng lặp hoặc sai lệch mã quyền giữa các Controller | Không | Tất cả 11 hằng số trong `QuyenAdmin` được đặt tên trùng khớp chính xác với 11 Admin Controller hiện có và các giá trị trong `PROJECT.md`. |
| NullReferenceException khi đọc `PhanQuyen` từ tài khoản cũ | Không | `PhanQuyen` có default value `"ALL"`, đồng thời hàm `KiemTraCoQuyen` được thiết kế phòng vệ: nếu `chuoiPhanQuyen` null/rỗng thì trả về `false`, không ném ngoại lệ. |

---

## 5. Kết Luận & Khuyến Nghị Thực Hiện

1. **Thứ tự thực hiện khuyến nghị**:
   - Bước 1: Tạo mới `WEB_TinhNguyen/MoHinh/QuyenAdmin.cs`.
   - Bước 2: Cập nhật `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs`.
   - Bước 3: Cập nhật `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`.
   - Bước 4: Cập nhật `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`.
   - Bước 5: Biên dịch `dotnet build` và chạy lại 518 bài test `dotnet run --project tests/E2ETests`.

Thiết kế trên đảm bảo đáp ứng 100% mục tiêu của Milestone 1, tương thích tuyệt đối với kiến trúc tổng thể của dự án và dọn đường hoàn hảo cho các Milestone kế tiếp.
