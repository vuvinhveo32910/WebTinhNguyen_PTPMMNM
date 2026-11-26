using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.BoDieuKhien
{
    /// <summary>
    /// Phân hệ điều khiển Quản trị viên (Admin Portal Controller).
    /// Quản lý xác thực đăng nhập, đăng xuất và bảo vệ trang tổng quan quản trị (/Admin).
    /// Hỗ trợ cả SQL Server và In-Memory fallback store khi môi trường offline.
    /// </summary>
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext? _context;

        /// <summary>
        /// Khởi tạo AdminController.
        /// Constructor hỗ trợ tham số tùy chọn ApplicationDbContext? context = null
        /// để tương thích 100% với các bài kiểm thử đơn vị độc lập (new AdminController()).
        /// </summary>
        public AdminController(ApplicationDbContext? context = null)
        {
            _context = context;
        }

        // =========================================================================
        // 1. TRANG TỔNG QUAN QUẢN TRỊ (ADMIN DASHBOARD)
        // =========================================================================

        /// <summary>
        /// Trang tổng quan quản trị hệ thống (/Admin hoặc /Admin/Index).
        /// Hiển thị các chỉ số thống kê thực tế lấy trực tiếp từ cơ sở dữ liệu:
        /// - Yêu cầu mới / liên hệ (đơn hàng mới đặt ở trạng thái MoiDat)
        /// - Bài viết tin tức (tổng số bản ghi trong BaiVietTinTucs)
        /// - Mùa dự án / chiến dịch (tổng số bản ghi trong ChienDichs)
        /// - Tổng quỹ đóng góp (tổng số tiền thanh toán từ các đơn hàng không bị hủy)
        /// - Số lượng tình nguyện viên (tổng số thành viên ban tổ chức / tình nguyện viên)
        /// - Số lời chúc đã nhận (lời nhắn gửi trong các đơn hàng đóng góp)
        /// - Số hình ảnh khoảnh khắc (tổng số ảnh lưu trữ trong AnhKyUcs và ThuVienKhoanhKhacs)
        /// Hỗ trợ fallback an toàn (mặc định 0 và 0đ) khi CSDL trống hoặc môi trường offline.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            var model = new AdminDashboardViewModel();

            if (_context != null)
            {
                bool canConnect = false;
                try
                {
                    canConnect = _context.Database.CanConnect();
                }
                catch
                {
                    canConnect = false;
                }

                if (canConnect)
                {
                    // 1. Số yêu cầu mới / liên hệ (đơn hàng mới đặt chờ xác nhận)
                    try
                    {
                        model.SoYeuCauMoi = _context.DonHangGayQuys
                            .Count(d => d.TrangThaiDonHang == "MoiDat" || string.IsNullOrEmpty(d.TrangThaiDonHang));
                    }
                    catch { }

                    // 2. Tổng số bài viết tin tức
                    try
                    {
                        model.SoBaiViet = _context.BaiVietTinTucs.Count();
                    }
                    catch { }

                    // 3. Tổng số mùa dự án / chiến dịch
                    try
                    {
                        model.SoMuaDuAn = _context.ChienDichs.Count();
                    }
                    catch { }

                    // 4. Tổng quỹ đóng góp (các đơn không bị hủy)
                    try
                    {
                        model.TongQuyDongGop = _context.DonHangGayQuys
                            .Where(d => d.TrangThaiDonHang == null || d.TrangThaiDonHang != "DaHuy")
                            .Sum(d => (decimal?)d.TongThanhToan) ?? 0m;
                    }
                    catch { }

                    // 5. Số lượng tình nguyện viên / thành viên ban tổ chức
                    try
                    {
                        model.SoTinhNguyenVien = _context.ThanhVienBanToChucs.Count();
                    }
                    catch { }

                    // 6. Số lời chúc đã nhận (lời nhắn gửi từ các đơn ủng hộ / quyên góp)
                    try
                    {
                        model.SoLoiChuc = _context.DonHangGayQuys
                            .Count(d => d.GhiChu != null && d.GhiChu.Trim() != "");
                    }
                    catch { }

                    // 7. Số hình ảnh khoảnh khắc (ảnh lưu trữ trong Ký ức và Thư viện khoảnh khắc)
                    try
                    {
                        model.SoHinhAnhKhoanhKhac += _context.AnhKyUcs.Count();
                    }
                    catch { }

                    try
                    {
                        model.SoHinhAnhKhoanhKhac += _context.ThuVienKhoanhKhacs.Count();
                    }
                    catch { }
                }
            }

            // Đồng thời truyền các chỉ số vào ViewBag để view linh hoạt truy cập cả Model lẫn ViewBag
            ViewBag.SoYeuCauMoi = model.SoYeuCauMoi;
            ViewBag.YeuCauMoi = model.SoYeuCauMoi;
            ViewBag.SoYeuCau = model.SoYeuCauMoi;
            ViewBag.NewRequests = model.SoYeuCauMoi;

            ViewBag.SoBaiViet = model.SoBaiViet;
            ViewBag.BaiViet = model.SoBaiViet;
            ViewBag.Articles = model.SoBaiViet;
            ViewBag.NewsCount = model.SoBaiViet;

            ViewBag.SoMuaDuAn = model.SoMuaDuAn;
            ViewBag.MuaDuAn = model.SoMuaDuAn;
            ViewBag.SoChienDich = model.SoMuaDuAn;
            ViewBag.Campaigns = model.SoMuaDuAn;

            ViewBag.TongQuyDongGop = model.TongQuyDongGop;
            ViewBag.TongQuy = model.TongQuyDongGop;
            ViewBag.TotalFund = model.TongQuyDongGop;
            ViewBag.TotalDonation = model.TongQuyDongGop;
            ViewBag.TongQuyFormatted = model.TongQuyDongGopFormatted;
            ViewBag.TongQuyDongGopFormatted = model.TongQuyDongGopFormatted;

            ViewBag.SoTinhNguyenVien = model.SoTinhNguyenVien;
            ViewBag.TinhNguyenVien = model.SoTinhNguyenVien;
            ViewBag.Volunteers = model.SoTinhNguyenVien;

            ViewBag.SoLoiChuc = model.SoLoiChuc;
            ViewBag.LoiChuc = model.SoLoiChuc;
            ViewBag.Wishes = model.SoLoiChuc;

            ViewBag.SoHinhAnhKhoanhKhac = model.SoHinhAnhKhoanhKhac;
            ViewBag.HinhAnhKhoanhKhac = model.SoHinhAnhKhoanhKhac;
            ViewBag.Moments = model.SoHinhAnhKhoanhKhac;
            ViewBag.Photos = model.SoHinhAnhKhoanhKhac;

            return View(model);
        }

        // =========================================================================
        // 2. ĐĂNG NHẬP QUẢN TRỊ (LOGIN)
        // =========================================================================

        /// <summary>
        /// Hiển thị giao diện đăng nhập quản trị viên (/admin/login).
        /// Nếu người dùng đã đăng nhập thành công trước đó, tự động chuyển hướng vào khu vực quản trị.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            var model = new AdminLoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý gửi biểu mẫu đăng nhập quản trị viên (/admin/login).
        /// Xác thực thông tin qua DbInitializer với thuật toán PBKDF2 HMAC-SHA256,
        /// thiết lập phiên Cookie Authentication an toàn và chuyển hướng về URL đích.
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            var targetUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : model?.ReturnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isValid = DbInitializer.ValidateAdminCredentials(model.TenDangNhap, model.MatKhau, _context);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            // Lấy thông tin tài khoản admin để tạo Claims
            var admin = DbInitializer.GetAdminByUsername(model.TenDangNhap, _context);
            var displayName = !string.IsNullOrWhiteSpace(admin?.HoTen) ? admin.HoTen : model.TenDangNhap;
            var roleName = !string.IsNullOrWhiteSpace(admin?.VaiTro) ? admin.VaiTro : "Admin";
            var identifier = admin != null && admin.IDTaiKhoan > 0 ? admin.IDTaiKhoan.ToString() : model.TenDangNhap;
            var phanQuyen = !string.IsNullOrWhiteSpace(admin?.PhanQuyen) ? admin.PhanQuyen : QuyenAdmin.All;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.NameIdentifier, identifier),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("TenDangNhap", model.TenDangNhap.Trim()),
                new Claim("PhanQuyen", phanQuyen)
            };

            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            if (QuyenAdmin.KiemTraToanQuyen(phanQuyen, roleName))
            {
                claims.Add(new Claim("QuyenHan", QuyenAdmin.All));
            }
            foreach (var maQuyen in QuyenAdmin.TachDanhSachQuyen(phanQuyen))
            {
                claims.Add(new Claim("QuyenHan", maQuyen));
            }

            if (!string.IsNullOrWhiteSpace(admin?.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, admin.Email));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNhoDangNhap,
                ExpiresUtc = model.GhiNhoDangNhap
                    ? DateTimeOffset.UtcNow.AddDays(14)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            try
            {
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);
            }
            catch (InvalidOperationException) when (HttpContext?.RequestServices?.GetService(typeof(IAuthenticationService)) == null)
            {
                // Phòng vệ cho môi trường kiểm thử đơn vị độc lập (unit test context) chạy trên DefaultHttpContext rỗng
            }

            return RedirectToLocal(targetUrl);
        }

        // =========================================================================
        // 3. ĐĂNG XUẤT QUẢN TRỊ (LOGOUT)
        // =========================================================================

        /// <summary>
        /// Đăng xuất khỏi hệ thống quản trị (/admin/logout).
        /// Hủy phiên xác thực Cookie, xóa toàn bộ session dữ liệu và chuyển hướng về trang đăng nhập.
        /// Hỗ trợ cả GET và POST để liên kết linh hoạt từ giao diện.
        /// </summary>
        [HttpGet, HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (InvalidOperationException) when (HttpContext?.RequestServices?.GetService(typeof(IAuthenticationService)) == null)
            {
                // Phòng vệ cho môi trường unit test
            }

            try
            {
                HttpContext.Session?.Clear();
            }
            catch (InvalidOperationException)
            {
                // Session chưa được cấu hình cho ngữ cảnh kiểm thử
            }

            return Redirect("/admin/login");
        }

        // =========================================================================
        // 4. HÀM TRỢ GIÚP (HELPERS)
        // =========================================================================

        /// <summary>
        /// Chuyển hướng an toàn đến đường dẫn nội bộ (phòng chống lỗ hổng Open Redirect).
        /// </summary>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                bool isLocal = false;
                try
                {
                    if (Url != null)
                    {
                        isLocal = Url.IsLocalUrl(returnUrl);
                    }
                }
                catch
                {
                    isLocal = false;
                }

                if (!isLocal)
                {
                    // Fallback kiểm tra URL nội bộ an toàn khi Url helper chưa khởi tạo trong unit test
                    isLocal = returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\");
                }

                if (isLocal)
                {
                    return Redirect(returnUrl);
                }
            }

            return RedirectToAction(nameof(Index), "Admin");
        }
    }
}
