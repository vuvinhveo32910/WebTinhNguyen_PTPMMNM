using System.Globalization;

namespace WEB_TinhNguyen.MoHinh
{
    /// <summary>
    /// ViewModel chứa các số liệu thống kê tổng quan thực tế cho Bảng điều khiển quản trị (Admin Dashboard).
    /// Hỗ trợ cả định dạng số nguyên và chuỗi tiền tệ chuẩn Việt Nam, an toàn khi dữ liệu trống.
    /// </summary>
    public class AdminDashboardViewModel
    {
        private static readonly CultureInfo ViCulture = new CultureInfo("vi-VN");

        /// <summary>
        /// Số yêu cầu mới / liên hệ (ví dụ: các đơn hàng / đăng ký quyên góp mới ở trạng thái "MoiDat").
        /// </summary>
        public int SoYeuCauMoi { get; set; } = 0;
        public int YeuCauMoi { get => SoYeuCauMoi; set => SoYeuCauMoi = value; }
        public int SoYeuCau { get => SoYeuCauMoi; set => SoYeuCauMoi = value; }
        public int NewRequests { get => SoYeuCauMoi; set => SoYeuCauMoi = value; }

        /// <summary>
        /// Tổng số bài viết tin tức đã đăng tải trên hệ thống.
        /// </summary>
        public int SoBaiViet { get; set; } = 0;
        public int BaiViet { get => SoBaiViet; set => SoBaiViet = value; }
        public int Articles { get => SoBaiViet; set => SoBaiViet = value; }
        public int NewsCount { get => SoBaiViet; set => SoBaiViet = value; }

        /// <summary>
        /// Tổng số mùa dự án / chiến dịch tình nguyện đã và đang triển khai.
        /// </summary>
        public int SoMuaDuAn { get; set; } = 0;
        public int MuaDuAn { get => SoMuaDuAn; set => SoMuaDuAn = value; }
        public int SoChienDich { get => SoMuaDuAn; set => SoMuaDuAn = value; }
        public int Campaigns { get => SoMuaDuAn; set => SoMuaDuAn = value; }

        /// <summary>
        /// Tổng số tiền quỹ thiện nguyện được đóng góp qua các kênh / đơn hàng (đơn không bị hủy).
        /// </summary>
        public decimal TongQuyDongGop { get; set; } = 0m;
        public decimal TongQuy { get => TongQuyDongGop; set => TongQuyDongGop = value; }
        public decimal TotalFund { get => TongQuyDongGop; set => TongQuyDongGop = value; }
        public decimal TotalDonation { get => TongQuyDongGop; set => TongQuyDongGop = value; }

        /// <summary>
        /// Chuỗi định dạng tiền tệ của tổng quỹ đóng góp (ví dụ: "125.500.000đ" hoặc mặc định "0đ").
        /// </summary>
        public string TongQuyDongGopFormatted => TongQuyDongGop.ToString("N0", ViCulture) + "đ";
        public string TongQuyFormatted => TongQuyDongGopFormatted;

        /// <summary>
        /// Tổng số lượng tình nguyện viên / thành viên ban tổ chức đã đăng ký tham gia.
        /// </summary>
        public int SoTinhNguyenVien { get; set; } = 0;
        public int TinhNguyenVien { get => SoTinhNguyenVien; set => SoTinhNguyenVien = value; }
        public int Volunteers { get => SoTinhNguyenVien; set => SoTinhNguyenVien = value; }

        /// <summary>
        /// Tổng số lời chúc, lời nhắn gửi yêu thương đã nhận được từ cộng đồng và các nhà hảo tâm.
        /// </summary>
        public int SoLoiChuc { get; set; } = 0;
        public int LoiChuc { get => SoLoiChuc; set => SoLoiChuc = value; }
        public int Wishes { get => SoLoiChuc; set => SoLoiChuc = value; }

        /// <summary>
        /// Tổng số hình ảnh khoảnh khắc đã lưu trữ (trong thư viện khoảnh khắc và ký ức).
        /// </summary>
        public int SoHinhAnhKhoanhKhac { get; set; } = 0;
        public int HinhAnhKhoanhKhac { get => SoHinhAnhKhoanhKhac; set => SoHinhAnhKhoanhKhac = value; }
        public int Moments { get => SoHinhAnhKhoanhKhac; set => SoHinhAnhKhoanhKhac = value; }
        public int Photos { get => SoHinhAnhKhoanhKhac; set => SoHinhAnhKhoanhKhac = value; }
    }
}
