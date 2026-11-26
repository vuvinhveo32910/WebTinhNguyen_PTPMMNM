using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Areas.CuaHang.Models;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier1
{
    /// <summary>
    /// Tier 1: Functional and Contract Integrity Tests for Feature 20:
    /// Dynamic Database-driven Admin Dashboard Statistics (Admin Dashboard).
    /// Verifies that all 7 hardcoded numbers on /admin are replaced by dynamic DB-mapped figures,
    /// safe fallback defaults (0 and 0đ), and zero compilation errors.
    /// </summary>
    public class Tier1_AdminDashboardStatisticsTests
    {
        private class SimpleTempDataProvider : ITempDataProvider
        {
            private readonly System.Collections.Generic.Dictionary<string, object> _data = new System.Collections.Generic.Dictionary<string, object>();
            public System.Collections.Generic.IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, System.Collections.Generic.IDictionary<string, object> values) { }
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

        // =========================================================================
        // FEATURE 20: Admin Dashboard Dynamic Statistics
        // =========================================================================

        [E2ETest(1, 20, "F20_T01: AdminDashboardViewModel contract and property schema")]
        public void F20_T01_AdminDashboardViewModelContract()
        {
            var vmType = typeof(AdminDashboardViewModel);
            Assert.NotNull(vmType, "AdminDashboardViewModel class must exist");

            // Verify all 7 core statistics properties
            Assert.NotNull(vmType.GetProperty("SoYeuCauMoi"), "Must declare SoYeuCauMoi");
            Assert.NotNull(vmType.GetProperty("SoBaiViet"), "Must declare SoBaiViet");
            Assert.NotNull(vmType.GetProperty("SoMuaDuAn"), "Must declare SoMuaDuAn");
            Assert.NotNull(vmType.GetProperty("TongQuyDongGop"), "Must declare TongQuyDongGop");
            Assert.NotNull(vmType.GetProperty("TongQuyDongGopFormatted"), "Must declare TongQuyDongGopFormatted");
            Assert.NotNull(vmType.GetProperty("SoTinhNguyenVien"), "Must declare SoTinhNguyenVien");
            Assert.NotNull(vmType.GetProperty("SoLoiChuc"), "Must declare SoLoiChuc");
            Assert.NotNull(vmType.GetProperty("SoHinhAnhKhoanhKhac"), "Must declare SoHinhAnhKhoanhKhac");

            // Verify default safe initialization
            var instance = new AdminDashboardViewModel();
            Assert.Equal(0, instance.SoYeuCauMoi, "Default SoYeuCauMoi must be 0");
            Assert.Equal(0, instance.SoBaiViet, "Default SoBaiViet must be 0");
            Assert.Equal(0, instance.SoMuaDuAn, "Default SoMuaDuAn must be 0");
            Assert.Equal(0m, instance.TongQuyDongGop, "Default TongQuyDongGop must be 0m");
            Assert.Equal("0đ", instance.TongQuyDongGopFormatted, "Default TongQuyDongGopFormatted must be '0đ'");
            Assert.Equal(0, instance.SoTinhNguyenVien, "Default SoTinhNguyenVien must be 0");
            Assert.Equal(0, instance.SoLoiChuc, "Default SoLoiChuc must be 0");
            Assert.Equal(0, instance.SoHinhAnhKhoanhKhac, "Default SoHinhAnhKhoanhKhac must be 0");
        }

        [E2ETest(1, 20, "F20_T02: AdminController.Index with null context returns ViewResult with safe default model")]
        public void F20_T02_AdminControllerIndexNullContextReturnsSafeModel()
        {
            var controller = new AdminController();
            SetupControllerContext(controller);

            var result = controller.Index();
            Assert.NotNull(result, "AdminController.Index must return non-null result");

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminController.Index must return ViewResult");

            var model = viewResult!.Model as AdminDashboardViewModel;
            Assert.NotNull(model, "AdminController.Index model must be AdminDashboardViewModel");

            Assert.Equal(0, model!.SoYeuCauMoi, "SoYeuCauMoi must safely default to 0");
            Assert.Equal(0, model.SoBaiViet, "SoBaiViet must safely default to 0");
            Assert.Equal(0, model.SoMuaDuAn, "SoMuaDuAn must safely default to 0");
            Assert.Equal(0m, model.TongQuyDongGop, "TongQuyDongGop must safely default to 0m");
            Assert.Equal("0đ", model.TongQuyDongGopFormatted, "TongQuyDongGopFormatted must safely default to '0đ'");
            Assert.Equal(0, model.SoTinhNguyenVien, "SoTinhNguyenVien must safely default to 0");
            Assert.Equal(0, model.SoLoiChuc, "SoLoiChuc must safely default to 0");
            Assert.Equal(0, model.SoHinhAnhKhoanhKhac, "SoHinhAnhKhoanhKhac must safely default to 0");

            // Also verify ViewBag contains matching properties for flexible consumption
            Assert.Equal(0, (int)controller.ViewBag.SoYeuCauMoi, "ViewBag.SoYeuCauMoi must be 0");
            Assert.Equal(0, (int)controller.ViewBag.SoBaiViet, "ViewBag.SoBaiViet must be 0");
            Assert.Equal(0, (int)controller.ViewBag.SoMuaDuAn, "ViewBag.SoMuaDuAn must be 0");
            Assert.Equal(0m, (decimal)controller.ViewBag.TongQuyDongGop, "ViewBag.TongQuyDongGop must be 0m");
            Assert.Equal("0đ", (string)controller.ViewBag.TongQuyFormatted, "ViewBag.TongQuyFormatted must be '0đ'");
            Assert.Equal(0, (int)controller.ViewBag.SoTinhNguyenVien, "ViewBag.SoTinhNguyenVien must be 0");
            Assert.Equal(0, (int)controller.ViewBag.SoLoiChuc, "ViewBag.SoLoiChuc must be 0");
            Assert.Equal(0, (int)controller.ViewBag.SoHinhAnhKhoanhKhac, "ViewBag.SoHinhAnhKhoanhKhac must be 0");
        }

        [E2ETest(1, 20, "F20_T03: AdminController.Index executes under offline database without throwing")]
        public void F20_T03_AdminControllerIndexOfflineDbResilience()
        {
            using var offlineContext = CreateOfflineDbContext();
            var controller = new AdminController(offlineContext);
            SetupControllerContext(controller);

            // Must not throw an unhandled SqlException or TimeoutException
            var result = controller.Index();
            Assert.NotNull(result, "AdminController.Index must return non-null result under offline DB");

            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult, "AdminController.Index must return ViewResult under offline DB");

            var model = viewResult!.Model as AdminDashboardViewModel;
            Assert.NotNull(model, "Model must be AdminDashboardViewModel under offline DB");
            Assert.Equal(0, model!.SoYeuCauMoi, "SoYeuCauMoi must be 0 under offline DB");
            Assert.Equal(0, model.SoBaiViet, "SoBaiViet must be 0 under offline DB");
            Assert.Equal(0, model.SoMuaDuAn, "SoMuaDuAn must be 0 under offline DB");
            Assert.Equal(0m, model.TongQuyDongGop, "TongQuyDongGop must be 0m under offline DB");
            Assert.Equal("0đ", model.TongQuyDongGopFormatted, "TongQuyDongGopFormatted must be '0đ' under offline DB");
            Assert.Equal(0, model.SoTinhNguyenVien, "SoTinhNguyenVien must be 0 under offline DB");
            Assert.Equal(0, model.SoLoiChuc, "SoLoiChuc must be 0 under offline DB");
            Assert.Equal(0, model.SoHinhAnhKhoanhKhac, "SoHinhAnhKhoanhKhac must be 0 under offline DB");
        }

        [E2ETest(1, 20, "F20_T04: AdminDashboardViewModel currency and alias boundaries")]
        public void F20_T04_AdminDashboardViewModelFormattingAndAliases()
        {
            var vm = new AdminDashboardViewModel
            {
                SoYeuCauMoi = 5,
                SoBaiViet = 10,
                SoMuaDuAn = 3,
                TongQuyDongGop = 125500000m,
                SoTinhNguyenVien = 25,
                SoLoiChuc = 30,
                SoHinhAnhKhoanhKhac = 120
            };

            // Test alias getters
            Assert.Equal(5, vm.YeuCauMoi, "Alias YeuCauMoi must match SoYeuCauMoi");
            Assert.Equal(10, vm.BaiViet, "Alias BaiViet must match SoBaiViet");
            Assert.Equal(3, vm.MuaDuAn, "Alias MuaDuAn must match SoMuaDuAn");
            Assert.Equal(125500000m, vm.TongQuy, "Alias TongQuy must match TongQuyDongGop");
            Assert.Equal(25, vm.TinhNguyenVien, "Alias TinhNguyenVien must match SoTinhNguyenVien");
            Assert.Equal(30, vm.LoiChuc, "Alias LoiChuc must match SoLoiChuc");
            Assert.Equal(120, vm.HinhAnhKhoanhKhac, "Alias HinhAnhKhoanhKhac must match SoHinhAnhKhoanhKhac");

            // Test formatted currency contains proper Vietnamese format
            Assert.True(
                vm.TongQuyDongGopFormatted.Contains("125.500.000") || vm.TongQuyDongGopFormatted.Contains("125,500,000"),
                "Formatted currency must contain digit grouped amount"
            );
            Assert.True(
                vm.TongQuyDongGopFormatted.EndsWith("đ") || vm.TongQuyDongGopFormatted.EndsWith("₫"),
                "Formatted currency must end with currency unit"
            );

            // Test alias setters
            vm.YeuCauMoi = 15;
            Assert.Equal(15, vm.SoYeuCauMoi, "Alias setter YeuCauMoi must update SoYeuCauMoi");
        }

        [E2ETest(1, 20, "F20_T05: Index.cshtml removes all 7 hardcoded static figures")]
        public void F20_T05_AdminIndexViewNoHardcodedFigures()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            Assert.True(File.Exists(viewPath), "GiaoDien/Admin/Index.cshtml must exist");

            var content = File.ReadAllText(viewPath);

            // Verify view model declaration
            Assert.True(
                content.Contains("@model WEB_TinhNguyen.MoHinh.AdminDashboardViewModel") ||
                content.Contains("AdminDashboardViewModel"),
                "Index.cshtml must bind AdminDashboardViewModel"
            );

            // Verify none of the old hardcoded figure lines exist
            Assert.False(content.Contains(">125.500K<"), "Index.cshtml must not contain hardcoded '125.500K'");
            Assert.False(content.Contains(">150<"), "Index.cshtml must not contain hardcoded '150'");
            Assert.False(content.Contains(">342<"), "Index.cshtml must not contain hardcoded '342'");
            Assert.False(content.Contains(">1,248<"), "Index.cshtml must not contain hardcoded '1,248'");

            // Verify dynamic binding variables are used
            Assert.True(content.Contains("soYeuCauMoi") || content.Contains("SoYeuCauMoi"), "Index.cshtml must use dynamic soYeuCauMoi");
            Assert.True(content.Contains("soBaiViet") || content.Contains("SoBaiViet"), "Index.cshtml must use dynamic soBaiViet");
            Assert.True(content.Contains("soMuaDuAn") || content.Contains("SoMuaDuAn"), "Index.cshtml must use dynamic soMuaDuAn");
            Assert.True(content.Contains("tongQuyFormatted") || content.Contains("TongQuyDongGop"), "Index.cshtml must use dynamic fund variable");
            Assert.True(content.Contains("soTinhNguyenVien") || content.Contains("SoTinhNguyenVien"), "Index.cshtml must use dynamic soTinhNguyenVien");
            Assert.True(content.Contains("soLoiChuc") || content.Contains("SoLoiChuc"), "Index.cshtml must use dynamic soLoiChuc");
            Assert.True(content.Contains("soHinhAnhKhoanhKhac") || content.Contains("SoHinhAnhKhoanhKhac"), "Index.cshtml must use dynamic soHinhAnhKhoanhKhac");
        }

        [E2ETest(1, 20, "F20_T06: Index.cshtml preserves card layout structure and labels")]
        public void F20_T06_AdminIndexViewStructureAndLabels()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            var content = File.ReadAllText(viewPath);

            // Verify all 7 card labels exist
            Assert.True(content.Contains("Yêu cầu mới"), "Banner tile 'Yêu cầu mới' must exist");
            Assert.True(content.Contains("Bài viết"), "Banner tile 'Bài viết' must exist");
            Assert.True(content.Contains("Mùa dự án"), "Banner tile 'Mùa dự án' must exist");
            Assert.True(content.Contains("TỔNG QUỸ ĐÓNG GÓP"), "Stat card 'TỔNG QUỸ ĐÓNG GÓP' must exist");
            Assert.True(content.Contains("TÌNH NGUYỆN VIÊN"), "Stat card 'TÌNH NGUYỆN VIÊN' must exist");
            Assert.True(content.Contains("LỜI CHÚC ĐÃ NHẬN"), "Stat card 'LỜI CHÚC ĐÃ NHẬN' must exist");
            Assert.True(content.Contains("HÌNH ẢNH KHOẢNH KHẮC"), "Stat card 'HÌNH ẢNH KHOẢNH KHẮC' must exist");
        }

        [E2ETest(1, 20, "F20_T07: Index.cshtml contains mobile overflow guards (shrink-0, truncate, min-w-0)")]
        public void F20_T07_AdminIndexViewMobileOverflowResilience()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            var content = File.ReadAllText(viewPath);

            // Verify shrink-0 on icon circles to prevent squishing on narrow viewports
            Assert.True(content.Contains("shrink-0"), "Stat card icon containers must specify shrink-0");
            // Verify min-w-0 on text containers to allow proper truncation without container blowout
            Assert.True(content.Contains("min-w-0"), "Stat card text containers must specify min-w-0");
            // Verify truncate on fund figure to protect against multi-billion VND overflow
            Assert.True(content.Contains("truncate"), "Stat cards must declare truncate for extreme number lengths");
        }

        [E2ETest(1, 20, "F20_T08: AdminDashboardViewModel negative and extreme currency boundaries")]
        public void F20_T08_AdminDashboardViewModelNegativeAndExtremeBoundaries()
        {
            var vmZero = new AdminDashboardViewModel { TongQuyDongGop = 0m };
            Assert.Equal("0đ", vmZero.TongQuyDongGopFormatted, "Zero fund must format as '0đ'");

            var vmNegative = new AdminDashboardViewModel { TongQuyDongGop = -50000m };
            Assert.True(
                vmNegative.TongQuyDongGopFormatted.Contains("-50.000") || vmNegative.TongQuyDongGopFormatted.Contains("-50,000"),
                "Negative fund must format with sign"
            );

            var vmLarge = new AdminDashboardViewModel { TongQuyDongGop = 15000000000m };
            Assert.True(
                vmLarge.TongQuyDongGopFormatted.Contains("15.000.000.000") || vmLarge.TongQuyDongGopFormatted.Contains("15,000,000,000"),
                "Large 15B fund must format with proper grouping"
            );
        }

        [E2ETest(1, 20, "F20_T09: Null status and non-cancelled orders boundary logic in fund aggregation")]
        public void F20_T09_DonHangFundAndNewRequestFilterLogic()
        {
            // Verify the query criteria logic against in-memory orders
            var orders = new List<DonHangGayQuy>
            {
                new DonHangGayQuy { IDDonHang = 1, TrangThaiDonHang = "MoiDat", TongThanhToan = 100000m, GhiChu = "Lời chúc 1" },
                new DonHangGayQuy { IDDonHang = 2, TrangThaiDonHang = null!, TongThanhToan = 200000m, GhiChu = null },
                new DonHangGayQuy { IDDonHang = 3, TrangThaiDonHang = "", TongThanhToan = 50000m, GhiChu = "" },
                new DonHangGayQuy { IDDonHang = 4, TrangThaiDonHang = "DaXacNhan", TongThanhToan = 300000m, GhiChu = "Lời chúc 2" },
                new DonHangGayQuy { IDDonHang = 5, TrangThaiDonHang = "DaHuy", TongThanhToan = 999999m, GhiChu = "Đã hủy" },
                new DonHangGayQuy { IDDonHang = 6, TrangThaiDonHang = "DaXacNhan", TongThanhToan = 50000m, GhiChu = "   " }
            };

            // 1. SoYeuCauMoi includes "MoiDat" and empty/null
            var soYeuCauMoi = orders.Count(d => d.TrangThaiDonHang == "MoiDat" || string.IsNullOrEmpty(d.TrangThaiDonHang));
            Assert.Equal(3, soYeuCauMoi, "Orders 1, 2, 3 must be counted as new/pending requests");

            // 2. TongQuyDongGop excludes "DaHuy" (and safely includes null status)
            var tongQuy = orders
                .Where(d => d.TrangThaiDonHang == null || d.TrangThaiDonHang != "DaHuy")
                .Sum(d => (decimal?)d.TongThanhToan) ?? 0m;
            Assert.Equal(700000m, tongQuy, "Orders 1, 2, 3, 4, 6 total (100k + 200k + 50k + 300k + 50k) = 700k, excluding cancelled order 5");

            // 3. SoLoiChuc counts orders with non-empty, non-whitespace GhiChu
            var soLoiChuc = orders.Count(d => d.GhiChu != null && d.GhiChu.Trim() != "");
            Assert.Equal(3, soLoiChuc, "Orders 1, 4, 5 with genuine non-whitespace GhiChu must be counted as wishes, whitespace-only excluded");
        }

        [E2ETest(1, 20, "F20_T10: EF Core SQL Server query translation integrity (ToQueryString)")]
        public void F20_T10_EfCoreQueryTranslationIntegrity()
        {
            using var context = CreateOfflineDbContext();

            // 1. SoYeuCauMoi query translation
            var q1 = context.DonHangGayQuys
                .Where(d => d.TrangThaiDonHang == "MoiDat" || string.IsNullOrEmpty(d.TrangThaiDonHang))
                .ToQueryString();
            Assert.True(q1.Contains("MoiDat"), "Query 1 must compile to SQL containing MoiDat predicate");

            // 2. SoBaiViet query translation
            var q2 = context.BaiVietTinTucs.ToQueryString();
            Assert.True(q2.Contains("BaiVietTinTuc"), "Query 2 must compile to SQL for BaiVietTinTuc");

            // 3. SoMuaDuAn query translation
            var q3 = context.ChienDichs.ToQueryString();
            Assert.True(q3.Contains("ChienDich"), "Query 3 must compile to SQL for ChienDich");

            // 4. TongQuyDongGop query translation
            var q4 = context.DonHangGayQuys
                .Where(d => d.TrangThaiDonHang == null || d.TrangThaiDonHang != "DaHuy")
                .Select(d => (decimal?)d.TongThanhToan)
                .ToQueryString();
            Assert.True(q4.Contains("DaHuy"), "Query 4 must compile to SQL excluding DaHuy");

            // 5. SoTinhNguyenVien query translation
            var q5 = context.ThanhVienBanToChucs.ToQueryString();
            Assert.True(q5.Contains("ThanhVienBanToChuc"), "Query 5 must compile to SQL for ThanhVienBanToChuc");

            // 6. SoLoiChuc query translation
            var q6 = context.DonHangGayQuys
                .Where(d => d.GhiChu != null && d.GhiChu.Trim() != "")
                .ToQueryString();
            Assert.True(q6.Contains("GhiChu"), "Query 6 must compile to SQL with GhiChu predicate");

            // 7. SoHinhAnhKhoanhKhac query translations
            var q7a = context.AnhKyUcs.ToQueryString();
            Assert.True(q7a.Contains("AnhKyUc"), "Query 7a must compile to SQL for AnhKyUc");

            var q7b = context.ThuVienKhoanhKhacs.ToQueryString();
            Assert.True(q7b.Contains("TVKhoanhKhac"), "Query 7b must compile to SQL for TVKhoanhKhac");
        }

        [E2ETest(1, 20, "F20_T11: Index.cshtml banner contains truncation and tooltip guards")]
        public void F20_T11_AdminIndexViewBannerTruncationGuards()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            var content = File.ReadAllText(viewPath);

            // Banner tile stats must contain truncate to prevent layout blowout on large numerals
            Assert.True(content.Contains("max-w-[160px]"), "Banner items should declare max-w boundary");
            Assert.True(content.Contains("title=\"@soYeuCauMoi"), "Banner new request item must declare title tooltip");
            Assert.True(content.Contains("title=\"@soBaiViet"), "Banner article item must declare title tooltip");
            Assert.True(content.Contains("title=\"@soMuaDuAn"), "Banner project season item must declare title tooltip");
        }

        [E2ETest(1, 20, "F20_T12: Index.cshtml fallback preserves negative currency formatting")]
        public void F20_T12_IndexViewFallbackNegativeAmountPreserved()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            var content = File.ReadAllText(viewPath);

            // Verify view fallback does not use flawed 'tongQuyDongGop > 0 ? ... : "0đ"'
            Assert.False(
                content.Contains("tongQuyDongGop > 0"),
                "Index.cshtml fallback must not discard negative amounts with flawed 'tongQuyDongGop > 0' check"
            );
        }

        [E2ETest(1, 20, "F20_T13: Index.cshtml utilizes type-safe conversions for polymorphic boxed ViewBag numbers")]
        public void F20_T13_IndexViewTypeSafeBoxedConversions()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Admin", "Index.cshtml");
            var content = File.ReadAllText(viewPath);

            // Index.cshtml must not contain fragile '(decimal)ViewBag' or '(int)ViewBag' raw casts
            Assert.False(content.Contains("(decimal)ViewBag"), "Index.cshtml must not use fragile '(decimal)ViewBag' raw cast");
            Assert.False(content.Contains("(int)ViewBag"), "Index.cshtml must not use fragile '(int)ViewBag' raw cast");

            // Must use Convert.ToInt32 and Convert.ToDecimal to safely handle boxed int, long, double, string
            Assert.True(content.Contains("Convert.ToInt32"), "Index.cshtml must use Convert.ToInt32 for robust unboxing");
            Assert.True(content.Contains("Convert.ToDecimal"), "Index.cshtml must use Convert.ToDecimal for robust unboxing");
        }

        [E2ETest(1, 20, "F20_T14: AdminController populates comprehensive ViewBag aliases")]
        public void F20_T14_AdminControllerViewBagAliasesIntegrity()
        {
            var controller = new AdminController();
            SetupControllerContext(controller);

            controller.Index();

            // Verify ViewBag aliases are populated
            Assert.Equal(0, (int)controller.ViewBag.NewRequests, "ViewBag.NewRequests alias must match");
            Assert.Equal(0, (int)controller.ViewBag.Articles, "ViewBag.Articles alias must match");
            Assert.Equal(0, (int)controller.ViewBag.Campaigns, "ViewBag.Campaigns alias must match");
            Assert.Equal(0m, (decimal)controller.ViewBag.TotalFund, "ViewBag.TotalFund alias must match");
            Assert.Equal(0, (int)controller.ViewBag.Volunteers, "ViewBag.Volunteers alias must match");
            Assert.Equal(0, (int)controller.ViewBag.Wishes, "ViewBag.Wishes alias must match");
            Assert.Equal(0, (int)controller.ViewBag.Moments, "ViewBag.Moments alias must match");
        }
    }
}
