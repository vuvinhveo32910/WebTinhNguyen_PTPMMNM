using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5 Adversarial Stress Harness for Milestone 1 Dynamic QR & Payment Management.
    /// Authors: teamwork_challenger_m1_2
    /// Focus:
    /// - DbInitializer in-memory thread safety under 50 concurrent mutating threads
    /// - Deep-copy isolation & immutability of returned objects
    /// - Strict channel data isolation (CuaHang vs QuyenGop) and priority ordering
    /// - Resilient offline fallback behavior in AdminCauHinhQRController when database connection fails
    /// - Automatic re-seed resilience when in-memory store is drained
    /// - VietQrHelper input stress & query sanitization
    /// </summary>
    public class Tier5_ChallengerM1_2_DynamicQrStressHarness
    {
        // =========================================================================
        // 1. CONCURRENCY & THREAD-SAFETY STRESS TESTS (50 PARALLEL THREADS)
        // =========================================================================

        [E2ETest(5, 4, "CH20: DbInitializer In-Memory QR store under heavy 50-thread concurrent mutations")]
        public void CH20_DbInitializer_InMemoryQr_HeavyConcurrency_50Threads()
        {
            DbInitializer.ResetInMemoryStore();

            var exceptions = new ConcurrentBag<Exception>();
            int threadCount = 50;
            int operationsPerThread = 20;

            Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, threadId =>
            {
                try
                {
                    for (int op = 0; op < operationsPerThread; op++)
                    {
                        int action = (threadId + op) % 5;
                        switch (action)
                        {
                            case 0: // Add
                                var added = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
                                {
                                    TenCauHinh = $"Concurrent Config T{threadId}_Op{op}",
                                    MaNganHang = "TCB",
                                    TenNganHang = "Techcombank",
                                    SoTaiKhoan = $"1900{threadId:D3}{op:D3}",
                                    TenChuTaiKhoan = $"CONCURRENT WORKER {threadId}",
                                    ApDungChoCuaHang = (threadId % 2 == 0),
                                    ApDungChoQuyenGop = (threadId % 2 != 0),
                                    TrangThaiHoatDong = true,
                                    ThuTu = op + 1
                                });
                                Assert.True(added.IDCauHinhQR > 0, "Added ID must be positive");
                                break;

                            case 1: // Update
                                var all = DbInitializer.GetInMemoryQrConfigs();
                                if (all.Count > 1)
                                {
                                    var target = all[all.Count - 1];
                                    target.TenChuTaiKhoan = $"UPDATED_BY_T{threadId}";
                                    target.GhiChuThem = $"Timestamp_{DateTime.UtcNow.Ticks}";
                                    DbInitializer.UpdateInMemoryQrConfig(target);
                                }
                                break;

                            case 2: // Toggle
                                var listForToggle = DbInitializer.GetInMemoryQrConfigs();
                                if (listForToggle.Count > 1)
                                {
                                    int targetId = listForToggle[listForToggle.Count - 1].IDCauHinhQR;
                                    DbInitializer.ToggleActiveInMemoryQrConfig(targetId);
                                }
                                break;

                            case 3: // Query Store & Donation channels
                                var activeStore = DbInitializer.GetActiveStoreQrConfig();
                                var activeDonations = DbInitializer.GetActiveDonationQrConfigs();
                                if (activeStore != null)
                                {
                                    Assert.True(activeStore.ApDungChoCuaHang, "Active store config must apply to store");
                                    Assert.True(activeStore.TrangThaiHoatDong, "Active store config must be active");
                                }
                                foreach (var don in activeDonations)
                                {
                                    Assert.True(don.ApDungChoQuyenGop, "Donation config must apply to donation");
                                    Assert.True(don.TrangThaiHoatDong, "Donation config must be active");
                                }
                                break;

                            case 4: // Delete recently added items (preserve baseline ID 1)
                                var listForDelete = DbInitializer.GetInMemoryQrConfigs();
                                var deleteCandidate = listForDelete.FirstOrDefault(x => x.IDCauHinhQR > 1);
                                if (deleteCandidate != null)
                                {
                                    DbInitializer.DeleteInMemoryQrConfig(deleteCandidate.IDCauHinhQR);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.True(exceptions.IsEmpty,
                $"Expected 0 concurrency exceptions across 50 threads, but encountered {exceptions.Count}: {exceptions.FirstOrDefault()?.Message}");

            // Post-condition: In-memory store remains structurally sound and queryable
            var finalConfigs = DbInitializer.GetInMemoryQrConfigs();
            Assert.True(finalConfigs.Count >= 1, "Store must remain consistent with at least 1 record");

            // Verify ID uniqueness
            var duplicateIds = finalConfigs.GroupBy(q => q.IDCauHinhQR).Where(g => g.Count() > 1).ToList();
            Assert.True(duplicateIds.Count == 0, "No duplicate IDCauHinhQR must exist in store after concurrent mutations");
        }

        // =========================================================================
        // 2. DATA ISOLATION & DEEP COPY MUTATION RESILIENCE
        // =========================================================================

        [E2ETest(5, 4, "CH21: DbInitializer returns isolated clones; mutations do not pollute internal store")]
        public void CH21_DbInitializer_InMemoryQr_DeepCopy_Isolation()
        {
            DbInitializer.ResetInMemoryStore();

            // 1. Test GetActiveStoreQrConfig clone isolation
            var activeStore = DbInitializer.GetActiveStoreQrConfig();
            Assert.NotNull(activeStore, "Default active store config must exist");
            var originalName = activeStore!.TenCauHinh;
            var originalAccount = activeStore.SoTaiKhoan;

            // Mutate caller's object
            activeStore.TenCauHinh = "COMPROMISED_EXTERNALLY";
            activeStore.SoTaiKhoan = "0000000000";
            activeStore.TrangThaiHoatDong = false;

            // Retrieve fresh copy from store
            var freshStore = DbInitializer.GetActiveStoreQrConfig();
            Assert.NotNull(freshStore, "Fresh active store config must be retrievable");
            Assert.Equal(originalName, freshStore!.TenCauHinh, "Internal store must remain immune to external mutations");
            Assert.Equal(originalAccount, freshStore.SoTaiKhoan, "Internal account number must not be modified");
            Assert.True(freshStore.TrangThaiHoatDong, "Internal status must remain active");

            // 2. Test GetInMemoryQrConfigs list clone isolation
            var list1 = DbInitializer.GetInMemoryQrConfigs();
            list1.Clear(); // Clear caller's list

            var list2 = DbInitializer.GetInMemoryQrConfigs();
            Assert.True(list2.Count > 0, "Internal store must not be cleared when caller clears returned list");

            // 3. Test GetInMemoryQrConfigById clone isolation
            var byId = DbInitializer.GetInMemoryQrConfigById(1);
            Assert.NotNull(byId, "ID 1 must exist");
            byId!.TenChuTaiKhoan = "MODIFIED_NAME";

            var byIdFresh = DbInitializer.GetInMemoryQrConfigById(1);
            Assert.Equal("NGUYEN THAI VINH", byIdFresh!.TenChuTaiKhoan, "Internal record must preserve original holder name");
        }

        // =========================================================================
        // 3. CHANNEL DATA ISOLATION & PRIORITY ORDERING
        // =========================================================================

        [E2ETest(5, 4, "CH22: Channel data isolation between CuaHang and QuyenGop")]
        public void CH22_DbInitializer_ChannelIsolation_StoreVsDonation()
        {
            DbInitializer.ResetInMemoryStore();

            // Clear baseline ID 1 to control exact state
            DbInitializer.DeleteInMemoryQrConfig(1);

            // Profile 1: Store Only
            var storeOnly = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản riêng Cửa Hàng",
                MaNganHang = "VCB",
                TenNganHang = "Vietcombank",
                SoTaiKhoan = "0011223344",
                TenChuTaiKhoan = "CUA HANG ONLY",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = true,
                ThuTu = 1
            });

            // Profile 2: Donation Only
            var donationOnly = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản riêng Gây Quỹ",
                MaNganHang = "BIDV",
                TenNganHang = "BIDV",
                SoTaiKhoan = "5566778899",
                TenChuTaiKhoan = "DONATION ONLY",
                ApDungChoCuaHang = false,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 1
            });

            // Profile 3: Both Channels
            var bothChannels = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản chung cả hai kênh",
                MaNganHang = "MB",
                TenNganHang = "MBBank",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "CHUNG HAI KENH",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 2
            });

            // Profile 4: Store Only, but Inactive
            var inactiveStore = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản Cửa Hàng đang tắt",
                MaNganHang = "TCB",
                TenNganHang = "Techcombank",
                SoTaiKhoan = "9999999999",
                TenChuTaiKhoan = "INACTIVE STORE",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = false,
                ThuTu = 0 // Lowest ThuTu, but disabled!
            });

            try
            {
                // Assert Store Channel:
                // Must return storeOnly (ThuTu 1 < 2, and inactiveStore is inactive)
                var currentStore = DbInitializer.GetActiveStoreQrConfig();
                Assert.NotNull(currentStore, "Active store config must not be null");
                Assert.Equal(storeOnly.IDCauHinhQR, currentStore!.IDCauHinhQR, "Store config must match storeOnly profile");
                Assert.Equal("0011223344", currentStore.SoTaiKhoan, "Store config account must match storeOnly");

                // Assert Donation Channel:
                // Must return donationOnly and bothChannels. Must NOT return storeOnly or inactiveStore!
                var currentDonations = DbInitializer.GetActiveDonationQrConfigs();
                Assert.Equal(2, currentDonations.Count, "Donation channel must have exactly 2 active profiles");
                Assert.True(currentDonations.Any(d => d.IDCauHinhQR == donationOnly.IDCauHinhQR), "Donation channel must contain donationOnly");
                Assert.True(currentDonations.Any(d => d.IDCauHinhQR == bothChannels.IDCauHinhQR), "Donation channel must contain bothChannels");
                Assert.False(currentDonations.Any(d => d.IDCauHinhQR == storeOnly.IDCauHinhQR), "Donation channel must NOT contain storeOnly");
                Assert.False(currentDonations.Any(d => d.IDCauHinhQR == inactiveStore.IDCauHinhQR), "Donation channel must NOT contain inactive profile");

                // Toggle storeOnly to inactive -> currentStore should automatically switch to bothChannels!
                DbInitializer.ToggleActiveInMemoryQrConfig(storeOnly.IDCauHinhQR);
                var fallbackStore = DbInitializer.GetActiveStoreQrConfig();
                Assert.NotNull(fallbackStore, "After deactivating primary store config, next eligible store config should activate");
                Assert.Equal(bothChannels.IDCauHinhQR, fallbackStore!.IDCauHinhQR, "Store config should now be bothChannels");

                // Toggle bothChannels to inactive -> store has no active configs
                DbInitializer.ToggleActiveInMemoryQrConfig(bothChannels.IDCauHinhQR);
                var emptyStore = DbInitializer.GetActiveStoreQrConfig();
                Assert.Null(emptyStore, "When all store configs are inactive, GetActiveStoreQrConfig must safely return null");
            }
            finally
            {
                DbInitializer.ResetInMemoryStore();
            }
        }

        [E2ETest(5, 4, "CH23: In-Memory QR priority ordering strictly adheres to ThuTu ascending")]
        public void CH23_DbInitializer_ChannelPriority_ThuTuOrdering()
        {
            DbInitializer.ResetInMemoryStore();
            DbInitializer.DeleteInMemoryQrConfig(1);

            var c3 = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Ưu tiên 3",
                MaNganHang = "MB",
                SoTaiKhoan = "33333333",
                TenChuTaiKhoan = "THU TU 3",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 3
            });

            var c1 = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Ưu tiên 1",
                MaNganHang = "VCB",
                SoTaiKhoan = "11111111",
                TenChuTaiKhoan = "THU TU 1",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 1
            });

            var c2 = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Ưu tiên 2",
                MaNganHang = "TCB",
                SoTaiKhoan = "22222222",
                TenChuTaiKhoan = "THU TU 2",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 2
            });

            try
            {
                // Store config should pick ThuTu 1 (c1)
                var activeStore = DbInitializer.GetActiveStoreQrConfig();
                Assert.NotNull(activeStore, "Active store config must exist");
                Assert.Equal(c1.IDCauHinhQR, activeStore!.IDCauHinhQR, "Lowest ThuTu (1) must be prioritized for Store");

                // Donation configs list should be ordered [c1, c2, c3]
                var activeDonations = DbInitializer.GetActiveDonationQrConfigs();
                Assert.Equal(3, activeDonations.Count, "All 3 active configs should appear for donation");
                Assert.Equal(1, activeDonations[0].ThuTu, "First donation item must have ThuTu 1");
                Assert.Equal(2, activeDonations[1].ThuTu, "Second donation item must have ThuTu 2");
                Assert.Equal(3, activeDonations[2].ThuTu, "Third donation item must have ThuTu 3");
            }
            finally
            {
                DbInitializer.ResetInMemoryStore();
            }
        }

        // =========================================================================
        // 4. DATABASE CONNECTION FAILURE RESILIENCE & OFFLINE FALLBACK
        // =========================================================================

        private class MockWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "WEB_TinhNguyen";
            public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
            public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
        }

        private class SimpleTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
            public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        [E2ETest(5, 4, "CH24: AdminCauHinhQRController handles database failure and falls back to in-memory store")]
        public async Task CH24_AdminController_Fallback_WhenDatabaseFails()
        {
            DbInitializer.ResetInMemoryStore();

            // Create a DbContext configured with an invalid, unreachable server address and 1s timeout
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65534;Database=NonExistentDb_FallbackTest;User Id=sa;Password=invalid_pwd;Connect Timeout=1;Encrypt=False;")
                .Options;

            var mockEnv = new MockWebHostEnvironment();

            using var offlineContext = new ApplicationDbContext(dbOptions);
            var controller = new AdminCauHinhQRController(offlineContext, mockEnv)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), new SimpleTempDataProvider())
            };

            // 1. Index() action must fall back seamlessly to in-memory store without throwing
            var indexResult = await controller.Index();
            Assert.NotNull(indexResult, "Index action must return a result");
            var viewResult = indexResult as ViewResult;
            Assert.NotNull(viewResult, "Index action must return ViewResult");
            var modelList = viewResult!.Model as List<CauHinhThanhToanQR>;
            Assert.NotNull(modelList, "ViewResult model must be a list of CauHinhThanhToanQR");
            Assert.True(modelList!.Count > 0, "Model list from in-memory fallback must contain configs");

            // 2. Create() POST must add to in-memory store when DB is offline
            var newConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = "Offline Fallback Add Test",
                MaNganHang = "ACB",
                TenNganHang = "Á Châu",
                SoTaiKhoan = "9988776655",
                TenChuTaiKhoan = "OFFLINE TESTER",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true
            };

            var createResult = await controller.Create(newConfig, null);
            var redirectResult = createResult as RedirectToActionResult;
            Assert.NotNull(redirectResult, "Create action must redirect upon successful in-memory creation");
            Assert.Equal("Index", redirectResult!.ActionName, "Create must redirect to Index");

            // Verify config was successfully recorded in in-memory store
            var savedInStore = DbInitializer.GetInMemoryQrConfigs().FirstOrDefault(q => q.SoTaiKhoan == "9988776655");
            Assert.NotNull(savedInStore, "New config must be present in in-memory store");

            int addedId = savedInStore!.IDCauHinhQR;

            // 3. Edit() GET & POST must work via in-memory store
            var editGetResult = await controller.Edit(addedId) as ViewResult;
            Assert.NotNull(editGetResult, "Edit GET must return ViewResult for offline record");
            var editModel = editGetResult!.Model as CauHinhThanhToanQR;
            Assert.NotNull(editModel, "Edit GET model must match record");
            Assert.Equal("Offline Fallback Add Test", editModel!.TenCauHinh, "Model name should match");

            editModel.TenChuTaiKhoan = "OFFLINE TESTER - MODIFIED";
            var editPostResult = await controller.Edit(addedId, editModel, null) as RedirectToActionResult;
            Assert.NotNull(editPostResult, "Edit POST must redirect to Index");

            var updatedInStore = DbInitializer.GetInMemoryQrConfigById(addedId);
            Assert.Equal("OFFLINE TESTER - MODIFIED", updatedInStore!.TenChuTaiKhoan, "Record in in-memory store must be updated");

            // 4. ToggleActive() POST must toggle in-memory status
            bool initialStatus = updatedInStore.TrangThaiHoatDong;
            await controller.ToggleActive(addedId);
            var toggledInStore = DbInitializer.GetInMemoryQrConfigById(addedId);
            Assert.Equal(!initialStatus, toggledInStore!.TrangThaiHoatDong, "Status must be inverted in in-memory store");

            // 5. Delete() POST must remove from in-memory store
            await controller.Delete(addedId);
            var deletedInStore = DbInitializer.GetInMemoryQrConfigById(addedId);
            Assert.Null(deletedInStore, "Deleted config must no longer exist in in-memory store");
        }

        [E2ETest(5, 4, "CH25: Drained in-memory store automatically re-seeds default MBBank configuration")]
        public void CH25_DbInitializer_EmptyStore_AutoReseedsDefault()
        {
            DbInitializer.ResetInMemoryStore();

            // Delete all configs
            var all = DbInitializer.GetInMemoryQrConfigs();
            foreach (var item in all)
            {
                DbInitializer.DeleteInMemoryQrConfig(item.IDCauHinhQR);
            }

            // In-memory list is now empty. Verify that querying store re-seeds baseline MBBank
            var activeStore = DbInitializer.GetActiveStoreQrConfig();
            Assert.NotNull(activeStore, "GetActiveStoreQrConfig must auto-reseed default MBBank config when empty");
            Assert.Equal("MB", activeStore!.MaNganHang, "Auto-reseeded bank must be MB");
            Assert.Equal("0924044283", activeStore.SoTaiKhoan, "Auto-reseeded account must be 0924044283");
            Assert.Equal("NGUYEN THAI VINH", activeStore.TenChuTaiKhoan, "Auto-reseeded holder must be NGUYEN THAI VINH");

            // Verify GetActiveDonationQrConfigs also auto-reseeds
            var activeDonations = DbInitializer.GetActiveDonationQrConfigs();
            Assert.True(activeDonations.Count > 0, "GetActiveDonationQrConfigs must auto-reseed when store is empty");
        }

        // =========================================================================
        // 5. VIETQR HELPER INPUT ADVERSARIAL STRESS & SANITIZATION
        // =========================================================================

        [E2ETest(5, 3, "CH26: VietQrHelper sanitizes special characters, whitespace, and extreme amount values")]
        public void CH26_VietQrHelper_AdversarialInputs_And_Escaping()
        {
            // Null/empty bank code and account fallback to default
            var urlFallback = VietQrHelper.GenerateUrl("", "");
            Assert.Contains("MB-0924044283-compact2.png", urlFallback, "Empty bank code and account must fallback to MB defaults");

            // Special characters and Unicode in Account Name
            var specialAccName = "Đặng Hoàng Gia & Nguyễn 'Bảo' <Long>";
            var urlName = VietQrHelper.GenerateUrl("VCB", "123456", amount: null, memo: null, tenChuTaiKhoan: specialAccName);
            Assert.False(urlName.Contains("<Long>"), "Raw HTML tags must not appear unencoded");
            Assert.Contains(Uri.EscapeDataString(specialAccName.Trim().ToUpperInvariant()), urlName, "Account name must be uppercase and URI encoded");

            // URL injection attack in memo
            var maliciousMemo = "GQ2026?hack=true&amount=1#fragment/injection";
            var urlInjection = VietQrHelper.GenerateUrl("MB", "0924044283", amount: 100000m, memo: maliciousMemo);
            Assert.Contains("addInfo=" + Uri.EscapeDataString(maliciousMemo), urlInjection, "Memo must be fully escaped to prevent query injection");

            // Large monetary ceiling
            var urlLarge = VietQrHelper.GenerateUrl("MB", "0924044283", amount: 999999999999m);
            Assert.Contains("amount=999999999999", urlLarge, "Large amounts must format as valid integer without scientific notation");

            // Negative or zero amount should omit amount parameter
            var urlNegative = VietQrHelper.GenerateUrl("MB", "0924044283", amount: -50000m);
            Assert.False(urlNegative.Contains("amount="), "Negative amounts must omit the amount parameter");

            var urlZero = VietQrHelper.GenerateUrl("MB", "0924044283", amount: 0m);
            Assert.False(urlZero.Contains("amount="), "Zero amounts must omit the amount parameter");
        }

        [E2ETest(5, 3, "CH27: CauHinhThanhToanQR GetQrUrl dual-mode resolution & param overrides")]
        public void CH27_CauHinhThanhToanQR_GetQrUrl_ResolutionLogic()
        {
            // Mode 1: Dynamic VietQR (SuDungVietQR = true)
            var dynamicConfig = new CauHinhThanhToanQR
            {
                MaNganHang = "TCB",
                SoTaiKhoan = "1900123456",
                TenChuTaiKhoan = "TRAN VAN C",
                SuDungVietQR = true,
                DuongDanAnhQRTuyChon = "/uploads/qr/custom.png" // Should be ignored when SuDungVietQR is true
            };

            var dynamicUrl = dynamicConfig.GetQrUrl(250000m, "ORDER-1234");
            Assert.Contains("img.vietqr.io/image/TCB-1900123456-compact2.png", dynamicUrl, "GetQrUrl must return VietQR URL when SuDungVietQR is true");
            Assert.Contains("amount=250000", dynamicUrl, "Must include amount");
            Assert.Contains("addInfo=ORDER-1234", dynamicUrl, "Must include memo");

            // Mode 2: Custom uploaded poster image (SuDungVietQR = false, valid path)
            var customConfig = new CauHinhThanhToanQR
            {
                MaNganHang = "TCB",
                SoTaiKhoan = "1900123456",
                TenChuTaiKhoan = "TRAN VAN C",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "/uploads/qr/standee_mbbank.png"
            };

            var customUrl = customConfig.GetQrUrl(250000m, "ORDER-1234");
            Assert.Equal("/uploads/qr/standee_mbbank.png", customUrl, "GetQrUrl must return custom image path when SuDungVietQR is false");

            // Mode 3: Custom image requested (SuDungVietQR = false), but path is empty -> must fallback gracefully to VietQR
            var fallbackConfig = new CauHinhThanhToanQR
            {
                MaNganHang = "BIDV",
                SoTaiKhoan = "88889999",
                TenChuTaiKhoan = "FALLBACK USER",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "" // Empty path!
            };

            var fallbackUrl = fallbackConfig.GetQrUrl(100000m, "DONATE");
            Assert.Contains("img.vietqr.io/image/BIDV-88889999-compact2.png", fallbackUrl, "Empty custom image must fall back gracefully to VietQR Napas247 URL");
        }
    }
}
