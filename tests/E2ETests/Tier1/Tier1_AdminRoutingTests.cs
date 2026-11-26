using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier1
{
    public class Tier1_AdminRoutingTests
    {
        #region Test Helpers

        private class MockEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "web_tinhnguyen_tests_" + Guid.NewGuid().ToString("N"));
            public IFileProvider WebRootFileProvider { get; set; } = null!;
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "WEB_TinhNguyen";
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
        }

        private class SimpleTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
            public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private static ApplicationDbContext CreateOfflineDbContext()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65534;Database=NonExistentDb_AdminOfflineTest;User Id=sa;Password=invalid_pwd;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(dbOptions);
        }

        private static void SetupControllerContext(Controller controller)
        {
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, new SimpleTempDataProvider());
        }

        #endregion

        // =========================================================================
        // FEATURE 19: Admin 8 Routes Navigation & Offline Resilience
        // =========================================================================

        [E2ETest(1, 19, "F19_T01: _AdminLayout.cshtml contains zero occurrences of GiaoDien/ in Url.Action")]
        public void F19_T01_AdminLayoutHrefPureControllerNames()
        {
            var layoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            Assert.True(File.Exists(layoutPath), "_AdminLayout.cshtml must exist");

            var content = File.ReadAllText(layoutPath);
            Assert.False(content.Contains("GiaoDien/"), "_AdminLayout.cshtml must not contain 'GiaoDien/' in controller names");
            Assert.False(content.Contains("%2F"), "_AdminLayout.cshtml must not contain escaped '%2F' slashes in URLs");

            // Verify all 8 pure controller names are present in action links
            var expectedControllers = new[]
            {
                "Admin",
                "AdminTrangChu",
                "AdminChuongTrinh",
                "AdminGayQuy",
                "AdminCauHinhQR",
                "AdminKyUc",
                "AdminBanToChuc",
                "AdminTinTuc"
            };

            foreach (var ctrl in expectedControllers)
            {
                Assert.Contains($"\"Index\", \"{ctrl}\"", content, $"_AdminLayout.cshtml must reference pure controller name '{ctrl}'");
            }
        }

        [E2ETest(1, 19, "F19_T02: _AdminLayout.cshtml specifies area = empty string for cross-area safety")]
        public void F19_T02_AdminLayoutCrossAreaIsolation()
        {
            var layoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            var content = File.ReadAllText(layoutPath);

            var rootControllers = new[]
            {
                "Admin",
                "AdminTrangChu",
                "AdminChuongTrinh",
                "AdminGayQuy",
                "AdminCauHinhQR",
                "AdminKyUc",
                "AdminBanToChuc",
                "AdminTinTuc",
                "Home"
            };

            foreach (var ctrl in rootControllers)
            {
                var pattern = $"\"{ctrl}\", new {{ area = \"\" }}";
                Assert.Contains(pattern, content, $"Link for '{ctrl}' must specify explicit 'new {{ area = \"\" }}' for cross-area safety");
            }
        }

        [E2ETest(1, 19, "F19_T03: Active state checks in _AdminLayout.cshtml check pure controller names")]
        public void F19_T03_AdminLayoutActiveStateChecks()
        {
            var layoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            var content = File.ReadAllText(layoutPath);

            Assert.Contains("== \"AdminChuongTrinh\"", content, "Active state check for Chuong Trinh must be 'AdminChuongTrinh'");
            Assert.Contains("== \"AdminGayQuy\"", content, "Active state check for Gay Quy must be 'AdminGayQuy'");
            Assert.False(content.Contains("== \"GiaoDien/AdminChuongTrinh\""), "Must not contain 'GiaoDien/AdminChuongTrinh' in active check");
            Assert.False(content.Contains("== \"GiaoDien/AdminGayQuy\""), "Must not contain 'GiaoDien/AdminGayQuy' in active check");
        }

        [E2ETest(1, 19, "F19_T04: Admin/Index.cshtml contains functional link for Quản lý Tin tức")]
        public void F19_T04_AdminDashboardTinTucShortcutLink()
        {
            var dashboardPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            Assert.True(File.Exists(dashboardPath), "Admin/Index.cshtml must exist");

            var content = File.ReadAllText(dashboardPath);
            Assert.Contains("AdminTinTuc", content, "Admin dashboard must link to AdminTinTuc for news management");
            Assert.False(content.Contains("<a href=\"#\" class=\"bg-white border border-gray-200 rounded-xl p-5 flex items-center gap-4 hover:border-orange-500"),
                "Admin dashboard Quản lý Tin tức card must not be a dead link '#'");
        }

        [E2ETest(1, 19, "F19_T05: All 8 Admin Razor view templates exist and declare _AdminLayout.cshtml")]
        public void F19_T05_AllEightAdminViewsExistAndUseAdminLayout()
        {
            var adminViews = new[]
            {
                Path.Combine("Admin", "Index.cshtml"),
                Path.Combine("AdminTrangChu", "Index.cshtml"),
                Path.Combine("AdminChuongTrinh", "Index.cshtml"),
                Path.Combine("AdminGayQuy", "Index.cshtml"),
                Path.Combine("AdminCauHinhQR", "Index.cshtml"),
                Path.Combine("AdminKyUc", "Index.cshtml"),
                Path.Combine("AdminBanToChuc", "Index.cshtml"),
                Path.Combine("AdminTinTuc", "Index.cshtml")
            };

            foreach (var relPath in adminViews)
            {
                var fullPath = Path.Combine(E2ETestContext.ViewsRoot, relPath);
                Assert.True(File.Exists(fullPath), $"Admin view template must exist: {relPath}");

                var content = File.ReadAllText(fullPath);
                Assert.Contains("_AdminLayout.cshtml", content, $"View {relPath} must reference _AdminLayout.cshtml");
            }
        }

        [E2ETest(1, 19, "F19_T06: AdminChuongTrinh and AdminKyUc themes are aligned without conflicting wrappers")]
        public void F19_T06_AdminChuongTrinhAndKyUcThemeAlignment()
        {
            var chuongTrinhPath = Path.Combine(E2ETestContext.ViewsRoot, "AdminChuongTrinh", "Index.cshtml");
            var kyUcPath = Path.Combine(E2ETestContext.ViewsRoot, "AdminKyUc", "Index.cshtml");

            var ctContent = File.ReadAllText(chuongTrinhPath);
            var kuContent = File.ReadAllText(kyUcPath);

            Assert.False(ctContent.Contains("p-6 bg-gray-50 min-h-screen"), "AdminChuongTrinh/Index.cshtml must not contain nested p-6 bg-gray-50 wrapper");
            Assert.False(kuContent.Contains("p-6 bg-gray-50 min-h-screen"), "AdminKyUc/Index.cshtml must not contain nested p-6 bg-gray-50 wrapper");
        }

        [E2ETest(1, 19, "F19_T07: All 8 Admin controllers exist and inherit from Microsoft.AspNetCore.Mvc.Controller")]
        public void F19_T07_AllEightAdminControllersExistAndInheritFromController()
        {
            var controllerNames = new[]
            {
                "AdminController",
                "AdminTrangChuController",
                "AdminChuongTrinhController",
                "AdminGayQuyController",
                "AdminCauHinhQRController",
                "AdminKyUcController",
                "AdminBanToChucController",
                "AdminTinTucController"
            };

            foreach (var name in controllerNames)
            {
                var fullName = $"WEB_TinhNguyen.BoDieuKhien.{name}";
                var type = Assert.RequireType(fullName, name);
                Assert.True(typeof(Controller).IsAssignableFrom(type), $"{name} must inherit from Controller");

                var indexMethod = type.GetMethod("Index", BindingFlags.Public | BindingFlags.Instance);
                Assert.NotNull(indexMethod, $"{name} must define public Index action");
            }
        }

        [E2ETest(1, 19, "F19_T08: AdminController.Index executes and returns ViewResult (HTTP 200)")]
        public void F19_T08_AdminControllerIndexReturnsView()
        {
            var controller = new AdminController();
            SetupControllerContext(controller);

            var result = controller.Index();
            Assert.NotNull(result, "AdminController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminController.Index must return ViewResult");
        }

        [E2ETest(1, 19, "F19_T09: AdminCauHinhQRController.Index executes with offline resilience and returns ViewResult")]
        public async Task F19_T09_AdminCauHinhQRControllerIndexReturnsViewResult()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();
            var controller = new AdminCauHinhQRController(offlineContext, mockEnv);
            SetupControllerContext(controller);

            var result = await controller.Index();
            Assert.NotNull(result, "AdminCauHinhQRController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminCauHinhQRController.Index must return ViewResult");
            Assert.NotNull(viewResult!.Model, "AdminCauHinhQRController.Index must return non-null model");
        }

        [E2ETest(1, 19, "F19_T10: AdminTrangChuController.Index executes with offline resilience and returns ViewResult")]
        public async Task F19_T10_AdminTrangChuControllerOfflineResilience()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();
            var controller = new AdminTrangChuController(offlineContext, mockEnv);
            SetupControllerContext(controller);

            var result = await controller.Index();
            Assert.NotNull(result, "AdminTrangChuController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminTrangChuController.Index must return ViewResult");
            var model = viewResult!.Model as IEnumerable<ChienDich>;
            Assert.NotNull(model, "AdminTrangChuController.Index model must be IEnumerable<ChienDich>");
        }

        [E2ETest(1, 19, "F19_T11: AdminChuongTrinhController.Index executes with offline resilience and returns ViewResult")]
        public async Task F19_T11_AdminChuongTrinhControllerOfflineResilience()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();
            var controller = new AdminChuongTrinhController(offlineContext, mockEnv);
            SetupControllerContext(controller);

            var result = await controller.Index();
            Assert.NotNull(result, "AdminChuongTrinhController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminChuongTrinhController.Index must return ViewResult");
            var model = viewResult!.Model as IEnumerable<ChienDich>;
            Assert.NotNull(model, "AdminChuongTrinhController.Index model must be IEnumerable<ChienDich>");
        }

        [E2ETest(1, 19, "F19_T12: AdminGayQuyController.Index executes with offline resilience and returns ViewResult")]
        public async Task F19_T12_AdminGayQuyControllerOfflineResilience()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();
            var controller = new AdminGayQuyController(offlineContext, mockEnv);
            SetupControllerContext(controller);

            var result = await controller.Index();
            Assert.NotNull(result, "AdminGayQuyController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminGayQuyController.Index must return ViewResult");
            var model = viewResult!.Model as IEnumerable<ChienDich>;
            Assert.NotNull(model, "AdminGayQuyController.Index model must be IEnumerable<ChienDich>");
        }

        [E2ETest(1, 19, "F19_T13: AdminKyUcController.Index executes with offline resilience and returns ViewResult")]
        public async Task F19_T13_AdminKyUcControllerOfflineResilience()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();
            var controller = new AdminKyUcController(offlineContext, mockEnv);
            SetupControllerContext(controller);

            var result = await controller.Index();
            Assert.NotNull(result, "AdminKyUcController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminKyUcController.Index must return ViewResult");
            var model = viewResult!.Model as IEnumerable<ChienDich>;
            Assert.NotNull(model, "AdminKyUcController.Index model must be IEnumerable<ChienDich>");
        }

        [E2ETest(1, 19, "F19_T14: AdminBanToChucController.Index executes with offline resilience and returns ViewResult")]
        public async Task F19_T14_AdminBanToChucControllerOfflineResilience()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();
            var controller = new AdminBanToChucController(offlineContext, mockEnv);
            SetupControllerContext(controller);

            var result = await controller.Index();
            Assert.NotNull(result, "AdminBanToChucController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminBanToChucController.Index must return ViewResult");
            var model = viewResult!.Model as IEnumerable<ChienDich>;
            Assert.NotNull(model, "AdminBanToChucController.Index model must be IEnumerable<ChienDich>");
        }

        [E2ETest(1, 19, "F19_T15: AdminTinTucController.Index executes with offline resilience and returns ViewResult")]
        public async Task F19_T15_AdminTinTucControllerOfflineResilience()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();
            var controller = new AdminTinTucController(offlineContext, mockEnv);
            SetupControllerContext(controller);

            var result = await controller.Index();
            Assert.NotNull(result, "AdminTinTucController.Index must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminTinTucController.Index must return ViewResult");
            var model = viewResult!.Model as IEnumerable<ChienDich>;
            Assert.NotNull(model, "AdminTinTucController.Index model must be IEnumerable<ChienDich>");
        }

        [E2ETest(1, 19, "F19_T16: All 8 Admin routes execute and return ViewResult (HTTP 200 OK equivalence)")]
        public async Task F19_T16_AllEightAdminRoutesReturnHttp200Equivalence()
        {
            using var offlineContext = CreateOfflineDbContext();
            var mockEnv = new MockEnvironment();

            var admin = new AdminController();
            SetupControllerContext(admin);

            var qr = new AdminCauHinhQRController(offlineContext, mockEnv);
            SetupControllerContext(qr);

            var trangChu = new AdminTrangChuController(offlineContext, mockEnv);
            SetupControllerContext(trangChu);

            var chuongTrinh = new AdminChuongTrinhController(offlineContext, mockEnv);
            SetupControllerContext(chuongTrinh);

            var gayQuy = new AdminGayQuyController(offlineContext, mockEnv);
            SetupControllerContext(gayQuy);

            var kyUc = new AdminKyUcController(offlineContext, mockEnv);
            SetupControllerContext(kyUc);

            var btc = new AdminBanToChucController(offlineContext, mockEnv);
            SetupControllerContext(btc);

            var tinTuc = new AdminTinTucController(offlineContext, mockEnv);
            SetupControllerContext(tinTuc);

            var r1 = admin.Index();
            var r2 = await qr.Index();
            var r3 = await trangChu.Index();
            var r4 = await chuongTrinh.Index();
            var r5 = await gayQuy.Index();
            var r6 = await kyUc.Index();
            var r7 = await btc.Index();
            var r8 = await tinTuc.Index();

            Assert.True(r1 is ViewResult, "/Admin must return ViewResult");
            Assert.True(r2 is ViewResult, "/AdminCauHinhQR must return ViewResult");
            Assert.True(r3 is ViewResult, "/AdminTrangChu must return ViewResult");
            Assert.True(r4 is ViewResult, "/AdminChuongTrinh must return ViewResult");
            Assert.True(r5 is ViewResult, "/AdminGayQuy must return ViewResult");
            Assert.True(r6 is ViewResult, "/AdminKyUc must return ViewResult");
            Assert.True(r7 is ViewResult, "/AdminBanToChuc must return ViewResult");
            Assert.True(r8 is ViewResult, "/AdminTinTuc must return ViewResult");
        }
    }
}
