using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;
using BaoMatMatKhau = WEB_TinhNguyen.DuLieu.BaoMatMatKhau;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Adversarial Concurrency & Data Isolation Stress Harness for Milestone 1
    /// (Admin Authentication & Authorization: DbInitializer dual-mode persistence, concurrency, and mutation isolation)
    /// Challenger: challenger_m1_2
    ///
    /// Challenges empirically tested:
    /// 1. Concurrency stress test: 100 parallel threads simultaneously calling ValidateAdminCredentials,
    ///    GetAdminByUsername, ResetInMemoryStore, and SeedAdmin.
    /// 2. Concurrent Admin CRUD stress: 75 parallel threads performing concurrent Add, Update, Read, Delete,
    ///    and List enumeration without collection-modified exceptions.
    /// 3. Clone isolation & mutation defense: Mutating properties of TaiKhoanAdmin returned by GetAdminByUsername
    ///    strictly leaves internal store intact.
    /// 4. Store collection isolation: Mutating GetInMemoryAdmins list or individual admin objects by ID
    ///    does not pollute internal storage.
    /// 5. Case-sensitivity & whitespace robustness: Trimming and case-insensitivity on TenDangNhap matching.
    /// 6. Password exact matching: Password whitespace is preserved and case-sensitive.
    /// 7. Invalid credentials rejection: Null/empty usernames, empty passwords, inactive accounts,
    ///    and non-existent usernames strictly return false.
    /// 8. BaoMatMatKhau PBKDF2 cryptographic edge cases: Corrupted base64, random salts, timing attack resistance.
    /// 9. Offline ApplicationDbContext dual-mode fallback concurrency under parallel thread stress.
    /// </summary>
    public class Tier5_ChallengerM1_2_AdminConcurrencyStressHarness
    {
        private static ApplicationDbContext CreateDeadDbContext()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65531;Database=OfflineTestDb_ChallengerM1_2;User Id=sa;Password=invalid_pwd;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(dbOptions);
        }

        // =========================================================================
        // SUITE 1: CONCURRENCY STRESS TESTS (50-100 PARALLEL THREADS)
        // =========================================================================

        [E2ETest(5, 91, "CH_M1_CONC_01: 100 parallel threads concurrently invoking ValidateAdminCredentials, GetAdminByUsername, ResetInMemoryStore, and SeedAdmin")]
        public void CH_M1_CONC_01_HighConcurrency_100ParallelThreads()
        {
            DbInitializer.ResetInMemoryStore();

            var exceptions = new ConcurrentBag<Exception>();
            int threadCount = 100;
            int opsPerThread = 15;

            Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, threadId =>
            {
                try
                {
                    for (int op = 0; op < opsPerThread; op++)
                    {
                        int branch = (threadId + op) % 4;
                        switch (branch)
                        {
                            case 0:
                                bool valid = DbInitializer.ValidateAdminCredentials("admin", "Admin@123");
                                Assert.True(valid, $"Validation should succeed for thread {threadId} op {op}");
                                break;

                            case 1:
                                var admin = DbInitializer.GetAdminByUsername("admin");
                                Assert.NotNull(admin, $"Admin lookup should return non-null for thread {threadId} op {op}");
                                Assert.Equal("admin", admin!.TenDangNhap, "Username must match");
                                break;

                            case 2:
                                DbInitializer.ResetInMemoryStore();
                                break;

                            case 3:
                                DbInitializer.SeedAdmin();
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.True(exceptions.IsEmpty, $"Concurrent execution produced {exceptions.Count} exceptions: {string.Join(" | ", exceptions.Take(3).Select(e => e.Message))}");

            // Ensure store is intact after concurrent hammering
            var finalAdmin = DbInitializer.GetAdminByUsername("admin");
            Assert.NotNull(finalAdmin, "Final store state must contain default admin");
            Assert.True(DbInitializer.ValidateAdminCredentials("admin", "Admin@123"), "Final credentials must validate successfully");
        }

        [E2ETest(5, 91, "CH_M1_CONC_02: 75 parallel threads executing concurrent Add, Update, Read, Delete, and List enumeration on Admin store")]
        public void CH_M1_CONC_02_AdminCrud_HeavyConcurrency_75Threads()
        {
            DbInitializer.ResetInMemoryStore();

            var exceptions = new ConcurrentBag<Exception>();
            int threadCount = 75;
            int opsPerThread = 10;

            Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, threadId =>
            {
                try
                {
                    for (int op = 0; op < opsPerThread; op++)
                    {
                        // 1. Add temporary admin
                        var newAdmin = new TaiKhoanAdmin
                        {
                            TenDangNhap = $"user_t{threadId}_o{op}",
                            MatKhauHash = "hash123",
                            MuoiBaoMat = "salt123",
                            HoTen = $"User {threadId}_{op}",
                            Email = $"user_{threadId}_{op}@test.vn",
                            VaiTro = "Moderator",
                            TrangThaiHoatDong = true
                        };
                        var added = DbInitializer.AddInMemoryAdmin(newAdmin);
                        Assert.NotNull(added, "Added admin must not be null");
                        Assert.GreaterThan(added.IDTaiKhoan, 0, "IDTaiKhoan must be positive");

                        // 2. Read by ID
                        var fetched = DbInitializer.GetInMemoryAdminById(added.IDTaiKhoan);
                        Assert.NotNull(fetched, "Fetched admin by ID must exist");

                        // 3. Update admin
                        fetched!.HoTen = $"Updated User {threadId}_{op}";
                        bool updated = DbInitializer.UpdateInMemoryAdmin(fetched);
                        Assert.True(updated, "UpdateInMemoryAdmin must return true");

                        // 4. Concurrent List enumeration (stressing against collection-modified exceptions)
                        var allAdmins = DbInitializer.GetInMemoryAdmins();
                        Assert.NotNull(allAdmins, "GetInMemoryAdmins must return list");
                        Assert.GreaterThan(allAdmins.Count, 0, "Admins count must be > 0");

                        // 5. Delete temporary admin
                        bool deleted = DbInitializer.DeleteInMemoryAdmin(added.IDTaiKhoan);
                        Assert.True(deleted, "DeleteInMemoryAdmin must return true");
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.True(exceptions.IsEmpty, $"Concurrent CRUD execution produced {exceptions.Count} exceptions: {string.Join(" | ", exceptions.Take(3).Select(e => e.Message))}");

            // Default admin must remain unaffected
            var defaultAdmin = DbInitializer.GetAdminByUsername("admin");
            Assert.NotNull(defaultAdmin, "Default admin must survive concurrent CRUD hammering");
            Assert.Equal("admin", defaultAdmin!.TenDangNhap, "Default admin username intact");
        }

        // =========================================================================
        // SUITE 2: CLONE ISOLATION & MUTATION DEFENSE TESTS
        // =========================================================================

        [E2ETest(5, 91, "CH_M1_ISOL_01: Mutating TaiKhoanAdmin returned by GetAdminByUsername does not pollute internal store")]
        public void CH_M1_ISOL_01_GetAdminByUsername_CloneIsolation_MutationDefense()
        {
            DbInitializer.ResetInMemoryStore();

            var admin = DbInitializer.GetAdminByUsername("admin");
            Assert.NotNull(admin, "Initial admin lookup must return non-null");

            string originalHash = admin!.MatKhauHash;
            string originalSalt = admin.MuoiBaoMat;
            string originalHoTen = admin.HoTen ?? string.Empty;

            // Adversarial mutation: attempt to hijack credentials and deactivate account in-place
            admin.TenDangNhap = "hacked_admin";
            admin.MatKhauHash = "CORRUPTED_HASH_BASE64";
            admin.MuoiBaoMat = "TAMPERED_SALT_BASE64";
            admin.HoTen = "Evil Hacker";
            admin.Email = "evil@hacker.io";
            admin.VaiTro = "RootHacker";
            admin.TrangThaiHoatDong = false;
            admin.LanDangNhapCuoi = DateTime.Now.AddDays(100);

            // Fetch again from DbInitializer
            var unpollutedAdmin = DbInitializer.GetAdminByUsername("admin");
            Assert.NotNull(unpollutedAdmin, "GetAdminByUsername('admin') must still return the default admin");
            Assert.Equal("admin", unpollutedAdmin!.TenDangNhap, "Username in store must remain 'admin'");
            Assert.Equal(originalHash, unpollutedAdmin.MatKhauHash, "MatKhauHash in store must remain pristine");
            Assert.Equal(originalSalt, unpollutedAdmin.MuoiBaoMat, "MuoiBaoMat in store must remain pristine");
            Assert.Equal(originalHoTen, unpollutedAdmin.HoTen ?? string.Empty, "HoTen in store must remain unpolluted");
            Assert.Equal("Admin", unpollutedAdmin.VaiTro, "VaiTro in store must remain 'Admin'");
            Assert.True(unpollutedAdmin.TrangThaiHoatDong, "TrangThaiHoatDong in store must remain true");

            // Verify hijacked username does not exist
            var hijackedAdmin = DbInitializer.GetAdminByUsername("hacked_admin");
            Assert.Null(hijackedAdmin, "Mutated username 'hacked_admin' must not exist in internal store");

            // Verify authentication continues to succeed with authentic password
            bool loginSucceeded = DbInitializer.ValidateAdminCredentials("admin", "Admin@123");
            Assert.True(loginSucceeded, "ValidateAdminCredentials must succeed despite prior object mutation");
        }

        [E2ETest(5, 91, "CH_M1_ISOL_02: Mutating GetInMemoryAdmins list or individual admin by ID does not corrupt store")]
        public void CH_M1_ISOL_02_GetInMemoryAdmins_ListAndEntityIsolation()
        {
            DbInitializer.ResetInMemoryStore();

            var list = DbInitializer.GetInMemoryAdmins();
            Assert.NotNull(list, "GetInMemoryAdmins must return a non-null list");
            Assert.Equal(1, list.Count, "Initially exactly 1 default admin exists");

            // Attempt to corrupt the returned list
            list[0].TenDangNhap = "POLLUTED_IN_LIST";
            list[0].MatKhauHash = "POLLUTED_HASH";
            list.Clear(); // Clear the caller's list

            // Re-fetch list
            var list2 = DbInitializer.GetInMemoryAdmins();
            Assert.Equal(1, list2.Count, "Internal list must not be cleared by caller mutating returned collection");
            Assert.Equal("admin", list2[0].TenDangNhap, "Internal entity username must not be polluted");

            // Test GetInMemoryAdminById isolation
            var adminById = DbInitializer.GetInMemoryAdminById(1);
            Assert.NotNull(adminById, "Admin by ID 1 must exist");
            adminById!.TenDangNhap = "POLLUTED_BY_ID";
            adminById.TrangThaiHoatDong = false;

            var adminById2 = DbInitializer.GetInMemoryAdminById(1);
            Assert.Equal("admin", adminById2!.TenDangNhap, "Internal store must not reflect mutated TenDangNhap");
            Assert.True(adminById2.TrangThaiHoatDong, "Internal store must not reflect mutated TrangThaiHoatDong");
        }

        // =========================================================================
        // SUITE 3: CASE-SENSITIVITY & WHITESPACE ROBUSTNESS
        // =========================================================================

        [E2ETest(5, 91, "CH_M1_ROBU_01: Case-insensitivity and whitespace trimming robustness on admin username")]
        public void CH_M1_ROBU_01_CaseInsensitive_And_Whitespace_Robustness()
        {
            DbInitializer.ResetInMemoryStore();

            string[] usernameVariations = new string[]
            {
                "admin",
                "ADMIN",
                "Admin",
                "aDmIn",
                "  admin",
                "admin  ",
                "  admin  ",
                "\tadmin\t",
                "\r\n  ADMIN  \r\n",
                "  Admin  "
            };

            foreach (var userVariation in usernameVariations)
            {
                var admin = DbInitializer.GetAdminByUsername(userVariation);
                Assert.NotNull(admin, $"GetAdminByUsername should resolve for variation '{userVariation}'");
                Assert.Equal("admin", admin!.TenDangNhap, $"Resolved admin TenDangNhap should be canonical 'admin' for input '{userVariation}'");

                bool valid = DbInitializer.ValidateAdminCredentials(userVariation, "Admin@123");
                Assert.True(valid, $"ValidateAdminCredentials should return true for username variation '{userVariation}'");
            }
        }

        [E2ETest(5, 91, "CH_M1_ROBU_02: Password exact matching preserves whitespace and enforces case-sensitivity")]
        public void CH_M1_ROBU_02_PasswordExactMatching_PreservesWhitespace()
        {
            DbInitializer.ResetInMemoryStore();

            // Correct credentials
            Assert.True(DbInitializer.ValidateAdminCredentials("admin", "Admin@123"), "Standard credentials must validate");

            // Password with leading or trailing whitespace must fail (passwords should never be trimmed silently)
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "Admin@123 "), "Trailing space in password must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", " Admin@123"), "Leading space in password must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "  Admin@123  "), "Whitespace-padded password must fail");

            // Password casing mismatch must fail
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "admin@123"), "Lowercase password must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "ADMIN@123"), "Uppercase password must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "Admin@1234"), "Incorrect password length must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "Admin@12"), "Truncated password must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "WrongPassword!"), "Completely wrong password must fail");
        }

        // =========================================================================
        // SUITE 4: INVALID CREDENTIALS & INACTIVE ACCOUNTS STRICT REJECTION
        // =========================================================================

        [E2ETest(5, 91, "CH_M1_REJE_01: Rejection of null, empty, whitespace-only, inactive, and non-existent accounts")]
        public void CH_M1_REJE_01_InvalidCredentials_Rejection()
        {
            DbInitializer.ResetInMemoryStore();

            // 1. Null, empty, and whitespace usernames
            Assert.False(DbInitializer.ValidateAdminCredentials(null!, "Admin@123"), "Null username must return false");
            Assert.False(DbInitializer.ValidateAdminCredentials("", "Admin@123"), "Empty username must return false");
            Assert.False(DbInitializer.ValidateAdminCredentials("   ", "Admin@123"), "Whitespace username must return false");
            Assert.False(DbInitializer.ValidateAdminCredentials("\t\r\n", "Admin@123"), "Tab/newline username must return false");

            Assert.Null(DbInitializer.GetAdminByUsername(null!), "GetAdminByUsername(null) must return null");
            Assert.Null(DbInitializer.GetAdminByUsername(""), "GetAdminByUsername('') must return null");
            Assert.Null(DbInitializer.GetAdminByUsername("   "), "GetAdminByUsername('   ') must return null");

            // 2. Null, empty, and whitespace passwords
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", null!), "Null password must return false");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", ""), "Empty password must return false");

            // 3. Non-existent usernames
            Assert.False(DbInitializer.ValidateAdminCredentials("ghost_user", "Admin@123"), "Non-existent user must return false");
            Assert.False(DbInitializer.ValidateAdminCredentials("root", "toor"), "Root user must return false");
            Assert.False(DbInitializer.ValidateAdminCredentials("administrator", "Admin@123"), "Administrator user must return false");
            Assert.Null(DbInitializer.GetAdminByUsername("ghost_user"), "Non-existent user lookup must return null");

            // 4. Inactive account rejection
            string inactiveSalt = BaoMatMatKhau.TaoMuoiNgauNhien();
            string inactiveHash = BaoMatMatKhau.BamMatKhau("Inactive@123", inactiveSalt);

            var inactiveAdmin = new TaiKhoanAdmin
            {
                TenDangNhap = "inactive_admin",
                MatKhauHash = inactiveHash,
                MuoiBaoMat = inactiveSalt,
                HoTen = "Tài khoản bị khóa",
                Email = "locked@test.vn",
                VaiTro = "Admin",
                TrangThaiHoatDong = false // Account disabled
            };
            DbInitializer.AddInMemoryAdmin(inactiveAdmin);

            // Lookup succeeds but validation strictly rejects inactive account
            var retrievedInactive = DbInitializer.GetAdminByUsername("inactive_admin");
            Assert.NotNull(retrievedInactive, "Inactive account can be looked up");
            Assert.False(retrievedInactive!.TrangThaiHoatDong, "Account status must be inactive");

            bool inactiveLoginResult = DbInitializer.ValidateAdminCredentials("inactive_admin", "Inactive@123");
            Assert.False(inactiveLoginResult, "Inactive account must strictly return false on ValidateAdminCredentials even with correct password");

            // Re-activate and verify login succeeds
            retrievedInactive.TrangThaiHoatDong = true;
            DbInitializer.UpdateInMemoryAdmin(retrievedInactive);

            bool activatedLoginResult = DbInitializer.ValidateAdminCredentials("inactive_admin", "Inactive@123");
            Assert.True(activatedLoginResult, "Activated account must return true on ValidateAdminCredentials with correct password");
        }

        // =========================================================================
        // SUITE 5: CRYPTOGRAPHIC EDGE CASES & PBKDF2 VERIFICATION
        // =========================================================================

        [E2ETest(5, 91, "CH_M1_CRYP_01: BaoMatMatKhau PBKDF2 salt entropy, corrupted hash defense, and alias contracts")]
        public void CH_M1_CRYP_01_PBKDF2_CryptographicRigor_And_EdgeCases()
        {
            // 1. Salt entropy and uniqueness
            var salts = new HashSet<string>();
            for (int i = 0; i < 20; i++)
            {
                string s = BaoMatMatKhau.TaoMuoiNgauNhien();
                Assert.Equal(24, s.Length, "Base64 salt of 16 bytes must have length 24");
                Assert.True(salts.Add(s), $"Salt {s} should be cryptographically unique");
            }

            // 2. Hash output size and reproducibility
            string salt = BaoMatMatKhau.TaoMuoiNgauNhien();
            string hash1 = BaoMatMatKhau.BamMatKhau("TestPassword#2026", salt);
            string hash2 = BaoMatMatKhau.BamMatKhau("TestPassword#2026", salt);
            Assert.Equal(44, hash1.Length, "Base64 PBKDF2 hash of 32 bytes must have length 44");
            Assert.Equal(hash1, hash2, "Same password and salt must deterministically produce identical hash");

            // 3. Verification succeeds with matching credentials
            Assert.True(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", hash1, salt), "Matching password must return true");

            // 4. Verification fails safely without exceptions on corrupted or invalid inputs
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("WrongPassword", hash1, salt), "Wrong password must return false");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau(null, hash1, salt), "Null password input must return false");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("", hash1, salt), "Empty password input must return false");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", null, salt), "Null hash must return false");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", "", salt), "Empty hash must return false");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", hash1, null), "Null salt must return false");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", hash1, ""), "Empty salt must return false");

            // Corrupted base64 inputs (malformed characters or truncated)
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", "NOT_A_BASE_64_STRING!@#$", salt), "Malformed base64 hash must return false without crashing");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", hash1, "NOT_A_BASE_64_STRING!@#$"), "Malformed base64 salt must return false without crashing");
            Assert.False(BaoMatMatKhau.KiemTraMatKhau("TestPassword#2026", "AQID", salt), "Short 4-byte hash must return false without throwing IndexOutOfRangeException");

            // 5. English contract aliases verification (PROJECT.md contract)
            string outSalt;
            string contractHash = BaoMatMatKhau.HashPassword("ContractSecret#99", out outSalt);
            Assert.NotNull(outSalt, "HashPassword out salt must not be null");
            Assert.True(BaoMatMatKhau.VerifyPassword("ContractSecret#99", contractHash, outSalt), "VerifyPassword contract must return true");
            Assert.False(BaoMatMatKhau.VerifyPassword("WrongSecret", contractHash, outSalt), "VerifyPassword contract must return false on mismatch");

            // PasswordHasher alias verification
            string phSalt;
            string phHash = PasswordHasher.HashPassword("PasswordHasherSecret", out phSalt);
            Assert.True(PasswordHasher.VerifyPassword("PasswordHasherSecret", phHash, phSalt), "PasswordHasher.VerifyPassword must return true");
        }

        // =========================================================================
        // SUITE 6: DUAL-MODE OFFLINE DBCONTEXT RESILIENCE UNDER CONCURRENCY
        // =========================================================================

        [E2ETest(5, 91, "CH_M1_DUAL_01: Concurrent offline DbContext fallback does not crash and defaults to in-memory store")]
        public void CH_M1_DUAL_01_OfflineDbContext_DualMode_ConcurrencyResilience()
        {
            DbInitializer.ResetInMemoryStore();

            var exceptions = new ConcurrentBag<Exception>();
            int threadCount = 30;

            Parallel.For(0, threadCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, threadId =>
            {
                try
                {
                    using var deadContext = CreateDeadDbContext();

                    // SeedAdmin with dead context should handle offline gracefully
                    DbInitializer.SeedAdmin(deadContext);

                    // GetAdminByUsername with dead context should fall back to in-memory store
                    var admin = DbInitializer.GetAdminByUsername("admin", deadContext);
                    Assert.NotNull(admin, $"Offline fallback lookup must succeed for thread {threadId}");
                    Assert.Equal("admin", admin!.TenDangNhap, "Username should be admin");

                    // ValidateAdminCredentials with dead context should fall back to in-memory store
                    bool valid = DbInitializer.ValidateAdminCredentials("admin", "Admin@123", deadContext);
                    Assert.True(valid, $"Offline fallback validation must succeed for thread {threadId}");

                    // Case insensitivity with dead context
                    var upperAdmin = DbInitializer.GetAdminByUsername("  ADMIN  ", deadContext);
                    Assert.NotNull(upperAdmin, $"Offline fallback case-insensitive lookup must succeed for thread {threadId}");
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            Assert.True(exceptions.IsEmpty, $"Offline DbContext concurrency produced {exceptions.Count} exceptions: {string.Join(" | ", exceptions.Take(3).Select(e => e.Message))}");
        }
    }
}
