using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.BoDieuKhien
{
    /// <summary>
    /// Phân hệ Quản trị Tài khoản Admin & Phân quyền chi tiết theo từng phân hệ (R1 & R2).
    /// Cho phép tài khoản có quyền QuanLyTaiKhoan (hoặc Toàn quyền hệ thống "ALL") tạo, sửa, bật/tắt, xóa
    /// tài khoản quản trị và cấp quyền tùy ý (1-2 phân hệ hoặc Toàn bộ cấu hình hệ thống).
    /// Hỗ trợ cơ chế Dual-Mode: đồng bộ SQL Server ([dbo].[TaiKhoanAdmin]) và In-Memory Store.
    /// </summary>
    [Authorize]
    [PhanQuyenAdmin(QuyenAdmin.QuanLyTaiKhoan)]
    public class AdminTaiKhoanController : Controller
    {
        private readonly ApplicationDbContext? _context;

        public AdminTaiKhoanController(ApplicationDbContext? context = null)
        {
            _context = context;
        }

        private bool BiChanQuyen() => User?.Identity?.IsAuthenticated == true && !User.CoQuyen(QuyenAdmin.QuanLyTaiKhoan);

        private void SetFlashMessage(string key, string message)
        {
            try
            {
                if (TempData != null)
                {
                    TempData[key] = message;
                }
            }
            catch
            {
                // Ignore if TempData provider is not configured in unit test harness
            }
        }

        // =========================================================================
        // 1. DANH SÁCH TÀI KHOẢN QUẢN TRỊ (INDEX)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);

            List<TaiKhoanAdmin> danhSach;
            try
            {
                if (_context != null && _context.Database.CanConnect())
                {
                    DbInitializer.SeedAdmin(_context);
                    danhSach = await _context.TaiKhoanAdmins
                        .OrderBy(a => a.IDTaiKhoan)
                        .ToListAsync();
                }
                else
                {
                    danhSach = DbInitializer.GetInMemoryAdmins();
                }
            }
            catch
            {
                danhSach = DbInitializer.GetInMemoryAdmins();
            }

            return View(danhSach);
        }

        // =========================================================================
        // 2. THÊM TÀI KHOẢN QUẢN TRỊ MỚI & PHÂN QUYỀN (CREATE GET & POST)
        // =========================================================================
        [HttpGet]
        public IActionResult Create()
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);

            var vm = new AdminTaiKhoanCreateViewModel
            {
                ToanQuyen = false,
                TrangThaiHoatDong = true,
                DanhSachQuyen = new List<string>()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminTaiKhoanCreateViewModel model)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);

            if (model == null)
            {
                return BadRequest();
            }

            var tenDangNhap = (model.TenDangNhap ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(tenDangNhap))
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), "Vui lòng nhập tên đăng nhập.");
            }
            else if (tenDangNhap.Length < 3)
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập phải có ít nhất 3 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(model.MatKhau) || model.MatKhau.Length < 6)
            {
                ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu phải có từ 6 ký tự trở lên.");
            }
            else if (!string.Equals(model.MatKhau, model.XacNhanMatKhau, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.XacNhanMatKhau), "Mật khẩu xác nhận không khớp.");
            }

            if (string.IsNullOrWhiteSpace(model.HoTen))
            {
                ModelState.AddModelError(nameof(model.HoTen), "Vui lòng nhập họ và tên quản trị viên.");
            }

            // Kiểm tra quyền được chọn
            var chuoiPhanQuyen = QuyenAdmin.ChuanHoaChuoiPhanQuyen(model.DanhSachQuyen, model.ToanQuyen);
            if (string.Equals(chuoiPhanQuyen, QuyenAdmin.None, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.DanhSachQuyen), "Vui lòng chọn ít nhất 1 phân hệ quản lý hoặc bật Toàn quyền hệ thống.");
            }

            // Kiểm tra trùng tên đăng nhập
            bool daTonTai = false;
            try
            {
                if (_context != null && _context.Database.CanConnect())
                {
                    var normalized = tenDangNhap.ToLowerInvariant();
                    daTonTai = await _context.TaiKhoanAdmins
                        .AnyAsync(a => a.TenDangNhap.ToLower() == normalized);
                }
            }
            catch
            {
                // Fallback kiểm tra In-Memory
            }

            if (!daTonTai)
            {
                var existingInMem = DbInitializer.GetInMemoryAdminByUsername(tenDangNhap);
                if (existingInMem != null)
                {
                    daTonTai = true;
                }
            }

            if (daTonTai)
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), $"Tên đăng nhập \"{tenDangNhap}\" đã tồn tại trong hệ thống.");
            }

            if (!ModelState.IsValid)
            {
                model.DanhSachQuyen ??= new List<string>();
                return View(model);
            }

            var salt = WEB_TinhNguyen.DuLieu.BaoMatMatKhau.TaoMuoiNgauNhien();
            var hash = WEB_TinhNguyen.DuLieu.BaoMatMatKhau.BamMatKhau(model.MatKhau, salt);

            var taiKhoanMoi = new TaiKhoanAdmin
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = hash,
                MuoiBaoMat = salt,
                HoTen = model.HoTen.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                VaiTro = string.Equals(chuoiPhanQuyen, QuyenAdmin.All, StringComparison.OrdinalIgnoreCase) ? "SuperAdmin" : "Admin",
                PhanQuyen = chuoiPhanQuyen,
                TrangThaiHoatDong = model.TrangThaiHoatDong,
                NgayTao = DateTime.Now
            };

            bool dbSaved = false;
            try
            {
                if (_context != null && _context.Database.CanConnect())
                {
                    _context.TaiKhoanAdmins.Add(taiKhoanMoi);
                    await _context.SaveChangesAsync();
                    dbSaved = true;
                }
            }
            catch
            {
                dbSaved = false;
            }

            // Luôn đồng bộ vào In-Memory Store để đảm bảo hoạt động nhất quán
            var existingMem = DbInitializer.GetInMemoryAdminByUsername(tenDangNhap);
            if (existingMem == null)
            {
                DbInitializer.AddInMemoryAdmin(taiKhoanMoi.Clone());
            }
            else if (!dbSaved)
            {
                taiKhoanMoi.IDTaiKhoan = existingMem.IDTaiKhoan;
                DbInitializer.UpdateInMemoryAdmin(taiKhoanMoi);
            }

            SetFlashMessage("Success", $"Đã tạo tài khoản quản trị \"{taiKhoanMoi.TenDangNhap}\" và phân quyền thành công!");
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 3. CHỈNH SỬA TÀI KHOẢN & CẬP NHẬT PHÂN QUYỀN (EDIT GET & POST)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);

            if (id <= 0)
            {
                SetFlashMessage("Error", "Mã tài khoản không hợp lệ.");
                return RedirectToAction(nameof(Index));
            }

            TaiKhoanAdmin? admin = null;
            try
            {
                if (_context != null && _context.Database.CanConnect())
                {
                    admin = await _context.TaiKhoanAdmins.FindAsync(id);
                }
            }
            catch
            {
                admin = null;
            }

            if (admin == null)
            {
                admin = DbInitializer.GetInMemoryAdminById(id);
            }

            if (admin == null)
            {
                SetFlashMessage("Error", $"Không tìm thấy tài khoản quản trị #{id}.");
                return RedirectToAction(nameof(Index));
            }

            var vm = new AdminTaiKhoanEditViewModel
            {
                IDTaiKhoan = admin.IDTaiKhoan,
                TenDangNhap = admin.TenDangNhap,
                HoTen = admin.HoTen,
                Email = admin.Email,
                ToanQuyen = admin.LaToanQuyen,
                DanhSachQuyen = admin.LayDanhSachQuyen(),
                TrangThaiHoatDong = admin.TrangThaiHoatDong
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminTaiKhoanEditViewModel model)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);

            if (model == null || id <= 0 || (model.IDTaiKhoan > 0 && id != model.IDTaiKhoan))
            {
                SetFlashMessage("Error", "Dữ liệu tài khoản cần cập nhật không hợp lệ.");
                return RedirectToAction(nameof(Index));
            }

            model.IDTaiKhoan = id;

            TaiKhoanAdmin? existingDb = null;
            try
            {
                if (_context != null && _context.Database.CanConnect())
                {
                    existingDb = await _context.TaiKhoanAdmins.FindAsync(id);
                }
            }
            catch
            {
                existingDb = null;
            }

            var existingMem = DbInitializer.GetInMemoryAdminById(id);
            var targetAccount = existingDb ?? existingMem;

            if (targetAccount == null)
            {
                SetFlashMessage("Error", $"Không tìm thấy tài khoản #{id} để cập nhật.");
                return RedirectToAction(nameof(Index));
            }

            model.TenDangNhap = targetAccount.TenDangNhap;
            bool isRootAdmin = string.Equals(targetAccount.TenDangNhap, "admin", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(model.HoTen))
            {
                ModelState.AddModelError(nameof(model.HoTen), "Vui lòng nhập họ và tên hiển thị.");
            }

            if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
            {
                if (model.MatKhauMoi.Length < 6)
                {
                    ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu mới phải có ít nhất 6 ký tự.");
                }
                else if (!string.Equals(model.MatKhauMoi, model.XacNhanMatKhauMoi, StringComparison.Ordinal))
                {
                    ModelState.AddModelError(nameof(model.XacNhanMatKhauMoi), "Mật khẩu xác nhận không khớp.");
                }
            }

            // Tài khoản gốc "admin" luôn giữ toàn quyền hệ thống và trạng thái hoạt động
            string chuoiPhanQuyen = isRootAdmin
                ? QuyenAdmin.All
                : QuyenAdmin.ChuanHoaChuoiPhanQuyen(model.DanhSachQuyen, model.ToanQuyen);

            if (!isRootAdmin && string.Equals(chuoiPhanQuyen, QuyenAdmin.None, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.DanhSachQuyen), "Vui lòng chọn ít nhất 1 phân hệ quản lý hoặc bật Toàn quyền hệ thống.");
            }

            if (!ModelState.IsValid)
            {
                model.DanhSachQuyen ??= new List<string>();
                return View(model);
            }

            string newSalt = targetAccount.MuoiBaoMat;
            string newHash = targetAccount.MatKhauHash;
            if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
            {
                newSalt = WEB_TinhNguyen.DuLieu.BaoMatMatKhau.TaoMuoiNgauNhien();
                newHash = WEB_TinhNguyen.DuLieu.BaoMatMatKhau.BamMatKhau(model.MatKhauMoi, newSalt);
            }

            string newHoTen = model.HoTen.Trim();
            string? newEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            bool newStatus = isRootAdmin ? true : model.TrangThaiHoatDong;
            string newVaiTro = isRootAdmin
                ? "Admin"
                : (string.Equals(chuoiPhanQuyen, QuyenAdmin.All, StringComparison.OrdinalIgnoreCase) ? "SuperAdmin" : "Admin");

            if (existingDb != null && _context != null)
            {
                try
                {
                    existingDb.HoTen = newHoTen;
                    existingDb.Email = newEmail;
                    existingDb.MatKhauHash = newHash;
                    existingDb.MuoiBaoMat = newSalt;
                    existingDb.VaiTro = newVaiTro;
                    existingDb.PhanQuyen = chuoiPhanQuyen;
                    existingDb.TrangThaiHoatDong = newStatus;
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Fallback In-Memory
                }
            }

            // Đồng bộ In-Memory Store
            var memToUpdate = existingMem ?? DbInitializer.GetInMemoryAdminByUsername(targetAccount.TenDangNhap);
            if (memToUpdate != null)
            {
                memToUpdate.HoTen = newHoTen;
                memToUpdate.Email = newEmail;
                memToUpdate.MatKhauHash = newHash;
                memToUpdate.MuoiBaoMat = newSalt;
                memToUpdate.VaiTro = newVaiTro;
                memToUpdate.PhanQuyen = chuoiPhanQuyen;
                memToUpdate.TrangThaiHoatDong = newStatus;
                DbInitializer.UpdateInMemoryAdmin(memToUpdate);
            }

            SetFlashMessage("Success", $"Đã cập nhật tài khoản \"{targetAccount.TenDangNhap}\" thành công!");
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 4. BẬT / TẮT TRẠNG THÁI HOẠT ĐỘNG (TOGGLE STATUS)
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);

            if (id <= 0)
            {
                SetFlashMessage("Error", "Mã tài khoản không hợp lệ.");
                return RedirectToAction(nameof(Index));
            }

            TaiKhoanAdmin? dbAccount = null;
            try
            {
                if (_context != null && _context.Database.CanConnect())
                {
                    dbAccount = await _context.TaiKhoanAdmins.FindAsync(id);
                }
            }
            catch
            {
                dbAccount = null;
            }

            var memAccount = DbInitializer.GetInMemoryAdminById(id);
            var target = dbAccount ?? memAccount;

            if (target == null)
            {
                SetFlashMessage("Error", $"Không tìm thấy tài khoản #{id}.");
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(target.TenDangNhap, "admin", StringComparison.OrdinalIgnoreCase))
            {
                SetFlashMessage("Error", "Không thể khóa tài khoản Quản trị viên tối cao (admin) mặc định của hệ thống.");
                return RedirectToAction(nameof(Index));
            }

            bool newStatus = !target.TrangThaiHoatDong;

            if (dbAccount != null && _context != null)
            {
                try
                {
                    dbAccount.TrangThaiHoatDong = newStatus;
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Fallback In-Memory
                }
            }

            if (memAccount != null)
            {
                memAccount.TrangThaiHoatDong = newStatus;
                DbInitializer.UpdateInMemoryAdmin(memAccount);
            }
            else
            {
                var byName = DbInitializer.GetInMemoryAdminByUsername(target.TenDangNhap);
                if (byName != null)
                {
                    byName.TrangThaiHoatDong = newStatus;
                    DbInitializer.UpdateInMemoryAdmin(byName);
                }
            }

            SetFlashMessage("Success", $"Đã {(newStatus ? "kích hoạt" : "khóa")} tài khoản \"{target.TenDangNhap}\" thành công!");
            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // 5. XÓA TÀI KHOẢN QUẢN TRỊ (DELETE POST)
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (BiChanQuyen()) return StatusCode(StatusCodes.Status403Forbidden);

            if (id <= 0)
            {
                SetFlashMessage("Error", "Mã tài khoản không hợp lệ.");
                return RedirectToAction(nameof(Index));
            }

            TaiKhoanAdmin? dbAccount = null;
            try
            {
                if (_context != null && _context.Database.CanConnect())
                {
                    dbAccount = await _context.TaiKhoanAdmins.FindAsync(id);
                }
            }
            catch
            {
                dbAccount = null;
            }

            var memAccount = DbInitializer.GetInMemoryAdminById(id);
            var target = dbAccount ?? memAccount;

            if (target == null)
            {
                SetFlashMessage("Error", $"Không tìm thấy tài khoản #{id} để xóa.");
                return RedirectToAction(nameof(Index));
            }

            if (string.Equals(target.TenDangNhap, "admin", StringComparison.OrdinalIgnoreCase))
            {
                SetFlashMessage("Error", "Không thể xóa tài khoản Quản trị viên gốc (admin) của hệ thống.");
                return RedirectToAction(nameof(Index));
            }

            var currentUsername = User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(currentUsername) &&
                string.Equals(target.TenDangNhap, currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                SetFlashMessage("Error", "Bạn không thể tự xóa tài khoản đang đăng nhập.");
                return RedirectToAction(nameof(Index));
            }

            if (dbAccount != null && _context != null)
            {
                try
                {
                    _context.TaiKhoanAdmins.Remove(dbAccount);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // Fallback In-Memory
                }
            }

            if (memAccount != null)
            {
                DbInitializer.DeleteInMemoryAdmin(memAccount.IDTaiKhoan);
            }
            else
            {
                var byName = DbInitializer.GetInMemoryAdminByUsername(target.TenDangNhap);
                if (byName != null)
                {
                    DbInitializer.DeleteInMemoryAdmin(byName.IDTaiKhoan);
                }
            }

            SetFlashMessage("Success", $"Đã xóa tài khoản quản trị \"{target.TenDangNhap}\" thành công!");
            return RedirectToAction(nameof(Index));
        }
    }
}
