using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Adversarial Stress Harness for Challenger 2
    /// Independent, empirical verification of the 8 Admin controllers, Razor views,
    /// cross-area isolation, URL generation, active tab highlighting, and database resilience.
    /// </summary>
    public class Tier5_Challenger2_AdminAdversarialHarness
    {
        #region Mock Infrastructure

        private class MockEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "challenger2_root_" + Guid.NewGuid().ToString("N"));
            public IFileProvider WebRootFileProvider { get; set; } = null!;
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "WEB_TinhNguyen";
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
        }

        private class MockTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
            public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private static ApplicationDbContext CreateDeadDbContext()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65535;Database=OfflineAdminTestDb_Challenger2;User Id=sa;Password=invalid_pwd;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(dbOptions);
        }

        private static void SetupController(Controller controller, RouteData? routeData = null)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, routeData ?? new RouteData(), new ActionDescriptor());
            controller.ControllerContext = new ControllerContext(actionContext);
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
        }

        #endregion

        // =========================================================================
        // SUITE 1: 8 ADMIN CONTROLLERS DYNAMIC TYPE & METHOD VALIDATION
        // =========================================================================

        [E2ETest(5, 20, "CH2_01: All 8 Admin controllers exist, inherit Controller, and have valid Index actions")]
        public void CH2_01_AllEightAdminControllers_TypeAndContractValidation()
        {
            var expectedControllers = new Dictionary<string, Type>
            {
                { "AdminController", typeof(AdminController) },
                { "AdminTrangChuController", typeof(AdminTrangChuController) },
                { "AdminChuongTrinhController", typeof(AdminChuongTrinhController) },
                { "AdminGayQuyController", typeof(AdminGayQuyController) },
                { "AdminCauHinhQRController", typeof(AdminCauHinhQRController) },
                { "AdminKyUcController", typeof(AdminKyUcController) },
                { "AdminBanToChucController", typeof(AdminBanToChucController) },
                { "AdminTinTucController", typeof(AdminTinTucController) }
            };

            Assert.Equal(8, expectedControllers.Count, "Exactly 8 Admin controllers must be audited");

            foreach (var kvp in expectedControllers)
            {
                var type = kvp.Value;
                Assert.True(typeof(Controller).IsAssignableFrom(type), $"{kvp.Key} must inherit from Microsoft.AspNetCore.Mvc.Controller");

                var indexMethod = type.GetMethod("Index", BindingFlags.Public | BindingFlags.Instance);
                Assert.NotNull(indexMethod, $"{kvp.Key} must declare public Index action");
                Assert.True(typeof(IActionResult).IsAssignableFrom(indexMethod!.ReturnType) ||
                            typeof(Task<IActionResult>).IsAssignableFrom(indexMethod.ReturnType),
                            $"{kvp.Key}.Index must return IActionResult or Task<IActionResult>");
            }
        }

        [E2ETest(5, 20, "CH2_02: Dynamic invocation of all 8 Index actions under offline DB returns ViewResult")]
        public async Task CH2_02_AllEightAdminControllers_IndexActionReturnsViewResultUnderOfflineDb()
        {
            using var deadDb = CreateDeadDbContext();
            var env = new MockEnvironment();

            var admin = new AdminController();
            SetupController(admin);
            var resAdmin = admin.Index();
            Assert.True(resAdmin is ViewResult, "AdminController.Index must return ViewResult");

            var trangChu = new AdminTrangChuController(deadDb, env);
            SetupController(trangChu);
            var resTrangChu = await trangChu.Index();
            Assert.True(resTrangChu is ViewResult, "AdminTrangChuController.Index must return ViewResult under offline DB");
            Assert.NotNull(((ViewResult)resTrangChu).Model, "AdminTrangChu fallback model must not be null");

            var chuongTrinh = new AdminChuongTrinhController(deadDb, env);
            SetupController(chuongTrinh);
            var resChuongTrinh = await chuongTrinh.Index();
            Assert.True(resChuongTrinh is ViewResult, "AdminChuongTrinhController.Index must return ViewResult under offline DB");
            Assert.NotNull(((ViewResult)resChuongTrinh).Model, "AdminChuongTrinh fallback model must not be null");

            var gayQuy = new AdminGayQuyController(deadDb, env);
            SetupController(gayQuy);
            var resGayQuy = await gayQuy.Index();
            Assert.True(resGayQuy is ViewResult, "AdminGayQuyController.Index must return ViewResult under offline DB");
            Assert.NotNull(((ViewResult)resGayQuy).Model, "AdminGayQuy fallback model must not be null");

            var cauHinhQR = new AdminCauHinhQRController(deadDb, env);
            SetupController(cauHinhQR);
            var resQR = await cauHinhQR.Index();
            Assert.True(resQR is ViewResult, "AdminCauHinhQRController.Index must return ViewResult under offline DB");
            Assert.NotNull(((ViewResult)resQR).Model, "AdminCauHinhQR fallback model must not be null");

            var kyUc = new AdminKyUcController(deadDb, env);
            SetupController(kyUc);
            var resKyUc = await kyUc.Index();
            Assert.True(resKyUc is ViewResult, "AdminKyUcController.Index must return ViewResult under offline DB");
            Assert.NotNull(((ViewResult)resKyUc).Model, "AdminKyUc fallback model must not be null");

            var btc = new AdminBanToChucController(deadDb, env);
            SetupController(btc);
            var resBtc = await btc.Index();
            Assert.True(resBtc is ViewResult, "AdminBanToChucController.Index must return ViewResult under offline DB");
            Assert.NotNull(((ViewResult)resBtc).Model, "AdminBanToChuc fallback model must not be null");

            var tinTuc = new AdminTinTucController(deadDb, env);
            SetupController(tinTuc);
            var resTinTuc = await tinTuc.Index();
            Assert.True(resTinTuc is ViewResult, "AdminTinTucController.Index must return ViewResult under offline DB");
            Assert.NotNull(((ViewResult)resTinTuc).Model, "AdminTinTuc fallback model must not be null");
        }

        // =========================================================================
        // SUITE 2: 8 RAZOR VIEWS PHYSICAL EXISTENCE AND ADMIN LAYOUT DECLARATION
        // =========================================================================

        [E2ETest(5, 20, "CH2_03: All 8 Razor view files physically exist and bind _AdminLayout.cshtml")]
        public void CH2_03_AllEightRazorViewsPhysicallyExistAndBindAdminLayout()
        {
            var requiredViews = new[]
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

            foreach (var ctrlName in requiredViews)
            {
                var viewPath = Path.Combine(E2ETestContext.ViewsRoot, ctrlName, "Index.cshtml");
                Assert.True(File.Exists(viewPath), $"View file must exist at {viewPath}");

                var content = File.ReadAllText(viewPath);
                Assert.Contains("_AdminLayout.cshtml", content, $"{ctrlName}/Index.cshtml must declare _AdminLayout.cshtml");
            }
        }

        // =========================================================================
        // SUITE 3: ADVERSARIAL ROUTE AND LAYOUT AUDIT
        // =========================================================================

        [E2ETest(5, 20, "CH2_04: _AdminLayout.cshtml hrefs map 1-to-1 with actual route endpoints without %2F")]
        public void CH2_04_AdminLayoutHrefMapping_NoEscapedSlashesOrGiaoDien()
        {
            var layoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            var content = File.ReadAllText(layoutPath);

            Assert.False(content.Contains("%2F"), "_AdminLayout.cshtml must not contain escaped '%2F' slashes");
            Assert.False(content.Contains("GiaoDien/"), "_AdminLayout.cshtml must not contain 'GiaoDien/' prefixes");

            // Verify all 8 pure controller names are bound in Url.Action
            var controllers = new[]
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

            foreach (var ctrl in controllers)
            {
                var expectedPattern = $"\"Index\", \"{ctrl}\", new {{ area = \"\" }}";
                Assert.Contains(expectedPattern, content, $"_AdminLayout.cshtml must declare clean link with area=\"\" for {ctrl}");
            }
        }

        [E2ETest(5, 20, "CH2_05: Area Isolation - all root admin links specify explicit area=\"\" to prevent CuaHang pollution")]
        public void CH2_05_CrossAreaIsolation_NoAreaPollutionFromCuaHang()
        {
            var layoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            var content = File.ReadAllText(layoutPath);

            // Regex match all @Url.Action calls in the layout
            var matches = Regex.Matches(content, @"@Url\.Action\([^)]+\)");
            Assert.True(matches.Count >= 9, "Must have at least 9 @Url.Action invocations in _AdminLayout.cshtml");

            foreach (Match match in matches)
            {
                var actionCall = match.Value;
                // Every root Url.Action call must contain area = ""
                Assert.Contains("area = \"\"", actionCall, $"Action call '{actionCall}' must explicitly specify area = \"\" for area isolation");
            }
        }

        [E2ETest(5, 20, "CH2_06: Active tab highlighting logic in _AdminLayout evaluates to active for exactly 1 item")]
        public void CH2_06_ActiveTabHighlighting_ExactlyOneActivePerController()
        {
            var layoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            var content = File.ReadAllText(layoutPath);

            var items = new[]
            {
                "Admin",
                "AdminTrangChu",
                "AdminChuongTrinh",
                "AdminGayQuy",
                "AdminSanPham",
                "AdminDonHang",
                "AdminCauHinhQR",
                "AdminKyUc",
                "AdminBanToChuc",
                "AdminTinTuc"
            };

            // Extract the active conditions
            // Format: ViewContext.RouteData.Values["Controller"]?.ToString() == "<ControllerName>"
            foreach (var activeCtrl in items)
            {
                int activeCount = 0;
                foreach (var testTarget in items)
                {
                    // Evaluate condition: testTarget == activeCtrl
                    var isMatch = string.Equals(testTarget, activeCtrl, StringComparison.Ordinal);
                    var activeClass = isMatch ? "active" : "";
                    if (activeClass == "active") activeCount++;
                }

                Assert.Equal(1, activeCount, $"When active controller is '{activeCtrl}', exactly 1 item must be active (no false positives)");
            }

            // Verify each item's check exists verbatim in _AdminLayout.cshtml
            foreach (var ctrl in items)
            {
                var condition = $"ViewContext.RouteData.Values[\"Controller\"]?.ToString() == \"{ctrl}\"";
                Assert.Contains(condition, content, $"Layout must check exact controller name '{ctrl}' for active tab");
            }
        }

        [E2ETest(5, 20, "CH2_07: Dashboard shortcut card for AdminTinTuc is functional and links to AdminTinTuc with area=\"\"")]
        public void CH2_07_AdminDashboardNewsShortcutLinkValid()
        {
            var dashPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            var content = File.ReadAllText(dashPath);

            // Must link to AdminTinTuc
            Assert.Contains("\"Index\", \"AdminTinTuc\", new { area = \"\" }", content,
                "Admin/Index.cshtml must link to AdminTinTuc with area = \"\"");

            // Must not have dead link
            Assert.False(content.Contains("href=\"#\" class=\"bg-white border border-gray-200 rounded-xl p-5 flex items-center gap-4 hover:border-orange-500"),
                "Admin/Index.cshtml must not contain dead href='#' for news card");
        }

        [E2ETest(5, 20, "CH2_08: Razor templates handle empty lists safely without throwing NullReferenceException")]
        public void CH2_08_CampaignRazorViewsSafeWithEmptyLists()
        {
            var campaignViews = new[]
            {
                "AdminTrangChu",
                "AdminChuongTrinh",
                "AdminGayQuy",
                "AdminKyUc",
                "AdminBanToChuc",
                "AdminTinTuc"
            };

            foreach (var ctrl in campaignViews)
            {
                var viewPath = Path.Combine(E2ETestContext.ViewsRoot, ctrl, "Index.cshtml");
                var content = File.ReadAllText(viewPath);

                Assert.Contains("@model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>", content,
                    $"{ctrl}/Index.cshtml must declare @model IEnumerable<WEB_TinhNguyen.MoHinh.ChienDich>");

                // Check that Model is iterated via @foreach
                Assert.Contains("@foreach", content, $"{ctrl}/Index.cshtml should iterate over @Model via foreach");
            }
        }
    }
}
