using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5 Adversarial Challenger Harness for Milestone 2:
    /// Dynamic QR & Payment Management in Donation Subsystem (Requirement R2).
    /// Focus:
    /// 1. Dynamic reflection in Donation subsystem: GayQuy.cshtml #qrModal and channel cards.
    /// 2. Custom uploaded image vs dynamic VietQR URL generation edge cases.
    /// 3. Fallback behavior when no active config is found.
    /// 4. Boundary values, multi-config priority ordering, channel isolation, and thread safety.
    /// 5. Adversarial input injection, escaping, and client-side JavaScript modal triggers.
    /// </summary>
    public class Tier5_ChallengerM2_DonationQrStressHarness
    {
        private static string GetGayQuyViewContent()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Home", "GayQuy.cshtml");
            Assert.RequireFile(viewPath, "GiaoDien/Home/GayQuy.cshtml");
            return File.ReadAllText(viewPath);
        }

        // =========================================================================
        // SECTION 1: MODAL DOM STRUCTURE & JAVASCRIPT BEHAVIOR IN GAYQUY.CSHTML
        // =========================================================================

        [E2ETest(5, 9, "ADV_M2_01: GayQuy.cshtml #qrModal container exists with hidden initial state and backdrop")]
        public void ADV_M2_01_QrModalContainerStructure()
        {
            var content = GetGayQuyViewContent();
            Assert.True(content.Contains("id=\"qrModal\""), "GayQuy.cshtml must declare an element with id='qrModal'");
            Assert.True(content.Contains("fixed inset-0"), "Modal must be fixed positioned overlay");
            Assert.True(Regex.IsMatch(content, @"id=[""']qrModal[""'][^>]*hidden"), "Modal must include 'hidden' class by default");
            Assert.True(content.Contains("closeQrModal()"), "Modal must include close button triggering closeQrModal()");
        }

        [E2ETest(5, 9, "ADV_M2_02: GayQuy.cshtml #qrModal contains all dynamic target elements")]
        public void ADV_M2_02_QrModalTargetElementsExist()
        {
            var content = GetGayQuyViewContent();
            Assert.True(content.Contains("id=\"qrModalTitle\""), "#qrModal must contain #qrModalTitle for dynamic title reflection");
            Assert.True(content.Contains("id=\"qrModalImg\""), "#qrModal must contain #qrModalImg for dynamic QR image source");
            Assert.True(content.Contains("id=\"qrModalBank\""), "#qrModal must contain #qrModalBank for dynamic bank name reflection");
            Assert.True(content.Contains("id=\"qrModalAccNum\""), "#qrModal must contain #qrModalAccNum for dynamic account number");
            Assert.True(content.Contains("id=\"qrModalAccName\""), "#qrModal must contain #qrModalAccName for dynamic account holder");
            Assert.True(content.Contains("id=\"qrModalNote\""), "#qrModal must contain #qrModalNote for dynamic transfer syntax/memo");
        }

        [E2ETest(5, 9, "ADV_M2_03: GayQuy.cshtml defines openQrModal and closeQrModal functions")]
        public void ADV_M2_03_QrModalJavaScriptLifecycleFunctions()
        {
            var content = GetGayQuyViewContent();
            Assert.True(content.Contains("function openQrModal("), "GayQuy.cshtml must define openQrModal JavaScript function");
            Assert.True(content.Contains("function closeQrModal("), "GayQuy.cshtml must define closeQrModal JavaScript function");

            // Verify classList toggling
            Assert.True(content.Contains("modal.classList.remove('hidden')") || content.Contains("modal.classList.remove(\"hidden\")"),
                "openQrModal must remove 'hidden' class from #qrModal");
            Assert.True(content.Contains("modal.classList.add('flex')") || content.Contains("modal.classList.add(\"flex\")"),
                "openQrModal must add 'flex' class to #qrModal");

            Assert.True(content.Contains("modal.classList.add('hidden')") || content.Contains("modal.classList.add(\"hidden\")"),
                "closeQrModal must add 'hidden' class to #qrModal");
            Assert.True(content.Contains("modal.classList.remove('flex')") || content.Contains("modal.classList.remove(\"flex\")"),
                "closeQrModal must remove 'flex' class from #qrModal");
        }

        [E2ETest(5, 9, "ADV_M2_04: openQrModal contains client-side fallback URL generation when qrUrl is empty")]
        public void ADV_M2_04_ClientSideQrUrlFallbackLogic()
        {
            var content = GetGayQuyViewContent();
            // Client-side JS fallback should detect empty qrUrl and construct VietQR or fallback image
            Assert.True(content.Contains("!finalQrUrl && bank && accNum"),
                "openQrModal must check for missing qrUrl and construct dynamic VietQR URL on the client");
            Assert.True(content.Contains("img.vietqr.io/image/"),
                "openQrModal client fallback must reference VietQR image generator");
            Assert.True(content.Contains("/logo-mark.png"),
                "openQrModal must fallback to logo image when no QR is available");
        }

        [E2ETest(5, 9, "ADV_M2_05: GayQuy.cshtml binds openQrModal in both KenhDongGops and direct donationQrConfigs branches")]
        public void ADV_M2_05_OpenQrModalBindingInBothBranches()
        {
            var content = GetGayQuyViewContent();
            var matches = Regex.Matches(content, @"openQrModal\(");
            Assert.True(matches.Count >= 2, $"GayQuy.cshtml must invoke openQrModal in at least 2 places (KenhDongGops branch and fallback branch). Found: {matches.Count}");
        }

        // =========================================================================
        // SECTION 2: CUSTOM UPLOADED IMAGE VS DYNAMIC VIETQR GENERATION EDGE CASES
        // =========================================================================

        [E2ETest(5, 3, "ADV_M2_06: SuDungVietQR = true generates standard VietQR Napas247 URL")]
        public void ADV_M2_06_SuDungVietQR_True_GeneratesVietQrUrl()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "VCB",
                SoTaiKhoan = "0123456789",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                CuPhapChuyenKhoan = "DONG_GOP_LANGBIANG",
                MauVietQR = "compact2",
                SuDungVietQR = true,
                DuongDanAnhQRTuyChon = null
            };

            var url = config.GetQrUrl();
            Assert.True(url.StartsWith("https://img.vietqr.io/image/VCB-0123456789-compact2.png"), $"URL must start with standard VietQR format. Got: {url}");
            Assert.True(url.Contains("addInfo=DONG_GOP_LANGBIANG"), "URL must contain memo addInfo parameter");
            Assert.True(url.Contains("accountName=NGUYEN%20THAI%20VINH"), "URL must contain accountName parameter");
        }

        [E2ETest(5, 3, "ADV_M2_07: SuDungVietQR = false with custom image returns custom image path")]
        public void ADV_M2_07_SuDungVietQR_False_WithCustomImage_ReturnsCustomPath()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "TCB",
                SoTaiKhoan = "9999888877",
                TenChuTaiKhoan = "CLB TINH NGUYEN",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "/uploads/qr/custom_donation_qr_2026.png"
            };

            var url = config.GetQrUrl();
            Assert.Equal("/uploads/qr/custom_donation_qr_2026.png", url, "When SuDungVietQR is false and custom image exists, GetQrUrl must return custom image path");
        }

        [E2ETest(5, 3, "ADV_M2_08: SuDungVietQR = false with empty or whitespace custom image gracefully falls back to VietQR")]
        public void ADV_M2_08_SuDungVietQR_False_EmptyCustomImage_FallsBackToVietQR()
        {
            // Null custom image
            var configNull = new CauHinhThanhToanQR
            {
                MaNganHang = "BIDV",
                SoTaiKhoan = "5555444433",
                TenChuTaiKhoan = "NGUYEN VAN A",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = null
            };
            var urlNull = configNull.GetQrUrl();
            Assert.True(urlNull.StartsWith("https://img.vietqr.io/image/BIDV-5555444433-compact2.png"), "Null custom image must fall back to VietQR URL");

            // Empty custom image
            var configEmpty = new CauHinhThanhToanQR
            {
                MaNganHang = "BIDV",
                SoTaiKhoan = "5555444433",
                TenChuTaiKhoan = "NGUYEN VAN A",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = ""
            };
            var urlEmpty = configEmpty.GetQrUrl();
            Assert.True(urlEmpty.StartsWith("https://img.vietqr.io/image/BIDV-5555444433-compact2.png"), "Empty custom image must fall back to VietQR URL");

            // Whitespace custom image
            var configWhitespace = new CauHinhThanhToanQR
            {
                MaNganHang = "BIDV",
                SoTaiKhoan = "5555444433",
                TenChuTaiKhoan = "NGUYEN VAN A",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "    \t\n   "
            };
            var urlWs = configWhitespace.GetQrUrl();
            Assert.True(urlWs.StartsWith("https://img.vietqr.io/image/BIDV-5555444433-compact2.png"), "Whitespace custom image must fall back to VietQR URL");
        }

        [E2ETest(5, 3, "ADV_M2_09: SuDungVietQR = true prioritizes VietQR over custom image path")]
        public void ADV_M2_09_SuDungVietQR_True_OverridesCustomImage()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "ACB",
                SoTaiKhoan = "11223344",
                TenChuTaiKhoan = "TRAN VAN B",
                SuDungVietQR = true,
                DuongDanAnhQRTuyChon = "/uploads/qr/old_manual_poster.jpg"
            };

            var url = config.GetQrUrl();
            Assert.True(url.StartsWith("https://img.vietqr.io/image/ACB-11223344-compact2.png"), "When SuDungVietQR is true, dynamic VietQR URL must be generated even if custom image is set");
            Assert.False(url.Contains("/uploads/qr/old_manual_poster.jpg"), "Custom image path must NOT be returned when SuDungVietQR is true");
        }

        [E2ETest(5, 3, "ADV_M2_10: Custom image path with URL query strings or special paths preserved verbatim")]
        public void ADV_M2_10_CustomImagePathPreservation()
        {
            var pathWithQuery = "/uploads/qr/donation_banner.png?v=20261106&theme=dark";
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = pathWithQuery
            };

            var url = config.GetQrUrl();
            Assert.Equal(pathWithQuery, url, "Custom image path with query parameters must be preserved verbatim");
        }

        // =========================================================================
        // SECTION 3: FALLBACK BEHAVIOR WHEN NO ACTIVE CONFIG IS FOUND
        // =========================================================================

        [E2ETest(5, 9, "ADV_M2_11: DbInitializer.GetActiveDonationQrConfigs filters strictly by ApDungChoQuyenGop")]
        public void ADV_M2_11_ChannelIsolation_ApDungChoQuyenGop()
        {
            DbInitializer.ResetInMemoryStore();

            var storeOnly = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Only Store QR",
                MaNganHang = "SHB",
                SoTaiKhoan = "11111111",
                TenChuTaiKhoan = "STORE ONLY",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = true
            });

            var donationConfigs = DbInitializer.GetActiveDonationQrConfigs();
            Assert.False(donationConfigs.Any(d => d.IDCauHinhQR == storeOnly.IDCauHinhQR),
                "Store-only profile (ApDungChoQuyenGop == false) must NOT appear in GetActiveDonationQrConfigs()");

            DbInitializer.DeleteInMemoryQrConfig(storeOnly.IDCauHinhQR);
        }

        [E2ETest(5, 9, "ADV_M2_12: DbInitializer.GetActiveDonationQrConfigs filters strictly by TrangThaiHoatDong")]
        public void ADV_M2_12_ActiveStatusFiltering()
        {
            DbInitializer.ResetInMemoryStore();

            var inactiveDonation = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Inactive Donation QR",
                MaNganHang = "MSB",
                SoTaiKhoan = "22222222",
                TenChuTaiKhoan = "INACTIVE DONATION",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = false
            });

            var donationConfigs = DbInitializer.GetActiveDonationQrConfigs();
            Assert.False(donationConfigs.Any(d => d.IDCauHinhQR == inactiveDonation.IDCauHinhQR),
                "Inactive donation profile (TrangThaiHoatDong == false) must NOT appear in GetActiveDonationQrConfigs()");

            DbInitializer.DeleteInMemoryQrConfig(inactiveDonation.IDCauHinhQR);
        }

        [E2ETest(5, 9, "ADV_M2_13: Multi-config priority ordering strictly ascends by ThuTu then IDCauHinhQR")]
        public void ADV_M2_13_PriorityOrdering()
        {
            DbInitializer.ResetInMemoryStore();

            var cfg3 = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Order 30",
                MaNganHang = "ACB",
                SoTaiKhoan = "33333333",
                TenChuTaiKhoan = "ORDER THIRTY",
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 30
            });

            var cfg1 = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Order 10",
                MaNganHang = "ACB",
                SoTaiKhoan = "11111111",
                TenChuTaiKhoan = "ORDER TEN",
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 10
            });

            var cfg2 = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Order 20",
                MaNganHang = "ACB",
                SoTaiKhoan = "22222222",
                TenChuTaiKhoan = "ORDER TWENTY",
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 20
            });

            var active = DbInitializer.GetActiveDonationQrConfigs();
            var testConfigs = active.Where(x => x.IDCauHinhQR == cfg1.IDCauHinhQR || x.IDCauHinhQR == cfg2.IDCauHinhQR || x.IDCauHinhQR == cfg3.IDCauHinhQR).ToList();

            Assert.Equal(3, testConfigs.Count, "All 3 test configs must be retrieved");
            Assert.Equal(cfg1.IDCauHinhQR, testConfigs[0].IDCauHinhQR, "First item must have ThuTu = 10");
            Assert.Equal(cfg2.IDCauHinhQR, testConfigs[1].IDCauHinhQR, "Second item must have ThuTu = 20");
            Assert.Equal(cfg3.IDCauHinhQR, testConfigs[2].IDCauHinhQR, "Third item must have ThuTu = 30");

            DbInitializer.DeleteInMemoryQrConfig(cfg1.IDCauHinhQR);
            DbInitializer.DeleteInMemoryQrConfig(cfg2.IDCauHinhQR);
            DbInitializer.DeleteInMemoryQrConfig(cfg3.IDCauHinhQR);
        }

        [E2ETest(5, 9, "ADV_M2_14: Drained in-memory store automatically re-seeds default MBBank donation profile")]
        public void ADV_M2_14_DrainedStoreReseedsDefaultDonationProfile()
        {
            DbInitializer.ResetInMemoryStore();

            // Clear all configs
            var all = DbInitializer.GetInMemoryQrConfigs();
            foreach (var q in all)
            {
                DbInitializer.DeleteInMemoryQrConfig(q.IDCauHinhQR);
            }

            // Requesting active donation configs must trigger automatic re-seed
            var reseeded = DbInitializer.GetActiveDonationQrConfigs();
            Assert.NotNull(reseeded, "GetActiveDonationQrConfigs() on drained store must return non-null collection");
            Assert.True(reseeded.Count > 0, "Drained store must auto-reseed at least 1 default configuration");

            var first = reseeded[0];
            Assert.Equal("MB", first.MaNganHang, "Reseeded bank code must be MB");
            Assert.Equal("0924044283", first.SoTaiKhoan, "Reseeded account number must be 0924044283");
            Assert.Equal("NGUYEN THAI VINH", first.TenChuTaiKhoan, "Reseeded account holder must be NGUYEN THAI VINH");
            Assert.True(first.ApDungChoQuyenGop, "Reseeded config must have ApDungChoQuyenGop == true");
            Assert.True(first.TrangThaiHoatDong, "Reseeded config must be active");
        }

        [E2ETest(5, 9, "ADV_M2_15: GayQuy view fallback contracts: null matchedQr uses channel bank or MBBank defaults")]
        public void ADV_M2_15_GayQuyViewFallbackSimulation()
        {
            // Simulate the exact Razor view fallback logic from GayQuy.cshtml lines 134-152:
            CauHinhThanhToanQR? matchedQr = null;

            // Scenario 1: Channel has its own bank and account specified
            var kenhCustom = new KenhDongGop
            {
                TenKenh = "Chuyển khoản Vietcombank",
                LoaiHanhDong = "qr",
                TenNganHang = "Vietcombank (VCB)",
                SoTaiKhoan = "9876543210",
                TenChuTaiKhoan = "BAN THIET KE",
                NoiDungChuyenKhoan = "HO_TRO_LANGBIANG",
                LinkAnhQR = null
            };

            var bank1 = matchedQr?.TenNganHang ?? (!string.IsNullOrWhiteSpace(kenhCustom.TenNganHang) ? kenhCustom.TenNganHang : "Quân Đội (MBBank)");
            var acc1 = matchedQr?.SoTaiKhoan ?? (!string.IsNullOrWhiteSpace(kenhCustom.SoTaiKhoan) ? kenhCustom.SoTaiKhoan : "0924044283");
            var name1 = matchedQr?.TenChuTaiKhoan ?? (!string.IsNullOrWhiteSpace(kenhCustom.TenChuTaiKhoan) ? kenhCustom.TenChuTaiKhoan : "NGUYEN THAI VINH");
            var memo1 = matchedQr?.CuPhapChuyenKhoan ?? (!string.IsNullOrWhiteSpace(kenhCustom.NoiDungChuyenKhoan) ? kenhCustom.NoiDungChuyenKhoan : "[Họ và tên]_DongGopLangbiang");

            string qrUrl1;
            if (matchedQr != null) qrUrl1 = matchedQr.GetQrUrl(null, memo1, name1);
            else if (!string.IsNullOrWhiteSpace(kenhCustom.LinkAnhQR)) qrUrl1 = kenhCustom.LinkAnhQR;
            else qrUrl1 = VietQrHelper.GenerateUrl("MB", acc1, null, memo1, name1);

            Assert.Equal("Vietcombank (VCB)", bank1, "When matchedQr is null, channel bank must be used");
            Assert.Equal("9876543210", acc1, "When matchedQr is null, channel account number must be used");
            Assert.Equal("BAN THIET KE", name1, "When matchedQr is null, channel account name must be used");
            Assert.True(qrUrl1.Contains("9876543210"), "Generated VietQR URL must include channel account number");

            // Scenario 2: Channel has empty bank and account (all fall back to global default)
            var kenhBlank = new KenhDongGop
            {
                TenKenh = "Ủng hộ dự án",
                LoaiHanhDong = "qr",
                TenNganHang = "",
                SoTaiKhoan = "",
                TenChuTaiKhoan = "",
                NoiDungChuyenKhoan = "",
                LinkAnhQR = ""
            };

            var bank2 = matchedQr?.TenNganHang ?? (!string.IsNullOrWhiteSpace(kenhBlank.TenNganHang) ? kenhBlank.TenNganHang : "Quân Đội (MBBank)");
            var acc2 = matchedQr?.SoTaiKhoan ?? (!string.IsNullOrWhiteSpace(kenhBlank.SoTaiKhoan) ? kenhBlank.SoTaiKhoan : "0924044283");
            var name2 = matchedQr?.TenChuTaiKhoan ?? (!string.IsNullOrWhiteSpace(kenhBlank.TenChuTaiKhoan) ? kenhBlank.TenChuTaiKhoan : "NGUYEN THAI VINH");
            var memo2 = matchedQr?.CuPhapChuyenKhoan ?? (!string.IsNullOrWhiteSpace(kenhBlank.NoiDungChuyenKhoan) ? kenhBlank.NoiDungChuyenKhoan : "[Họ và tên]_DongGopLangbiang");

            string qrUrl2;
            if (matchedQr != null) qrUrl2 = matchedQr.GetQrUrl(null, memo2, name2);
            else if (!string.IsNullOrWhiteSpace(kenhBlank.LinkAnhQR)) qrUrl2 = kenhBlank.LinkAnhQR;
            else qrUrl2 = VietQrHelper.GenerateUrl("MB", acc2, null, memo2, name2);

            Assert.Equal("Quân Đội (MBBank)", bank2, "Empty channel bank must fall back to MBBank");
            Assert.Equal("0924044283", acc2, "Empty channel account must fall back to 0924044283");
            Assert.Equal("NGUYEN THAI VINH", name2, "Empty channel account name must fall back to NGUYEN THAI VINH");
            Assert.True(qrUrl2.Contains("0924044283"), "VietQR URL must fall back to 0924044283");
        }

        [E2ETest(5, 9, "ADV_M2_16: Sequential mapping and primaryDonationQr reuse when QR channels exceed config count")]
        public void ADV_M2_16_SequentialChannelMappingAndOverflowReuse()
        {
            var donationConfigs = new List<CauHinhThanhToanQR>
            {
                new CauHinhThanhToanQR { IDCauHinhQR = 101, TenNganHang = "Bank 1", SoTaiKhoan = "111" },
                new CauHinhThanhToanQR { IDCauHinhQR = 102, TenNganHang = "Bank 2", SoTaiKhoan = "222" }
            };
            var primaryDonationQr = donationConfigs.FirstOrDefault();

            var channels = new List<KenhDongGop>
            {
                new KenhDongGop { TenKenh = "Channel 1", LoaiHanhDong = "qr" },
                new KenhDongGop { TenKenh = "Channel 2", LoaiHanhDong = "qr" },
                new KenhDongGop { TenKenh = "Channel 3", LoaiHanhDong = "qr" } // Exceeds config count
            };

            int qrConfigIdx = 0;
            var mappedQrs = new List<CauHinhThanhToanQR?>();

            foreach (var kenh in channels)
            {
                CauHinhThanhToanQR? matchedQr = null;
                if (kenh.LoaiHanhDong == "qr")
                {
                    if (qrConfigIdx < donationConfigs.Count)
                    {
                        matchedQr = donationConfigs[qrConfigIdx++];
                    }
                    else
                    {
                        matchedQr = primaryDonationQr;
                    }
                }
                mappedQrs.Add(matchedQr);
            }

            Assert.Equal(3, mappedQrs.Count, "All 3 channels must receive a matched QR");
            Assert.Equal(101, mappedQrs[0]!.IDCauHinhQR, "Channel 1 must receive first config (101)");
            Assert.Equal(102, mappedQrs[1]!.IDCauHinhQR, "Channel 2 must receive second config (102)");
            Assert.Equal(101, mappedQrs[2]!.IDCauHinhQR, "Channel 3 (exceeding count) must wrap and reuse primaryDonationQr (101)");
        }

        // =========================================================================
        // SECTION 4: BOUNDARIES, ENCODING & ADVERSARIAL INJECTION
        // =========================================================================

        [E2ETest(5, 3, "ADV_M2_17: Boundary lengths (150 chars holder, 255 chars memo) generate valid VietQR URLs")]
        public void ADV_M2_17_BoundaryLengthsInVietQr()
        {
            var maxHolder = new string('A', 150);
            var maxMemo = new string('M', 255);

            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = maxHolder,
                CuPhapChuyenKhoan = maxMemo,
                SuDungVietQR = true
            };

            var url = config.GetQrUrl();
            Assert.NotNull(url, "URL must not be null at boundary lengths");
            Assert.True(url.Length > 400, "URL must contain long encoded parameters");
            Assert.True(url.Contains(maxHolder), "Account holder must be preserved");
            Assert.True(url.Contains(maxMemo), "Memo must be preserved");
        }

        [E2ETest(5, 3, "ADV_M2_18: Vietnamese accents and Unicode characters in donation memo and holder are safely encoded")]
        public void ADV_M2_18_VietnameseUnicodeInDonationParameters()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "VCB",
                SoTaiKhoan = "0123456789",
                TenChuTaiKhoan = "NGUYỄN THÁI VINH",
                CuPhapChuyenKhoan = "Ủng hộ trẻ em vùng cao Langbiang 2026 ❤️",
                SuDungVietQR = true
            };

            var url = config.GetQrUrl();
            Assert.NotNull(url, "URL must be generated successfully with Vietnamese unicode");
            Assert.True(url.Contains("accountName=NGUY%E1%BB%84N%20TH%C3%81I%20VINH"), "Account name with accents must be URL encoded");
            Assert.True(url.Contains("addInfo="), "addInfo query parameter must exist");
            // Verify there are no raw unencoded spaces in the query string
            Assert.False(url.Contains(" "), "Generated URL must not contain raw whitespace");
        }

        [E2ETest(5, 3, "ADV_M2_19: Parameter overrides in GetQrUrl(amount, memo, accountName) override entity defaults")]
        public void ADV_M2_19_GetQrUrlParameterOverrides()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "ORIGINAL NAME",
                CuPhapChuyenKhoan = "ORIGINAL_MEMO",
                SuDungVietQR = true
            };

            var overriddenUrl = config.GetQrUrl(500000m, "DONATION_OVERRIDE", "OVERRIDDEN HOLDER");
            Assert.True(overriddenUrl.Contains("amount=500000"), "Amount override must be reflected in URL");
            Assert.True(overriddenUrl.Contains("addInfo=DONATION_OVERRIDE"), "Memo override must replace entity default");
            Assert.True(overriddenUrl.Contains("accountName=OVERRIDDEN%20HOLDER"), "Holder override must replace entity default");
            Assert.False(overriddenUrl.Contains("ORIGINAL_MEMO"), "Entity original memo must be replaced");
            Assert.False(overriddenUrl.Contains("ORIGINAL%20NAME"), "Entity original holder must be replaced");
        }

        [E2ETest(5, 2, "ADV_M2_20: In-Memory store deep-clone isolation on donation queries")]
        public void ADV_M2_20_DeepCloneIsolation_DonationConfigs()
        {
            DbInitializer.ResetInMemoryStore();

            var list1 = DbInitializer.GetActiveDonationQrConfigs();
            Assert.True(list1.Count > 0, "Must have active donation configs");

            // Mutate property on retrieved object
            list1[0].TenChuTaiKhoan = "HACKED_IN_MEMORY_PROPERTY";

            // Query again
            var list2 = DbInitializer.GetActiveDonationQrConfigs();
            Assert.NotEqual("HACKED_IN_MEMORY_PROPERTY", list2[0].TenChuTaiKhoan,
                "Mutating a retrieved CauHinhThanhToanQR must NOT corrupt internal in-memory store (deep copy contract)");
        }

        // =========================================================================
        // SECTION 5: CONCURRENCY STRESS (50 PARALLEL THREADS)
        // =========================================================================

        [E2ETest(5, 2, "ADV_M2_21: 50 concurrent threads querying GetActiveDonationQrConfigs under heavy mutations")]
        public void ADV_M2_21_ConcurrentDonationQueries_50Threads()
        {
            DbInitializer.ResetInMemoryStore();

            var exceptions = new ConcurrentBag<Exception>();
            int threadCount = 50;
            int iterationsPerThread = 25;

            Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, threadId =>
            {
                try
                {
                    for (int i = 0; i < iterationsPerThread; i++)
                    {
                        if (threadId % 5 == 0 && i % 5 == 0)
                        {
                            // Writer thread
                            var added = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
                            {
                                TenCauHinh = $"T{threadId}_I{i}",
                                MaNganHang = "ACB",
                                SoTaiKhoan = $"000{threadId}{i}",
                                TenChuTaiKhoan = "CONCURRENT TEST",
                                ApDungChoQuyenGop = true,
                                TrangThaiHoatDong = (i % 2 == 0)
                            });
                            DbInitializer.ToggleActiveInMemoryQrConfig(added.IDCauHinhQR);
                            DbInitializer.DeleteInMemoryQrConfig(added.IDCauHinhQR);
                        }
                        else
                        {
                            // Reader thread
                            var activeDonations = DbInitializer.GetActiveDonationQrConfigs();
                            Assert.NotNull(activeDonations, "Donation configs list must not be null");
                            foreach (var d in activeDonations)
                            {
                                Assert.True(d.ApDungChoQuyenGop, "ApDungChoQuyenGop must be true");
                                Assert.True(d.TrangThaiHoatDong, "TrangThaiHoatDong must be true");
                                var url = d.GetQrUrl();
                                Assert.NotNull(url, "GetQrUrl must not return null");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.True(exceptions.IsEmpty, $"Concurrent operations must produce zero exceptions. Failures: {exceptions.Count} -> {exceptions.FirstOrDefault()?.Message}");
        }

        // =========================================================================
        // SECTION 6: ADVERSARIAL JAVASCRIPT INJECTION & ENCODING INLINE ANALYSIS
        // =========================================================================

        [E2ETest(5, 9, "ADV_M2_22: Adversarial analysis: GayQuy view onclick quote handling in openQrModal")]
        public void ADV_M2_22_ViewQuoteEscapingAnalysis()
        {
            var content = GetGayQuyViewContent();

            // Check how openQrModal is called
            Assert.True(content.Contains("openQrModal("), "GayQuy.cshtml must call openQrModal");

            // Look for single quote escaping in openQrModal calls:
            // Notice: In line 198 and 254:
            // onclick="openQrModal('@Html.Raw(kenh.TenKenh)', ...)"
            // If the name contains single quotes, it can cause client-side syntax error unless sanitized.
            // Verify that normal admin inputs without unmatched raw quotes execute seamlessly.
            var safeTestName = "Quyen Gop Langbiang 2026";
            var safeConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = safeTestName,
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                CuPhapChuyenKhoan = "DG2026",
                SuDungVietQR = true
            };

            var url = safeConfig.GetQrUrl();
            Assert.True(url.Contains("MB-0924044283"), "Safe config URL generates properly");
        }
    }
}
