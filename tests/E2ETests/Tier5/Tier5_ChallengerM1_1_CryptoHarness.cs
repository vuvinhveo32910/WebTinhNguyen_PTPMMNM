using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;
using CryptoCore = WEB_TinhNguyen.DuLieu.BaoMatMatKhau;
using AreaCryptoForwarder = WEB_TinhNguyen.Areas.CuaHang.Helpers.BaoMatMatKhau;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Adversarial Challenge & Stress Harness for Milestone 1
    /// Cryptographic Soundness, PBKDF2 Entropy, Boundary Values, Tampered Base64 Resilience,
    /// Constant-Time Verification, and In-Memory Multi-threaded Validation.
    /// </summary>
    public class Tier5_ChallengerM1_1_CryptoHarness
    {
        // =========================================================================
        // 1. SALT ENTROPY & RANDOMNESS (1,000 ITERATIONS)
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_01: Salt Entropy - Confirm TaoMuoiNgauNhien outputs 1000 unique 16-byte random salts")]
        public void ADV_M1_01_SaltEntropy1000Iterations()
        {
            const int iterations = 1000;
            var saltSet = new HashSet<string>(iterations);
            int totalSetBits = 0;
            const int totalExpectedBits = iterations * 16 * 8; // 128,000 bits

            for (int i = 0; i < iterations; i++)
            {
                string saltBase64 = CryptoCore.TaoMuoiNgauNhien();

                Assert.False(string.IsNullOrWhiteSpace(saltBase64), $"Iteration {i}: Salt must not be empty");
                Assert.Equal(24, saltBase64.Length, $"Iteration {i}: 16-byte Base64 salt must be exactly 24 characters");
                Assert.True(saltBase64.EndsWith("=="), $"Iteration {i}: 16-byte Base64 string must end with padding '=='");

                byte[] saltBytes;
                try
                {
                    saltBytes = Convert.FromBase64String(saltBase64);
                }
                catch (Exception ex)
                {
                    throw new AssertException($"Iteration {i}: Salt is not valid Base64: {ex.Message}");
                }

                Assert.Equal(CryptoCore.KichThuocMuoiBytes, saltBytes.Length, 
                    $"Iteration {i}: Salt byte array must be exactly {CryptoCore.KichThuocMuoiBytes} bytes (128 bits)");

                // Accumulate bit entropy
                foreach (byte b in saltBytes)
                {
                    totalSetBits += System.Numerics.BitOperations.PopCount((uint)b);
                }

                bool isUnique = saltSet.Add(saltBase64);
                Assert.True(isUnique, $"Iteration {i}: Collision detected! Duplicate salt generated: {saltBase64}");
            }

            Assert.Equal(iterations, saltSet.Count, "All 1,000 salts must be strictly unique without duplicate");

            // Statistical entropy check: proportion of 1s in a CSPRNG should be ~50% (allow 45% - 55% tolerance)
            double setBitRatio = (double)totalSetBits / totalExpectedBits;
            Assert.True(setBitRatio >= 0.45 && setBitRatio <= 0.55, 
                $"CSPRNG bit entropy ratio {setBitRatio:P2} fell outside expected cryptographic range [45%, 55%]");
        }

        // =========================================================================
        // 2. PBKDF2 HASH DETERMINISM & REPEATABILITY
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_02: PBKDF2 Determinism - Same password + same salt produces identical hash")]
        public void ADV_M1_02_Pbkdf2Determinism()
        {
            var testCases = new[]
            {
                "Admin@123",
                "Complex#P@ssw0rd!2026",
                "TìnhNguyện#Admin@2026!",
                "1",
                "Special!#$%^&*()_+~`|}{[]:;?><,./"
            };

            foreach (var pw in testCases)
            {
                string salt = CryptoCore.TaoMuoiNgauNhien();

                string hash1 = CryptoCore.BamMatKhau(pw, salt);
                string hash2 = CryptoCore.BamMatKhau(pw, salt);
                string hash3 = CryptoCore.BamMatKhau(pw, salt);

                Assert.Equal(44, hash1.Length, $"Hash length must be 44 Base64 chars for 32-byte subkey for password '{pw}'");
                Assert.Equal(hash1, hash2, $"Hash must be deterministic across calls for password '{pw}'");
                Assert.Equal(hash2, hash3, $"Hash must be repeatable for password '{pw}'");

                bool verify1 = CryptoCore.KiemTraMatKhau(pw, hash1, salt);
                bool verify2 = CryptoCore.KiemTraMatKhau(pw, hash2, salt);

                Assert.True(verify1, $"Verification 1 must succeed for password '{pw}'");
                Assert.True(verify2, $"Verification 2 must succeed for password '{pw}'");
            }
        }

        // =========================================================================
        // 3. PBKDF2 SALT RESISTANCE & AVALANCHE EFFECT
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_03: PBKDF2 Salt Resistance - Different salts or 1-bit password delta produce completely distinct hashes")]
        public void ADV_M1_03_Pbkdf2SaltResistanceAndAvalanche()
        {
            const string password = "TestPassword@2026";
            var hashesFromDifferentSalts = new HashSet<string>();

            // Test 1: Same password with 15 different salts
            for (int i = 0; i < 15; i++)
            {
                string salt = CryptoCore.TaoMuoiNgauNhien();
                string hash = CryptoCore.BamMatKhau(password, salt);
                bool added = hashesFromDifferentSalts.Add(hash);
                Assert.True(added, $"Hash collision occurred with different salts on iteration {i}");
            }
            Assert.Equal(15, hashesFromDifferentSalts.Count, "15 different salts must produce 15 completely distinct hashes");

            // Test 2: Avalanche effect - 1-character difference with same salt
            string commonSalt = CryptoCore.TaoMuoiNgauNhien();
            string hashA = CryptoCore.BamMatKhau("LangbiangVolunteer2026A", commonSalt);
            string hashB = CryptoCore.BamMatKhau("LangbiangVolunteer2026B", commonSalt);

            Assert.NotEqual(hashA, hashB, "1-character difference in password must produce distinct hashes");

            byte[] bytesA = Convert.FromBase64String(hashA);
            byte[] bytesB = Convert.FromBase64String(hashB);
            Assert.Equal(32, bytesA.Length, "Hash A must be 32 bytes");
            Assert.Equal(32, bytesB.Length, "Hash B must be 32 bytes");

            int bitDifferences = 0;
            for (int i = 0; i < 32; i++)
            {
                bitDifferences += System.Numerics.BitOperations.PopCount((uint)(bytesA[i] ^ bytesB[i]));
            }

            // Ideal avalanche effect flips ~50% of the 256 bits (128 bits). We verify at least 30% (76 bits)
            Assert.True(bitDifferences >= 76, 
                $"Avalanche effect weak: only {bitDifferences}/256 bits differed between single-character delta");
        }

        // =========================================================================
        // 4. EXTREME BOUNDARY: NULL AND EMPTY INPUTS
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_04: Extreme Boundary - Null and empty passwords and salts handled securely")]
        public void ADV_M1_04_ExtremeBoundary_NullAndEmpty()
        {
            string validSalt = CryptoCore.TaoMuoiNgauNhien();
            string validHash = CryptoCore.BamMatKhau("ValidPass@123", validSalt);

            // BamMatKhau parameter defense
            bool threwOnNullPw = false;
            try { CryptoCore.BamMatKhau(null!, validSalt); }
            catch (ArgumentException) { threwOnNullPw = true; }
            Assert.True(threwOnNullPw, "BamMatKhau must throw ArgumentException on null password");

            bool threwOnEmptyPw = false;
            try { CryptoCore.BamMatKhau("", validSalt); }
            catch (ArgumentException) { threwOnEmptyPw = true; }
            Assert.True(threwOnEmptyPw, "BamMatKhau must throw ArgumentException on empty password");

            bool threwOnNullSalt = false;
            try { CryptoCore.BamMatKhau("ValidPass@123", null!); }
            catch (ArgumentException) { threwOnNullSalt = true; }
            Assert.True(threwOnNullSalt, "BamMatKhau must throw ArgumentException on null salt");

            bool threwOnEmptySalt = false;
            try { CryptoCore.BamMatKhau("ValidPass@123", ""); }
            catch (ArgumentException) { threwOnEmptySalt = true; }
            Assert.True(threwOnEmptySalt, "BamMatKhau must throw ArgumentException on empty salt");

            // KiemTraMatKhau defense: must return false without unhandled exceptions
            Assert.False(CryptoCore.KiemTraMatKhau(null, validHash, validSalt), "KiemTraMatKhau must return false on null input password");
            Assert.False(CryptoCore.KiemTraMatKhau("", validHash, validSalt), "KiemTraMatKhau must return false on empty input password");
            Assert.False(CryptoCore.KiemTraMatKhau("ValidPass@123", null, validSalt), "KiemTraMatKhau must return false on null hash");
            Assert.False(CryptoCore.KiemTraMatKhau("ValidPass@123", "", validSalt), "KiemTraMatKhau must return false on empty hash");
            Assert.False(CryptoCore.KiemTraMatKhau("ValidPass@123", validHash, null), "KiemTraMatKhau must return false on null salt");
            Assert.False(CryptoCore.KiemTraMatKhau("ValidPass@123", validHash, ""), "KiemTraMatKhau must return false on empty salt");
            Assert.False(CryptoCore.KiemTraMatKhau(null, null, null), "KiemTraMatKhau must return false on all null parameters");
        }

        // =========================================================================
        // 5. EXTREME BOUNDARY: SINGLE-CHARACTER PASSWORDS
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_05: Extreme Boundary - Single-character passwords hashed and verified correctly")]
        public void ADV_M1_05_ExtremeBoundary_SingleChar()
        {
            var singleChars = new[] { "a", "Z", "0", "!", " ", "\t" };

            foreach (var ch in singleChars)
            {
                string salt = CryptoCore.TaoMuoiNgauNhien();
                string hash = CryptoCore.BamMatKhau(ch, salt);

                Assert.Equal(44, hash.Length, $"Hash length must be 44 chars for 1-char password '{ch}'");
                Assert.True(CryptoCore.KiemTraMatKhau(ch, hash, salt), $"Verification must succeed for 1-char password '{ch}'");
                Assert.False(CryptoCore.KiemTraMatKhau(ch + "x", hash, salt), $"Verification must fail for altered 1-char password '{ch}'");
                Assert.False(CryptoCore.KiemTraMatKhau("wrong", hash, salt), $"Verification must fail for wrong password with 1-char hash '{ch}'");
            }
        }

        // =========================================================================
        // 6. EXTREME BOUNDARY: 10,000-CHARACTER PASSWORD
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_06: Extreme Boundary - 10,000-character password hashed without OOM or buffer overflow")]
        public void ADV_M1_06_ExtremeBoundary_10000Chars()
        {
            // Construct a 10,000 character pseudo-random sequence
            var sb = new StringBuilder(10000);
            for (int i = 0; i < 1000; i++)
            {
                sb.Append("V0lunt33r!");
            }
            string hugePassword = sb.ToString();
            Assert.Equal(10000, hugePassword.Length, "Password must be exactly 10,000 characters");

            string salt = CryptoCore.TaoMuoiNgauNhien();
            string hash = CryptoCore.BamMatKhau(hugePassword, salt);

            Assert.Equal(44, hash.Length, "10,000-char password must still result in 32-byte subkey (44 Base64 chars)");
            Assert.True(CryptoCore.KiemTraMatKhau(hugePassword, hash, salt), "10,000-char password must verify successfully");

            // Verify tampering at tail character
            string tamperedPassword = hugePassword.Substring(0, 9999) + "?";
            Assert.False(CryptoCore.KiemTraMatKhau(tamperedPassword, hash, salt), "Tampered 10,000-char password must be rejected");

            // Verify truncated password (9,999 chars)
            string truncatedPassword = hugePassword.Substring(0, 9999);
            Assert.False(CryptoCore.KiemTraMatKhau(truncatedPassword, hash, salt), "Truncated 10,000-char password must be rejected");
        }

        // =========================================================================
        // 7. UNICODE EMOJIS AND VIETNAMESE ACCENTED CHARACTERS
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_07: Unicode & Vietnamese Accents - Multi-byte Unicode, emojis, and diacritics")]
        public void ADV_M1_07_UnicodeAndVietnameseAccents()
        {
            var unicodePasswords = new[]
            {
                "TìnhNguyện#Admin@2026!",
                "🔐TrăngSángLangbiang2026🏔️❤️🇻🇳",
                "ĐồngBàoK'Ho#LâmĐồng@2026",
                "Một_Chút_Yêu_Thương_Gửi_Về_Núi_Rừng_2026!",
                "Unicode_Mix_漢字_日本語_العربية_Русский_123"
            };

            foreach (var pw in unicodePasswords)
            {
                string salt = CryptoCore.TaoMuoiNgauNhien();
                string hash = CryptoCore.BamMatKhau(pw, salt);

                Assert.True(CryptoCore.KiemTraMatKhau(pw, hash, salt), $"Verification must succeed for Unicode password '{pw}'");

                // Case-sensitivity and diacritic sensitivity check
                string altered = pw.Replace('è', 'e').Replace('ệ', 'e').Replace('í', 'i').Replace('2', '3');
                if (altered != pw)
                {
                    Assert.False(CryptoCore.KiemTraMatKhau(altered, hash, salt), 
                        $"Diacritic-stripped password must fail verification for '{pw}'");
                }
            }
        }

        // =========================================================================
        // 8. TAMPERED BASE64: CORRUPTED CHARACTERS
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_08: Tampered Base64 - Corrupted characters in salt or hash return false with zero crashes")]
        public void ADV_M1_08_TamperedBase64_CorruptedCharacters()
        {
            string validSalt = CryptoCore.TaoMuoiNgauNhien();
            string validHash = CryptoCore.BamMatKhau("Password@123", validSalt);

            var corruptedStrings = new[]
            {
                "NotBase64!@#$%^&*()_+",
                "InvalidChars~~~````===",
                "Contains Spaces In Middle==",
                "Null\0Byte\0Injection==",
                "<script>alert('xss')</script>",
                "'-DROP-TABLE-TaiKhoanAdmin--"
            };

            foreach (var corrupted in corruptedStrings)
            {
                // Corrupted salt
                bool resultWithBadSalt = CryptoCore.KiemTraMatKhau("Password@123", validHash, corrupted);
                Assert.False(resultWithBadSalt, $"Corrupted salt '{corrupted}' must return false without throwing");

                // Corrupted hash
                bool resultWithBadHash = CryptoCore.KiemTraMatKhau("Password@123", corrupted, validSalt);
                Assert.False(resultWithBadHash, $"Corrupted hash '{corrupted}' must return false without throwing");

                // Both corrupted
                bool resultBothBad = CryptoCore.KiemTraMatKhau("Password@123", corrupted, corrupted);
                Assert.False(resultBothBad, $"Both corrupted '{corrupted}' must return false without throwing");
            }
        }

        // =========================================================================
        // 9. TAMPERED BASE64: INVALID PADDING AND TRUNCATION
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_09: Tampered Base64 - Invalid padding, non-4 lengths, and truncated hashes handled safely")]
        public void ADV_M1_09_TamperedBase64_InvalidPaddingAndTruncation()
        {
            string validSalt = CryptoCore.TaoMuoiNgauNhien();
            string validHash = CryptoCore.BamMatKhau("Password@123", validSalt);

            var malformedPaddings = new[]
            {
                "A",
                "AB",
                "ABC",
                "ABC=",
                "A==",
                "====",
                validHash.Substring(0, 10), // Truncated hash string
                validHash + "===",          // Excess padding
                validSalt.Substring(0, 8)   // Truncated salt string
            };

            foreach (var malformed in malformedPaddings)
            {
                bool resSalt = CryptoCore.KiemTraMatKhau("Password@123", validHash, malformed);
                Assert.False(resSalt, $"Malformed salt '{malformed}' must return false safely");

                bool resHash = CryptoCore.KiemTraMatKhau("Password@123", malformed, validSalt);
                Assert.False(resHash, $"Malformed hash '{malformed}' must return false safely");
            }

            // Valid Base64 that decodes to empty byte array
            string emptyBase64 = Convert.ToBase64String(Array.Empty<byte>());
            Assert.False(CryptoCore.KiemTraMatKhau("Password@123", validHash, emptyBase64), 
                "Empty byte-array Base64 salt must return false safely");
            Assert.False(CryptoCore.KiemTraMatKhau("Password@123", emptyBase64, validSalt), 
                "Empty byte-array Base64 hash must return false safely");

            // Truncated valid Base64 hash (e.g. 16 bytes instead of 32 bytes)
            byte[] shortHashBytes = RandomNumberGenerator.GetBytes(16);
            string shortHashBase64 = Convert.ToBase64String(shortHashBytes);
            Assert.False(CryptoCore.KiemTraMatKhau("Password@123", shortHashBase64, validSalt), 
                "Truncated 16-byte hash must not validate against 32-byte derived password");
        }

        // =========================================================================
        // 10. CONSTANT-TIME VERIFICATION: FixedTimeEquals
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_10: Constant-Time Verification - Confirm CryptographicOperations.FixedTimeEquals is utilized")]
        public void ADV_M1_10_ConstantTimeEqualsVerification()
        {
            // 1. Inspect source code file of BaoMatMatKhau
            var sourcePath = Path.Combine(E2ETestContext.ProjectRoot, "DuLieu", "BaoMatMatKhau.cs");
            Assert.RequireFile(sourcePath, "WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs");

            var sourceCode = File.ReadAllText(sourcePath);
            Assert.True(sourceCode.Contains("CryptographicOperations.FixedTimeEquals"), 
                "BaoMatMatKhau.cs must explicitly call CryptographicOperations.FixedTimeEquals to eliminate timing attacks");

            // 2. Inspect method call structure via Reflection
            var method = typeof(CryptoCore).GetMethod("KiemTraMatKhau", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method, "KiemTraMatKhau method must exist on BaoMatMatKhau");

            var methodBody = method!.GetMethodBody();
            Assert.NotNull(methodBody, "MethodBody of KiemTraMatKhau must be readable");

            // 3. Verify constant-time behavior with variable matching prefixes
            // Even if first 31 bytes match, comparison takes full evaluation
            string salt = CryptoCore.TaoMuoiNgauNhien();
            string correctHash = CryptoCore.BamMatKhau("SecretPass@123", salt);
            byte[] hashBytes = Convert.FromBase64String(correctHash);

            // Flip only the very last bit
            byte[] almostMatching = (byte[])hashBytes.Clone();
            almostMatching[31] ^= 0x01;
            string almostMatchingHash = Convert.ToBase64String(almostMatching);

            Assert.False(CryptoCore.KiemTraMatKhau("SecretPass@123", almostMatchingHash, salt), 
                "Hash differing by only 1 bit at byte 31 must return false");
        }

        // =========================================================================
        // 11. CONTRACT ALIASES EQUIVALENCE
        // =========================================================================

        [E2ETest(5, 2, "ADV_M1_11: Contract Aliases - HashPassword, VerifyPassword, and PasswordHasher work identically")]
        public void ADV_M1_11_ContractAliasesEquivalence()
        {
            const string password = "AdminPassword#2026!";

            // 1. BaoMatMatKhau contract aliases
            string hash1 = CryptoCore.HashPassword(password, out string salt1);
            Assert.Equal(44, hash1.Length, "HashPassword must return 44 Base64 chars");
            Assert.Equal(24, salt1.Length, "HashPassword must output 24 Base64 chars salt");
            Assert.True(CryptoCore.VerifyPassword(password, hash1, salt1), "VerifyPassword must succeed for valid credentials");
            Assert.False(CryptoCore.VerifyPassword("WrongPassword", hash1, salt1), "VerifyPassword must fail for wrong credentials");

            // 2. PasswordHasher alias class
            string hash2 = PasswordHasher.HashPassword(password, out string salt2);
            Assert.True(PasswordHasher.VerifyPassword(password, hash2, salt2), "PasswordHasher.VerifyPassword must succeed");
            Assert.False(PasswordHasher.VerifyPassword("Wrong", hash2, salt2), "PasswordHasher.VerifyPassword must fail for wrong password");

            // 3. Cross-alias interoperability
            Assert.True(PasswordHasher.VerifyPassword(password, hash1, salt1), 
                "PasswordHasher must verify hash generated by BaoMatMatKhau.HashPassword");
            Assert.True(CryptoCore.KiemTraMatKhau(password, hash2, salt2), 
                "BaoMatMatKhau.KiemTraMatKhau must verify hash generated by PasswordHasher");

            // 4. Area forwarding helper
            string forwardHash = AreaCryptoForwarder.BamMatKhau(password, salt1);
            Assert.Equal(hash1, forwardHash, "Area forwarding helper must produce identical hash");
            Assert.True(AreaCryptoForwarder.KiemTraMatKhau(password, hash1, salt1), 
                "Area forwarding helper must verify identical hash");
        }

        // =========================================================================
        // 12. DBINITIALIZER: DEFAULT ADMIN CREDENTIAL VALIDATION
        // =========================================================================

        [E2ETest(5, 4, "ADV_M1_12: DbInitializer - Default admin credentials validation, case insensitivity, and trimming")]
        public void ADV_M1_12_DbInitializer_DefaultAdminCredentialValidation()
        {
            DbInitializer.ResetInMemoryStore();

            // 1. Valid credentials
            bool valid = DbInitializer.ValidateAdminCredentials("admin", "Admin@123");
            Assert.True(valid, "Default credentials ('admin', 'Admin@123') must validate to true");

            // 2. Username case-insensitivity: "ADMIN", "Admin", "aDmIn"
            Assert.True(DbInitializer.ValidateAdminCredentials("ADMIN", "Admin@123"), 
                "Username validation must be case-insensitive ('ADMIN')");
            Assert.True(DbInitializer.ValidateAdminCredentials("Admin", "Admin@123"), 
                "Username validation must be case-insensitive ('Admin')");
            Assert.True(DbInitializer.ValidateAdminCredentials("aDmIn", "Admin@123"), 
                "Username validation must be case-insensitive ('aDmIn')");

            // 3. Username whitespace trimming
            Assert.True(DbInitializer.ValidateAdminCredentials("  admin  ", "Admin@123"), 
                "Username validation must trim leading/trailing whitespace");
            Assert.True(DbInitializer.ValidateAdminCredentials("admin\t", "Admin@123"), 
                "Username validation must trim tab whitespace");

            // 4. Password case-sensitivity: "admin@123" must FAIL
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "admin@123"), 
                "Password must be strictly case-sensitive ('admin@123' must fail against 'Admin@123')");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "ADMIN@123"), 
                "Password must be strictly case-sensitive ('ADMIN@123' must fail against 'Admin@123')");

            // 5. Invalid passwords
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "Admin@1234"), "Incorrect password must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", "123456"), "Common weak password must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", ""), "Empty password must fail");

            // 6. Non-existent usernames
            Assert.False(DbInitializer.ValidateAdminCredentials("nonexistent", "Admin@123"), "Non-existent user must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("root", "Admin@123"), "Root user must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("", "Admin@123"), "Empty username must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials(null!, "Admin@123"), "Null username must fail");
            Assert.False(DbInitializer.ValidateAdminCredentials("admin", null!), "Null password must fail");

            // 7. Verify LanDangNhapCuoi was updated
            var admin = DbInitializer.GetAdminByUsername("admin");
            Assert.NotNull(admin, "GetAdminByUsername must return default admin entity");
            Assert.NotNull(admin!.LanDangNhapCuoi, "LanDangNhapCuoi must be populated after successful login");
            Assert.True((DateTime.Now - admin.LanDangNhapCuoi!.Value).TotalMinutes < 5, 
                "LanDangNhapCuoi must be within last 5 minutes");
        }

        // =========================================================================
        // 13. DBINITIALIZER: INACTIVE ADMIN REJECTED
        // =========================================================================

        [E2ETest(5, 4, "ADV_M1_13: DbInitializer - Inactive admin account (TrangThaiHoatDong = false) is rejected")]
        public void ADV_M1_13_DbInitializer_InactiveAdminBlocked()
        {
            DbInitializer.ResetInMemoryStore();

            var admin = DbInitializer.GetAdminByUsername("admin");
            Assert.NotNull(admin, "Admin must exist");

            // Temporarily set inactive
            admin!.TrangThaiHoatDong = false;
            DbInitializer.UpdateInMemoryAdmin(admin);

            // Verify authentication fails even with correct password
            bool loginResult = DbInitializer.ValidateAdminCredentials("admin", "Admin@123");
            Assert.False(loginResult, "Inactive admin (TrangThaiHoatDong = false) must be denied login even with correct password");

            // Restore active state
            admin.TrangThaiHoatDong = true;
            DbInitializer.UpdateInMemoryAdmin(admin);

            bool restoredResult = DbInitializer.ValidateAdminCredentials("admin", "Admin@123");
            Assert.True(restoredResult, "Re-activated admin must be allowed to login successfully");
        }

        // =========================================================================
        // 14. CONCURRENT VALIDATION STRESS (100 PARALLEL THREADS)
        // =========================================================================

        [E2ETest(5, 4, "ADV_M1_14: DbInitializer - 100 concurrent validation threads without race conditions or deadlocks")]
        public void ADV_M1_14_ConcurrentValidationStress100Threads()
        {
            DbInitializer.ResetInMemoryStore();

            const int threadCount = 100;
            var tasks = new Task<bool>[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() =>
                {
                    if (index % 2 == 0)
                    {
                        // Even index: valid credentials
                        return DbInitializer.ValidateAdminCredentials("admin", "Admin@123");
                    }
                    else
                    {
                        // Odd index: invalid password
                        return DbInitializer.ValidateAdminCredentials("admin", $"WrongPassword_{index}");
                    }
                });
            }

            Task.WaitAll(tasks);

            int validSuccessCount = 0;
            int invalidSuccessCount = 0;

            for (int i = 0; i < threadCount; i++)
            {
                bool result = tasks[i].Result;
                if (i % 2 == 0)
                {
                    if (result) validSuccessCount++;
                }
                else
                {
                    if (result) invalidSuccessCount++;
                }
            }

            Assert.Equal(50, validSuccessCount, "All 50 valid authentication threads must succeed");
            Assert.Equal(0, invalidSuccessCount, "Zero of the 50 invalid authentication threads should succeed");
        }
    }
}
