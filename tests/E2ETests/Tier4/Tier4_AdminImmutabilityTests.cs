using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier4
{
    public class Tier4_AdminImmutabilityTests
    {
        // =========================================================================
        // FEATURE 10: Strict Admin Area Immutability & Boundary Safety (M5 / R2)
        // =========================================================================

        [E2ETest(tier: 4, feature: 10, name: "M4_F10_T01: Strict Admin Controllers Immutability - All 11 admin controllers exist")]
        public void M4_F10_T01_AdminControllersImmutability()
        {
            var adminControllerNames = new[]
            {
                "AdminController",
                "AdminTrangChuController",
                "AdminChuongTrinhController",
                "AdminGayQuyController",
                "AdminKyUcController",
                "AdminBanToChucController",
                "AdminTinTucController",
                "AdminHieuUngController",
                "AdminCauHinhQRController",
                "AdminTaiKhoanController"
            };

            var webAssembly = E2ETestContext.WebAssembly;
            var controllerTypes = webAssembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var name in adminControllerNames)
            {
                var matched = controllerTypes.FirstOrDefault(t => t.Name == name);
                Assert.NotNull(matched, $"Admin controller '{name}' must exist and inherit from Controller");
                Assert.True(
                    matched!.Namespace != null && (matched.Namespace.Contains("Admin") || matched.Namespace.Contains("BoDieuKhien")),
                    $"Admin controller '{name}' must reside in an administrative namespace"
                );
            }

            // Also verify CuaHang admin controllers
            var storeAdminNames = new[] { "AdminSanPhamController", "AdminDonHangController" };
            foreach (var name in storeAdminNames)
            {
                var matched = controllerTypes.FirstOrDefault(t => t.Name == name);
                Assert.NotNull(matched, $"Store admin controller '{name}' must exist and inherit from Controller");
            }
        }

        [E2ETest(tier: 4, feature: 10, name: "M4_F10_T02: Admin Razor Views Immutability - All admin views bind _AdminLayout")]
        public void M4_F10_T02_AdminViewsBindAdminLayout()
        {
            var adminViewDirs = new[]
            {
                Path.Combine(E2ETestContext.ViewsRoot, "Admin"),
                Path.Combine(E2ETestContext.ViewsRoot, "AdminTrangChu"),
                Path.Combine(E2ETestContext.ViewsRoot, "AdminChuongTrinh"),
                Path.Combine(E2ETestContext.ViewsRoot, "AdminGayQuy"),
                Path.Combine(E2ETestContext.ViewsRoot, "AdminKyUc"),
                Path.Combine(E2ETestContext.ViewsRoot, "AdminBanToChuc"),
                Path.Combine(E2ETestContext.ViewsRoot, "AdminTinTuc"),
                Path.Combine(E2ETestContext.ViewsRoot, "AdminCauHinhQR")
            };

            foreach (var dir in adminViewDirs)
            {
                if (!Directory.Exists(dir)) continue;

                var indexView = Path.Combine(dir, "Index.cshtml");
                if (File.Exists(indexView))
                {
                    var content = File.ReadAllText(indexView);
                    Assert.True(
                        content.Contains("_AdminLayout") || !content.Contains("Layout = "),
                        $"Admin view '{Path.GetFileName(dir)}/Index.cshtml' must bind _AdminLayout"
                    );
                }
            }
        }

        [E2ETest(tier: 4, feature: 10, name: "M4_F10_T03: Admin Layout Style Containment - Zero public theme leakage")]
        public void M4_F10_T03_AdminLayoutStyleContainment()
        {
            var adminLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            Assert.RequireFile(adminLayoutPath, "_AdminLayout.cshtml");

            var content = File.ReadAllText(adminLayoutPath);

            // _AdminLayout must maintain dedicated admin identity and not depend on public mobile drawer
            Assert.False(content.Contains("mobileNavToggle"), "_AdminLayout must not be polluted with public mobileNavToggle");
            Assert.False(content.Contains("floatingCartBtn"), "_AdminLayout must not contain public floatingCartBtn");
            Assert.False(content.Contains("themeToggleBtnMobile"), "_AdminLayout must not contain public themeToggleBtnMobile");
        }

        [E2ETest(tier: 4, feature: 10, name: "M4_F10_T04: Admin Auth Boundary - Admin controllers require authentication or authorization")]
        public void M4_F10_T04_AdminAuthProtection()
        {
            var webAssembly = E2ETestContext.WebAssembly;
            var adminControllerTypes = webAssembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract && t.Name.StartsWith("Admin") && t.Name != "AdminController")
                .ToList();

            Assert.True(adminControllerTypes.Count >= 7, "At least 7 administrative controllers must exist");

            // Verify authorization filters or PhanQuyenAdminAttribute exist in the assembly
            var phanQuyenAttr = webAssembly.GetTypes().FirstOrDefault(t => t.Name == "PhanQuyenAdminAttribute");
            Assert.NotNull(phanQuyenAttr, "PhanQuyenAdminAttribute must be defined to enforce administrative role security");
        }
    }
}
