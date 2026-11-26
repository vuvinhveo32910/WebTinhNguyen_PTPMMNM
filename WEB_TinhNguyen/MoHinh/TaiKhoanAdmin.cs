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
    /// phân quyền chi tiết theo từng phân hệ và trạng thái hoạt động.
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

        /// <summary>
        /// Danh sách các phân hệ chức năng được cấp quyền (cách nhau bởi dấu phẩy, ví dụ: "TrangChu,TinTuc")
        /// hoặc "ALL" cho toàn quyền quản trị hệ thống.
        /// </summary>
        [Display(Name = "Phân quyền chức năng")]
        public string PhanQuyen { get; set; } = QuyenAdmin.All;

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThaiHoatDong { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Lần đăng nhập cuối")]
        public DateTime? LanDangNhapCuoi { get; set; }

        /// <summary>
        /// Kiểm tra xem tài khoản này có toàn quyền hệ thống ("ALL" / "SuperAdmin") hay không.
        /// </summary>
        [NotMapped]
        public bool LaToanQuyen => QuyenAdmin.KiemTraToanQuyen(PhanQuyen, VaiTro);

        /// <summary>
        /// Kiểm tra xem tài khoản này có quyền truy cập phân hệ <paramref name="maPhanHe"/> hay không.
        /// </summary>
        public bool CoQuyen(string maPhanHe)
        {
            return QuyenAdmin.KiemTraQuyenChuoi(PhanQuyen, maPhanHe, VaiTro);
        }

        /// <summary>
        /// Lấy danh sách các mã phân hệ được cấp quyền cho tài khoản này.
        /// </summary>
        public List<string> LayDanhSachQuyen()
        {
            return QuyenAdmin.TachDanhSachQuyen(PhanQuyen);
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
                PhanQuyen = this.PhanQuyen,
                TrangThaiHoatDong = this.TrangThaiHoatDong,
                NgayTao = this.NgayTao,
                LanDangNhapCuoi = this.LanDangNhapCuoi
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

    /// <summary>
    /// ViewModel dùng cho form thêm mới tài khoản quản trị viên và phân quyền chức năng.
    /// </summary>
    public class AdminTaiKhoanCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 100 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string? XacNhanMatKhau { get; set; }

        [StringLength(150, ErrorMessage = "Họ và tên không được vượt quá 150 ký tự")]
        [Display(Name = "Họ và tên")]
        public string? HoTen { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "SubAdmin";

        [Display(Name = "Cấp toàn quyền quản trị hệ thống (Tất cả phân hệ)")]
        public bool ToanQuyen { get; set; } = false;

        [Display(Name = "Danh sách phân hệ được cấp quyền")]
        public List<string> DanhSachQuyen { get; set; } = new List<string>();

        [Display(Name = "Trạng thái kích hoạt")]
        public bool TrangThaiHoatDong { get; set; } = true;
    }

    /// <summary>
    /// ViewModel dùng cho form chỉnh sửa tài khoản quản trị viên và cập nhật phân quyền chức năng.
    /// </summary>
    public class AdminTaiKhoanEditViewModel
    {
        public int IDTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 100 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
        public string? MatKhauMoi { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string? XacNhanMatKhauMoi { get; set; }

        [StringLength(150, ErrorMessage = "Họ và tên không được vượt quá 150 ký tự")]
        [Display(Name = "Họ và tên")]
        public string? HoTen { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "SubAdmin";

        [Display(Name = "Cấp toàn quyền quản trị hệ thống (Tất cả phân hệ)")]
        public bool ToanQuyen { get; set; } = false;

        [Display(Name = "Danh sách phân hệ được cấp quyền")]
        public List<string> DanhSachQuyen { get; set; } = new List<string>();

        [Display(Name = "Trạng thái kích hoạt")]
        public bool TrangThaiHoatDong { get; set; } = true;
    }
}
