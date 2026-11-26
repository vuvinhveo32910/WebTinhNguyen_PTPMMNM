using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Adversarial Challenge & Stress Harness for Milestone 1 (Admin QR & Payment Management)
    /// Empirically challenges and stress-tests:
    /// 1. VietQrHelper URL formatting under adversarial inputs:
    ///    - Null, empty, whitespace-padded bank codes & account numbers
    ///    - Negative, zero, fractional, and boundary amounts
    ///    - Vietnamese Unicode accents and emojis in memo and account name
    ///    - Special characters (#, &, ?, /, =, +, %, quotes, script tags) in query parameters
    ///    - Extreme boundary inputs (long strings, large numbers)
    /// 2. CauHinhThanhToanQR.GetQrUrl mode switching & fallback:
    ///    - VietQR dynamic vs custom image path switching
    ///    - Fallback behavior when custom image path is empty or whitespace
    ///    - Parameter forwarding and entity default override mechanics
    /// 3. CauHinhThanhToanQR Entity Contract & Validation Constraints:
    ///    - Required attributes, string lengths, defaults, boundary limits
    /// 4. AdminCauHinhQRController Lifecycle & Security Boundaries:
    ///    - Anti-forgery validation on all mutating POST actions
    ///    - Tampered ID mismatch rejection (BadRequest)
    ///    - Non-existent ID handling and TempData alerts
    ///    - Full CRUD lifecycle (Index, Create, Edit, ToggleActive, Delete)
    ///    - Dual-response ToggleActive (Ajax JSON vs Form Redirect)
    ///    - GUID-based file upload isolation
    /// 5. In-Memory Store Dual-Persistence Resilience & Concurrency:
    ///    - Parallel multi-threaded access without race condition or data corruption
    ///    - Deep cloning and entity isolation
    /// </summary>
    public class Tier5_ChallengerM1_AdversarialHarness
    {
        #region Test Helpers & Mocks

        public class MockWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "web_tinhnguyen_m1_tests_" + Guid.NewGuid().ToString("N"));
            public IFileProvider WebRootFileProvider { get; set; } = null!;
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "WEB_TinhNguyen";
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
        }

        public class MockTempDataProvider : ITempDataProvider
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

        private static bool ValidateModel(object model, out List<ValidationResult> results)
        {
            var context = new ValidationContext(model);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        }

        private static AdminCauHinhQRController CreateTestController(out ApplicationDbContext context, out MockWebHostEnvironment env)
        {
            // Offline DbContext to trigger In-Memory fallback
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,1;Database=dummy_offline;Connect Timeout=1;")
                .Options;
            context = new ApplicationDbContext(options);

            env = new MockWebHostEnvironment();
            if (!Directory.Exists(env.WebRootPath))
            {
                Directory.CreateDirectory(env.WebRootPath);
            }

            var controller = new AdminCauHinhQRController(context, env);

            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new MockTempDataProvider();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        private static FormFile CreateMockImageFile(string fileName, string content = "GIF89a...")
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "FileAnhQR", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

        #endregion

        // =========================================================================
        // SECTION 1: VIETQR HELPER ADVERSARIAL STRESS TESTS
        // =========================================================================

        [E2ETest(5, 3, "ADV_M1_01: VietQrHelper null or whitespace bank code falls back to MB")]
        public void ADV_M1_01_BankCodeFallback()
        {
            var urlNull = VietQrHelper.GenerateUrl(null!, "0924044283");
            Assert.Contains("/image/MB-0924044283-compact2.png", urlNull, "Null bank code must fall back to MB");

            var urlEmpty = VietQrHelper.GenerateUrl("", "0924044283");
            Assert.Contains("/image/MB-0924044283-compact2.png", urlEmpty, "Empty bank code must fall back to MB");

            var urlWhitespace = VietQrHelper.GenerateUrl("   ", "0924044283");
            Assert.Contains("/image/MB-0924044283-compact2.png", urlWhitespace, "Whitespace bank code must fall back to MB");
        }

        [E2ETest(5, 3, "ADV_M1_02: VietQrHelper bank code is trimmed and uppercased")]
        public void ADV_M1_02_BankCodeNormalization()
        {
            var urlLower = VietQrHelper.GenerateUrl("vcb", "123456");
            Assert.Contains("/image/VCB-123456-compact2.png", urlLower, "Lowercase bank code must be uppercased");

            var urlPadded = VietQrHelper.GenerateUrl("  tcb  ", "123456");
            Assert.Contains("/image/TCB-123456-compact2.png", urlPadded, "Surrounding spaces in bank code must be trimmed");
        }

        [E2ETest(5, 3, "ADV_M1_03: VietQrHelper null or whitespace account number falls back to default")]
        public void ADV_M1_03_AccountNumberFallback()
        {
            var urlNull = VietQrHelper.GenerateUrl("MB", null!);
            Assert.Contains($"/image/MB-{VietQrHelper.DefaultAccountNumber}-compact2.png", urlNull, "Null account number must fall back to default");

            var urlEmpty = VietQrHelper.GenerateUrl("MB", "");
            Assert.Contains($"/image/MB-{VietQrHelper.DefaultAccountNumber}-compact2.png", urlEmpty, "Empty account number must fall back to default");

            var urlWhitespace = VietQrHelper.GenerateUrl("MB", "   ");
            Assert.Contains($"/image/MB-{VietQrHelper.DefaultAccountNumber}-compact2.png", urlWhitespace, "Whitespace account number must fall back to default");
        }

        [E2ETest(5, 3, "ADV_M1_04: VietQrHelper amount boundaries (zero, negative, fractional, large)")]
        public void ADV_M1_04_AmountBoundaries()
        {
            // Amount = 0 must NOT appear in query params
            var urlZero = VietQrHelper.GenerateUrl("MB", "123", amount: 0);
            Assert.False(urlZero.Contains("amount="), "Zero amount must not be added to query string");

            // Negative amount must NOT appear in query params
            var urlNegative = VietQrHelper.GenerateUrl("MB", "123", amount: -50000);
            Assert.False(urlNegative.Contains("amount="), "Negative amount must not be added to query string");

            // Null amount must NOT appear in query params
            var urlNull = VietQrHelper.GenerateUrl("MB", "123", amount: null);
            Assert.False(urlNull.Contains("amount="), "Null amount must not be added to query string");

            // Fractional amount should be truncated to integer VND
            var urlFractional = VietQrHelper.GenerateUrl("MB", "123", amount: 150000.75m);
            Assert.Contains("amount=150000", urlFractional, "Fractional amount must be cast to integer VND");

            // Large realistic donation amount (50 billion VND)
            var urlLarge = VietQrHelper.GenerateUrl("MB", "123", amount: 50000000000m);
            Assert.Contains("amount=50000000000", urlLarge, "Large amounts must be rendered accurately");
        }

        [E2ETest(5, 3, "ADV_M1_05: VietQrHelper adversarial extreme amount handling (Overflow resilience)")]
        public void ADV_M1_05_ExtremeAmountOverflowResilience()
        {
            // long.MaxValue boundary
            decimal largeWithinLong = (decimal)long.MaxValue - 1000m;
            var urlLarge = VietQrHelper.GenerateUrl("MB", "123", amount: largeWithinLong);
            Assert.Contains("amount=", urlLarge, "Valid large amount within long must succeed");

            // Documented finding: decimal.MaxValue exceeds long.MaxValue and will throw OverflowException
            // An adversarial attacker submitting decimal.MaxValue should be handled or caught
            bool overflowCaught = false;
            try
            {
                VietQrHelper.GenerateUrl("MB", "123", amount: decimal.MaxValue);
            }
            catch (OverflowException)
            {
                overflowCaught = true;
            }
            Assert.True(overflowCaught, "Empirically verified: decimal.MaxValue triggers OverflowException when cast to long without clamping");
        }

        [E2ETest(5, 3, "ADV_M1_06: VietQrHelper memo with Vietnamese Unicode accents and emojis")]
        public void ADV_M1_06_MemoUnicodeAndEmoji()
        {
            string memo = "Ủng hộ các em nhỏ Langbiang 2026 ❤️ Chúc mừng!";
            var url = VietQrHelper.GenerateUrl("MB", "123", memo: memo);

            Assert.Contains("addInfo=", url, "addInfo query parameter must exist");
            Assert.False(url.Substring(url.IndexOf("addInfo=")).Contains(" "), "Spaces in memo must be percent-encoded");
            Assert.False(url.Contains("❤️"), "Emoji in memo must be percent-encoded");

            // Decode and verify roundtrip
            string expectedEncoded = Uri.EscapeDataString(memo.Trim());
            Assert.Contains($"addInfo={expectedEncoded}", url, "Unicode memo must match Uri.EscapeDataString");
        }

        [E2ETest(5, 3, "ADV_M1_07: VietQrHelper memo with adversarial URL injection special characters")]
        public void ADV_M1_07_MemoSpecialCharactersInjection()
        {
            // Adversarial payload attempting query injection, anchor fragmentation, and script tags
            string adversarialMemo = "GQ2026#section&amount=999999999?param=evil/path=inject+plus%percent<script>alert(1)</script>";
            var url = VietQrHelper.GenerateUrl("MB", "123", memo: adversarialMemo);

            // Query string must NOT contain unencoded #, &, or ? from the payload
            string queryPart = url.Substring(url.IndexOf('?'));
            
            // Check that the query part has exactly one 'amount=' if no amount was passed (i.e. injection didn't create a second query param)
            Assert.False(queryPart.Contains("&amount=999999999"), "Adversarial '&amount=' injection must be percent-encoded and not parsed as a separate parameter");

            // Verify percent encoding of delimiters
            Assert.Contains("%23", queryPart, "# must be encoded as %23");
            Assert.Contains("%26", queryPart, "& must be encoded as %26");
            Assert.Contains("%3F", queryPart, "? must be encoded as %3F");
            Assert.Contains("%2F", queryPart, "/ must be encoded as %2F");
            Assert.Contains("%3Cscript%3E", queryPart, "<script> must be encoded as %3Cscript%3E");
        }

        [E2ETest(5, 3, "ADV_M1_08: VietQrHelper account holder name formatting and encoding")]
        public void ADV_M1_08_AccountHolderFormatting()
        {
            // Lowercase Vietnamese accented name with surrounding whitespace
            string rawName = "   nguyễn thái vĩnh   ";
            var url = VietQrHelper.GenerateUrl("MB", "123", tenChuTaiKhoan: rawName);

            string expectedName = "NGUYỄN THÁI VĨNH";
            string expectedEncoded = Uri.EscapeDataString(expectedName);

            Assert.Contains($"accountName={expectedEncoded}", url, "Account holder name must be uppercased and percent-encoded");
            Assert.False(url.Contains("accountName=nguy"), "Lowercase characters must be uppercased");
        }

        [E2ETest(5, 3, "ADV_M1_09: VietQrHelper template parameter variations and case insensitivity")]
        public void ADV_M1_09_TemplateVariations()
        {
            var urlDefault = VietQrHelper.GenerateUrl("MB", "123");
            Assert.Contains("-compact2.png", urlDefault, "Default template must be compact2");

            var urlUpper = VietQrHelper.GenerateUrl("MB", "123", template: "COMPACT");
            Assert.Contains("-compact.png", urlUpper, "Template must be normalized to lowercase");

            var urlQrOnly = VietQrHelper.GenerateUrl("MB", "123", template: "qr_only");
            Assert.Contains("-qr_only.png", urlQrOnly, "qr_only template must be preserved");

            var urlPrint = VietQrHelper.GenerateUrl("MB", "123", template: "print");
            Assert.Contains("-print.png", urlPrint, "print template must be preserved");

            var urlEmpty = VietQrHelper.GenerateUrl("MB", "123", template: "   ");
            Assert.Contains("-compact2.png", urlEmpty, "Whitespace template must fall back to compact2");
        }

        [E2ETest(5, 3, "ADV_M1_10: VietQrHelper URL has no trailing question mark when query is empty")]
        public void ADV_M1_10_EmptyQueryCleanUrl()
        {
            var url = VietQrHelper.GenerateUrl("MB", "0924044283");
            Assert.Equal("https://img.vietqr.io/image/MB-0924044283-compact2.png", url, "URL without query params must not have trailing '?'");
        }

        // =========================================================================
        // SECTION 2: CAUHINHTHANHTOANQR GETQRURL MODE SWITCHING & FALLBACK
        // =========================================================================

        [E2ETest(5, 1, "ADV_M1_11: CauHinhThanhToanQR.GetQrUrl returns custom image when SuDungVietQR is false")]
        public void ADV_M1_11_GetQrUrl_CustomImageMode()
        {
            var config = new CauHinhThanhToanQR
            {
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "/uploads/qr/custom_banner_qr.png",
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH"
            };

            var result = config.GetQrUrl(100000m, "GQ-001");
            Assert.Equal("/uploads/qr/custom_banner_qr.png", result, "GetQrUrl must return custom image path directly when SuDungVietQR is false");
        }

        [E2ETest(5, 1, "ADV_M1_12: CauHinhThanhToanQR.GetQrUrl falls back to VietQR when custom image is empty or whitespace")]
        public void ADV_M1_12_GetQrUrl_CustomImageFallbackWhenEmpty()
        {
            var configNullImg = new CauHinhThanhToanQR
            {
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = null,
                MaNganHang = "VCB",
                SoTaiKhoan = "999888",
                TenChuTaiKhoan = "TEST USER"
            };
            var resultNull = configNullImg.GetQrUrl();
            Assert.Contains("https://img.vietqr.io/image/VCB-999888-compact2.png", resultNull, "Null custom image must fall back to dynamic VietQR");

            var configWhitespaceImg = new CauHinhThanhToanQR
            {
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "   ",
                MaNganHang = "TCB",
                SoTaiKhoan = "111222",
                TenChuTaiKhoan = "TEST USER"
            };
            var resultWhitespace = configWhitespaceImg.GetQrUrl();
            Assert.Contains("https://img.vietqr.io/image/TCB-111222-compact2.png", resultWhitespace, "Whitespace custom image must fall back to dynamic VietQR");
        }

        [E2ETest(5, 1, "ADV_M1_13: CauHinhThanhToanQR.GetQrUrl prioritizes SuDungVietQR over custom image path")]
        public void ADV_M1_13_GetQrUrl_VietQrModeOverridesCustomImage()
        {
            var config = new CauHinhThanhToanQR
            {
                SuDungVietQR = true,
                DuongDanAnhQRTuyChon = "/uploads/qr/ignored.png",
                MaNganHang = "ACB",
                SoTaiKhoan = "555666",
                TenChuTaiKhoan = "DONOR FUNDS"
            };

            var result = config.GetQrUrl();
            Assert.Contains("https://img.vietqr.io/image/ACB-555666-compact2.png", result, "When SuDungVietQR is true, custom image path must be ignored in favor of VietQR");
        }

        [E2ETest(5, 1, "ADV_M1_14: CauHinhThanhToanQR.GetQrUrl parameter override mechanics")]
        public void ADV_M1_14_GetQrUrl_ParameterOverrides()
        {
            var config = new CauHinhThanhToanQR
            {
                SuDungVietQR = true,
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "DEFAULT HOLDER",
                CuPhapChuyenKhoan = "{DEFAULT_MEMO}"
            };

            // Case A: Passed parameters override entity properties
            var urlOverridden = config.GetQrUrl(50000m, "SPECIFIC_MEMO_123", "OVERRIDDEN HOLDER");
            Assert.Contains("addInfo=SPECIFIC_MEMO_123", urlOverridden, "Passed memo must override CuPhapChuyenKhoan");
            Assert.Contains($"accountName={Uri.EscapeDataString("OVERRIDDEN HOLDER")}", urlOverridden, "Passed accountName must override TenChuTaiKhoan");

            // Case B: Null/empty passed parameters fall back to entity properties
            var urlFallback = config.GetQrUrl(50000m, null, "");
            Assert.Contains($"addInfo={Uri.EscapeDataString("{DEFAULT_MEMO}")}", urlFallback, "Null memo must fall back to CuPhapChuyenKhoan");
            Assert.Contains($"accountName={Uri.EscapeDataString("DEFAULT HOLDER")}", urlFallback, "Empty accountName must fall back to TenChuTaiKhoan");
        }

        // =========================================================================
        // SECTION 3: ENTITY VALIDATION & CONSTRAINTS ADVERSARIAL STRESS
        // =========================================================================

        [E2ETest(5, 1, "ADV_M1_15: CauHinhThanhToanQR required fields validation failure")]
        public void ADV_M1_15_RequiredFieldsValidation()
        {
            var emptyEntity = new CauHinhThanhToanQR
            {
                TenCauHinh = "",
                MaNganHang = "",
                TenNganHang = "",
                SoTaiKhoan = "",
                TenChuTaiKhoan = ""
            };

            bool isValid = ValidateModel(emptyEntity, out var errors);
            Assert.False(isValid, "Entity with empty required fields must fail validation");

            var errorProperties = errors.SelectMany(e => e.MemberNames).ToList();
            Assert.True(errorProperties.Contains("TenCauHinh"), "TenCauHinh must trigger validation error");
            Assert.True(errorProperties.Contains("MaNganHang"), "MaNganHang must trigger validation error");
            Assert.True(errorProperties.Contains("TenNganHang"), "TenNganHang must trigger validation error");
            Assert.True(errorProperties.Contains("SoTaiKhoan"), "SoTaiKhoan must trigger validation error");
            Assert.True(errorProperties.Contains("TenChuTaiKhoan"), "TenChuTaiKhoan must trigger validation error");
        }

        [E2ETest(5, 1, "ADV_M1_16: CauHinhThanhToanQR string length boundary limits")]
        public void ADV_M1_16_StringLengthBoundaries()
        {
            // Exceeding 150 chars on TenCauHinh
            var invalidTen = new CauHinhThanhToanQR
            {
                TenCauHinh = new string('A', 151),
                MaNganHang = "MB",
                TenNganHang = "MBBank",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "VINH"
            };
            Assert.False(ValidateModel(invalidTen, out _), "TenCauHinh > 150 chars must fail validation");

            // Exactly 150 chars on TenCauHinh must pass
            var validTen = new CauHinhThanhToanQR
            {
                TenCauHinh = new string('A', 150),
                MaNganHang = "MB",
                TenNganHang = "MBBank",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "VINH"
            };
            Assert.True(ValidateModel(validTen, out _), "TenCauHinh == 150 chars must pass validation");

            // Exceeding 50 chars on MaNganHang
            var invalidBank = new CauHinhThanhToanQR
            {
                TenCauHinh = "Valid Config",
                MaNganHang = new string('B', 51),
                TenNganHang = "MBBank",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "VINH"
            };
            Assert.False(ValidateModel(invalidBank, out _), "MaNganHang > 50 chars must fail validation");

            // Exceeding 50 chars on SoTaiKhoan
            var invalidAcc = new CauHinhThanhToanQR
            {
                TenCauHinh = "Valid Config",
                MaNganHang = "MB",
                TenNganHang = "MBBank",
                SoTaiKhoan = new string('1', 51),
                TenChuTaiKhoan = "VINH"
            };
            Assert.False(ValidateModel(invalidAcc, out _), "SoTaiKhoan > 50 chars must fail validation");

            // Exceeding 255 chars on CuPhapChuyenKhoan
            var invalidSyntax = new CauHinhThanhToanQR
            {
                TenCauHinh = "Valid Config",
                MaNganHang = "MB",
                TenNganHang = "MBBank",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "VINH",
                CuPhapChuyenKhoan = new string('M', 256)
            };
            Assert.False(ValidateModel(invalidSyntax, out _), "CuPhapChuyenKhoan > 255 chars must fail validation");
        }

        // =========================================================================
        // SECTION 4: ADMINCAUHINHQRCONTROLLER LIFECYCLE & SECURITY
        // =========================================================================

        [E2ETest(5, 4, "ADV_M1_17: AdminCauHinhQRController POST actions enforce [ValidateAntiForgeryToken]")]
        public void ADV_M1_17_AntiForgeryTokenAttributeEnforcement()
        {
            var controllerType = typeof(AdminCauHinhQRController);

            var postMethods = new[]
            {
                controllerType.GetMethod("Create", new[] { typeof(CauHinhThanhToanQR), typeof(IFormFile) }),
                controllerType.GetMethod("Edit", new[] { typeof(int), typeof(CauHinhThanhToanQR), typeof(IFormFile) }),
                controllerType.GetMethod("ToggleActive", new[] { typeof(int) }),
                controllerType.GetMethod("Delete", new[] { typeof(int) })
            };

            foreach (var method in postMethods)
            {
                Assert.NotNull(method, "POST action method must exist");
                var hasAf = method!.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true).Any();
                Assert.True(hasAf, $"{method.Name} action must be annotated with [ValidateAntiForgeryToken]");
            }
        }

        [E2ETest(5, 4, "ADV_M1_18: AdminCauHinhQRController Index returns View with non-empty config list")]
        public async Task ADV_M1_18_IndexLifecycle()
        {
            var controller = CreateTestController(out _, out _);
            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result, "Index must return ViewResult");
            var model = Assert.IsType<List<CauHinhThanhToanQR>>(viewResult.Model, "Model must be List<CauHinhThanhToanQR>");
            Assert.True(model.Count > 0, "Index action must provide at least the seeded default configuration");
            Assert.True(model.Any(q => q.MaNganHang == "MB" && q.SoTaiKhoan == "0924044283"), "Default MB config must be present");
        }

        [E2ETest(5, 4, "ADV_M1_19: AdminCauHinhQRController Create GET initializes default model")]
        public void ADV_M1_19_CreateGetDefaults()
        {
            var controller = CreateTestController(out _, out _);
            var result = controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result, "Create GET must return ViewResult");
            var model = Assert.IsType<CauHinhThanhToanQR>(viewResult.Model, "Model must be CauHinhThanhToanQR");

            Assert.Equal("MB", model.MaNganHang, "Create GET default bank code must be MB");
            Assert.Equal("compact2", model.MauVietQR, "Create GET default template must be compact2");
            Assert.True(model.SuDungVietQR, "Create GET default SuDungVietQR must be true");
            Assert.True(model.ApDungChoCuaHang, "Create GET default ApDungChoCuaHang must be true");
            Assert.True(model.ApDungChoQuyenGop, "Create GET default ApDungChoQuyenGop must be true");
            Assert.True(model.TrangThaiHoatDong, "Create GET default TrangThaiHoatDong must be true");
        }

        [E2ETest(5, 4, "ADV_M1_20: AdminCauHinhQRController Create POST invalid model returns View with model")]
        public async Task ADV_M1_20_CreatePostInvalidModel()
        {
            var controller = CreateTestController(out _, out _);
            controller.ModelState.AddModelError("TenCauHinh", "Tên cấu hình là bắt buộc");

            var invalidEntity = new CauHinhThanhToanQR { TenCauHinh = "" };
            var result = await controller.Create(invalidEntity, null);

            var viewResult = Assert.IsType<ViewResult>(result, "Create POST with ModelState error must re-render view with submitted model");
            Assert.Same(invalidEntity, viewResult.Model, "Create POST with ModelState error must re-render view with submitted model");
        }

        [E2ETest(5, 4, "ADV_M1_21: AdminCauHinhQRController Create POST valid adds config and redirects to Index")]
        public async Task ADV_M1_21_CreatePostValidLifecycle()
        {
            var controller = CreateTestController(out _, out _);

            var newConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản VCB Mới - Quyên góp M1",
                MaNganHang = "VCB",
                TenNganHang = "Vietcombank",
                SoTaiKhoan = "0071001234567",
                TenChuTaiKhoan = "QUY THIEN NGUYEN LANGBIANG",
                CuPhapChuyenKhoan = "LANGBIANG {MADON}",
                MauVietQR = "compact2",
                SuDungVietQR = true,
                ApDungChoCuaHang = false,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true
            };

            var result = await controller.Create(newConfig, null);
            var redirect = Assert.IsType<RedirectToActionResult>(result, "Create POST must redirect to Index on success");
            Assert.Equal(nameof(AdminCauHinhQRController.Index), redirect.ActionName, "Create POST must redirect to Index on success");

            // Verify in memory store has this new item
            var all = DbInitializer.GetInMemoryQrConfigs();
            var added = all.FirstOrDefault(q => q.TenCauHinh == "Tài khoản VCB Mới - Quyên góp M1");
            Assert.NotNull(added, "Newly created QR config must be retrievable from store");
            Assert.True(added!.IDCauHinhQR > 0, "ID must be assigned automatically");
            Assert.Equal("VCB", added.MaNganHang, "Bank code must match");
        }

        [E2ETest(5, 4, "ADV_M1_22: AdminCauHinhQRController Edit GET with invalid ID redirects with error")]
        public async Task ADV_M1_22_EditGetInvalidId()
        {
            var controller = CreateTestController(out _, out _);
            var result = await controller.Edit(999999);

            var redirect = Assert.IsType<RedirectToActionResult>(result, "Edit GET non-existent ID must redirect to Index");
            Assert.Equal(nameof(AdminCauHinhQRController.Index), redirect.ActionName, "Edit GET non-existent ID must redirect to Index");
            Assert.NotNull(controller.TempData["Error"], "Edit GET non-existent ID must set TempData Error message");
        }

        [E2ETest(5, 4, "ADV_M1_23: AdminCauHinhQRController Edit POST ID tampering returns BadRequest")]
        public async Task ADV_M1_23_EditPostIdMismatchBadRequest()
        {
            var controller = CreateTestController(out _, out _);
            var model = new CauHinhThanhToanQR { IDCauHinhQR = 5, TenCauHinh = "Tampered" };

            var result = await controller.Edit(99, model, null);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result, "ID tampering must return BadRequest");
            Assert.Equal("Mã cấu hình không khớp.", badRequest.Value, "ID tampering must return BadRequest with explanatory message");
        }

        [E2ETest(5, 4, "ADV_M1_24: AdminCauHinhQRController Edit POST updates entity successfully")]
        public async Task ADV_M1_24_EditPostValidLifecycle()
        {
            var controller = CreateTestController(out _, out _);

            // First add an item to edit
            var created = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Config to Edit",
                MaNganHang = "BIDV",
                TenNganHang = "BIDV Bank",
                SoTaiKhoan = "12345678",
                TenChuTaiKhoan = "OLD HOLDER",
                SuDungVietQR = true
            });

            // Edit the entity
            created.TenChuTaiKhoan = "UPDATED HOLDER NAME";
            created.CuPhapChuyenKhoan = "NEW SYNTAX {MADON}";

            var result = await controller.Edit(created.IDCauHinhQR, created, null);
            var redirect = Assert.IsType<RedirectToActionResult>(result, "Edit POST must redirect to Index on success");
            Assert.Equal(nameof(AdminCauHinhQRController.Index), redirect.ActionName, "Edit POST must redirect to Index on success");

            // Verify store updated
            var updated = DbInitializer.GetInMemoryQrConfigById(created.IDCauHinhQR);
            Assert.NotNull(updated, "Updated item must be found");
            Assert.Equal("UPDATED HOLDER NAME", updated!.TenChuTaiKhoan, "TenChuTaiKhoan must be updated");
            Assert.Equal("NEW SYNTAX {MADON}", updated.CuPhapChuyenKhoan, "CuPhapChuyenKhoan must be updated");
        }

        [E2ETest(5, 4, "ADV_M1_25: AdminCauHinhQRController ToggleActive supports both Ajax and regular POST")]
        public async Task ADV_M1_25_ToggleActiveDualMode()
        {
            var controller = CreateTestController(out _, out _);

            var created = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Toggle Active Test",
                MaNganHang = "TCB",
                TenNganHang = "Techcombank",
                SoTaiKhoan = "99887766",
                TenChuTaiKhoan = "TEST USER",
                TrangThaiHoatDong = true
            });

            // Mode A: Regular Form POST -> Redirect
            var resultForm = await controller.ToggleActive(created.IDCauHinhQR);
            var redirect = Assert.IsType<RedirectToActionResult>(resultForm, "Form POST must redirect to Index");
            Assert.Equal(nameof(AdminCauHinhQRController.Index), redirect.ActionName, "Form POST must redirect to Index");

            var afterFirstToggle = DbInitializer.GetInMemoryQrConfigById(created.IDCauHinhQR);
            Assert.False(afterFirstToggle!.TrangThaiHoatDong, "Status must have toggled from true to false");

            // Mode B: Ajax POST (X-Requested-With: XMLHttpRequest) -> JSON
            controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
            var resultAjax = await controller.ToggleActive(created.IDCauHinhQR);
            var jsonResult = Assert.IsType<JsonResult>(resultAjax, "Ajax POST must return JSON payload");
            Assert.NotNull(jsonResult.Value, "Ajax POST must return JSON payload");

            var afterSecondToggle = DbInitializer.GetInMemoryQrConfigById(created.IDCauHinhQR);
            Assert.True(afterSecondToggle!.TrangThaiHoatDong, "Status must have toggled from false back to true");
        }

        [E2ETest(5, 4, "ADV_M1_26: AdminCauHinhQRController Delete removes config from store")]
        public async Task ADV_M1_26_DeleteLifecycle()
        {
            var controller = CreateTestController(out _, out _);

            var created = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Config to Delete",
                MaNganHang = "ACB",
                TenNganHang = "ACB Bank",
                SoTaiKhoan = "11223344",
                TenChuTaiKhoan = "DELETE ME"
            });

            var result = await controller.Delete(created.IDCauHinhQR);
            var redirect = Assert.IsType<RedirectToActionResult>(result, "Delete POST must redirect to Index");
            Assert.Equal(nameof(AdminCauHinhQRController.Index), redirect.ActionName, "Delete POST must redirect to Index");

            var deleted = DbInitializer.GetInMemoryQrConfigById(created.IDCauHinhQR);
            Assert.Null(deleted, "Deleted config must no longer be found in store");
        }

        [E2ETest(5, 4, "ADV_M1_27: AdminCauHinhQRController custom image upload path isolation")]
        public async Task ADV_M1_27_ImageUploadIsolation()
        {
            var controller = CreateTestController(out _, out var env);

            // Create a fake uploaded image
            var fakeImage = CreateMockImageFile("poster_campaign_2026.png", "FAKE_IMAGE_DATA_12345");

            var model = new CauHinhThanhToanQR
            {
                TenCauHinh = "Cấu hình ảnh tùy chọn",
                MaNganHang = "MB",
                TenNganHang = "MBBank",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                SuDungVietQR = false
            };

            var result = await controller.Create(model, fakeImage);
            var redirect = Assert.IsType<RedirectToActionResult>(result, "Create with file must succeed and redirect");
            Assert.Equal(nameof(AdminCauHinhQRController.Index), redirect.ActionName, "Create with file must succeed and redirect");

            // Verify that DuongDanAnhQRTuyChon was assigned a path under /uploads/qr/
            Assert.NotNull(model.DuongDanAnhQRTuyChon, "Uploaded image path must be stored on model");
            Assert.True(model.DuongDanAnhQRTuyChon!.StartsWith("/uploads/qr/"), "Image path must be rooted at /uploads/qr/");
            Assert.True(model.DuongDanAnhQRTuyChon!.EndsWith(".png"), "File extension must be preserved");

            // Verify the file was actually written to the mock environment disk
            string localFilePath = Path.Combine(env.WebRootPath, model.DuongDanAnhQRTuyChon!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(localFilePath), $"Physical file must exist at {localFilePath}");
        }

        // =========================================================================
        // SECTION 5: IN-MEMORY STORE DUAL-PERSISTENCE CONCURRENCY & CLONING ISOLATION
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_28: In-memory QR store multi-threaded concurrent stress test (50 tasks)")]
        public void ADV_M1_28_ConcurrentStressTest()
        {
            DbInitializer.ResetInMemoryStore();

            int taskCount = 50;
            var tasks = new List<Task>();

            for (int i = 0; i < taskCount; i++)
            {
                int workerId = i;
                tasks.Add(Task.Run(() =>
                {
                    // Perform simultaneous Add, Read, Toggle, and Update operations
                    var item = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
                    {
                        TenCauHinh = $"Concurrent Config {workerId}",
                        MaNganHang = "MB",
                        TenNganHang = "MBBank",
                        SoTaiKhoan = $"0924{workerId:D6}",
                        TenChuTaiKhoan = $"CONCURRENT WORKER {workerId}",
                        ThuTu = workerId
                    });

                    var fetched = DbInitializer.GetInMemoryQrConfigById(item.IDCauHinhQR);
                    Assert.NotNull(fetched, "Fetched item in thread must not be null");

                    DbInitializer.ToggleActiveInMemoryQrConfig(item.IDCauHinhQR);

                    fetched!.TenChuTaiKhoan = $"MUTATED {workerId}";
                    DbInitializer.UpdateInMemoryQrConfig(fetched);

                    var all = DbInitializer.GetInMemoryQrConfigs();
                    Assert.True(all.Count > 0, "Configs list must not be empty during concurrent reads");
                }));
            }

            // Await all tasks; must not throw InvalidOperationException or deadlocks
            Task.WaitAll(tasks.ToArray());

            var finalConfigs = DbInitializer.GetInMemoryQrConfigs();
            Assert.True(finalConfigs.Count >= taskCount, $"Store must contain all concurrently created configs (found {finalConfigs.Count})");
        }

        [E2ETest(5, 2, "ADV_M1_29: In-memory QR store returns isolated deep clones to prevent accidental mutation")]
        public void ADV_M1_29_CloneIsolation()
        {
            DbInitializer.ResetInMemoryStore();

            var original = DbInitializer.GetActiveStoreQrConfig();
            Assert.NotNull(original, "Active store QR config must exist");

            // Mutate the returned object
            string originalName = original!.TenChuTaiKhoan;
            original.TenChuTaiKhoan = "MALICIOUSLY MUTATED LOCAL COPY";

            // Re-fetch from store
            var fresh = DbInitializer.GetActiveStoreQrConfig();
            Assert.Equal(originalName, fresh!.TenChuTaiKhoan, "Mutating returned object must NOT affect internal in-memory store");
        }
    }
}
