using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WEB_TinhNguyen.Areas.CuaHang.Controllers;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.Areas.CuaHang.Models;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Adversarial Challenge & Stress Harness for Milestone 2: Dynamic QR Frontend & Subsystem Display Integration (R2).
    /// Author: teamwork_challenger_m2_1 (EMPIRICAL CHALLENGER — critic, specialist)
    /// Focus:
    /// 1. Dynamic Reflection on Admin Add (Create): Store confirmation and Admin Order Inspection dynamically reflect newly added profiles.
    /// 2. Dynamic Reflection on Admin Edit (Update): Store and Admin Order views dynamically reflect modified bank, account, holder, and template.
    /// 3. Dynamic Reflection on Admin ToggleActive: Store falls through to the next active profile in priority order.
    /// 4. Fallback Resilience: When all configs are inactive or null, Store confirmation and Admin Order views maintain fallback stability without crashing.
    /// 5. Mode Switching: Custom uploaded QR image (SuDungVietQR = false) vs dynamic Napas247 VietQR URL.
    /// 6. Channel Isolation: Strict boundary enforcement between Store-only (ApDungChoCuaHang) and Donation-only (ApDungChoQuyenGop) configs.
    /// 7. Edge Cases: Non-existent order IDs, COD payment orders, URL-escaping with adversarial characters.
    /// 8. Heavy Concurrency: 50 concurrent threads interleaving Admin mutations with Store & Admin Order reads.
    /// </summary>
    public class Tier5_ChallengerM2_1_DynamicReflectionHarness
    {
        #region Test Mocks & Helpers

        private class MockWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "web_tinhnguyen_m2_chal_" + Guid.NewGuid().ToString("N"));
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
            public void SaveTempData(HttpContext context, IDictionary<string, object> values)
            {
                foreach (var kvp in values)
                {
                    _data[kvp.Key] = kvp.Value;
                }
            }
        }

        private static ApplicationDbContext CreateOfflineContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65534;Database=OfflineTestDb_ChalM2;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(options);
        }

        private static AdminCauHinhQRController CreateAdminQrController(ApplicationDbContext context)
        {
            var env = new MockWebHostEnvironment();
            if (!Directory.Exists(env.WebRootPath))
            {
                Directory.CreateDirectory(env.WebRootPath);
            }

            var controller = new AdminCauHinhQRController(context, env);
            var httpContext = new DefaultHttpContext();
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static GioHangController CreateGioHangController(ApplicationDbContext context)
        {
            var controller = new GioHangController(context);
            var httpContext = new DefaultHttpContext();
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static AdminDonHangController CreateAdminDonHangController(ApplicationDbContext context)
        {
            var controller = new AdminDonHangController(context);
            var httpContext = new DefaultHttpContext();
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static HomeController CreateHomeController(ApplicationDbContext context)
        {
            var logger = NullLogger<HomeController>.Instance;
            var controller = new HomeController(logger, context);
            var httpContext = new DefaultHttpContext();
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        #endregion

        // =========================================================================
        // SCENARIO 1: Dynamic Reflection on Admin Add (Create)
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_01: Admin creates new QR profile -> Store confirmation dynamically reflects new bank, account, and VietQR URL")]
        public async Task ADV_M2_01_AdminCreateNewQrConfig_StoreConfirmationReflectsImmediately()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminController = CreateAdminQrController(context);
            var storeController = CreateGioHangController(context);

            // Admin adds new ACB bank configuration with priority ThuTu = 0 (highest)
            var newConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản ACB Chi nhánh Hà Nội",
                MaNganHang = "ACB",
                TenNganHang = "Ngân Hàng Á Châu (ACB)",
                SoTaiKhoan = "1234567890",
                TenChuTaiKhoan = "TRAN THI HOA",
                MauVietQR = "compact2",
                SuDungVietQR = true,
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 0
            };

            var createResult = await adminController.Create(newConfig, null);
            Assert.NotNull(createResult as RedirectToActionResult, "Admin Create must redirect to Index");

            // Customer views order confirmation
            var confirmResult = await storeController.DatHangThanhCong(1);
            Assert.NotNull(confirmResult, "DatHangThanhCong must return a result");
            var viewResult = confirmResult as ViewResult;
            Assert.NotNull(viewResult, "DatHangThanhCong must return ViewResult");

            var resolvedQr = storeController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(resolvedQr, "ViewBag.CauHinhQR must not be null");
            Assert.Equal("ACB", resolvedQr!.MaNganHang, "Reflected bank code must match newly created config");
            Assert.Equal("1234567890", resolvedQr.SoTaiKhoan, "Reflected account number must match newly created config");
            Assert.Equal("TRAN THI HOA", resolvedQr.TenChuTaiKhoan, "Reflected account holder must match newly created config");

            var qrUrl = resolvedQr.GetQrUrl(100000m, "GQ2026-TEST");
            Assert.Contains("img.vietqr.io/image/ACB-1234567890-compact2.png", qrUrl, "Generated QR URL must reflect ACB bank and account number");
            Assert.Contains("accountName=TRAN%20THI%20HOA", qrUrl, "Generated QR URL must reflect account holder name");
        }

        [E2ETest(5, 8, "ADV_M2_02: Admin creates new QR profile -> Admin Order Inspection dynamically reflects new bank and account")]
        public async Task ADV_M2_02_AdminCreateNewQrConfig_AdminOrderDetailReflectsImmediately()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminQrController = CreateAdminQrController(context);
            var adminOrderController = CreateAdminDonHangController(context);

            var newConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản Vietcombank Hải Phòng",
                MaNganHang = "VCB",
                TenNganHang = "Ngoại thương Việt Nam (Vietcombank)",
                SoTaiKhoan = "0071009999999",
                TenChuTaiKhoan = "LE HOANG NAM",
                MauVietQR = "compact",
                SuDungVietQR = true,
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = true,
                ThuTu = 0
            };

            await adminQrController.Create(newConfig, null);

            var detailResult = await adminOrderController.ChiTiet(1);
            Assert.NotNull(detailResult, "ChiTiet must return a result");
            var viewResult = detailResult as ViewResult;
            Assert.NotNull(viewResult, "ChiTiet must return ViewResult");

            var resolvedQr = adminOrderController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(resolvedQr, "ViewBag.CauHinhQR must be set in ChiTiet");
            Assert.Equal("VCB", resolvedQr!.MaNganHang, "Reflected bank code must be VCB");
            Assert.Equal("0071009999999", resolvedQr.SoTaiKhoan, "Reflected account number must be 0071009999999");
            Assert.Equal("LE HOANG NAM", resolvedQr.TenChuTaiKhoan, "Reflected account holder must be LE HOANG NAM");
        }

        // =========================================================================
        // SCENARIO 2: Dynamic Reflection on Admin Edit (Update)
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_03: Admin edits existing QR profile -> Store and Admin Order Inspection dynamically reflect updated details")]
        public async Task ADV_M2_03_AdminEditExistingQrConfig_StoreAndAdminReflectUpdatedValues()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminQrController = CreateAdminQrController(context);
            var storeController = CreateGioHangController(context);
            var adminOrderController = CreateAdminDonHangController(context);

            // Fetch current default config
            var configs = DbInitializer.GetInMemoryQrConfigs();
            Assert.True(configs.Count > 0, "Initial configs must exist");
            var primaryConfig = configs.First();

            // Admin edits the config: changing bank, account, holder, and template
            primaryConfig.MaNganHang = "TCB";
            primaryConfig.TenNganHang = "Techcombank";
            primaryConfig.SoTaiKhoan = "1903888777666";
            primaryConfig.TenChuTaiKhoan = "DANG QUOC BAO";
            primaryConfig.MauVietQR = "compact";

            var editResult = await adminQrController.Edit(primaryConfig.IDCauHinhQR, primaryConfig, null);
            Assert.NotNull(editResult as RedirectToActionResult, "Edit must redirect to Index");

            // Verify Store confirmation reflects updated values
            await storeController.DatHangThanhCong(1);
            var storeQr = storeController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(storeQr, "Store ViewBag.CauHinhQR must not be null");
            Assert.Equal("TCB", storeQr!.MaNganHang, "Store must reflect updated bank TCB");
            Assert.Equal("1903888777666", storeQr.SoTaiKhoan, "Store must reflect updated account number");
            Assert.Equal("DANG QUOC BAO", storeQr.TenChuTaiKhoan, "Store must reflect updated holder");

            var storeQrUrl = storeQr.GetQrUrl(50000m, "GQ-TEST-EDIT");
            Assert.Contains("TCB-1903888777666-compact.png", storeQrUrl, "Store QR URL must use updated TCB and compact template");
            Assert.Contains("accountName=DANG%20QUOC%20BAO", storeQrUrl, "Store QR URL must use updated holder");

            // Verify Admin Order Inspection reflects updated values
            await adminOrderController.ChiTiet(1);
            var adminOrderQr = adminOrderController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(adminOrderQr, "Admin Order ViewBag.CauHinhQR must not be null");
            Assert.Equal("TCB", adminOrderQr!.MaNganHang, "Admin Order must reflect updated bank TCB");
            Assert.Equal("1903888777666", adminOrderQr.SoTaiKhoan, "Admin Order must reflect updated account number");
            Assert.Equal("DANG QUOC BAO", adminOrderQr.TenChuTaiKhoan, "Admin Order must reflect updated holder");
        }

        // =========================================================================
        // SCENARIO 3: Dynamic Reflection on Admin ToggleActive (Deactivation Handover)
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_04: Admin deactivates primary profile -> Store and Admin Order Inspection dynamically fall through to secondary profile")]
        public async Task ADV_M2_04_AdminToggleActiveToFalse_StoreFallsThroughToNextActiveConfig()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminQrController = CreateAdminQrController(context);
            var storeController = CreateGioHangController(context);
            var adminOrderController = CreateAdminDonHangController(context);

            // Config 1: Primary (ThuTu = 1, active)
            var config1 = new CauHinhThanhToanQR
            {
                TenCauHinh = "Config 1 - MBBank",
                MaNganHang = "MB",
                TenNganHang = "Quân Đội",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                ApDungChoCuaHang = true,
                TrangThaiHoatDong = true,
                ThuTu = 1
            };
            var added1 = DbInitializer.AddInMemoryQrConfig(config1);

            // Config 2: Secondary (ThuTu = 2, active)
            var config2 = new CauHinhThanhToanQR
            {
                TenCauHinh = "Config 2 - BIDV",
                MaNganHang = "BIDV",
                TenNganHang = "Đầu tư và Phát triển",
                SoTaiKhoan = "58110000123456",
                TenChuTaiKhoan = "TRAN TRUNG KIEN",
                ApDungChoCuaHang = true,
                TrangThaiHoatDong = true,
                ThuTu = 2
            };
            var added2 = DbInitializer.AddInMemoryQrConfig(config2);

            // Prior to toggle: Config 1 is active primary
            await storeController.DatHangThanhCong(1);
            var qrBefore = storeController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(qrBefore, "Before toggle, QR config must exist");
            Assert.Equal("0924044283", qrBefore!.SoTaiKhoan, "Before toggle, Config 1 account must be active");

            // Admin deactivates Config 1
            await adminQrController.ToggleActive(added1.IDCauHinhQR);

            // Store confirmation must now dynamically resolve Config 2
            await storeController.DatHangThanhCong(1);
            var qrAfterStore = storeController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(qrAfterStore, "After toggle, secondary QR config must be resolved");
            Assert.Equal("58110000123456", qrAfterStore!.SoTaiKhoan, "Store must reflect secondary config account");
            Assert.Equal("BIDV", qrAfterStore.MaNganHang, "Store must reflect secondary config bank");

            // Admin Order Inspection must also dynamically resolve Config 2
            await adminOrderController.ChiTiet(1);
            var qrAfterAdmin = adminOrderController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(qrAfterAdmin, "Admin Order must resolve secondary QR config");
            Assert.Equal("58110000123456", qrAfterAdmin!.SoTaiKhoan, "Admin Order must reflect secondary config account");
        }

        // =========================================================================
        // SCENARIO 4: Fallback Resilience When All Configs Inactive
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_05: All QR configs inactive -> Store confirmation and Admin Order Inspection maintain safe fallback defaults without crashing")]
        public async Task ADV_M2_05_AllConfigsDisabled_GracefulFallbackDefaultInViews()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminQrController = CreateAdminQrController(context);
            var storeController = CreateGioHangController(context);
            var adminOrderController = CreateAdminDonHangController(context);

            // Deactivate all existing configs in store
            var allConfigs = DbInitializer.GetInMemoryQrConfigs();
            foreach (var cfg in allConfigs)
            {
                if (cfg.TrangThaiHoatDong)
                {
                    await adminQrController.ToggleActive(cfg.IDCauHinhQR);
                }
            }

            // Verify in-memory store returns null when all are inactive
            var activeStoreQr = DbInitializer.GetActiveStoreQrConfig();
            Assert.Null(activeStoreQr, "GetActiveStoreQrConfig must return null when no config is active");

            // Invoke Store confirmation: must NOT throw NullReferenceException
            var storeResult = await storeController.DatHangThanhCong(1);
            Assert.NotNull(storeResult as ViewResult, "DatHangThanhCong must succeed even when all configs are inactive");
            Assert.Null(storeController.ViewBag.CauHinhQR, "ViewBag.CauHinhQR is null as expected");

            // Invoke Admin Order inspection: must NOT throw NullReferenceException
            var adminResult = await adminOrderController.ChiTiet(1);
            Assert.NotNull(adminResult as ViewResult, "ChiTiet must succeed even when all configs are inactive");
            Assert.Null(adminOrderController.ViewBag.CauHinhQR, "ViewBag.CauHinhQR is null as expected");
        }

        // =========================================================================
        // SCENARIO 5: Custom Uploaded QR Poster Mode (SuDungVietQR = false)
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_06: Admin configures custom QR poster image -> Views dynamically render custom image path instead of VietQR URL")]
        public async Task ADV_M2_06_AdminCustomQrImageUpload_OverridesVietQrUrl()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminQrController = CreateAdminQrController(context);
            var storeController = CreateGioHangController(context);
            var adminOrderController = CreateAdminDonHangController(context);

            var customConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản ảnh QR Standee Sự Kiện",
                MaNganHang = "MB",
                TenNganHang = "Quân Đội",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "/uploads/qr/custom_standee_langbiang.png",
                ApDungChoCuaHang = true,
                TrangThaiHoatDong = true,
                ThuTu = 0
            };

            await adminQrController.Create(customConfig, null);

            // Store confirmation
            await storeController.DatHangThanhCong(1);
            var storeQr = storeController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(storeQr, "Store QR config must exist");
            Assert.False(storeQr!.SuDungVietQR, "SuDungVietQR must be false");
            Assert.Equal("/uploads/qr/custom_standee_langbiang.png", storeQr.GetQrUrl(150000m, "GQ-CUSTOM"), "Store QR URL must return custom poster image path");

            // Admin Order inspection
            await adminOrderController.ChiTiet(1);
            var adminQr = adminOrderController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(adminQr, "Admin Order QR config must exist");
            Assert.False(adminQr!.SuDungVietQR, "SuDungVietQR must be false");
            Assert.Equal("/uploads/qr/custom_standee_langbiang.png", adminQr.GetQrUrl(150000m, "GQ-CUSTOM"), "Admin Order QR URL must return custom poster image path");
        }

        // =========================================================================
        // SCENARIO 6: Channel Filtering & Isolation Enforcement
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_07: Donation-only QR profile is strictly excluded by Store confirmation and Admin Order Inspection")]
        public async Task ADV_M2_07_DonationOnlyConfigNotPickedByStore()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminQrController = CreateAdminQrController(context);
            var storeController = CreateGioHangController(context);
            var homeController = CreateHomeController(context);

            // Config A: Donation-only with highest priority (ThuTu = 0)
            var donationOnly = new CauHinhThanhToanQR
            {
                TenCauHinh = "Kênh Quyên Góp Độc Quyền",
                MaNganHang = "VPB",
                TenNganHang = "VPBank",
                SoTaiKhoan = "999888111",
                TenChuTaiKhoan = "QUY THIEN NGUYEN",
                ApDungChoCuaHang = false,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 0
            };
            await adminQrController.Create(donationOnly, null);

            // Config B: Store profile (ThuTu = 1)
            var storeConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản Bán Hàng Gây Quỹ",
                MaNganHang = "MB",
                TenNganHang = "Quân Đội",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = true,
                ThuTu = 1
            };
            await adminQrController.Create(storeConfig, null);

            // Verify Store confirmation DOES NOT pick donationOnly despite higher priority
            await storeController.DatHangThanhCong(1);
            var storeResolved = storeController.ViewBag.CauHinhQR as CauHinhThanhToanQR;
            Assert.NotNull(storeResolved, "Store resolved config must not be null");
            Assert.Equal("MB", storeResolved!.MaNganHang, "Store must pick MB, ignoring donation-only config");
            Assert.Equal("0924044283", storeResolved.SoTaiKhoan, "Store must pick store account 0924044283");

            // Verify Donation page DOES include donationOnly config
            await homeController.GayQuy(null);
            var donationList = homeController.ViewBag.CauHinhQRDongGop as List<CauHinhThanhToanQR>;
            Assert.NotNull(donationList, "Donation configs must not be null");
            Assert.True(donationList!.Any(q => q.SoTaiKhoan == "999888111"), "Donation page must include donation-only config");
        }

        [E2ETest(5, 7, "ADV_M2_08: Store-only QR profile is strictly excluded by Donation fundraising channel")]
        public async Task ADV_M2_08_StoreOnlyConfigNotPickedByDonation()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var adminQrController = CreateAdminQrController(context);
            var homeController = CreateHomeController(context);

            // Store-only profile with highest priority (ThuTu = 0)
            var storeOnly = new CauHinhThanhToanQR
            {
                TenCauHinh = "Chuyên thu tiền bán hàng",
                MaNganHang = "ICB",
                TenNganHang = "VietinBank",
                SoTaiKhoan = "101009876543",
                TenChuTaiKhoan = "CUA HANG GAY QUY",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = true,
                ThuTu = 0
            };
            await adminQrController.Create(storeOnly, null);

            // Verify Donation page DOES NOT include store-only profile
            await homeController.GayQuy(null);
            var donationList = homeController.ViewBag.CauHinhQRDongGop as List<CauHinhThanhToanQR>;
            Assert.NotNull(donationList, "Donation configs list must exist");
            Assert.False(donationList!.Any(q => q.SoTaiKhoan == "101009876543"), "Donation page must NOT include store-only config");
        }

        // =========================================================================
        // SCENARIO 7: Edge Cases (Invalid ID, COD Payment, URL Encoding)
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_09: Non-existent order IDs handled gracefully (Redirect / 404 NotFound)")]
        public async Task ADV_M2_09_OrderNotFound_Handling()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            var storeController = CreateGioHangController(context);
            var adminOrderController = CreateAdminDonHangController(context);

            // DatHangThanhCong with invalid ID (<= 0 or non-existent)
            var storeZero = await storeController.DatHangThanhCong(0);
            var redirectZero = storeZero as RedirectToActionResult;
            Assert.NotNull(redirectZero, "DatHangThanhCong(0) must redirect");
            Assert.Equal("Index", redirectZero!.ActionName, "Must redirect to Index");

            var storeNonExistent = await storeController.DatHangThanhCong(999999);
            var redirectNonExistent = storeNonExistent as RedirectToActionResult;
            Assert.NotNull(redirectNonExistent, "DatHangThanhCong(999999) must redirect");

            // ChiTiet with invalid ID (<= 0 or non-existent)
            var adminZero = await adminOrderController.ChiTiet(0);
            Assert.NotNull(adminZero as NotFoundResult, "ChiTiet(0) must return 404 NotFound");

            var adminNonExistent = await adminOrderController.ChiTiet(999999);
            Assert.NotNull(adminNonExistent as NotFoundResult, "ChiTiet(999999) must return 404 NotFound");
        }

        [E2ETest(5, 7, "ADV_M2_10: COD order preserves active QR configuration resolution for view inspection")]
        public async Task ADV_M2_10_OrderWithCOD_PreservesQrConfigAvailability()
        {
            DbInitializer.ResetInMemoryStore();
            using var context = CreateOfflineContext();

            // Create a COD order in memory
            var codOrder = new DonHangGayQuy
            {
                HoTenNguoiMua = "Khách Hàng COD",
                SoDienThoai = "0987654321",
                DiaChiNhanHang = "123 Đường Test, TP.HCM",
                PhuongThucThanhToan = "COD",
                TrangThaiDonHang = "MoiDat",
                TrangThaiThanhToan = "ChuaThanhToan",
                TongTienHang = 120000m,
                PhiVanChuyen = 0m,
                TongThanhToan = 120000m
            };
            var savedOrder = DbInitializer.AddInMemoryOrder(codOrder);

            var storeController = CreateGioHangController(context);
            var adminOrderController = CreateAdminDonHangController(context);

            var storeResult = await storeController.DatHangThanhCong(savedOrder.IDDonHang);
            var storeView = storeResult as ViewResult;
            Assert.NotNull(storeView, "DatHangThanhCong must return ViewResult for COD order");
            Assert.NotNull(storeController.ViewBag.CauHinhQR, "ViewBag.CauHinhQR should remain available even for COD orders");

            var adminResult = await adminOrderController.ChiTiet(savedOrder.IDDonHang);
            var adminView = adminResult as ViewResult;
            Assert.NotNull(adminView, "ChiTiet must return ViewResult for COD order");
            Assert.NotNull(adminOrderController.ViewBag.CauHinhQR, "ViewBag.CauHinhQR should remain available for COD order inspection");
        }

        [E2ETest(5, 7, "ADV_M2_11: Adversarial characters in transfer memo and account holder are safely escaped")]
        public void ADV_M2_11_AdversarialCharacters_SafeUrlEscaping()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYỄN THÁI VINH <ADMIN>",
                CuPhapChuyenKhoan = "{MADON}"
            };

            var adversarialMemo = "GQ2026/01#TAG?amount=100&admin=true<script>alert(1)</script>";
            var url = config.GetQrUrl(100000m, adversarialMemo);

            // Verify adversarial characters are properly escaped in the query string
            Assert.False(url.Contains("<script>"), "Script tag must not appear unescaped in QR URL");
            Assert.Contains("GQ2026%2F01%23TAG%3Famount%3D100%26admin%3Dtrue", url, "Special characters must be percent-encoded");
            Assert.Contains("accountName=", url, "Account holder parameter must exist");
        }

        // =========================================================================
        // SCENARIO 8: High Concurrency Multithreaded Stress Test (50 Threads)
        // =========================================================================

        [E2ETest(5, 7, "ADV_M2_12: 50 concurrent tasks performing interleaved Admin mutations and Store / Admin Order reads")]
        public void ADV_M2_12_HeavyConcurrency_AdminMutations_And_SubsystemReads()
        {
            DbInitializer.ResetInMemoryStore();

            var exceptions = new ConcurrentBag<Exception>();
            int threadCount = 50;

            Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, threadId =>
            {
                try
                {
                    using var ctx = CreateOfflineContext();
                    var adminController = CreateAdminQrController(ctx);
                    var storeController = CreateGioHangController(ctx);
                    var adminOrderController = CreateAdminDonHangController(ctx);

                    if (threadId % 3 == 0)
                    {
                        // Admin Mutation: Add new profile
                        var newCfg = new CauHinhThanhToanQR
                        {
                            TenCauHinh = $"Concurrent Profile #{threadId}",
                            MaNganHang = "TCB",
                            TenNganHang = "Techcombank",
                            SoTaiKhoan = $"190000{threadId:D4}",
                            TenChuTaiKhoan = $"CONCURRENT USER {threadId}",
                            ApDungChoCuaHang = true,
                            ApDungChoQuyenGop = true,
                            TrangThaiHoatDong = true,
                            ThuTu = threadId
                        };
                        adminController.Create(newCfg, null).GetAwaiter().GetResult();
                    }
                    else if (threadId % 3 == 1)
                    {
                        // Store Confirmation Read
                        var result = storeController.DatHangThanhCong(1).GetAwaiter().GetResult();
                        Assert.NotNull(result, "Concurrent DatHangThanhCong must not return null");
                    }
                    else
                    {
                        // Admin Order Detail Read
                        var result = adminOrderController.ChiTiet(1).GetAwaiter().GetResult();
                        Assert.NotNull(result, "Concurrent ChiTiet must not return null");
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.True(exceptions.IsEmpty, $"Concurrent operations threw {exceptions.Count} exceptions: " +
                string.Join("; ", exceptions.Select(e => e.Message)));
        }
    }
}
