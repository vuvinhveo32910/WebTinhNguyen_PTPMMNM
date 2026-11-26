using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEB_TinhNguyen.BoDieuKhien;

namespace WEB_TinhNguyen.MoHinh
{
    /// <summary>
    /// Thực thể quản lý cấu hình mã QR và thông tin tài khoản chuyển khoản ngân hàng.
    /// Dùng chung cho phân hệ Cửa hàng gây quỹ và Kênh đóng góp thiện nguyện.
    /// Cho phép cấu hình động, tự sinh VietQR chuẩn Napas247 hoặc dùng ảnh tùy chọn.
    /// </summary>
    [Table("CauHinhThanhToanQR")]
    public class CauHinhThanhToanQR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã cấu hình")]
        public int IDCauHinhQR { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên cấu hình (ví dụ: Tài khoản chính MBBank)")]
        [StringLength(150, ErrorMessage = "Tên cấu hình không được vượt quá 150 ký tự")]
        [Display(Name = "Tên cấu hình")]
        public string TenCauHinh { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn hoặc nhập mã ngân hàng")]
        [StringLength(50, ErrorMessage = "Mã ngân hàng không được vượt quá 50 ký tự")]
        [Display(Name = "Mã ngân hàng")]
        public string MaNganHang { get; set; } = "MB";

        [Required(ErrorMessage = "Vui lòng nhập tên ngân hàng")]
        [StringLength(150, ErrorMessage = "Tên ngân hàng không được vượt quá 150 ký tự")]
        [Display(Name = "Tên ngân hàng")]
        public string TenNganHang { get; set; } = "Quân Đội (MBBank)";

        [Required(ErrorMessage = "Vui lòng nhập số tài khoản ngân hàng")]
        [StringLength(50, ErrorMessage = "Số tài khoản không được vượt quá 50 ký tự")]
        [Display(Name = "Số tài khoản")]
        public string SoTaiKhoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên chủ tài khoản")]
        [StringLength(150, ErrorMessage = "Tên chủ tài khoản không được vượt quá 150 ký tự")]
        [Display(Name = "Tên chủ tài khoản")]
        public string TenChuTaiKhoan { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Cú pháp chuyển khoản tối đa 255 ký tự")]
        [Display(Name = "Cú pháp chuyển khoản gợi ý")]
        public string? CuPhapChuyenKhoan { get; set; }

        [StringLength(50)]
        [Display(Name = "Mẫu VietQR")]
        public string MauVietQR { get; set; } = "compact2";

        [StringLength(1000)]
        [Display(Name = "Đường dẫn ảnh QR tùy chọn")]
        public string? DuongDanAnhQRTuyChon { get; set; }

        [Display(Name = "Sử dụng VietQR tự động")]
        public bool SuDungVietQR { get; set; } = true;

        [Display(Name = "Hướng dẫn thanh toán")]
        public string? HuongDanThanhToan { get; set; }

        [Display(Name = "Ghi chú thêm")]
        public string? GhiChuThem { get; set; }

        [Display(Name = "Áp dụng cho Cửa hàng")]
        public bool ApDungChoCuaHang { get; set; } = true;

        [Display(Name = "Áp dụng cho Quyên góp")]
        public bool ApDungChoQuyenGop { get; set; } = true;

        [Display(Name = "Trạng thái hoạt động")]
        public bool TrangThaiHoatDong { get; set; } = true;

        [Display(Name = "Thứ tự ưu tiên")]
        public int ThuTu { get; set; } = 1;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }

        /// <summary>
        /// Tạo hoặc lấy URL hình ảnh QR hiển thị theo cấu hình.
        /// Nếu SuDungVietQR = false và có ảnh tải lên tùy chọn -> trả về ảnh tùy chọn.
        /// Ngược lại -> sinh URL VietQR chuẩn Napas247 từ VietQrHelper.
        /// </summary>
        public string GetQrUrl(decimal? amount = null, string? memo = null, string? accountName = null)
        {
            if (!SuDungVietQR && !string.IsNullOrWhiteSpace(DuongDanAnhQRTuyChon))
            {
                return DuongDanAnhQRTuyChon;
            }

            var effectiveMemo = !string.IsNullOrWhiteSpace(memo)
                ? memo
                : CuPhapChuyenKhoan;

            var effectiveHolder = !string.IsNullOrWhiteSpace(accountName)
                ? accountName
                : TenChuTaiKhoan;

            return VietQrHelper.GenerateUrl(
                MaNganHang,
                SoTaiKhoan,
                amount,
                effectiveMemo,
                effectiveHolder,
                MauVietQR);
        }
    }
}
