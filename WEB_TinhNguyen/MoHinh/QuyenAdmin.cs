using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;

namespace WEB_TinhNguyen.MoHinh
{
    /// <summary>
    /// Mô tả thông tin chi tiết một phân hệ chức năng trong khu vực Quản trị (/admin).
    /// Dùng để hiển thị danh sách checkbox phân quyền và điều hướng động.
    /// </summary>
    public class ThongTinPhanHeQuyen
    {
        public string MaPhanHe { get; set; } = string.Empty;
        public string TenHienThi { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string Icon => IconClass;
        public string ControllerName { get; set; } = string.Empty;
        public string UrlPath { get; set; } = string.Empty;
        public string NhomPhanHe { get; set; } = "Nội dung";
    }

    /// <summary>
    /// Định nghĩa mã phân quyền chuẩn cho toàn bộ 11 phân hệ quản trị trong hệ thống.
    /// </summary>
    public static class QuyenAdmin
    {
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

        public const string All = "ALL";
        public const string None = "NONE";
        public const string SuperAdmin = "SuperAdmin";

        /// <summary>
        /// Danh sách tất cả 11 mã phân hệ hợp lệ trong hệ thống.
        /// </summary>
        public static readonly IReadOnlyList<string> TatCaMaPhanHe = new List<string>
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
        /// Danh sách thông tin chi tiết 11 phân hệ quản trị phục vụ giao diện Admin và kiểm tra phân quyền.
        /// </summary>
        public static readonly IReadOnlyList<ThongTinPhanHeQuyen> DanhSachPhanHe = new List<ThongTinPhanHeQuyen>
        {
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = TrangChu,
                TenHienThi = "Cấu hình Trang chủ",
                MoTa = "Quản lý banner, đếm ngược, thông điệp chiến dịch trên trang chủ",
                IconClass = "fas fa-home",
                ControllerName = "AdminTrangChu",
                UrlPath = "/AdminTrangChu",
                NhomPhanHe = "Nội dung Chiến dịch"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = ChuongTrinh,
                TenHienThi = "Cấu hình Chương trình",
                MoTa = "Quản lý chặng hành trình và chi tiết lộ trình tình nguyện",
                IconClass = "fas fa-calendar-alt",
                ControllerName = "AdminChuongTrinh",
                UrlPath = "/AdminChuongTrinh",
                NhomPhanHe = "Nội dung Chiến dịch"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = GayQuy,
                TenHienThi = "Cấu hình Đóng Góp",
                MoTa = "Quản lý nội dung kêu gọi đóng góp, kênh gây quỹ và nhà tài trợ",
                IconClass = "fas fa-hand-holding-heart",
                ControllerName = "AdminGayQuy",
                UrlPath = "/AdminGayQuy",
                NhomPhanHe = "Nội dung Chiến dịch"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = CauHinhQR,
                TenHienThi = "Cấu hình Thanh toán QR",
                MoTa = "Thêm, sửa, xóa tài khoản ngân hàng và mã QR VietQR thanh toán",
                IconClass = "fas fa-qrcode",
                ControllerName = "AdminCauHinhQR",
                UrlPath = "/AdminCauHinhQR",
                NhomPhanHe = "Cửa hàng & Tài chính"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = KyUc,
                TenHienThi = "Cấu hình Ký ức",
                MoTa = "Quản lý thư viện hình ảnh khoảnh khắc và tổng kết các mùa dự án",
                IconClass = "fas fa-images",
                ControllerName = "AdminKyUc",
                UrlPath = "/AdminKyUc",
                NhomPhanHe = "Nội dung Chiến dịch"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = BanToChuc,
                TenHienThi = "Cấu hình Ban Tổ Chức",
                MoTa = "Quản lý danh sách thành viên và cơ cấu Ban tổ chức",
                IconClass = "fas fa-sitemap",
                ControllerName = "AdminBanToChuc",
                UrlPath = "/AdminBanToChuc",
                NhomPhanHe = "Nội dung Chiến dịch"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = TinTuc,
                TenHienThi = "Cấu hình Tin tức",
                MoTa = "Đăng tải, chỉnh sửa và quản lý các bài viết tin tức",
                IconClass = "fas fa-newspaper",
                ControllerName = "AdminTinTuc",
                UrlPath = "/AdminTinTuc",
                NhomPhanHe = "Nội dung Chiến dịch"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = HieuUng,
                TenHienThi = "Thư viện Hiệu ứng",
                MoTa = "Quản lý và tải lên các tệp hiệu ứng giao diện (.js)",
                IconClass = "fas fa-magic",
                ControllerName = "AdminHieuUng",
                UrlPath = "/AdminHieuUng",
                NhomPhanHe = "Hệ thống & Quản trị"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = SanPham,
                TenHienThi = "Sản phẩm Gây Quỹ",
                MoTa = "Thêm, sửa, xóa sản phẩm và quản lý tồn kho cửa hàng gây quỹ",
                IconClass = "fas fa-boxes",
                ControllerName = "AdminSanPham",
                UrlPath = "/CuaHang/AdminSanPham",
                NhomPhanHe = "Cửa hàng & Tài chính"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = DonHang,
                TenHienThi = "Đơn hàng Gây Quỹ",
                MoTa = "Tra cứu, xác nhận thanh toán và cập nhật trạng thái đơn hàng",
                IconClass = "fas fa-receipt",
                ControllerName = "AdminDonHang",
                UrlPath = "/CuaHang/AdminDonHang",
                NhomPhanHe = "Cửa hàng & Tài chính"
            },
            new ThongTinPhanHeQuyen
            {
                MaPhanHe = QuanLyTaiKhoan,
                TenHienThi = "Quản lý Tài khoản Admin",
                MoTa = "Thêm tài khoản quản trị viên mới và phân quyền chức năng chi tiết",
                IconClass = "fas fa-user-shield",
                ControllerName = "AdminTaiKhoan",
                UrlPath = "/AdminTaiKhoan",
                NhomPhanHe = "Hệ thống & Quản trị"
            }
        };

        /// <summary>
        /// Chuẩn hóa mã phân hệ (hỗ trợ cả trường hợp truyền tên Controller như "AdminTrangChu" -> "TrangChu").
        /// </summary>
        public static string ChuanHoaMaPhanHe(string? maPhanHe)
        {
            if (string.IsNullOrWhiteSpace(maPhanHe))
            {
                return string.Empty;
            }

            string trimmed = maPhanHe.Trim();
            if (string.Equals(trimmed, All, StringComparison.OrdinalIgnoreCase) || trimmed == "*")
            {
                return All;
            }
            if (string.Equals(trimmed, None, StringComparison.OrdinalIgnoreCase))
            {
                return None;
            }

            if (string.Equals(trimmed, "AdminTaiKhoan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "AdminTaiKhoanController", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "TaiKhoan", StringComparison.OrdinalIgnoreCase))
            {
                return QuanLyTaiKhoan;
            }

            if (trimmed.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - "Controller".Length);
            }

            if (trimmed.StartsWith("Admin", StringComparison.OrdinalIgnoreCase) && trimmed.Length > 5)
            {
                string withoutAdmin = trimmed.Substring(5);
                var matchedByWithoutAdmin = TatCaMaPhanHe.FirstOrDefault(m =>
                    string.Equals(m, withoutAdmin, StringComparison.OrdinalIgnoreCase));
                if (matchedByWithoutAdmin != null)
                {
                    return matchedByWithoutAdmin;
                }
            }

            var matched = TatCaMaPhanHe.FirstOrDefault(m =>
                string.Equals(m, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? trimmed;
        }

        /// <summary>
        /// Tách chuỗi phân quyền (dạng "TrangChu,TinTuc" hoặc "ALL") thành danh sách các mã phân hệ chuẩn.
        /// </summary>
        public static List<string> TachDanhSachQuyen(string? phanQuyen)
        {
            if (string.IsNullOrWhiteSpace(phanQuyen))
            {
                return new List<string>();
            }

            var tokens = phanQuyen
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => ChuanHoaMaPhanHe(t))
                .Where(t => !string.IsNullOrWhiteSpace(t) && !string.Equals(t, None, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (tokens.Any(t => string.Equals(t, All, StringComparison.OrdinalIgnoreCase)))
            {
                return TatCaMaPhanHe.ToList();
            }

            return tokens;
        }

        /// <summary>
        /// Kiểm tra xem chuỗi phân quyền hoặc vai trò có phải là toàn quyền hệ thống ("ALL" / "SuperAdmin") hay không.
        /// </summary>
        public static bool KiemTraToanQuyen(string? phanQuyen, string? vaiTro = null)
        {
            if (string.Equals(vaiTro?.Trim(), SuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(phanQuyen))
            {
                return false;
            }

            var tokens = phanQuyen
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim());

            return tokens.Any(t =>
                string.Equals(t, All, StringComparison.OrdinalIgnoreCase) ||
                t == "*" ||
                string.Equals(t, SuperAdmin, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Kiểm tra xem chuỗi phân quyền có cấp quyền cho phân hệ <paramref name="maPhanHe"/> hay không.
        /// </summary>
        public static bool KiemTraQuyenChuoi(string? phanQuyen, string? maPhanHe, string? vaiTro = null)
        {
            if (string.IsNullOrWhiteSpace(maPhanHe))
            {
                return true;
            }

            if (KiemTraToanQuyen(phanQuyen, vaiTro))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(phanQuyen) ||
                string.Equals(phanQuyen.Trim(), None, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string target = ChuanHoaMaPhanHe(maPhanHe);
            if (string.Equals(target, All, StringComparison.OrdinalIgnoreCase))
            {
                return KiemTraToanQuyen(phanQuyen, vaiTro);
            }

            var grantedList = TachDanhSachQuyen(phanQuyen);
            return grantedList.Any(q => string.Equals(q, target, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Chuẩn hóa danh sách quyền được chọn từ giao diện thành chuỗi lưu trữ CSDL ("ALL", "TrangChu,TinTuc", hoặc "NONE").
        /// </summary>
        public static string ChuanHoaChuoiPhanQuyen(IEnumerable<string>? danhSachQuyen, bool toanQuyen = false)
        {
            if (toanQuyen)
            {
                return All;
            }

            if (danhSachQuyen == null)
            {
                return None;
            }

            var normalized = danhSachQuyen
                .Where(q => !string.IsNullOrWhiteSpace(q))
                .Select(q => ChuanHoaMaPhanHe(q))
                .Where(q => !string.IsNullOrWhiteSpace(q) && !string.Equals(q, None, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Any(q => string.Equals(q, All, StringComparison.OrdinalIgnoreCase)) ||
                normalized.Count >= TatCaMaPhanHe.Count && TatCaMaPhanHe.All(m => normalized.Contains(m, StringComparer.OrdinalIgnoreCase)))
            {
                return All;
            }

            if (normalized.Count == 0)
            {
                return None;
            }

            return string.Join(",", normalized);
        }

        /// <summary>
        /// Lấy tên hiển thị tiếng Việt của một mã phân hệ.
        /// </summary>
        public static string LayTenHienThi(string? maPhanHe)
        {
            if (string.IsNullOrWhiteSpace(maPhanHe)) return string.Empty;
            if (string.Equals(maPhanHe.Trim(), All, StringComparison.OrdinalIgnoreCase))
            {
                return "Toàn quyền hệ thống";
            }

            string target = ChuanHoaMaPhanHe(maPhanHe);
            var info = DanhSachPhanHe.FirstOrDefault(p =>
                string.Equals(p.MaPhanHe, target, StringComparison.OrdinalIgnoreCase));
            return info?.TenHienThi ?? target;
        }
    }

    /// <summary>
    /// Các phương thức mở rộng (Extension Methods) cho <see cref="ClaimsPrincipal"/>
    /// giúp kiểm tra quyền hạn trực tiếp trong Controller và Razor Views (`@if (User.CoQuyen(...))`).
    /// </summary>
    public static class PhanQuyenHelper
    {
        /// <summary>
        /// Kiểm tra người dùng hiện tại có quyền truy cập vào phân hệ <paramref name="maPhanHe"/> hay không.
        /// </summary>
        public static bool CoQuyen(this ClaimsPrincipal? user, string? maPhanHe)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            // Trang tổng quan /Admin chung cho mọi tài khoản quản trị đã đăng nhập
            if (string.IsNullOrWhiteSpace(maPhanHe) ||
                string.Equals(maPhanHe.Trim(), "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(maPhanHe.Trim(), "AdminController", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string target = QuyenAdmin.ChuanHoaMaPhanHe(maPhanHe);

            // 1. Kiểm tra nếu người dùng có claim PhanQuyen hoặc QuyenHan tường minh
            var phanQuyenClaims = user.FindAll("PhanQuyen").ToList();
            var quyenHanClaims = user.FindAll("QuyenHan").ToList();

            if (phanQuyenClaims.Count > 0 || quyenHanClaims.Count > 0)
            {
                foreach (var claim in phanQuyenClaims)
                {
                    if (QuyenAdmin.KiemTraQuyenChuoi(claim.Value, target))
                    {
                        return true;
                    }
                }

                foreach (var claim in quyenHanClaims)
                {
                    if (QuyenAdmin.KiemTraQuyenChuoi(claim.Value, target))
                    {
                        return true;
                    }
                }

                return false;
            }

            // 2. Nếu principal có claim TenDangNhap, đối chiếu với kho tài khoản trong DbInitializer
            var username = user.FindFirst("TenDangNhap")?.Value ?? user.Identity.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var admin = DbInitializer.GetAdminByUsername(username);
                if (admin != null)
                {
                    if (!admin.TrangThaiHoatDong)
                    {
                        return false;
                    }
                    return QuyenAdmin.KiemTraQuyenChuoi(admin.PhanQuyen, target, admin.VaiTro);
                }
            }

            // 3. Kiểm tra theo Role Claim (SuperAdmin hoặc Admin truyền thống trong unit test)
            if (user.IsInRole(QuyenAdmin.SuperAdmin))
            {
                return true;
            }

            if (user.IsInRole("Admin") && !user.IsInRole("SubAdmin"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra người dùng hiện tại có toàn quyền quản trị hệ thống hay không.
        /// </summary>
        public static bool LaToanQuyen(this ClaimsPrincipal? user)
        {
            return user.CoQuyen(QuyenAdmin.All);
        }
    }
}
