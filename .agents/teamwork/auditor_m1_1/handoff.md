# Forensic Audit Report: Milestone 1 Integrity Audit

**Auditor**: `auditor_m1_1`  
**Role**: Forensic Integrity Auditor  
**Milestone**: M1 (Admin Auth Schema & PBKDF2 Password Security)  
**Profile**: General Project  
**Integrity Mode**: Development (also verified compliant under Demo and Benchmark criteria)  
**Verdict**: **CLEAN**  

---

## 1. Observation

### 1.1 Scope and Code Base Inspection
Audited all files modified or created for Milestone 1 by `worker_m1`:
1. `D:\web_tinh_nguyen\WEB_TinhNguyen\MoHinh\TaiKhoanAdmin.cs`:
   - Entity `TaiKhoanAdmin` mapped to table `[Table("TaiKhoanAdmin")]` with unique index `[Index(nameof(TenDangNhap), IsUnique = true)]`.
   - Properties: `int IDTaiKhoan`, `string TenDangNhap`, `string MatKhauHash` (length 500), `string MuoiBaoMat` (length 100), `string? HoTen`, `string? Email`, `string VaiTro` (default "Admin"), `bool TrangThaiHoatDong` (default true), `DateTime NgayTao`, `DateTime? LanDangNhapCuoi`.
   - Method `Clone()` provides deep copy isolation.
   - Contains `AdminLoginViewModel` with properties `TenDangNhap`, `MatKhau` (`[DataType(DataType.Password)]`), `GhiNhoDangNhap`, and `ReturnUrl`.
   - **Zero plain-text password field**: No property stores plain-text passwords in `TaiKhoanAdmin`.

2. `D:\web_tinh_nguyen\WEB_TinhNguyen\DuLieu\ApplicationDbContext.cs`:
   - Line 56-57: `public DbSet<TaiKhoanAdmin> TaiKhoanAdmins { get; set; }` and singular alias `public DbSet<TaiKhoanAdmin> TaiKhoanAdmin { get => TaiKhoanAdmins; set => TaiKhoanAdmins = value; }`.
   - Lines 226-240: In `OnModelCreating`, configured `modelBuilder.Entity<TaiKhoanAdmin>` with table `"TaiKhoanAdmin"`, Primary Key `IDTaiKhoan` (`ValueGeneratedOnAdd`), unique index on `TenDangNhap`, column length limits (500 for `MatKhauHash`, 100 for `MuoiBaoMat`), and default values.

3. `D:\web_tinh_nguyen\WEB_TinhNguyen\DuLieu\BaoMatMatKhau.cs`:
   - Line 19: `public const int KichThuocMuoiBytes = 16;` (128-bit entropy).
   - Line 24: `public const int KichThuocHashBytes = 32;` (256-bit subkey).
   - Line 29: `public const int SoVongLap = 100_000;` (100,000 PBKDF2 iterations).
   - Line 35-39: `TaoMuoiNgauNhien()`:
     ```csharp
     byte[] saltBytes = RandomNumberGenerator.GetBytes(KichThuocMuoiBytes);
     return Convert.ToBase64String(saltBytes);
     ```
     Uses cryptographically secure random bytes via CSPRNG (`RandomNumberGenerator.GetBytes`).
   - Lines 48-68: `BamMatKhau(string matKhau, string muoiBaoMat)`:
     ```csharp
     byte[] saltBytes = Convert.FromBase64String(muoiBaoMat);
     byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
         matKhau,
         saltBytes,
         SoVongLap,
         HashAlgorithmName.SHA256,
         KichThuocHashBytes);
     return Convert.ToBase64String(hashBytes);
     ```
     Genuine PBKDF2 HMAC-SHA256 calculation.
   - Lines 78-105: `KiemTraMatKhau(string? matKhauNhap, string? matKhauHashDaLuu, string? muoiBaoMat)`:
     ```csharp
     byte[] saltBytes = Convert.FromBase64String(muoiBaoMat);
     byte[] expectedHashBytes = Convert.FromBase64String(matKhauHashDaLuu);
     byte[] actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(
         matKhauNhap,
         saltBytes,
         SoVongLap,
         HashAlgorithmName.SHA256,
         expectedHashBytes.Length > 0 ? expectedHashBytes.Length : KichThuocHashBytes);

     return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
     ```
     Recalculates the PBKDF2 hash using the provided salt and input password, comparing bytes with `CryptographicOperations.FixedTimeEquals` to prevent side-channel timing attacks.
   - Lines 114-136: Provides interface contracts `HashPassword`, `VerifyPassword`, and alias class `PasswordHasher`.

4. `D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\CuaHang\Helpers\BaoMatMatKhau.cs`:
   - Pure forwarder delegating directly to `WEB_TinhNguyen.DuLieu.BaoMatMatKhau`.

5. `D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\CuaHang\Helpers\DbInitializer.cs`:
   - Lines 43-54: `GetOrCreateDefaultAdminCredentials()` generates a random 16-byte salt via `BaoMatMatKhau.TaoMuoiNgauNhien()` and computes `_defaultAdminHash = BaoMatMatKhau.BamMatKhau("Admin@123", _defaultAdminSalt)`.
   - Lines 59-79: `GetDefaultAdmins()` creates the default admin entity (`admin`) with `MatKhauHash` and `MuoiBaoMat`, with zero plaintext password retained.
   - Lines 413-427 & 1010-1026: Idempotent DDL `IF OBJECT_ID(N'[dbo].[TaiKhoanAdmin]', N'U') IS NULL` creates the table in SQL Server with columns `[IDTaiKhoan]`, `[TenDangNhap]`, `[MatKhauHash]`, `[MuoiBaoMat]`, `[HoTen]`, `[Email]`, `[VaiTro]`, `[TrangThaiHoatDong]`, `[NgayTao]`, `[LanDangNhapCuoi]`. Zero plain-text columns.
   - Lines 1114-1170: `ValidateAdminCredentials(username, password, context)`:
     Retrieves the admin entity via `GetAdminByUsername` and verifies credentials strictly using `BaoMatMatKhau.KiemTraMatKhau(password, admin.MatKhauHash, admin.MuoiBaoMat)`. No backdoor shortcuts or hardcoded plain-text string equality checks exist. On success, updates `LanDangNhapCuoi` in memory and in SQL Server (if online).

### 1.2 View Restriction Verification
- Inspected `WEB_TinhNguyen/Areas/CuaHang/Views/`:
  - LastWriteTime audit shows zero files modified under `WEB_TinhNguyen/Areas/CuaHang/Views/` during Milestone 1.
  - The restricted view folder remained completely untouched.

### 1.3 Independent Build & Test Execution
- **Build Solution Command**:
  ```powershell
  dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx
  ```
  **Verbatim Result**:
  ```
  Build succeeded.
      2 Warning(s)
      0 Error(s)
  Time Elapsed 00:00:12.33
  ```

- **Full E2E Regression Test Suite Command**:
  ```powershell
  dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
  ```
  **Verbatim Result**:
  ```
  ================================================================================
                                TEST EXECUTION SUMMARY
  ================================================================================
    Tier 1: Total: 161 | Passed: 161 | Failed:   0 | Skipped:   0
    Tier 2: Total:  28 | Passed:  28 | Failed:   0 | Skipped:   0
    Tier 3: Total:  18 | Passed:  18 | Failed:   0 | Skipped:   0
    Tier 4: Total:  12 | Passed:  12 | Failed:   0 | Skipped:   0
    Tier 5: Total: 281 | Passed: 281 | Failed:   0 | Skipped:   0
  --------------------------------------------------------------------------------
    OVERALL: 500 Tests | 500 Passed | 0 Failed | 0 Skipped in 11.47s
  ================================================================================
  ```
  All 500 tests executed and passed 100% with 0 failures and 0 skipped.

---

## 2. Logic Chain

1. **Static Analysis & Schema Integrity**:
   - Examination of `TaiKhoanAdmin.cs` and the SQL DDL in `DbInitializer.cs` confirms that password storage is exclusively handled via `MatKhauHash` (NVARCHAR 500) and `MuoiBaoMat` (NVARCHAR 100). No plain-text passwords or backdoor comparison strings are stored in the database schema or model.
2. **Cryptographic Algorithm Authenticity**:
   - `BaoMatMatKhau.cs` calls `RandomNumberGenerator.GetBytes(16)` for salt generation, which guarantees 128 bits of cryptographic entropy.
   - `BamMatKhau` and `KiemTraMatKhau` invoke standard .NET BCL `Rfc2898DeriveBytes.Pbkdf2` specifying `HashAlgorithmName.SHA256`, 100,000 iterations, and 32 bytes output.
   - Verification uses `CryptographicOperations.FixedTimeEquals` on byte spans, eliminating timing side-channels.
3. **Cheat & Dummy Implementation Absence**:
   - Code inspection of `ValidateAdminCredentials` confirms that authentication requires finding the admin by `TenDangNhap` and recalculating the PBKDF2 hash against `admin.MuoiBaoMat`.
   - String searches across the repository confirmed that `"Admin@123"` appears only in XML comments and in `GetOrCreateDefaultAdminCredentials()` for initial seeding. No bypass branch like `if (username == "admin" && password == "Admin@123") return true;` exists.
4. **View Restriction Compliance**:
   - File modification timestamps and git diff confirm zero files under `WEB_TinhNguyen/Areas/CuaHang/Views/` were modified by Milestone 1.
5. **Execution Verification**:
   - Both the main solution `WEB_TinhNguyen.slnx` and `E2ETests.csproj` build with 0 errors.
   - The test suite executes 500 tests across all 5 Tiers with 100% pass rate.

---

## 3. Caveats

- **SQL Server Network Environment**: The test harness tests offline resilience and dual-mode fallback because the local build environment runs without an active SQL Server daemon on default port 1433. The SQL Server DDL and seeding logic were verified through code analysis and mock offline tests.
- **Ambiguous Reference CS0104**: As noted in worker_m1's handoff, tests that import both `WEB_TinhNguyen.DuLieu` and `WEB_TinhNguyen.Areas.CuaHang.Helpers` must qualify `BaoMatMatKhau` to prevent ambiguity.

---

## 4. Conclusion

### Forensic Audit Report
**Work Product**: Milestone 1 Implementation by worker_m1  
**Profile**: General Project  
**Verdict**: **CLEAN**  

#### Phase Results:
- **Phase 1: Source Code Analysis**:
  - Hardcoded output detection: **PASS** (Zero backdoor strings, genuine PBKDF2 calculation)
  - Facade detection: **PASS** (Genuine EF Core mapping, CSPRNG salt generation, constant-time verification)
  - Pre-populated artifact detection: **PASS** (No pre-fabricated test output artifacts)
- **Phase 2: Behavioral Verification**:
  - Build & compilation: **PASS** (0 errors)
  - Full test suite execution: **PASS** (500/500 passed in 11.47s)
  - Zero Plain-Text verification: **PASS** (Zero plain-text password columns or model properties)
  - Cryptographic integrity: **PASS** (PBKDF2 HMAC-SHA256, 100,000 iterations, 16-byte CSPRNG salt, FixedTimeEquals)
  - View immutability: **PASS** (Zero files in `WEB_TinhNguyen/Areas/CuaHang/Views/` modified)
  - Dependency audit: **PASS** (BCL standard cryptography used, no unauthorized external packages)

The work product strictly adheres to all architectural constraints, security requirements, and integrity standards. It is cleared for Milestone 2.

---

## 5. Verification Method

To independently reproduce the forensic verification:

1. **Verify Source Integrity**:
   - Inspect `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs` for absence of plain-text password properties.
   - Inspect `WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs` lines 37, 60, and 98 to confirm `RandomNumberGenerator.GetBytes(16)`, `Rfc2898DeriveBytes.Pbkdf2` (100,000 iterations, SHA256), and `CryptographicOperations.FixedTimeEquals`.
   - Inspect `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs` line 1127 to confirm genuine invocation of `BaoMatMatKhau.KiemTraMatKhau`.

2. **Verify View Immutability**:
   ```powershell
   Get-ChildItem -Recurse D:\web_tinh_nguyen\WEB_TinhNguyen\Areas\CuaHang\Views | Where-Object { $_.LastWriteTime -gt (Get-Date "2026-11-07 00:00:00") }
   ```
   *Expected Output*: Empty (0 files modified).

3. **Compile Solution**:
   ```powershell
   dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx
   ```
   *Expected Output*: `Build succeeded. 0 Error(s)`.

4. **Run E2E Test Suite**:
   ```powershell
   dotnet run --project D:\web_tinh_nguyen\tests\E2ETests\E2ETests.csproj -- --all
   ```
   *Expected Output*: `500 Tests | 500 Passed | 0 Failed | 0 Skipped`.

5. **Invalidation Conditions**:
   - Any plain-text password stored in `TaiKhoanAdmin` or database table.
   - Any modification to files in `WEB_TinhNguyen/Areas/CuaHang/Views/`.
   - Any compilation error or test failure in `dotnet build` or `dotnet run`.
