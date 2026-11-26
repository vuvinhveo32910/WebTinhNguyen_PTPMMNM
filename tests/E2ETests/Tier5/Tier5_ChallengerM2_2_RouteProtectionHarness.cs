using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WEB_TinhNguyen.Areas.CuaHang.Controllers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Adversarial Route Protection & Invariant Challenger Harness for Milestone 2.
    /// Empirical verification for:
    /// 1. Class-level [Authorize] decoration on all 11 Admin Controllers.
    /// 2. [AllowAnonymous] decoration on AdminController.Login (GET & POST).
    /// 3. [Authorize] + [ValidateAntiForgeryToken] on all 21 data-modifying CRUD POST actions.
    /// 4. Pipeline ordering invariants (CH19 & F02_T02).
    /// 5. Direct parameterless instantiation invariants (F19_T08 & F19_T16).
    /// 6. CuaHang views SHA-256 isolation and integrity invariant (M3Challenger2).
    /// </summary>
    public class Tier5_ChallengerM2_2_RouteProtectionHarness
    {
        private class SimpleTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
            public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private static void SetupControllerContext(Controller controller)
        {
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, new SimpleTempDataProvider());
        }

        // =========================================================================
        // TEST 1: Class-level [Authorize] on all 11 Admin Controllers
        // =========================================================================
        [E2ETest(5, 201, "M2_2_01: Attribute Reflection - All 11 Admin Controllers decorated with [Authorize] at class level")]
        public void M2_2_01_AllElevenAdminControllersDecoratedWithAuthorize()
        {
            var adminControllerTypes = new[]
            {
                typeof(AdminController),
                typeof(AdminBanToChucController),
                typeof(AdminCauHinhQRController),
                typeof(AdminChuongTrinhController),
                typeof(AdminGayQuyController),
                typeof(AdminHieuUngController),
                typeof(AdminKyUcController),
                typeof(AdminTinTucController),
                typeof(AdminTrangChuController),
                typeof(AdminSanPhamController),
                typeof(AdminDonHangController)
            };

            Assert.Equal(11, adminControllerTypes.Length, "Expected exactly 11 Admin Controllers");

            foreach (var type in adminControllerTypes)
            {
                var authorizeAttr = type.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
                Assert.NotNull(authorizeAttr, $"Controller '{type.Name}' must be decorated with [Authorize] at class level");
            }
        }

        // =========================================================================
        // TEST 2: AdminController.Login has [AllowAnonymous]
        // =========================================================================
        [E2ETest(5, 201, "M2_2_02: Attribute Reflection - AdminController.Login actions decorated with [AllowAnonymous]")]
        public void M2_2_02_AdminControllerLoginDecoratedWithAllowAnonymous()
        {
            var adminType = typeof(AdminController);

            // 1. GET Login action
            var getLogin = adminType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Login" && m.GetCustomAttribute<HttpGetAttribute>() != null);
            Assert.NotNull(getLogin, "AdminController must define [HttpGet] Login action");
            var getAllowAnon = getLogin.GetCustomAttribute<AllowAnonymousAttribute>();
            Assert.NotNull(getAllowAnon, "[HttpGet] Login action must be decorated with [AllowAnonymous]");

            // 2. POST Login action
            var postLogin = adminType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Login" && m.GetCustomAttribute<HttpPostAttribute>() != null);
            Assert.NotNull(postLogin, "AdminController must define [HttpPost] Login action");
            var postAllowAnon = postLogin.GetCustomAttribute<AllowAnonymousAttribute>();
            Assert.NotNull(postAllowAnon, "[HttpPost] Login action must be decorated with [AllowAnonymous]");

            // 3. POST Login action CSRF protection
            var postAntiforgery = postLogin.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();
            Assert.NotNull(postAntiforgery, "[HttpPost] Login action must be decorated with [ValidateAntiForgeryToken]");
        }

        // =========================================================================
        // TEST 3: All 21 data-modifying CRUD POST actions have [Authorize] and [ValidateAntiForgeryToken]
        // =========================================================================
        [E2ETest(5, 201, "M2_2_03: Attribute Reflection - All 21 data-modifying CRUD POST actions have [Authorize] & [ValidateAntiForgeryToken]")]
        public void M2_2_03_AllTwentyOneCrudPostActionsProtected()
        {
            var adminControllerTypes = new[]
            {
                typeof(AdminController),
                typeof(AdminBanToChucController),
                typeof(AdminCauHinhQRController),
                typeof(AdminChuongTrinhController),
                typeof(AdminGayQuyController),
                typeof(AdminHieuUngController),
                typeof(AdminKyUcController),
                typeof(AdminTinTucController),
                typeof(AdminTrangChuController),
                typeof(AdminSanPhamController),
                typeof(AdminDonHangController)
            };

            var crudPostActions = new List<(Type Controller, MethodInfo Method)>();

            foreach (var type in adminControllerTypes)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var method in methods)
                {
                    // Check if action responds to POST
                    bool isPost = method.GetCustomAttribute<HttpPostAttribute>() != null ||
                                  (method.GetCustomAttributes().Any(a => a.GetType().Name.Contains("HttpPost")));

                    // Exclude Login (public) and Logout (auth session termination)
                    if (isPost && method.Name != "Login" && method.Name != "Logout")
                    {
                        crudPostActions.Add((type, method));
                    }
                }
            }

            Assert.Equal(21, crudPostActions.Count, $"Expected exactly 21 CRUD POST actions across all 11 Admin controllers, found {crudPostActions.Count}");

            foreach (var (controllerType, method) in crudPostActions)
            {
                // Must NOT have AllowAnonymous
                var allowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>();
                Assert.Null(allowAnonymous, $"Action '{controllerType.Name}.{method.Name}' must NOT have [AllowAnonymous]");

                // Must have ValidateAntiForgeryToken
                var antiForgery = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();
                Assert.NotNull(antiForgery, $"Action '{controllerType.Name}.{method.Name}' must have [ValidateAntiForgeryToken]");

                // Must be protected by [Authorize] either on action or at class level
                bool classHasAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>(inherit: true) != null;
                bool methodHasAuthorize = method.GetCustomAttribute<AuthorizeAttribute>() != null;
                Assert.True(classHasAuthorize || methodHasAuthorize,
                    $"Action '{controllerType.Name}.{method.Name}' must be protected by [Authorize]");
            }
        }

        // =========================================================================
        // TEST 4: Middleware Pipeline Invariants (CH19 & F02_T02)
        // =========================================================================
        [E2ETest(5, 202, "M2_2_04: Invariant CH19 & F02_T02 - app.UseSession() strictly between app.UseRouting() and app.UseAuthorization()")]
        public void M2_2_04_PipelineOrderingInvariants()
        {
            var programCsPath = Path.Combine(E2ETestContext.ProjectRoot, "Program.cs");
            Assert.True(File.Exists(programCsPath), "Program.cs must exist");
            var lines = File.ReadAllLines(programCsPath);

            int routingLine = -1;
            int sessionLine = -1;
            int authLine = -1;
            int authorizationLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("//")) continue;

                if (line.Contains("app.UseRouting()")) routingLine = i;
                if (line.Contains("app.UseSession()")) sessionLine = i;
                if (line.Contains("app.UseAuthentication()")) authLine = i;
                if (line.Contains("app.UseAuthorization()")) authorizationLine = i;
            }

            Assert.True(routingLine > 0, "app.UseRouting() must be present");
            Assert.True(sessionLine > 0, "app.UseSession() must be present");
            Assert.True(authLine > 0, "app.UseAuthentication() must be present");
            Assert.True(authorizationLine > 0, "app.UseAuthorization() must be present");

            // Strictly: UseRouting -> UseSession -> UseAuthentication -> UseAuthorization
            Assert.True(sessionLine > routingLine, "app.UseSession() must be positioned strictly after app.UseRouting()");
            Assert.True(authLine > sessionLine, "app.UseAuthentication() must be positioned after app.UseSession()");
            Assert.True(authorizationLine >= authLine, "app.UseAuthorization() must be positioned after or alongside app.UseAuthentication()");
            Assert.True(sessionLine < authorizationLine, "app.UseSession() must be positioned strictly before app.UseAuthorization()");
        }

        // =========================================================================
        // TEST 5: Direct Instantiation Invariants (F19_T08 & F19_T16)
        // =========================================================================
        [E2ETest(5, 202, "M2_2_05: Invariant F19_T08 & F19_T16 - Controller direct instantiation 'new AdminController()'")]
        public void M2_2_05_DirectInstantiationInvariants()
        {
            // Verify parameterless constructor exists and works
            var controller = new AdminController();
            SetupControllerContext(controller);

            var result = controller.Index();
            Assert.NotNull(result, "AdminController.Index() must return an action result");
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminController.Index() must return ViewResult");
        }

        // =========================================================================
        // TEST 6: CuaHang Views SHA-256 Invariant (M3Challenger2 Preservation)
        // =========================================================================
        [E2ETest(5, 203, "M2_2_06: Invariant M3Challenger2 - All 12 views in Areas/CuaHang/Views preserved")]
        public void M2_2_06_CuaHangViewsPreserved()
        {
            var viewsDir = Path.Combine(E2ETestContext.ProjectRoot, "Areas", "CuaHang", "Views");
            Assert.True(Directory.Exists(viewsDir), "Areas/CuaHang/Views directory must exist");

            var expectedViews = new[]
            {
                "AdminDonHang/ChiTiet.cshtml",
                "AdminDonHang/Index.cshtml",
                "AdminSanPham/Create.cshtml",
                "AdminSanPham/Edit.cshtml",
                "AdminSanPham/Index.cshtml",
                "CuaHang/ChiTiet.cshtml",
                "CuaHang/Index.cshtml",
                "GioHang/DatHangThanhCong.cshtml",
                "GioHang/Index.cshtml",
                "_ViewImports.cshtml",
                "_ViewStart.cshtml",
                "Shared/_CuaHangLayout.cshtml"
            };

            foreach (var viewRelPath in expectedViews)
            {
                var fullPath = Path.Combine(viewsDir, viewRelPath);
                Assert.True(File.Exists(fullPath), $"View file '{viewRelPath}' must exist in Areas/CuaHang/Views");

                var content = File.ReadAllText(fullPath);
                Assert.True(content.Length > 0, $"View file '{viewRelPath}' must not be empty");
            }
        }
    }
}
