# Cryptographic Password Hasher Investigation & Design Report (`BaoMatMatKhau.cs`)

**Agent ID**: explorer_m1_2  
**Role**: Cryptographic Hasher Explorer  
**Milestone**: M1 (Database Setup & Admin Seeding)  
**Date**: 2026-11-07  

---

## 1. Observation

### 1.1 Existing Codebase & Environment State
- **Framework & Runtime**:
  - `WEB_TinhNguyen/WEB_TinhNguyen.csproj` targets `net10.0` (`<TargetFramework>net10.0</TargetFramework>`, Nullable enabled, ImplicitUsings enabled).
  - Dotnet SDK version: `10.0.402`.
  - Microsoft.AspNetCore.App shared framework is implicitly referenced via `<Project Sdk="Microsoft.NET.Sdk.Web">`.
- **Existing Files in Scope**:
  - `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`: Namespace `WEB_TinhNguyen.DuLieu`.
  - `WEB_TinhNguyen/DuLieu/Compatibility.cs`: Contains global usings (`WEB_TinhNguyen.MoHinh`, `WEB_TinhNguyen.BoDieuKhien`) and backward compatibility aliases.
  - `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`: Namespace `WEB_TinhNguyen.Areas.CuaHang.Helpers`, imports `WEB_TinhNguyen.DuLieu`.
  - `WEB_TinhNguyen/MoHinh/`: Will host `TaiKhoanAdmin.cs` (explored by `explorer_m1_1`).
  - No existing password hashing helper or cryptographic utility exists in `WEB_TinhNguyen`.
- **Test Suite Status**:
  - Command: `dotnet run --project tests/E2ETests/E2ETests.csproj`
  - Result: 477/477 tests pass (Tier 1: 161, Tier 2: 28, Tier 3: 18, Tier 4: 12, Tier 5: 258) with 0 errors and 0 warnings.
- **Sibling Explorer Requirements**:
  - `explorer_m1_1`: Designing `TaiKhoanAdmin.cs` with columns `MatKhauHash` (NVARCHAR 500) and `MuoiBaoMat` (NVARCHAR 100).
  - `explorer_m1_3`: Designing `DbInitializer.SeedAdmin(context)` requiring default admin seed (`admin` / `Admin@123`), and `ValidateAdminCredentials(username, password, context)`.
  - `PROJECT.md` §Interface Contracts:
    - `public static string HashPassword(string password, out string salt)`
    - `public static bool VerifyPassword(string password, string hash, string salt)`
  - Dispatch prompt requirements:
    - `public static string TaoMuoiNgauNhien()` (generates cryptographic random salt, base64 encoded).
    - `public static string BamMatKhau(string matKhau, string muoiBaoMat)` (hashes password with salt, returns base64 hash).
    - `public static bool KiemTraMatKhau(string matKhauNhap, string matKhauHashDaLuu, string muoiBaoMat)` (uses constant-time comparison `CryptographicOperations.FixedTimeEquals` to prevent timing attacks).
    - PBKDF2 (`Rfc2898DeriveBytes` with HMAC-SHA256, >= 100,000 iterations, 16-byte random salt).
    - Zero plain-text guarantee.

### 1.2 Cryptographic Primitives Benchmark
- Tested via isolated runtime script:
  - `RandomNumberGenerator.GetBytes(16)` generates 16 bytes (128 bits) of entropy -> Base64 string is 24 characters.
  - `Rfc2898DeriveBytes.Pbkdf2` using HMAC-SHA256, 100,000 iterations, 32 bytes output key length -> Base64 string is 44 characters.
  - Execution speed: ~12 ms per hash on current CPU.
  - `CryptographicOperations.FixedTimeEquals`: Validated constant-time comparison on byte spans.
  - Edge cases verified: Unicode passwords (`TìnhNguyện@2026🇻🇳🔒`), null/empty checks, corrupted Base64, and length mismatches handled safely without throwing unhandled exceptions.

---

## 2. Logic Chain

### 2.1 Algorithm Selection: PBKDF2 with HMAC-SHA256
- **Observation**: Task requires PBKDF2 with HMAC-SHA256, >= 100,000 iterations, 16-byte random salt.
- **Reasoning**:
  1. PBKDF2 (Password-Based Key Derivation Function 2, RFC 2898 / RFC 8018) is the industry standard recommended by NIST (SP 800-63B) and OWASP.
  2. In .NET 10, `Rfc2898DeriveBytes.Pbkdf2` is natively implemented in `System.Security.Cryptography`, calling OS-native crypto libraries (CNG on Windows, OpenSSL on Linux).
  3. Unlike the legacy `new Rfc2898DeriveBytes(...)` constructors which emit obsolete warning `SYSLIB0041` in .NET 7+, the static method `Rfc2898DeriveBytes.Pbkdf2` is the modern, zero-warning, high-performance API.
  4. 100,000 iterations provide strong resistance against offline GPU/ASIC rainbow table and dictionary cracking attacks while maintaining a user-acceptable login duration of ~12ms.

### 2.2 Salt Generation: CSPRNG (128-bit / 16 bytes)
- **Observation**: Rainbow table and pre-computation attacks succeed when salts are missing, short, or static.
- **Reasoning**:
  1. 16 bytes (128 bits) provides $2^{128}$ possible salt values, rendering rainbow tables completely useless.
  2. `RandomNumberGenerator.GetBytes(16)` uses the OS cryptographic pseudo-random number generator (CSPRNG), guaranteeing non-predictability.
  3. `Convert.ToBase64String(salt)` outputs a 24-character Base64 string (ending in `==`), which fits safely within `MuoiBaoMat` (`NVARCHAR(100)`).

### 2.3 Derived Key & Hash Output (256-bit / 32 bytes)
- **Observation**: Hash output must fit into `MatKhauHash` (`NVARCHAR(500)`).
- **Reasoning**:
  1. A 32-byte (256-bit) derived key matches the native output size of HMAC-SHA256.
  2. `Convert.ToBase64String(hash)` produces a 44-character Base64 string (ending in `=`).
  3. 44 characters is compact, clean, and easily fits in `NVARCHAR(500)`.

### 2.4 Prevention of Timing Attacks via `CryptographicOperations.FixedTimeEquals`
- **Observation**: Regular string equality comparison (`a == b`) short-circuits on the first mismatched character. An attacker measuring HTTP response times down to nanoseconds/microseconds can infer matching character prefixes.
- **Reasoning**:
  1. `CryptographicOperations.FixedTimeEquals(actualSpan, expectedSpan)` executes in constant time regardless of where or whether byte differences exist.
  2. Verification logic must decode the stored Base64 hash into bytes and compare raw byte spans using `CryptographicOperations.FixedTimeEquals`.
  3. If input is malformed Base64, null, or empty, the method catches the error and safely returns `false`, preventing denial-of-service or unhandled 500 errors.

### 2.5 Unified API Contract & Backward Compatibility
- **Observation**:
  - Prompt requires: `TaoMuoiNgauNhien()`, `BamMatKhau(string, string)`, `KiemTraMatKhau(string, string, string)`.
  - `PROJECT.md` contracts require: `HashPassword(string, out string)`, `VerifyPassword(string, string, string)`.
  - Dispatch allows placement in `WEB_TinhNguyen/DuLieu/` or `WEB_TinhNguyen/Areas/CuaHang/Helpers/`.
- **Reasoning**:
  1. Primary location should be `WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs` under namespace `WEB_TinhNguyen.DuLieu` because security and data hashing belong to the core data infrastructure.
  2. Provide convenience aliases:
     - `HashPassword(password, out salt)` calls `TaoMuoiNgauNhien()` then `BamMatKhau()`.
     - `VerifyPassword(password, hash, salt)` directly forwards to `KiemTraMatKhau(password, hash, salt)`.
  3. Also provide forwarding class in `WEB_TinhNguyen.Areas.CuaHang.Helpers` (or alias in `Compatibility.cs`), plus `PasswordHasher` alias, ensuring any caller in any module or test compiles without reference friction.

### 2.6 Zero Plain-Text Guarantee Audit
- **Observation**: Prompt requires verification that no plain text password is ever stored or logged.
- **Reasoning**:
  1. **Database Schema**: Table `[dbo].[TaiKhoanAdmin]` only has `[MatKhauHash]` and `[MuoiBaoMat]`. It has NO column for plain-text password.
  2. **Entity Model**: Model `TaiKhoanAdmin.cs` contains only `string MatKhauHash` and `string MuoiBaoMat`. It does NOT have a `MatKhau` property.
  3. **In-Memory Store**: `DbInitializer._inMemoryAdmins` stores `TaiKhoanAdmin` objects containing only hashed passwords and salts.
  4. **Seeding**: In `DbInitializer.SeedAdmin`, `Admin@123` is immediately hashed using `BaoMatMatKhau.TaoMuoiNgauNhien()` and `BaoMatMatKhau.BamMatKhau()`. The seed admin record is persisted with the hash and salt only. The plain text string is local and immediately discarded.
  5. **View Model Separation**: `AdminLoginViewModel` contains `string MatKhau` for form submission only. It is never persisted, never serialized to session, and never logged.
  6. **Logging Hygiene**: In `AdminController.Login`, loggers must ONLY log the username (`TenDangNhap`), never the password (`model.MatKhau`).
  7. **Cookie Claims**: Authentication cookie must only contain user ID, username, and role claims. Never store password hash or password in cookie claims.

---

## 3. Caveats

1. **Iteration Count**:
   - 100,000 iterations is chosen as the standard balance between security and login performance (~12ms). If future hardware or security policies require higher iterations (e.g. 210,000), `IterationCount` should be defined as a public constant `public const int Iterations = 100_000;`.
2. **Backward Compatibility with Legacy Hashes**:
   - Since this is a fresh admin authentication subsystem and no legacy user passwords exist, no migration path from MD5/SHA1 is needed.
3. **No Third-Party Package Dependencies**:
   - `System.Security.Cryptography` is part of the standard .NET 10 BCL. No external packages (like BCrypt.Net) are needed, keeping the dependency tree clean and avoiding transitive vulnerability alerts.

---

## 4. Conclusion & Concrete Specification

### 4.1 Primary Implementation: `WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs`

```csharp
// =========================================================================
// TIỆN ÍCH BẢO MẬT MẬT KHẨU (PBKDF2 HMAC-SHA256)
// =========================================================================

using System;
using System.Security.Cryptography;

namespace WEB_TinhNguyen.DuLieu
{
    /// <summary>
    /// Cung cấp các phương thức băm và kiểm tra mật khẩu bằng thuật toán chuẩn PBKDF2 (HMAC-SHA256).
    /// Đảm bảo tính ngẫu nhiên của muối bảo mật (Salt) và chống tấn công Timing Attack.
    /// </summary>
    public static class BaoMatMatKhau
    {
        /// <summary>
        /// Kích thước muối ngẫu nhiên (16 bytes = 128-bit entropy).
        /// </summary>
        public const int KichThuocMuoiBytes = 16;

        /// <summary>
        /// Kích thước chuỗi băm đầu ra (32 bytes = 256-bit subkey).
        /// </summary>
        public const int KichThuocHashBytes = 32;

        /// <summary>
        /// Số vòng lặp băm PBKDF2 (tối thiểu 100.000 vòng theo chuẩn an toàn hiện đại).
        /// </summary>
        public const int SoVongLap = 100_000;

        /// <summary>
        /// Tạo chuỗi muối (salt) ngẫu nhiên 16 bytes bằng bộ sinh số ngẫu nhiên mật mã (CSPRNG).
        /// Trả về chuỗi được mã hóa Base64 (24 ký tự).
        /// </summary>
        public static string TaoMuoiNgauNhien()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(KichThuocMuoiBytes);
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Băm mật khẩu cùng với chuỗi muối bằng thuật toán PBKDF2 (HMAC-SHA256, 100.000 vòng lặp).
        /// Trả về chuỗi băm 32 bytes được mã hóa Base64 (44 ký tự).
        /// </summary>
        /// <param name="matKhau">Mật khẩu dạng chuỗi văn bản thuần cần băm.</param>
        /// <param name="muoiBaoMat">Chuỗi muối ngẫu nhiên định dạng Base64.</param>
        /// <returns>Chuỗi hash Base64 của mật khẩu.</returns>
        public static string BamMatKhau(string matKhau, string muoiBaoMat)
        {
            if (string.IsNullOrEmpty(matKhau))
            {
                throw new ArgumentException("Mật khẩu không được để trống.", nameof(matKhau));
            }
            if (string.IsNullOrEmpty(muoiBaoMat))
            {
                throw new ArgumentException("Muối bảo mật không được để trống.", nameof(muoiBaoMat));
            }

            byte[] saltBytes = Convert.FromBase64String(muoiBaoMat);
            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                matKhau,
                saltBytes,
                SoVongLap,
                HashAlgorithmName.SHA256,
                KichThuocHashBytes);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Kiểm tra tính khớp nối của mật khẩu người dùng nhập vào so với chuỗi hash đã lưu trong CSDL.
        /// Sử dụng so sánh đẳng thời (FixedTimeEquals) để triệt tiêu hoàn toàn nguy cơ Timing Attack.
        /// </summary>
        /// <param name="matKhauNhap">Mật khẩu do người dùng nhập từ form đăng nhập.</param>
        /// <param name="matKhauHashDaLuu">Chuỗi băm Base64 đã lưu trong cơ sở dữ liệu.</param>
        /// <param name="muoiBaoMat">Chuỗi muối Base64 đã lưu trong cơ sở dữ liệu.</param>
        /// <returns>True nếu mật khẩu chính xác; ngược lại False.</returns>
        public static bool KiemTraMatKhau(string? matKhauNhap, string? matKhauHashDaLuu, string? muoiBaoMat)
        {
            if (string.IsNullOrEmpty(matKhauNhap) || 
                string.IsNullOrEmpty(matKhauHashDaLuu) || 
                string.IsNullOrEmpty(muoiBaoMat))
            {
                return false;
            }

            try
            {
                byte[] saltBytes = Convert.FromBase64String(muoiBaoMat);
                byte[] expectedHashBytes = Convert.FromBase64String(matKhauHashDaLuu);
                byte[] actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                    matKhauNhap,
                    saltBytes,
                    SoVongLap,
                    HashAlgorithmName.SHA256,
                    expectedHashBytes.Length > 0 ? expectedHashBytes.Length : KichThuocHashBytes);

                return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
            }
            catch
            {
                // Phòng vệ an toàn khi chuỗi Base64 không hợp lệ
                return false;
            }
        }

        // =========================================================================
        // CONTRACT ALIASES (Tương thích giao diện tiếng Anh theo PROJECT.md)
        // =========================================================================

        /// <summary>
        /// Tạo muối mới và băm mật khẩu, trả về chuỗi hash và chuỗi muối qua tham số out salt.
        /// </summary>
        public static string HashPassword(string password, out string salt)
        {
            salt = TaoMuoiNgauNhien();
            return BamMatKhau(password, salt);
        }

        /// <summary>
        /// Kiểm tra mật khẩu dựa trên hash và salt đã lưu.
        /// </summary>
        public static bool VerifyPassword(string password, string hash, string salt)
        {
            return KiemTraMatKhau(password, hash, salt);
        }
    }

    /// <summary>
    /// Alias tương thích cho PasswordHasher để hỗ trợ các kiểm thử và tài liệu tiếng Anh.
    /// </summary>
    public static class PasswordHasher
    {
        public static string HashPassword(string password, out string salt) => BaoMatMatKhau.HashPassword(password, out salt);
        public static bool VerifyPassword(string password, string hash, string salt) => BaoMatMatKhau.VerifyPassword(password, hash, salt);
    }
}
```

### 4.2 Forwarding Helper: `WEB_TinhNguyen/Areas/CuaHang/Helpers/BaoMatMatKhau.cs`

```csharp
// =========================================================================
// LỚP CHUYỂN TIẾP CHO KHU VỰC CỬA HÀNG VÀ CÁC MODULE NGOẠI VI
// =========================================================================

namespace WEB_TinhNguyen.Areas.CuaHang.Helpers
{
    /// <summary>
    /// Chuyển tiếp tới WEB_TinhNguyen.DuLieu.BaoMatMatKhau để đảm bảo mã gọi từ bất kỳ namespace nào cũng tương thích 100%.
    /// </summary>
    public static class BaoMatMatKhau
    {
        public static string TaoMuoiNgauNhien() => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.TaoMuoiNgauNhien();
        public static string BamMatKhau(string matKhau, string muoiBaoMat) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.BamMatKhau(matKhau, muoiBaoMat);
        public static bool KiemTraMatKhau(string? matKhauNhap, string? matKhauHashDaLuu, string? muoiBaoMat) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.KiemTraMatKhau(matKhauNhap, matKhauHashDaLuu, muoiBaoMat);
        public static string HashPassword(string password, out string salt) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.HashPassword(password, out salt);
        public static bool VerifyPassword(string password, string hash, string salt) => WEB_TinhNguyen.DuLieu.BaoMatMatKhau.VerifyPassword(password, hash, salt);
    }
}
```

### 4.3 Integration Instructions for Other M1 Components
1. **`DbInitializer.SeedAdmin(ApplicationDbContext context)`**:
   ```csharp
   // Sinh muối ngẫu nhiên và băm mật khẩu cho tài khoản admin mặc định
   string salt = BaoMatMatKhau.TaoMuoiNgauNhien();
   string hash = BaoMatMatKhau.BamMatKhau("Admin@123", salt);

   var defaultAdmin = new TaiKhoanAdmin
   {
       TenDangNhap = "admin",
       MatKhauHash = hash,
       MuoiBaoMat = salt,
       HoTen = "Quản trị viên Hệ thống",
       Email = "admin@tinhnguyen.vn",
       VaiTro = "Admin",
       TrangThaiHoatDong = true,
       NgayTao = DateTime.Now
   };
   ```
2. **`DbInitializer.ValidateAdminCredentials(username, password, context)`**:
   ```csharp
   public static bool ValidateAdminCredentials(string username, string password, ApplicationDbContext? context = null)
   {
       if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
           return false;

       var admin = GetAdminByUsername(username, context);
       if (admin == null || !admin.TrangThaiHoatDong)
           return false;

       return BaoMatMatKhau.KiemTraMatKhau(password, admin.MatKhauHash, admin.MuoiBaoMat);
   }
   ```

---

## 5. Verification Method

### 5.1 Build Verification
- Command:
  ```powershell
  dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen\WEB_TinhNguyen.csproj
  ```
- Expected Result: 0 errors, 0 warnings related to obsolete cryptography methods.

### 5.2 Test Verification
- Command:
  ```powershell
  dotnet run --project tests/E2ETests/E2ETests.csproj
  ```
- Expected Result: All existing 477 tests pass.

### 5.3 Cryptographic Quality Test Cases (for M1 Challenger & E2E Tests)
When implemented, the test harness should verify:
1. `TaoMuoiNgauNhien()`:
   - Returns non-empty Base64 string of length 24.
   - Two successive calls return different salts (entropy check).
2. `BamMatKhau(password, salt)`:
   - Returns non-empty Base64 string of length 44.
   - Same password + same salt = identical hash (deterministic).
   - Same password + different salt = different hash (salting protection).
   - Empty password or salt throws `ArgumentException`.
3. `KiemTraMatKhau(input, hash, salt)`:
   - Correct password returns `true`.
   - Wrong password returns `false`.
   - Wrong salt returns `false`.
   - Null or empty inputs return `false` without throwing exceptions.
   - Malformed Base64 inputs return `false` without crashing.
   - Unicode / Vietnamese passwords (`Admin@123`, `KhẩuBảoMật@2026`) match correctly.
4. Zero Plain-Text Audit:
   - Verify `TaiKhoanAdmin` model contains no `MatKhau` property.
   - Verify SQL table `[dbo].[TaiKhoanAdmin]` schema has no plain-text password column.
   - Verify logger in `AdminController` never logs `model.MatKhau`.
