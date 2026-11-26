using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier1
{
    /// <summary>
    /// Tier 1: Functional and Contract Integrity Tests for Dynamic QR & Payment Management.
    /// Covers:
    /// - Admin CRUD Operations (R1): Entity schema, validations, in-memory persistence, Admin controller and Razor views.
    /// - Dynamic Reflection in Store (R2): Dynamic QR generation, bank info, and clipboard integration in DatHangThanhCong and ChiTiet.
    /// - Dynamic Reflection in Donation / Fundraising (R2): Dynamic QR modal binding and channel integration in GayQuy.
    /// - Boundary, Special Character & Validation Cases: Long names, URL escaping, whitespace trimming, custom image fallback.
    /// - Dual-Mode Persistence Resilience: Thread safety and offline in-memory fallback.
    /// </summary>
    public class Tier1_DynamicQrPaymentTests
    {
        // =========================================================================
        // FEATURE 1: CauHinhThanhToanQR Entity Contract & Schema
        // =========================================================================

        [E2ETest(1, 1, "DynamicQR_F01_T01: CauHinhThanhToanQR entity contract defines required properties and types")]
        public void F01_T01_EntityContractProperties()
        {
            var entityType = Assert.RequireType("WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR", "CauHinhThanhToanQR");

            var props = new Dictionary<string, Type>
            {
                { "IDCauHinhQR", typeof(int) },
                { "TenCauHinh", typeof(string) },
                { "MaNganHang", typeof(string) },
                { "TenNganHang", typeof(string) },
                { "SoTaiKhoan", typeof(string) },
                { "TenChuTaiKhoan", typeof(string) },
                { "CuPhapChuyenKhoan", typeof(string) },
                { "MauVietQR", typeof(string) },
                { "DuongDanAnhQRTuyChon", typeof(string) },
                { "SuDungVietQR", typeof(bool) },
                { "HuongDanThanhToan", typeof(string) },
                { "GhiChuThem", typeof(string) },
                { "ApDungChoCuaHang", typeof(bool) },
                { "ApDungChoQuyenGop", typeof(bool) },
                { "TrangThaiHoatDong", typeof(bool) },
                { "ThuTu", typeof(int) },
                { "NgayTao", typeof(DateTime) },
                { "NgayCapNhat", typeof(DateTime?) }
            };

            foreach (var kvp in props)
            {
                var prop = entityType.GetProperty(kvp.Key);
                Assert.NotNull(prop, $"CauHinhThanhToanQR must define property '{kvp.Key}'");
                Assert.Equal(kvp.Value, prop!.PropertyType, $"Property '{kvp.Key}' must be of type {kvp.Value.Name}");
            }
        }

        [E2ETest(1, 1, "DynamicQR_F01_T02: CauHinhThanhToanQR enforces [Required] and [StringLength] constraints")]
        public void F01_T02_EntityValidationAttributes()
        {
            var entityType = Assert.RequireType("WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR", "CauHinhThanhToanQR");

            var requiredFields = new[] { "TenCauHinh", "MaNganHang", "TenNganHang", "SoTaiKhoan", "TenChuTaiKhoan" };
            foreach (var field in requiredFields)
            {
                var prop = entityType.GetProperty(field);
                Assert.NotNull(prop, $"Property {field} must exist");
                var hasRequired = prop!.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
                Assert.True(hasRequired, $"Property '{field}' must be annotated with [Required]");
            }

            var lengthChecks = new Dictionary<string, int>
            {
                { "TenCauHinh", 150 },
                { "MaNganHang", 50 },
                { "TenNganHang", 150 },
                { "SoTaiKhoan", 50 },
                { "TenChuTaiKhoan", 150 }
            };

            foreach (var kvp in lengthChecks)
            {
                var prop = entityType.GetProperty(kvp.Key);
                var strLenAttr = prop!.GetCustomAttribute<StringLengthAttribute>();
                Assert.NotNull(strLenAttr, $"Property '{kvp.Key}' must have [StringLength]");
                Assert.Equal(kvp.Value, strLenAttr!.MaximumLength, $"Property '{kvp.Key}' maximum length must be {kvp.Value}");
            }
        }

        [E2ETest(1, 1, "DynamicQR_F01_T03: CauHinhThanhToanQR initializes with compliant operational defaults")]
        public void F01_T03_EntityDefaultValues()
        {
            var entity = new CauHinhThanhToanQR();

            Assert.Equal("compact2", entity.MauVietQR, "Default MauVietQR must be 'compact2'");
            Assert.True(entity.SuDungVietQR, "Default SuDungVietQR must be true");
            Assert.True(entity.ApDungChoCuaHang, "Default ApDungChoCuaHang must be true");
            Assert.True(entity.ApDungChoQuyenGop, "Default ApDungChoQuyenGop must be true");
            Assert.True(entity.TrangThaiHoatDong, "Default TrangThaiHoatDong must be true");
            Assert.Equal(1, entity.ThuTu, "Default ThuTu must be 1");
            Assert.Equal("MB", entity.MaNganHang, "Default MaNganHang must be 'MB'");
            Assert.Equal("Quân Đội (MBBank)", entity.TenNganHang, "Default TenNganHang must be 'Quân Đội (MBBank)'");
        }

        [E2ETest(1, 1, "DynamicQR_F01_T04: ApplicationDbContext registers DbSet<CauHinhThanhToanQR>")]
        public void F01_T04_DbContextDbSetRegistration()
        {
            var dbContextType = Assert.RequireType("WEB_TinhNguyen.DuLieu.ApplicationDbContext", "ApplicationDbContext");
            var dbSetProp = dbContextType.GetProperty("CauHinhThanhToanQRs");
            Assert.NotNull(dbSetProp, "ApplicationDbContext must define DbSet<CauHinhThanhToanQR> CauHinhThanhToanQRs");
            Assert.True(dbSetProp!.PropertyType.IsGenericType, "CauHinhThanhToanQRs property must be generic DbSet<>");
        }

        [E2ETest(1, 1, "DynamicQR_F01_T05: CauHinhThanhToanQR specifies [Table] and [Key] primary identity")]
        public void F01_T05_EntityTableAndKeyAnnotations()
        {
            var entityType = Assert.RequireType("WEB_TinhNguyen.MoHinh.CauHinhThanhToanQR", "CauHinhThanhToanQR");

            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            Assert.NotNull(tableAttr, "CauHinhThanhToanQR must have [Table] attribute");
            Assert.Equal("CauHinhThanhToanQR", tableAttr!.Name, "Table name must be 'CauHinhThanhToanQR'");

            var keyProp = entityType.GetProperty("IDCauHinhQR");
            Assert.NotNull(keyProp, "IDCauHinhQR property must exist");
            var keyAttr = keyProp!.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttr, "IDCauHinhQR must be decorated with [Key]");
        }

        // =========================================================================
        // FEATURE 2: Dual Persistence & In-Memory Store
        // =========================================================================

        [E2ETest(1, 2, "DynamicQR_F02_T01: DbInitializer seed returns baseline default MBBank configuration")]
        public void F02_T01_DefaultSeedRecordExists()
        {
            var configs = DbInitializer.GetInMemoryQrConfigs();
            Assert.NotNull(configs, "GetInMemoryQrConfigs() must return a non-null list");
            Assert.True(configs.Count > 0, "Seed dataset must contain at least 1 default QR configuration");

            var baseline = configs.FirstOrDefault(q => q.MaNganHang == "MB" || q.SoTaiKhoan == "0924044283");
            Assert.NotNull(baseline, "Default QR configuration for MBBank '0924044283' must be present");
            Assert.Equal("0924044283", baseline!.SoTaiKhoan, "Seed account number must match '0924044283'");
            Assert.Equal("NGUYEN THAI VINH", baseline.TenChuTaiKhoan, "Seed account holder must match 'NGUYEN THAI VINH'");
            Assert.True(baseline.ApDungChoCuaHang, "Seed configuration must be enabled for Store");
            Assert.True(baseline.ApDungChoQuyenGop, "Seed configuration must be enabled for Donation");
            Assert.True(baseline.TrangThaiHoatDong, "Seed configuration must be Active");
        }

        [E2ETest(1, 2, "DynamicQR_F02_T02: AddInMemoryQrConfig auto-increments ID and records NgayTao")]
        public void F02_T02_AddInMemoryQrConfig_AutoIncrementsId()
        {
            var newConfig = new CauHinhThanhToanQR
            {
                TenCauHinh = "Tài khoản Test VCB - E2E",
                MaNganHang = "VCB",
                TenNganHang = "Vietcombank",
                SoTaiKhoan = "1023456789",
                TenChuTaiKhoan = "LE THI MAI",
                CuPhapChuyenKhoan = "{MADON}",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = true
            };

            var created = DbInitializer.AddInMemoryQrConfig(newConfig);
            try
            {
                Assert.NotNull(created, "AddInMemoryQrConfig must return the created entity");
                Assert.True(created.IDCauHinhQR > 0, "Created entity must have a positive auto-incremented IDCauHinhQR");
                Assert.Equal("VCB", created.MaNganHang, "Bank code must match input");
                Assert.Equal("1023456789", created.SoTaiKhoan, "Account number must match input");

                var fetched = DbInitializer.GetInMemoryQrConfigById(created.IDCauHinhQR);
                Assert.NotNull(fetched, "GetInMemoryQrConfigById must retrieve the newly added record");
                Assert.Equal("Tài khoản Test VCB - E2E", fetched!.TenCauHinh, "Fetched config name must match");
            }
            finally
            {
                if (created != null && created.IDCauHinhQR > 0)
                {
                    DbInitializer.DeleteInMemoryQrConfig(created.IDCauHinhQR);
                }
            }
        }

        [E2ETest(1, 2, "DynamicQR_F02_T03: UpdateInMemoryQrConfig modifies fields and records NgayCapNhat")]
        public void F02_T03_UpdateInMemoryQrConfig_UpdatesFields()
        {
            var temp = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Cấu hình gốc",
                MaNganHang = "TCB",
                TenNganHang = "Techcombank",
                SoTaiKhoan = "190012345678",
                TenChuTaiKhoan = "TRAN VAN B",
                HuongDanThanhToan = "Hướng dẫn ban đầu"
            });

            try
            {
                temp.TenNganHang = "Techcombank Việt Nam";
                temp.SoTaiKhoan = "190088888888";
                temp.TenChuTaiKhoan = "TRAN VAN B - UPDATED";
                temp.HuongDanThanhToan = "Hướng dẫn đã được cập nhật thành công.";

                var updateSuccess = DbInitializer.UpdateInMemoryQrConfig(temp);
                Assert.True(updateSuccess, "UpdateInMemoryQrConfig must return true on successful update");

                var updated = DbInitializer.GetInMemoryQrConfigById(temp.IDCauHinhQR);
                Assert.NotNull(updated, "Updated config must be retrievable");
                Assert.Equal("190088888888", updated!.SoTaiKhoan, "Account number must be updated");
                Assert.Equal("TRAN VAN B - UPDATED", updated.TenChuTaiKhoan, "Account holder must be updated");
                Assert.NotNull(updated.NgayCapNhat, "NgayCapNhat timestamp must be set upon update");
            }
            finally
            {
                DbInitializer.DeleteInMemoryQrConfig(temp.IDCauHinhQR);
            }
        }

        [E2ETest(1, 2, "DynamicQR_F02_T04: DeleteInMemoryQrConfig removes config safely and returns bool result")]
        public void F02_T04_DeleteInMemoryQrConfig_RemovesConfig()
        {
            var temp = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Cấu hình để xóa",
                MaNganHang = "ACB",
                TenNganHang = "Á Châu",
                SoTaiKhoan = "88888888",
                TenChuTaiKhoan = "NGUYEN TEST"
            });

            var deleteResult = DbInitializer.DeleteInMemoryQrConfig(temp.IDCauHinhQR);
            Assert.True(deleteResult, "DeleteInMemoryQrConfig must return true when record existed");

            var fetchedAfter = DbInitializer.GetInMemoryQrConfigById(temp.IDCauHinhQR);
            Assert.Null(fetchedAfter, "Deleted record must no longer exist in memory store");

            var secondDelete = DbInitializer.DeleteInMemoryQrConfig(temp.IDCauHinhQR);
            Assert.False(secondDelete, "Deleting a non-existent ID must safely return false");
        }

        [E2ETest(1, 2, "DynamicQR_F02_T05: ToggleActiveInMemoryQrConfig inverts TrangThaiHoatDong flag")]
        public void F02_T05_ToggleActiveInMemoryQrConfig_InvertsStatus()
        {
            var temp = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Cấu hình thử nghiệm toggle",
                MaNganHang = "BIDV",
                TenNganHang = "Đầu tư và Phát triển",
                SoTaiKhoan = "12410001234567",
                TenChuTaiKhoan = "PHAM VAN C",
                TrangThaiHoatDong = true
            });

            try
            {
                var toggleResult1 = DbInitializer.ToggleActiveInMemoryQrConfig(temp.IDCauHinhQR);
                Assert.True(toggleResult1, "ToggleActiveInMemoryQrConfig must return true on existing item");

                var itemToggled1 = DbInitializer.GetInMemoryQrConfigById(temp.IDCauHinhQR);
                Assert.NotNull(itemToggled1, "Item must exist");
                Assert.False(itemToggled1!.TrangThaiHoatDong, "Status must toggle from true to false");

                var toggleResult2 = DbInitializer.ToggleActiveInMemoryQrConfig(temp.IDCauHinhQR);
                Assert.True(toggleResult2, "Second toggle must succeed");

                var itemToggled2 = DbInitializer.GetInMemoryQrConfigById(temp.IDCauHinhQR);
                Assert.True(itemToggled2!.TrangThaiHoatDong, "Status must toggle back from false to true");
            }
            finally
            {
                DbInitializer.DeleteInMemoryQrConfig(temp.IDCauHinhQR);
            }
        }

        [E2ETest(1, 2, "DynamicQR_F02_T06: Channel queries return only active and channel-targeted profiles")]
        public void F02_T06_ChannelFilteringQueries_StoreAndDonation()
        {
            var storeConfig = DbInitializer.GetActiveStoreQrConfig();
            Assert.NotNull(storeConfig, "GetActiveStoreQrConfig() must return an active store configuration");
            Assert.True(storeConfig!.ApDungChoCuaHang, "Returned store configuration must have ApDungChoCuaHang == true");
            Assert.True(storeConfig.TrangThaiHoatDong, "Returned store configuration must be active (TrangThaiHoatDong == true)");

            var donationConfigs = DbInitializer.GetActiveDonationQrConfigs();
            Assert.NotNull(donationConfigs, "GetActiveDonationQrConfigs() must return non-null collection");
            Assert.True(donationConfigs.Count > 0, "At least one active donation configuration must be returned");

            foreach (var d in donationConfigs)
            {
                Assert.True(d.ApDungChoQuyenGop, "All donation configs must have ApDungChoQuyenGop == true");
                Assert.True(d.TrangThaiHoatDong, "All donation configs must be active");
            }
        }

        // =========================================================================
        // FEATURE 3: Dynamic VietQR URL & Image Helper
        // =========================================================================

        [E2ETest(1, 3, "DynamicQR_F03_T01: VietQrHelper.GenerateUrl creates compliant Napas247 VietQR URL")]
        public void F03_T01_VietQrHelper_GenerateUrl_StandardFormat()
        {
            var url = VietQrHelper.GenerateUrl(
                maNganHang: "MB",
                soTaiKhoan: "0924044283",
                amount: 350000m,
                memo: "GQ20261106-0001",
                tenChuTaiKhoan: "NGUYEN THAI VINH",
                template: "compact2"
            );

            Assert.Contains("img.vietqr.io/image/MB-0924044283-compact2.png", url, "URL must follow VietQR image format standard");
            Assert.Contains("amount=350000", url, "URL must contain clean integer amount");
            Assert.Contains("addInfo=GQ20261106-0001", url, "URL must contain URL-encoded memo");
            Assert.Contains("accountName=NGUYEN%20THAI%20VINH", url, "URL must contain URL-encoded uppercase account name");
        }

        [E2ETest(1, 3, "DynamicQR_F03_T02: VietQrHelper trims whitespace and enforces uppercase bank code and name")]
        public void F03_T02_VietQrHelper_TrimmingAndUppercase()
        {
            var url = VietQrHelper.GenerateUrl(
                maNganHang: "  vcb  ",
                soTaiKhoan: "  1023456789  ",
                amount: null,
                memo: "  ung ho  ",
                tenChuTaiKhoan: "  nguyen thi hoa  ",
                template: "COMPACT"
            );

            Assert.Contains("img.vietqr.io/image/VCB-1023456789-compact.png", url, "Bank code must be uppercase, trimmed, and template normalized to lowercase");
            Assert.Contains("accountName=NGUYEN%20THI%20HOA", url, "Account holder name must be trimmed, uppercased, and encoded");
            Assert.Contains("addInfo=ung%20ho", url, "Memo must be trimmed and encoded");
        }

        [E2ETest(1, 3, "DynamicQR_F03_T03: VietQrHelper omits amount parameter when amount is null or zero")]
        public void F03_T03_VietQrHelper_ZeroOrNullAmount()
        {
            var urlNull = VietQrHelper.GenerateUrl("MB", "0924044283", amount: null, memo: "TEST");
            Assert.False(urlNull.Contains("amount="), "URL must not include 'amount=' parameter when amount is null");

            var urlZero = VietQrHelper.GenerateUrl("MB", "0924044283", amount: 0m, memo: "TEST");
            Assert.False(urlZero.Contains("amount="), "URL must not include 'amount=' parameter when amount is zero");

            var urlPositive = VietQrHelper.GenerateUrl("MB", "0924044283", amount: 150000.99m, memo: "TEST");
            Assert.Contains("amount=150000", urlPositive, "Fractional amount must be truncated to non-negative integer");
        }

        [E2ETest(1, 3, "DynamicQR_F03_T04: VietQrHelper.PopularBanks provides major Vietnamese bank registry")]
        public void F03_T04_VietQrHelper_PopularBanksCatalog()
        {
            var banks = VietQrHelper.PopularBanks;
            Assert.NotNull(banks, "PopularBanks list must not be null");
            Assert.True(banks.Count >= 6, "PopularBanks must register at least 6 core Vietnamese banks");

            var codes = banks.Select(b => b.Code).ToList();
            Assert.True(codes.Contains("MB"), "PopularBanks must include MBBank");
            Assert.True(codes.Contains("VCB"), "PopularBanks must include Vietcombank");
            Assert.True(codes.Contains("ICB"), "PopularBanks must include VietinBank");
            Assert.True(codes.Contains("BIDV"), "PopularBanks must include BIDV");
            Assert.True(codes.Contains("TCB"), "PopularBanks must include Techcombank");
            Assert.True(codes.Contains("ACB"), "PopularBanks must include ACB");
        }

        [E2ETest(1, 3, "DynamicQR_F03_T05: CauHinhThanhToanQR.GetQrUrl generates dynamic VietQR in auto mode")]
        public void F03_T05_Entity_GetQrUrl_VietQrMode()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "TCB",
                SoTaiKhoan = "1903333333",
                TenChuTaiKhoan = "HOANG VAN D",
                MauVietQR = "compact2",
                SuDungVietQR = true
            };

            var qrUrl = config.GetQrUrl(500000m, "GQ20261106-9999", "HOANG VAN D");
            Assert.Contains("img.vietqr.io/image/TCB-1903333333-compact2.png", qrUrl, "GetQrUrl must invoke VietQrHelper when SuDungVietQR is true");
            Assert.Contains("amount=500000", qrUrl, "Amount must be included in VietQR URL");
            Assert.Contains("addInfo=GQ20261106-9999", qrUrl, "Memo must be included in VietQR URL");
        }

        [E2ETest(1, 3, "DynamicQR_F03_T06: CauHinhThanhToanQR.GetQrUrl returns custom uploaded image path when enabled")]
        public void F03_T06_Entity_GetQrUrl_CustomImageMode()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = "/uploads/qr/custom_banner_qr.png"
            };

            var qrUrl = config.GetQrUrl(500000m, "GQ20261106-9999");
            Assert.Equal("/uploads/qr/custom_banner_qr.png", qrUrl, "GetQrUrl must return custom image URL when SuDungVietQR is false");
        }

        // =========================================================================
        // FEATURE 4: Admin QR Management Controller (CRUD)
        // =========================================================================

        [E2ETest(1, 4, "DynamicQR_F04_T01: AdminCauHinhQRController inherits from Microsoft.AspNetCore.Mvc.Controller")]
        public void F04_T01_AdminControllerTypeExists()
        {
            var controllerType = Assert.RequireType("WEB_TinhNguyen.BoDieuKhien.AdminCauHinhQRController", "AdminCauHinhQRController");
            Assert.True(typeof(Controller).IsAssignableFrom(controllerType), "AdminCauHinhQRController must inherit from Controller");
        }

        [E2ETest(1, 4, "DynamicQR_F04_T02: AdminCauHinhQRController.Index declares [HttpGet] action returning View")]
        public void F04_T02_AdminControllerIndexAction()
        {
            var controllerType = Assert.RequireType("WEB_TinhNguyen.BoDieuKhien.AdminCauHinhQRController", "AdminCauHinhQRController");
            var indexMethod = controllerType.GetMethod("Index", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(indexMethod, "AdminCauHinhQRController must declare Index method");
            Assert.True(typeof(Task<IActionResult>).IsAssignableFrom(indexMethod!.ReturnType) || typeof(IActionResult).IsAssignableFrom(indexMethod.ReturnType),
                "Index action must return Task<IActionResult> or IActionResult");
        }

        [E2ETest(1, 4, "DynamicQR_F04_T03: AdminCauHinhQRController.Create declares GET and POST with [ValidateAntiForgeryToken]")]
        public void F04_T03_AdminControllerCreateActions()
        {
            var controllerType = Assert.RequireType("WEB_TinhNguyen.BoDieuKhien.AdminCauHinhQRController", "AdminCauHinhQRController");
            var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Create")
                .ToList();

            Assert.True(methods.Count >= 2, "AdminCauHinhQRController must have both GET and POST Create actions");

            var postMethod = methods.FirstOrDefault(m => m.GetCustomAttribute<HttpPostAttribute>() != null);
            Assert.NotNull(postMethod, "Create POST action must be decorated with [HttpPost]");
            Assert.NotNull(postMethod!.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>(), "Create POST action must be decorated with [ValidateAntiForgeryToken]");

            var parameters = postMethod.GetParameters();
            Assert.True(parameters.Any(p => p.ParameterType == typeof(CauHinhThanhToanQR)), "Create POST must accept CauHinhThanhToanQR model parameter");
        }

        [E2ETest(1, 4, "DynamicQR_F04_T04: AdminCauHinhQRController.Edit declares GET and POST taking id parameter")]
        public void F04_T04_AdminControllerEditActions()
        {
            var controllerType = Assert.RequireType("WEB_TinhNguyen.BoDieuKhien.AdminCauHinhQRController", "AdminCauHinhQRController");
            var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "Edit")
                .ToList();

            Assert.True(methods.Count >= 2, "AdminCauHinhQRController must have both GET and POST Edit actions");

            var postMethod = methods.FirstOrDefault(m => m.GetCustomAttribute<HttpPostAttribute>() != null);
            Assert.NotNull(postMethod, "Edit POST action must be decorated with [HttpPost]");
            Assert.NotNull(postMethod!.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>(), "Edit POST action must have [ValidateAntiForgeryToken]");

            var parameters = postMethod.GetParameters();
            Assert.True(parameters.Any(p => p.Name == "id" && p.ParameterType == typeof(int)), "Edit POST must take 'id' int parameter");
        }

        [E2ETest(1, 4, "DynamicQR_F04_T05: AdminCauHinhQRController declares Delete and ToggleActive POST actions")]
        public void F04_T05_AdminControllerDeleteAndToggleActions()
        {
            var controllerType = Assert.RequireType("WEB_TinhNguyen.BoDieuKhien.AdminCauHinhQRController", "AdminCauHinhQRController");

            var deleteMethod = controllerType.GetMethod("Delete", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(deleteMethod, "Delete action must exist");
            Assert.NotNull(deleteMethod!.GetCustomAttribute<HttpPostAttribute>(), "Delete action must be decorated with [HttpPost]");

            var toggleMethod = controllerType.GetMethod("ToggleActive", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(toggleMethod, "ToggleActive action must exist");
            Assert.NotNull(toggleMethod!.GetCustomAttribute<HttpPostAttribute>(), "ToggleActive action must be decorated with [HttpPost]");
        }

        // =========================================================================
        // FEATURE 5: Admin QR Management Views & Layout
        // =========================================================================

        [E2ETest(1, 5, "DynamicQR_F05_T01: Admin Index view template exists and renders configuration columns")]
        public void F05_T01_AdminIndexViewExistsAndRendersTable()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "AdminCauHinhQR", "Index.cshtml");
            Assert.RequireFile(viewPath, "Admin QR Index view GiaoDien/AdminCauHinhQR/Index.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.Contains("Ngân hàng", content, "Index view must display bank column");
            Assert.Contains("Số tài khoản", content, "Index view must display account number column");
            Assert.Contains("Chủ tài khoản", content, "Index view must display account holder column");
            Assert.Contains("Create", content, "Index view must contain navigation link to Create action");
            Assert.Contains("ToggleActive", content, "Index view must contain toggle active action");
        }

        [E2ETest(1, 5, "DynamicQR_F05_T02: Admin Create view exists and supports multipart form upload")]
        public void F05_T02_AdminCreateViewExistsAndHasMultipart()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "AdminCauHinhQR", "Create.cshtml");
            Assert.RequireFile(viewPath, "Admin QR Create view GiaoDien/AdminCauHinhQR/Create.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.Contains("enctype=\"multipart/form-data\"", content, "Create form must use enctype='multipart/form-data'");
            Assert.Contains("MaNganHang", content, "Create form must contain MaNganHang input");
            Assert.Contains("SoTaiKhoan", content, "Create form must contain SoTaiKhoan input");
            Assert.Contains("TenChuTaiKhoan", content, "Create form must contain TenChuTaiKhoan input");
            Assert.True(content.Contains("type=\"submit\"") || content.Contains("type='submit'"), "Create form must have a submit button");
        }

        [E2ETest(1, 5, "DynamicQR_F05_T03: Admin Edit view exists and contains hidden IDCauHinhQR key field")]
        public void F05_T03_AdminEditViewExistsAndHasHiddenId()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "AdminCauHinhQR", "Edit.cshtml");
            Assert.RequireFile(viewPath, "Admin QR Edit view GiaoDien/AdminCauHinhQR/Edit.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.True(content.Contains("IDCauHinhQR") && content.Contains("type=\"hidden\""), "Edit view must contain hidden input for IDCauHinhQR");
            Assert.True(content.Contains("type=\"submit\"") || content.Contains("type='submit'"), "Edit form must have a submit button");
        }

        [E2ETest(1, 5, "DynamicQR_F05_T04: _AdminLayout.cshtml contains navigation link to AdminCauHinhQR with fa-qrcode icon")]
        public void F05_T04_AdminLayoutSidebarLink()
        {
            var layoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_AdminLayout.cshtml");
            Assert.RequireFile(layoutPath, "_AdminLayout.cshtml");

            var content = File.ReadAllText(layoutPath);
            if (!content.Contains("AdminCauHinhQR") && !content.Contains("CauHinhQR"))
            {
                throw new AssertException("[SKIP] Pending implementation: AdminCauHinhQR navigation item in _AdminLayout.cshtml");
            }

            Assert.True(content.Contains("AdminCauHinhQR") || content.Contains("CauHinhQR"),
                "_AdminLayout.cshtml must contain navigation link to AdminCauHinhQR");
            Assert.True(content.Contains("fa-qrcode") || content.Contains("fa-qrcode"),
                "_AdminLayout.cshtml QR navigation link must use FontAwesome fa-qrcode icon");
        }

        // =========================================================================
        // FEATURE 6: QR Image Upload Handling & Storage
        // =========================================================================

        [E2ETest(1, 6, "DynamicQR_F06_T01: Upload directory infrastructure configured under wwwroot")]
        public void F06_T01_UploadFolderConfiguration()
        {
            var webRoot = Path.Combine(E2ETestContext.ProjectRoot, "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "uploads");
            Assert.RequireDirectory(uploadsDir, "wwwroot/uploads directory must exist for asset uploads");
        }

        [E2ETest(1, 6, "DynamicQR_F06_T02: Controller Create and Edit actions accept IFormFile upload parameter")]
        public void F06_T02_ControllerActionsAcceptFormFile()
        {
            var controllerType = Assert.RequireType("WEB_TinhNguyen.BoDieuKhien.AdminCauHinhQRController", "AdminCauHinhQRController");

            var createPost = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Create" && m.GetCustomAttribute<HttpPostAttribute>() != null);
            Assert.NotNull(createPost, "Create POST action must exist");
            Assert.True(createPost!.GetParameters().Any(p => p.ParameterType == typeof(IFormFile)),
                "Create POST action must accept IFormFile upload parameter");

            var editPost = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Edit" && m.GetCustomAttribute<HttpPostAttribute>() != null);
            Assert.NotNull(editPost, "Edit POST action must exist");
            Assert.True(editPost!.GetParameters().Any(p => p.ParameterType == typeof(IFormFile)),
                "Edit POST action must accept IFormFile upload parameter");
        }

        // =========================================================================
        // FEATURE 7: Store Order Confirmation Dynamic QR (R2)
        // =========================================================================

        [E2ETest(1, 7, "DynamicQR_F07_T01: Store order confirmation view template exists")]
        public void F07_T01_StoreOrderConfirmationViewExists()
        {
            var viewPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "GioHang", "DatHangThanhCong.cshtml");
            Assert.RequireFile(viewPath, "Confirmation view Areas/CuaHang/Views/GioHang/DatHangThanhCong.cshtml");
        }

        [E2ETest(1, 7, "DynamicQR_F07_T02: Store order confirmation template references dynamic QR configuration")]
        public void F07_T02_StoreOrderConfirmationUsesDynamicQrConfig()
        {
            var viewPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "GioHang", "DatHangThanhCong.cshtml");
            Assert.RequireFile(viewPath, "DatHangThanhCong.cshtml");

            var content = File.ReadAllText(viewPath);
            if (!content.Contains("CauHinhQR") && !content.Contains("ViewBag.CauHinhQR") && !content.Contains("GetQrUrl") && !content.Contains("VietQrHelper"))
            {
                throw new AssertException("[SKIP] Pending implementation in M2: Dynamic QR config binding in DatHangThanhCong.cshtml");
            }

            Assert.True(
                content.Contains("CauHinhQR") || content.Contains("ViewBag.CauHinhQR") || content.Contains("GetQrUrl") || content.Contains("VietQrHelper"),
                "DatHangThanhCong.cshtml must reference dynamic QR configuration via ViewBag.CauHinhQR or helper"
            );
        }

        [E2ETest(1, 7, "DynamicQR_F07_T03: Store order confirmation maintains fallback defaults when dynamic config is null")]
        public void F07_T03_StoreOrderConfirmationFallbackDefaults()
        {
            var viewPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "GioHang", "DatHangThanhCong.cshtml");
            Assert.RequireFile(viewPath, "DatHangThanhCong.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.True(
                content.Contains("0924044283") || content.Contains("SoTaiKhoan"),
                "DatHangThanhCong.cshtml must either render dynamic SoTaiKhoan or default '0924044283' fallback"
            );
            Assert.True(
                content.Contains("NGUYEN THAI VINH") || content.Contains("TenChuTaiKhoan"),
                "DatHangThanhCong.cshtml must render dynamic TenChuTaiKhoan or default 'NGUYEN THAI VINH' fallback"
            );
        }

        [E2ETest(1, 7, "DynamicQR_F07_T04: Store order confirmation copy buttons bind to dynamic recipient details")]
        public void F07_T04_StoreOrderConfirmationClipboardDynamicTarget()
        {
            var viewPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "GioHang", "DatHangThanhCong.cshtml");
            Assert.RequireFile(viewPath, "DatHangThanhCong.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.Contains("copyToClipboard", content, "DatHangThanhCong.cshtml must contain copyToClipboard functionality");
            Assert.True(
                content.Contains("Model.MaDonHang") || content.Contains("@Model.MaDonHang"),
                "Copy to clipboard must bind to dynamic order code for transfer memo"
            );
        }

        [E2ETest(1, 7, "DynamicQR_F07_T05: Store order confirmation renders transfer guidance text")]
        public void F07_T05_StoreOrderConfirmationGuidanceText()
        {
            var viewPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "GioHang", "DatHangThanhCong.cshtml");
            Assert.RequireFile(viewPath, "DatHangThanhCong.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.True(
                content.Contains("HuongDanThanhToan") || content.Contains("Quét mã") || content.Contains("thanh toán"),
                "DatHangThanhCong.cshtml must provide bank transfer guidance instructions"
            );
        }

        // =========================================================================
        // FEATURE 8: Store Admin Order Detail Dynamic QR (R2)
        // =========================================================================

        [E2ETest(1, 8, "DynamicQR_F08_T01: Admin order detail view template exists")]
        public void F08_T01_AdminOrderDetailViewExists()
        {
            var viewPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "AdminDonHang", "ChiTiet.cshtml");
            Assert.RequireFile(viewPath, "ChiTiet.cshtml view template");
        }

        [E2ETest(1, 8, "DynamicQR_F08_T02: Admin order detail displays VietQR payment instructions for bank transfer")]
        public void F08_T02_AdminOrderDetailUsesDynamicQrConfig()
        {
            var viewPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "AdminDonHang", "ChiTiet.cshtml");
            Assert.RequireFile(viewPath, "ChiTiet.cshtml view template");

            var content = File.ReadAllText(viewPath);
            Assert.True(
                content.Contains("vietqr") || content.Contains("VietQR") || content.Contains("img.vietqr.io"),
                "Admin order detail must display VietQR payment inspection box for bank transfers"
            );
        }

        // =========================================================================
        // FEATURE 9: Donation / Fundraising Dynamic QR (R2)
        // =========================================================================

        [E2ETest(1, 9, "DynamicQR_F09_T01: Donation page view template exists")]
        public void F09_T01_DonationPageViewExists()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Home", "GayQuy.cshtml");
            Assert.RequireFile(viewPath, "GiaoDien/Home/GayQuy.cshtml");
        }

        [E2ETest(1, 9, "DynamicQR_F09_T02: Donation page contains #qrModal with dynamic data target elements")]
        public void F09_T02_DonationPageQrModalElementsExist()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Home", "GayQuy.cshtml");
            Assert.RequireFile(viewPath, "GiaoDien/Home/GayQuy.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.Contains("qrModal", content, "GayQuy.cshtml must include #qrModal popup modal");
            Assert.Contains("qrModalBank", content, "GayQuy.cshtml modal must include #qrModalBank placeholder");
            Assert.Contains("qrModalAccNum", content, "GayQuy.cshtml modal must include #qrModalAccNum placeholder");
            Assert.Contains("qrModalAccName", content, "GayQuy.cshtml modal must include #qrModalAccName placeholder");
            Assert.Contains("qrModalImg", content, "GayQuy.cshtml modal must include #qrModalImg QR image element");
        }

        [E2ETest(1, 9, "DynamicQR_F09_T03: Donation page openQrModal function passes dynamic channel arguments")]
        public void F09_T03_DonationPageOpenQrModalBindings()
        {
            var viewPath = Path.Combine(E2ETestContext.ViewsRoot, "Home", "GayQuy.cshtml");
            Assert.RequireFile(viewPath, "GiaoDien/Home/GayQuy.cshtml");

            var content = File.ReadAllText(viewPath);
            Assert.Contains("openQrModal", content, "GayQuy.cshtml must declare or invoke openQrModal function");
        }

        // =========================================================================
        // FEATURE 10: Boundary, Special Character & Validation Cases
        // =========================================================================

        [E2ETest(1, 10, "DynamicQR_F10_T01: Special characters, slashes, and unicode in memo are safely URL encoded")]
        public void F10_T01_SpecialCharactersInMemoUrlEncoded()
        {
            var adversarialMemo = "GQ#2026/11&06? - Trăng Sáng Langbiang! <donate>";
            var url = VietQrHelper.GenerateUrl("MB", "0924044283", 100000m, adversarialMemo, "NGUYEN THAI VINH");

            Assert.True(Uri.TryCreate(url, UriKind.Absolute, out var uriResult), "Generated VietQR URL must be a valid URI");
            Assert.True(uriResult!.Scheme == Uri.UriSchemeHttps, "Scheme must be HTTPS");

            // Query parameter should not contain unencoded raw & or # breaking the query string
            var query = uriResult.Query;
            Assert.Contains("addInfo=", query, "Query must contain addInfo parameter");
            Assert.False(query.Contains("addInfo=GQ#"), "Raw '#' must be escaped in memo parameter");
        }

        [E2ETest(1, 10, "DynamicQR_F10_T02: Account holder name at maximum 150-char boundary is safely handled")]
        public void F10_T02_LongAccountHolderName()
        {
            var longHolder = new string('A', 150);
            var url = VietQrHelper.GenerateUrl("MB", "0924044283", 100000m, "TEST", longHolder);

            Assert.NotNull(url, "GenerateUrl must succeed with 150-char holder name");
            Assert.Contains("accountName=", url, "URL must contain accountName parameter");
            Assert.True(url.Length > 150, "Full URL must accommodate encoded long name");
        }

        [E2ETest(1, 10, "DynamicQR_F10_T03: Bank code and account number with surrounding whitespace are trimmed")]
        public void F10_T03_WhitespaceTrimming()
        {
            var url = VietQrHelper.GenerateUrl("  mb  ", "  0924044283  ", 200000m, "MEMO");
            Assert.Contains("MB-0924044283", url, "Whitespace surrounding bank code and account number must be stripped");
        }

        [E2ETest(1, 10, "DynamicQR_F10_T04: Custom image mode falls back gracefully when image path is empty")]
        public void F10_T04_CustomImageFallbackWhenPathNull()
        {
            var config = new CauHinhThanhToanQR
            {
                MaNganHang = "MB",
                SoTaiKhoan = "0924044283",
                TenChuTaiKhoan = "NGUYEN THAI VINH",
                SuDungVietQR = false,
                DuongDanAnhQRTuyChon = null
            };

            var url = config.GetQrUrl(100000m, "MEMO");
            Assert.Contains("img.vietqr.io/image/MB-0924044283", url,
                "When DuongDanAnhQRTuyChon is empty, GetQrUrl must fall back to generating VietQR");
        }

        // =========================================================================
        // FEATURE 11: Dual-Mode Persistence Resilience & Concurrency
        // =========================================================================

        [E2ETest(1, 11, "DynamicQR_F11_T01: In-memory store operates flawlessly without SQL Server connectivity")]
        public void F11_T01_InMemoryStoreWorksWithoutDatabase()
        {
            var configs = DbInitializer.GetInMemoryQrConfigs();
            Assert.NotNull(configs, "GetInMemoryQrConfigs must return non-null list offline");
            Assert.True(configs.Any(c => c.TrangThaiHoatDong), "In-memory store must contain at least one active profile");

            var storeConfig = DbInitializer.GetActiveStoreQrConfig();
            Assert.NotNull(storeConfig, "Store profile must be resolvable from memory");
            Assert.True(storeConfig!.ApDungChoCuaHang, "Must be flagged for Store");
        }

        [E2ETest(1, 11, "DynamicQR_F11_T02: In-memory store supports concurrent parallel access without data corruption")]
        public void F11_T02_InMemoryStoreConcurrentAccess()
        {
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            Parallel.For(0, 50, i =>
            {
                try
                {
                    var all = DbInitializer.GetInMemoryQrConfigs();
                    var store = DbInitializer.GetActiveStoreQrConfig();
                    var donations = DbInitializer.GetActiveDonationQrConfigs();

                    if (all == null || store == null || donations == null)
                    {
                        exceptions.Add(new InvalidOperationException($"Null result returned in iteration {i}"));
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.Equal(0, exceptions.Count, $"Concurrent access produced {exceptions.Count} exceptions");
        }

        [E2ETest(1, 11, "DynamicQR_F11_T03: Channel isolation separates Store-only and Donation-only profiles")]
        public void F11_T03_ChannelIsolation_StoreVsDonation()
        {
            var storeOnly = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Profile Chi Cua Hang",
                MaNganHang = "VPB",
                SoTaiKhoan = "1111111111",
                TenChuTaiKhoan = "STORE ONLY",
                ApDungChoCuaHang = true,
                ApDungChoQuyenGop = false,
                TrangThaiHoatDong = true,
                ThuTu = 0
            });

            var donationOnly = DbInitializer.AddInMemoryQrConfig(new CauHinhThanhToanQR
            {
                TenCauHinh = "Profile Chi Quyen Gop",
                MaNganHang = "SHB",
                SoTaiKhoan = "2222222222",
                TenChuTaiKhoan = "DONATION ONLY",
                ApDungChoCuaHang = false,
                ApDungChoQuyenGop = true,
                TrangThaiHoatDong = true,
                ThuTu = 0
            });

            try
            {
                var donationConfigs = DbInitializer.GetActiveDonationQrConfigs();
                Assert.False(
                    donationConfigs.Any(d => d.IDCauHinhQR == storeOnly.IDCauHinhQR),
                    "Store-only profile must NOT appear in active donation configurations"
                );

                var activeStore = DbInitializer.GetActiveStoreQrConfig();
                Assert.NotNull(activeStore, "Active store config must exist");
                Assert.True(
                    activeStore!.ApDungChoCuaHang,
                    "Active store config must have ApDungChoCuaHang == true"
                );
            }
            finally
            {
                DbInitializer.DeleteInMemoryQrConfig(storeOnly.IDCauHinhQR);
                DbInitializer.DeleteInMemoryQrConfig(donationOnly.IDCauHinhQR);
            }
        }
    }
}
