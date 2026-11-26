using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.BoDieuKhien
{
    /// <summary>
    /// Phân hệ Quản trị Cấu hình Thanh toán QR và Kênh chuyển khoản ngân hàng (Admin QR Payment Management).
    /// Cho phép thêm, sửa, xóa, bật/tắt các mẫu VietQR tự động và ảnh QR tùy chọn dùng cho Cửa hàng & Quyên góp.
    /// Hỗ trợ kiến trúc Dual-Mode (SQL Server online & In-Memory fallback khi offline).
    /// </summary>
    [Authorize]
    [PhanQuyenAdmin(QuyenAdmin.CauHinhQR)]
    public class AdminCauHinhQRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminCauHinhQRController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private bool BiChanQuyen() => User?.Identity?.IsAuthenticated == true && !User.CoQuyen(QuyenAdmin.CauHinhQR);

        private bool IsAjaxRequest()
        {
            return Request?.Headers != null &&
                   (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers["Accept"].ToString().Contains("application/json"));
        }

        // =========================================================================
        // 1. DANH SÁCH CẤU HÌNH QR (INDEX)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);
            List<CauHinhThanhToanQR> configs;

            try
            {
                if (_context.Database.CanConnect())
                {
                    configs = await _context.CauHinhThanhToanQRs
                        .OrderBy(q => q.ThuTu)
                        .ThenBy(q => q.IDCauHinhQR)
                        .ToListAsync();

                    // Nếu database mới và chưa có bản ghi, tự động seed cấu hình mặc định
                    if (!configs.Any())
                    {
                        var defaultQr = DbInitializer.GetDefaultQrConfig();
                        defaultQr.IDCauHinhQR = 0;
                        _context.CauHinhThanhToanQRs.Add(defaultQr);
                        await _context.SaveChangesAsync();

                        configs = await _context.CauHinhThanhToanQRs
                            .OrderBy(q => q.ThuTu)
                            .ThenBy(q => q.IDCauHinhQR)
                            .ToListAsync();
                    }
                }
                else
                {
                    configs = DbInitializer.GetInMemoryQrConfigs();
                }
            }
            catch
            {
                configs = DbInitializer.GetInMemoryQrConfigs();
            }

            return View(configs);
        }

        // =========================================================================
        // 2. THÊM MỚI CẤU HÌNH QR (CREATE)
        // =========================================================================
        [HttpGet]
        public IActionResult Create()
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);
            var model = new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản thanh toán mới",
                MaNganHang = "MB",
                TenNganHang = "Quân Đội (MBBank)",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                CuPhapChuyenKhoan = "{MADON}",
                MauVietQR = "compact2",
                SuDungVietQR = true,
                HuongDanThanhToan = "Mở ứng dụng ngân hàng hoặc ví điện tử bất kỳ (Vietcombank, MBBank, Techcombank, MoMo...) và Quét mã để thanh toán tự động",
                GhiChuThem = "Tài khoản chính thức của chiến dịch thiện nguyện Trăng Sáng Langbiang 2026.",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CauHinhThanhToanQR model, IFormFile? FileAnhQR)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);
            if (FileAnhQR != null && FileAnhQR.Length > 0)
            {
                try
                {
                    model.DuongDanAnhQRTuyChon = await SaveFileAsync(FileAnhQR, "qr");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lưu tệp ảnh QR: " + ex.Message);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.NgayTao = DateTime.Now;

            try
            {
                if (_context.Database.CanConnect())
                {
                    _context.CauHinhThanhToanQRs.Add(model);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    DbInitializer.AddInMemoryQrConfig(model);
                }
            }
            catch
            {
                DbInitializer.AddInMemoryQrConfig(model);
            }

            TempData["Success"] = $"Đã thêm cấu hình thanh toán QR \"{model.TenCauHinh}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 3. CHỈNH SỬA CẤU HÌNH QR (EDIT)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);
            CauHinhThanhToanQR? config = null;

            try
            {
                if (_context.Database.CanConnect())
                {
                    config = await _context.CauHinhThanhToanQRs.FindAsync(id);
                }
                else
                {
                    config = DbInitializer.GetInMemoryQrConfigById(id);
                }
            }
            catch
            {
                config = DbInitializer.GetInMemoryQrConfigById(id);
            }

            if (config == null)
            {
                TempData["Error"] = "Không tìm thấy cấu hình mã QR yêu cầu.";
                return RedirectToAction(nameof(Index));
            }

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CauHinhThanhToanQR model, IFormFile? FileAnhQR)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);
            if (id != model.IDCauHinhQR)
            {
                return BadRequest("Mã cấu hình không khớp.");
            }

            if (FileAnhQR != null && FileAnhQR.Length > 0)
            {
                try
                {
                    model.DuongDanAnhQRTuyChon = await SaveFileAsync(FileAnhQR, "qr");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Không thể tải lên tệp ảnh QR: " + ex.Message);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.NgayCapNhat = DateTime.Now;

            try
            {
                if (_context.Database.CanConnect())
                {
                    var existing = await _context.CauHinhThanhToanQRs.FindAsync(id);
                    if (existing == null)
                    {
                        TempData["Error"] = "Cấu hình QR không tồn tại trong cơ sở dữ liệu.";
                        return RedirectToAction(nameof(Index));
                    }

                    existing.TenCauHinh = model.TenCauHinh;
                    existing.MaNganHang = model.MaNganHang;
                    existing.TenNganHang = model.TenNganHang;
                    existing.SoTaiKhoan = model.SoTaiKhoan;
                    existing.TenChuTaiKhoan = model.TenChuTaiKhoan;
                    existing.CuPhapChuyenKhoan = model.CuPhapChuyenKhoan;
                    existing.MauVietQR = model.MauVietQR;
                    existing.SuDungVietQR = model.SuDungVietQR;
                    existing.HuongDanThanhToan = model.HuongDanThanhToan;
                    existing.GhiChuThem = model.GhiChuThem;
                    existing.ApDungChoCuaHang = model.ApDungChoCuaHang;
                    existing.ApDungChoQuyenGop = model.ApDungChoQuyenGop;
                    existing.TrangThaiHoatDong = model.TrangThaiHoatDong;
                    existing.ThuTu = model.ThuTu;
                    existing.NgayCapNhat = DateTime.Now;

                    if (!string.IsNullOrEmpty(model.DuongDanAnhQRTuyChon))
                    {
                        existing.DuongDanAnhQRTuyChon = model.DuongDanAnhQRTuyChon;
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    DbInitializer.UpdateInMemoryQrConfig(model);
                }
            }
            catch
            {
                DbInitializer.UpdateInMemoryQrConfig(model);
            }

            TempData["Success"] = $"Đã cập nhật cấu hình thanh toán QR \"{model.TenCauHinh}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 4. BẬT / TẮT TRẠNG THÁI HOẠT ĐỘNG (TOGGLE ACTIVE)
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);
            bool newStatus = false;
            string configName = $"#{id}";

            try
            {
                if (_context.Database.CanConnect())
                {
                    var config = await _context.CauHinhThanhToanQRs.FindAsync(id);
                    if (config != null)
                    {
                        config.TrangThaiHoatDong = !config.TrangThaiHoatDong;
                        config.NgayCapNhat = DateTime.Now;
                        newStatus = config.TrangThaiHoatDong;
                        configName = config.TenCauHinh;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var config = DbInitializer.GetInMemoryQrConfigById(id);
                    if (config != null)
                    {
                        configName = config.TenCauHinh;
                        DbInitializer.ToggleActiveInMemoryQrConfig(id);
                        newStatus = !config.TrangThaiHoatDong;
                    }
                }
            }
            catch
            {
                var config = DbInitializer.GetInMemoryQrConfigById(id);
                if (config != null)
                {
                    configName = config.TenCauHinh;
                    DbInitializer.ToggleActiveInMemoryQrConfig(id);
                    newStatus = !config.TrangThaiHoatDong;
                }
            }

            if (IsAjaxRequest())
            {
                return Json(new { success = true, isActive = newStatus, message = $"Đã {(newStatus ? "kích hoạt" : "vô hiệu hóa")} cấu hình \"{configName}\"" });
            }

            TempData["Success"] = $"Đã {(newStatus ? "kích hoạt" : "tắt")} cấu hình \"{configName}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 5. XÓA CẤU HÌNH QR (DELETE)
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);
            string configName = $"#{id}";

            try
            {
                if (_context.Database.CanConnect())
                {
                    var config = await _context.CauHinhThanhToanQRs.FindAsync(id);
                    if (config != null)
                    {
                        configName = config.TenCauHinh;
                        _context.CauHinhThanhToanQRs.Remove(config);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var config = DbInitializer.GetInMemoryQrConfigById(id);
                    if (config != null) configName = config.TenCauHinh;
                    DbInitializer.DeleteInMemoryQrConfig(id);
                }
            }
            catch
            {
                var config = DbInitializer.GetInMemoryQrConfigById(id);
                if (config != null) configName = config.TenCauHinh;
                DbInitializer.DeleteInMemoryQrConfig(id);
            }

            TempData["Success"] = $"Đã xóa cấu hình thanh toán QR \"{configName}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 6. TIỆN ÍCH LƯU TỆP HÌNH ẢNH (UPLOAD HELPER)
        // =========================================================================
        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var webRoot = _env?.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env?.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
            }

            string uploadsFolder = Path.Combine(webRoot, "uploads", subFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uniqueName = $"{Guid.NewGuid():N}{ext}";
            string filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{subFolder}/{uniqueName}";
        }
    }
}
