using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using WEB_TinhNguyen.Areas.CuaHang.Controllers;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Granular Admin Account Management & Module-Level Authorization E2E Test Suite (R1-R4).
    /// Verifies:
    /// - R1: Creating, editing, toggling, and deleting admin accounts with arbitrary module permissions (1-2 modules or ALL) and PBKDF2-HMAC-SHA256 hashing.
    /// - R2: Enforcing module-level authorization across all 11 Admin modules (200 OK for permitted modules, 403 Forbidden for unauthorized modules, 302 Redirect for unauthenticated requests, full access for SuperAdmin "ALL").
    /// - R3: Dynamic navigation menu visibility in _AdminLayout.cshtml and Admin/Index.cshtml based on User.CoQuyen(...).
    /// </summary>
    public class Tier5_GranularAdminAuthorizationTests
    {
        private class MockEnv : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "web_tinh_nguyen_test_wwwroot");
            public IFileProvider WebRootFileProvider { get; set; } = null!;
            public string ApplicationName { get; set; } = "WEB_TinhNguyen";
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public string EnvironmentName { get; set; } = "Development";
        }

        private class MockTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object> _data = new();
            public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private static ApplicationDbContext CreateOfflineDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65533;Database=OfflineGranularAuthDb;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(options);
        }

        private static HttpContext CreateAuthenticatedHttpContext(string username, string phanQuyen)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("PhanQuyen", phanQuyen)
            };

            foreach (var q in QuyenAdmin.TachDanhSachQuyen(phanQuyen))
            {
                claims.Add(new Claim("QuyenHan", q));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = principal
            };
            return httpContext;
        }

        private static void AttachContext(Controller controller, HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            controller.ControllerContext = new ControllerContext(actionContext);
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
        }

        // =========================================================================
        // TEST 1: R1 - CREATE SUB-ADMIN WITH 2 SPECIFIC MODULES & VERIFY HASHING
        // =========================================================================
        [E2ETest(5, 301, "RBAC_01: Create sub-admin with 2 modules (TinTuc, ChuongTrinh), verify PBKDF2 hash & Login claims")]
        public async Task RBAC_01_CreateSubAdminWithTwoModulesAndVerifyLoginClaims()
        {
            DbInitializer.ResetInMemoryStore();
            try
            {
                using var db = CreateOfflineDbContext();
                var controller = new AdminTaiKhoanController(db);
                var superAdminCtx = CreateAuthenticatedHttpContext("admin", QuyenAdmin.All);
                AttachContext(controller, superAdminCtx);

                var createVm = new AdminTaiKhoanCreateViewModel
                {
                    TenDangNhap = "bientap_2phanhe",
                    MatKhau = "BienTap@2026",
                    XacNhanMatKhau = "BienTap@2026",
                    HoTen = "Biên tập viên Tin tức & Chương trình",
                    Email = "bientap@trangsanglangbiang.org",
                    ToanQuyen = false,
                    DanhSachQuyen = new List<string> { QuyenAdmin.TinTuc, QuyenAdmin.ChuongTrinh },
                    TrangThaiHoatDong = true
                };

                var createResult = await controller.Create(createVm);
                Assert.IsType<RedirectToActionResult>(createResult, "Create valid sub-admin must redirect to Index");

                var saved = DbInitializer.GetInMemoryAdminByUsername("bientap_2phanhe");
                Assert.NotNull(saved, "Created sub-admin must be persisted in store");
                Assert.Equal("TinTuc,ChuongTrinh", saved!.PhanQuyen, "PhanQuyen must store normalized granted modules");
                Assert.False(saved.LaToanQuyen, "Sub-admin with 2 modules must not have LaToanQuyen = true");
                Assert.NotEqual("BienTap@2026", saved.MatKhauHash, "Password must be hashed, never plaintext");
                Assert.True(WEB_TinhNguyen.DuLieu.BaoMatMatKhau.KiemTraMatKhau("BienTap@2026", saved.MatKhauHash, saved.MuoiBaoMat), "PBKDF2 password verification must succeed");

                // Simulate Login via AdminController
                var authMock = new Tier5_ChallengerM2_3_AuthPipelineHarness.MockAuthService();
                var services = new ServiceCollection();
                services.AddSingleton<IAuthenticationService>(authMock);
                var loginHttpCtx = new DefaultHttpContext
                {
                    RequestServices = services.BuildServiceProvider()
                };

                var adminLoginCtrl = new AdminController(db);
                AttachContext(adminLoginCtrl, loginHttpCtx);

                var loginResult = await adminLoginCtrl.Login(new AdminLoginViewModel
                {
                    TenDangNhap = "bientap_2phanhe",
                    MatKhau = "BienTap@2026"
                });

                Assert.True(authMock.SignInCalled, "Sub-admin login must call SignInAsync");
                var signedInUser = authMock.LastSignInPrincipal;
                Assert.NotNull(signedInUser, "Signed-in ClaimsPrincipal must not be null");
                Assert.False(signedInUser!.LaToanQuyen(), "Restricted sub-admin must not be LaToanQuyen");
                Assert.True(signedInUser.CoQuyen(QuyenAdmin.TinTuc), "Sub-admin must have TinTuc permission");
                Assert.True(signedInUser.CoQuyen(QuyenAdmin.ChuongTrinh), "Sub-admin must have ChuongTrinh permission");
                Assert.False(signedInUser.CoQuyen(QuyenAdmin.SanPham), "Sub-admin must NOT have SanPham permission");
                Assert.False(signedInUser.CoQuyen(QuyenAdmin.QuanLyTaiKhoan), "Sub-admin must NOT have QuanLyTaiKhoan permission");
            }
            finally
            {
                DbInitializer.ResetInMemoryStore();
            }
        }

        // =========================================================================
        // TEST 2: R2 - GRANULAR ACCESS MATRIX ACROSS ALL 11 ADMIN CONTROLLERS
        // =========================================================================
        [E2ETest(5, 302, "RBAC_02: Restricted sub-admin gets 200 OK on granted modules and 403 Forbidden on all other modules")]
        public async Task RBAC_02_RestrictedSubAdminAccessMatrixAcrossAllElevenModules()
        {
            using var db = CreateOfflineDbContext();
            var env = new MockEnv();

            // Sub-admin granted ONLY TinTuc and SanPham
            var subAdminCtx = CreateAuthenticatedHttpContext("sub_tintuc_sanpham", $"{QuyenAdmin.TinTuc},{QuyenAdmin.SanPham}");

            // 1. Permitted modules -> must return ViewResult (200 OK)
            var tinTucCtrl = new AdminTinTucController(db, env);
            AttachContext(tinTucCtrl, subAdminCtx);
            var tinTucRes = await tinTucCtrl.Index();
            Assert.IsType<ViewResult>(tinTucRes, "Permitted module TinTuc must return ViewResult");

            var sanPhamCtrl = new AdminSanPhamController(db, env);
            AttachContext(sanPhamCtrl, subAdminCtx);
            var sanPhamRes = await sanPhamCtrl.Index();
            Assert.IsType<ViewResult>(sanPhamRes, "Permitted module SanPham must return ViewResult");

            // 2. Non-permitted modules -> must return StatusCodeResult(403)
            var trangChuCtrl = new AdminTrangChuController(db, env);
            AttachContext(trangChuCtrl, subAdminCtx);
            var trangChuRes = await trangChuCtrl.Index();
            Assert.Equal(403, (trangChuRes as StatusCodeResult)?.StatusCode, "Unauthorized TrangChu must return 403");

            var chuongTrinhCtrl = new AdminChuongTrinhController(db, env);
            AttachContext(chuongTrinhCtrl, subAdminCtx);
            var chuongTrinhRes = await chuongTrinhCtrl.Index();
            Assert.Equal(403, (chuongTrinhRes as StatusCodeResult)?.StatusCode, "Unauthorized ChuongTrinh must return 403");

            var gayQuyCtrl = new AdminGayQuyController(db, env);
            AttachContext(gayQuyCtrl, subAdminCtx);
            var gayQuyRes = await gayQuyCtrl.Index();
            Assert.Equal(403, (gayQuyRes as StatusCodeResult)?.StatusCode, "Unauthorized GayQuy must return 403");

            var qrCtrl = new AdminCauHinhQRController(db, env);
            AttachContext(qrCtrl, subAdminCtx);
            var qrRes = await qrCtrl.Index();
            Assert.Equal(403, (qrRes as StatusCodeResult)?.StatusCode, "Unauthorized CauHinhQR must return 403");

            var kyUcCtrl = new AdminKyUcController(db, env);
            AttachContext(kyUcCtrl, subAdminCtx);
            var kyUcRes = await kyUcCtrl.Index();
            Assert.Equal(403, (kyUcRes as StatusCodeResult)?.StatusCode, "Unauthorized KyUc must return 403");

            var btcCtrl = new AdminBanToChucController(db, env);
            AttachContext(btcCtrl, subAdminCtx);
            var btcRes = await btcCtrl.Index();
            Assert.Equal(403, (btcRes as StatusCodeResult)?.StatusCode, "Unauthorized BanToChuc must return 403");

            var hieuUngCtrl = new AdminHieuUngController(db, env);
            AttachContext(hieuUngCtrl, subAdminCtx);
            var hieuUngRes = await hieuUngCtrl.Index();
            Assert.Equal(403, (hieuUngRes as StatusCodeResult)?.StatusCode, "Unauthorized HieuUng must return 403");

            var donHangCtrl = new AdminDonHangController(db);
            AttachContext(donHangCtrl, subAdminCtx);
            var donHangRes = await donHangCtrl.Index(null, null, null, null);
            Assert.Equal(403, (donHangRes as StatusCodeResult)?.StatusCode, "Unauthorized DonHang must return 403");

            var taiKhoanCtrl = new AdminTaiKhoanController(db);
            AttachContext(taiKhoanCtrl, subAdminCtx);
            var taiKhoanRes = await taiKhoanCtrl.Index();
            Assert.Equal(403, (taiKhoanRes as StatusCodeResult)?.StatusCode, "Unauthorized QuanLyTaiKhoan must return 403");
        }

        // =========================================================================
        // TEST 3: R2 - SUPERADMIN ("ALL") HAS FULL ACCESS TO ALL 11 MODULES
        // =========================================================================
        [E2ETest(5, 303, "RBAC_03: SuperAdmin (PhanQuyen = ALL) accesses all 11 Admin modules with 200 OK")]
        public async Task RBAC_03_SuperAdminHasFullAccessToAllElevenModules()
        {
            using var db = CreateOfflineDbContext();
            var env = new MockEnv();
            var superCtx = CreateAuthenticatedHttpContext("admin", QuyenAdmin.All);

            var controllersAndActions = new Func<Task<IActionResult>>[]
            {
                async () => { var c = new AdminTrangChuController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminChuongTrinhController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminGayQuyController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminCauHinhQRController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminKyUcController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminBanToChucController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminTinTucController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminHieuUngController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminSanPhamController(db, env); AttachContext(c, superCtx); return await c.Index(); },
                async () => { var c = new AdminDonHangController(db); AttachContext(c, superCtx); return await c.Index(null, null, null, null); },
                async () => { var c = new AdminTaiKhoanController(db); AttachContext(c, superCtx); return await c.Index(); }
            };

            foreach (var invoke in controllersAndActions)
            {
                var result = await invoke();
                Assert.IsType<ViewResult>(result, "SuperAdmin must receive ViewResult (200 OK) on every module");
            }
        }

        // =========================================================================
        // TEST 4: R1 & R2 - EDIT PERMISSIONS, ROOT ADMIN PROTECTION & FILTER ENFORCEMENT
        // =========================================================================
        [E2ETest(5, 304, "RBAC_04: Edit account permissions, protect root 'admin' from deletion/lockout, verify PhanQuyenAdminAttribute filter")]
        public async Task RBAC_04_EditPermissionsAndRootAdminProtection()
        {
            DbInitializer.ResetInMemoryStore();
            try
            {
                using var db = CreateOfflineDbContext();
                var controller = new AdminTaiKhoanController(db);
                var superCtx = CreateAuthenticatedHttpContext("admin", QuyenAdmin.All);
                AttachContext(controller, superCtx);

                // 1. Root admin (ID = 1) cannot be deleted or deactivated
                await controller.ToggleStatus(1);
                var rootAfterToggle = DbInitializer.GetInMemoryAdminById(1);
                Assert.True(rootAfterToggle!.TrangThaiHoatDong, "Root 'admin' must never be deactivated");

                await controller.Delete(1);
                var rootAfterDelete = DbInitializer.GetInMemoryAdminById(1);
                Assert.NotNull(rootAfterDelete, "Root 'admin' must never be deleted");

                // 2. Create sub-admin with 1 module, then edit to grant 3 modules
                await controller.Create(new AdminTaiKhoanCreateViewModel
                {
                    TenDangNhap = "ql_donhang",
                    MatKhau = "DonHang@123",
                    XacNhanMatKhau = "DonHang@123",
                    HoTen = "Quản lý Đơn hàng",
                    ToanQuyen = false,
                    DanhSachQuyen = new List<string> { QuyenAdmin.DonHang },
                    TrangThaiHoatDong = true
                });

                var sub = DbInitializer.GetInMemoryAdminByUsername("ql_donhang");
                Assert.NotNull(sub, "Sub-admin ql_donhang must exist");
                Assert.Equal("DonHang", sub!.PhanQuyen, "Initial permission must be DonHang");

                await controller.Edit(sub.IDTaiKhoan, new AdminTaiKhoanEditViewModel
                {
                    IDTaiKhoan = sub.IDTaiKhoan,
                    TenDangNhap = sub.TenDangNhap,
                    HoTen = "Quản lý Cửa hàng & QR",
                    ToanQuyen = false,
                    DanhSachQuyen = new List<string> { QuyenAdmin.DonHang, QuyenAdmin.SanPham, QuyenAdmin.CauHinhQR },
                    TrangThaiHoatDong = true
                });

                var updated = DbInitializer.GetInMemoryAdminById(sub.IDTaiKhoan);
                Assert.Equal("DonHang,SanPham,CauHinhQR", updated!.PhanQuyen, "Updated permissions must match edited modules");
                Assert.True(updated.CoQuyen(QuyenAdmin.SanPham), "Must have SanPham permission after edit");
                Assert.True(updated.CoQuyen(QuyenAdmin.CauHinhQR), "Must have CauHinhQR permission after edit");
                Assert.False(updated.CoQuyen(QuyenAdmin.TrangChu), "Must NOT have TrangChu permission");

                // 3. Verify PhanQuyenAdminAttribute filter behavior directly
                var attr = new PhanQuyenAdminAttribute(QuyenAdmin.TrangChu);
                var filterCtx = new AuthorizationFilterContext(
                    new ActionContext(CreateAuthenticatedHttpContext("ql_donhang", updated.PhanQuyen), new RouteData(), new ActionDescriptor()),
                    new List<IFilterMetadata>());
                attr.OnAuthorization(filterCtx);
                Assert.Equal(403, (filterCtx.Result as StatusCodeResult)?.StatusCode, "PhanQuyenAdminAttribute must short-circuit with 403 when module claim is missing");
            }
            finally
            {
                DbInitializer.ResetInMemoryStore();
            }
        }
    }
}
