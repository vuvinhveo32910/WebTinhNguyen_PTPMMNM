# Handoff Report: DDL, Default Admin Seeding & Dual-Mode Store Architecture (`DbInitializer.cs`)

**Agent ID**: `explorer_m1_3`  
**Role**: DDL, Seeding & Dual-Mode Store Explorer  
**Milestone**: M1 (Database Setup & Admin Seeding)  
**Parent Conversation ID**: `8cd06411-5097-4e95-b532-893861716fb8`  
**Date**: 2026-11-07  

---

## 1. Observation

### 1.1 Existing Architecture & File State
- **File Under Investigation**: `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs` (906 lines).
- **Namespace & Usings** (`DbInitializer.cs` lines 1-10):
  ```csharp
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Microsoft.EntityFrameworkCore;
  using WEB_TinhNguyen.DuLieu;
  using WEB_TinhNguyen.Areas.CuaHang.Models;
  using WEB_TinhNguyen.MoHinh;
  ```
- **Existing Static State & Concurrency Lock** (`DbInitializer.cs` lines 17-25):
  ```csharp
  private static readonly object _syncLock = new object();
  private static readonly List<SanPhamGayQuy> _inMemoryProducts = new List<SanPhamGayQuy>();
  private static readonly List<DonHangGayQuy> _inMemoryOrders = new List<DonHangGayQuy>();
  private static readonly List<CauHinhThanhToanQR> _inMemoryQrConfigs = new List<CauHinhThanhToanQR>();
  private static int _nextProductId = 5;
  private static int _nextOrderId = 1;
  private static int _nextQrConfigId = 2;
  private static bool _sampleOrdersSeeded = false;
  ```
- **Existing Initialization Sequence** (`DbInitializer.cs` lines 26-30):
  ```csharp
  static DbInitializer()
  {
      ResetInMemoryStore();
      SeedSampleOrders();
  }
  ```
- **Existing Seeding Pipeline & SQL DDL Execution** (`DbInitializer.cs` lines 246-389):
  - `DbInitializer.Initialize(ApplicationDbContext context)` directly invokes `Seed(context)`.
  - `Seed(context)` executes raw SQL via `context.Database.ExecuteSqlRaw(@"...")` creating tables `[dbo].[SanPhamGayQuy]`, `[dbo].[DonHangGayQuy]`, `[dbo].[ChiTietDonHangGayQuy]`, and `[dbo].[CauHinhThanhToanQR]` using idempotent `IF OBJECT_ID(N'...', N'U') IS NULL` checks.
  - Seeds default store records if tables are empty (`context.SanPhamGayQuys.Any()`, `context.DonHangGayQuys.Any()`, `context.CauHinhThanhToanQRs.Any()`).
  - Entire DB block is wrapped in a top-level `try/catch` to gracefully fall back to in-memory stores if SQL Server is offline or connection times out.
- **Application Startup Call Site** (`WEB_TinhNguyen/Program.cs` lines 88-109):
  - In `Program.cs`, during application startup:
    ```csharp
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (context.Database.CanConnect())
    {
        try { context.Database.Migrate(); }
        catch (Exception migrateEx) { app.Logger.LogWarning(migrateEx, "Bỏ qua bước tự động Migrate..."); }
    }
    DbInitializer.Initialize(context);
    ```
- **Sibling Explorer Specifications**:
  - `explorer_m1_1` has designed `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs` and `DbSet<TaiKhoanAdmin> TaiKhoanAdmins` in `ApplicationDbContext.cs`.
  - `explorer_m1_2` has designed `WEB_TinhNguyen/DuLieu/BaoMatMatKhau.cs` (and forwarding class in `WEB_TinhNguyen.Areas.CuaHang.Helpers`) implementing PBKDF2 HMAC-SHA256:
    - `TaoMuoiNgauNhien()` -> 16-byte random salt Base64 (24 chars).
    - `BamMatKhau(string matKhau, string muoiBaoMat)` -> PBKDF2 with 100,000 iterations Base64 (44 chars).
    - `KiemTraMatKhau(string? matKhauNhap, string? matKhauHashDaLuu, string? muoiBaoMat)` -> constant-time verification using `CryptographicOperations.FixedTimeEquals`.
    - `HashPassword(string password, out string salt)` & `VerifyPassword(string password, string hash, string salt)`.
- **Existing Automated Test Baseline**:
  - Command: `dotnet run --project tests/E2ETests/E2ETests.csproj -- --all`
  - Result: 477/477 tests passed in 6.11 seconds with 0 failures, 0 skipped.
  - Stress harness in `Tier5_ChallengerTests.cs:869` (`C47_DbInitializer_ConcurrentMultiThreadedAccess_ThreadSafe`) validates thread-safety under heavy parallel access.

---

## 2. Logic Chain

### 2.1 Idempotent SQL Server DDL Strategy
1. **Observation**: `ORIGINAL_REQUEST.md` (§R1) requires automatic setup of SQL Server tables with idempotent table creation.
2. **Analysis**:
   - The required table DDL is:
     ```sql
     IF OBJECT_ID(N'[dbo].[TaiKhoanAdmin]', N'U') IS NULL
     BEGIN
         CREATE TABLE [dbo].[TaiKhoanAdmin] (
             [IDTaiKhoan] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
             [TenDangNhap] NVARCHAR(100) NOT NULL UNIQUE,
             [MatKhauHash] NVARCHAR(500) NOT NULL,
             [MuoiBaoMat] NVARCHAR(100) NOT NULL,
             [HoTen] NVARCHAR(150) NULL,
             [Email] NVARCHAR(150) NULL,
             [VaiTro] NVARCHAR(50) NOT NULL DEFAULT N'Admin',
             [TrangThaiHoatDong] BIT NOT NULL DEFAULT 1,
             [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
             [LanDangNhapCuoi] DATETIME2 NULL
         );
     END
     ```
   - Placing this script inside `DbInitializer.Seed(context)` alongside the existing 4 tables guarantees table creation on application boot.
   - Additionally, embedding this script inside a dedicated `SeedAdmin(ApplicationDbContext? context = null)` method ensures that any seeder, test setup, or administrative tool can invoke `SeedAdmin` independently without depending on full catalog seeding.

### 2.2 Default Admin Seeding Strategy
1. **Observation**: Prompt and PROJECT.md require default admin account:
   - Username: `admin`
   - Password: `Admin@123` (PBKDF2 HMAC-SHA256 hashed, unique salt)
   - Role: `Admin`
   - HoTen: `Quản trị viên Hệ thống`
   - Email: `admin@tinhnguyen.vn`
   - TrangThaiHoatDong: `true`
2. **Analysis**:
   - In `SeedAdmin(ApplicationDbContext context)`:
     - Check if admin already exists: `bool exists = context.TaiKhoanAdmins.Any(a => a.TenDangNhap == "admin");`.
     - If not exists:
       - Generate cryptographic random salt: `string salt = BaoMatMatKhau.TaoMuoiNgauNhien();`
       - Derive hash: `string hash = BaoMatMatKhau.BamMatKhau("Admin@123", salt);`
       - Create new `TaiKhoanAdmin` with `IDTaiKhoan = 0` (so SQL Server `IDENTITY(1,1)` generates the key).
       - Insert into `context.TaiKhoanAdmins.Add(...)` and `context.SaveChanges()`.
   - Wrap in `try/catch` to ensure database timeouts or permission restrictions never crash the app.

### 2.3 Dual-Mode In-Memory Store Architecture
1. **Observation**: Sandbox environments, offline tests, and unit tests execute without live SQL Server connectivity.
2. **Analysis**:
   - Maintain `private static readonly List<TaiKhoanAdmin> _inMemoryAdmins = new List<TaiKhoanAdmin>();`.
   - Guard all operations using existing `lock (_syncLock)`.
   - Provide `GetDefaultAdmins()` returning a list with default `admin` (`Admin@123` hashed via `BaoMatMatKhau`).
   - Cache default admin salt and hash in `_defaultAdminSalt` / `_defaultAdminHash` with `_adminHashLock` so repeated calls in high-iteration test suites do not redundantly re-run 100,000 PBKDF2 iterations, preserving sub-millisecond execution times.
   - Update `DbInitializer.ResetInMemoryStore()` to clear and reset `_inMemoryAdmins` to `GetDefaultAdmins()`.
   - Ensure all returned entities are cloned via `CloneAdmin(TaiKhoanAdmin a)` or `admin.Clone()`, adhering to the strict immutability and mutation-isolation requirements verified in test `C43`.

### 2.4 Helper Methods & Query Resolution Order
1. **Observation**: PROJECT.md and dispatch mandate:
   - `SeedAdmin(ApplicationDbContext? context = null)`
   - `GetAdminByUsername(string username, ApplicationDbContext? context = null)`
   - `ValidateAdminCredentials(string username, string password, ApplicationDbContext? context = null)`
2. **Resolution Logic**:
   - **`GetAdminByUsername(username, context)`**:
     1. Return `null` if `string.IsNullOrWhiteSpace(username)`.
     2. If `context != null`:
        - In `try/catch`, test `context.Database.CanConnect()`.
        - Query `context.TaiKhoanAdmins.AsNoTracking().FirstOrDefault(a => a.TenDangNhap.ToLower() == username.Trim().ToLower())`.
        - If found in DB, return cloned entity.
     3. Fallback to in-memory:
        - Enter `lock (_syncLock)`.
        - If `_inMemoryAdmins` is empty, initialize via `GetDefaultAdmins()`.
        - Search case-insensitively: `string.Equals(a.TenDangNhap, username.Trim(), StringComparison.OrdinalIgnoreCase)`.
        - Return cloned entity or `null`.
   - **`ValidateAdminCredentials(username, password, context)`**:
     1. Return `false` if `string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)`.
     2. Retrieve admin using `GetAdminByUsername(username, context)`.
     3. If `admin == null` or `!admin.TrangThaiHoatDong`, return `false`.
     4. Verify password via `BaoMatMatKhau.KiemTraMatKhau(password, admin.MatKhauHash, admin.MuoiBaoMat)`.
     5. If matched:
        - Update `LanDangNhapCuoi = DateTime.Now` in in-memory store.
        - If `context != null`, attempt updating `LanDangNhapCuoi` in SQL Server inside `try/catch`.
        - Return `true`.
     6. If not matched, return `false`.

---

## 3. Caveats

1. **Context Lifecycle & Parameterless Controllers**:
   - `AdminController` has a parameterless constructor for unit test compatibility (`F19_T08`).
   - Helper methods must support `context = null` cleanly without throwing `ArgumentNullException`.
2. **Case Insensitivity in Dual Mode**:
   - In SQL Server, `NVARCHAR` columns are case-insensitive by default.
   - In-Memory LINQ must use `StringComparison.OrdinalIgnoreCase` to maintain identical behavior across both modes.
3. **White-space Trimming**:
   - Usernames entered with leading or trailing whitespaces must be `.Trim()`med prior to lookup.
4. **Zero Plain-Text Leakage**:
   - Default password `"Admin@123"` is only passed to `BaoMatMatKhau.BamMatKhau` during seeding; plain text is never stored in any property, field, or log.

---

## 4. Conclusion & Concrete Code Specification

Below is the exact code to be added/updated in `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`.

### 4.1 Additional Static Fields (Add to top of `DbInitializer` class)
```csharp
private static readonly List<TaiKhoanAdmin> _inMemoryAdmins = new List<TaiKhoanAdmin>();
private static int _nextAdminId = 2;

// Cache chuỗi muối và hash của admin mặc định để tối ưu hóa hiệu năng trong kiểm thử song song
private static string? _defaultAdminSalt;
private static string? _defaultAdminHash;
private static readonly object _adminHashLock = new object();
```

### 4.2 Default Admin Credentials & Generator Methods
```csharp
/// <summary>
/// Lấy hoặc tạo chuỗi muối và mã băm PBKDF2 của tài khoản admin mặc định ("Admin@123").
/// Sử dụng bộ nhớ đệm an toàn theo luồng để tối ưu hóa hiệu năng trong các kiểm thử đa luồng.
/// </summary>
private static (string salt, string hash) GetOrCreateDefaultAdminCredentials()
{
    lock (_adminHashLock)
    {
        if (string.IsNullOrEmpty(_defaultAdminSalt) || string.IsNullOrEmpty(_defaultAdminHash))
        {
            _defaultAdminSalt = BaoMatMatKhau.TaoMuoiNgauNhien();
            _defaultAdminHash = BaoMatMatKhau.BamMatKhau("Admin@123", _defaultAdminSalt);
        }
        return (_defaultAdminSalt, _defaultAdminHash);
    }
}

/// <summary>
/// Danh sách tài khoản quản trị viên mặc định ("admin" / "Admin@123") với mật khẩu được băm bảo mật bằng PBKDF2.
/// </summary>
public static List<TaiKhoanAdmin> GetDefaultAdmins()
{
    var (salt, hash) = GetOrCreateDefaultAdminCredentials();

    return new List<TaiKhoanAdmin>
    {
        new TaiKhoanAdmin
        {
            IDTaiKhoan = 1,
            TenDangNhap = "admin",
            MatKhauHash = hash,
            MuoiBaoMat = salt,
            HoTen = "Quản trị viên Hệ thống",
            Email = "admin@tinhnguyen.vn",
            VaiTro = "Admin",
            TrangThaiHoatDong = true,
            NgayTao = DateTime.Now,
            LanDangNhapCuoi = null
        }
    };
}

/// <summary>
/// Tạo bản sao dữ liệu TaiKhoanAdmin để đảm bảo an toàn truy cập (Clone Isolation).
/// </summary>
private static TaiKhoanAdmin CloneAdmin(TaiKhoanAdmin a)
{
    return new TaiKhoanAdmin
    {
        IDTaiKhoan = a.IDTaiKhoan,
        TenDangNhap = a.TenDangNhap,
        MatKhauHash = a.MatKhauHash,
        MuoiBaoMat = a.MuoiBaoMat,
        HoTen = a.HoTen,
        Email = a.Email,
        VaiTro = a.VaiTro,
        TrangThaiHoatDong = a.TrangThaiHoatDong,
        NgayTao = a.NgayTao,
        LanDangNhapCuoi = a.LanDangNhapCuoi
    };
}
```

### 4.3 Update `ResetInMemoryStore()`
```csharp
public static void ResetInMemoryStore()
{
    lock (_syncLock)
    {
        _inMemoryProducts.Clear();
        _inMemoryProducts.AddRange(GetDefaultProducts());
        _inMemoryOrders.Clear();
        _nextProductId = 5;
        _nextOrderId = 1;
        SeedSampleOrders();
        _sampleOrdersSeeded = true;

        _inMemoryQrConfigs.Clear();
        _inMemoryQrConfigs.Add(GetDefaultQrConfig());
        _nextQrConfigId = 2;

        _inMemoryAdmins.Clear();
        _inMemoryAdmins.AddRange(GetDefaultAdmins());
        _nextAdminId = 2;
    }
}
```

### 4.4 Update `Seed(ApplicationDbContext context)`
In `Seed(context)`, inside `if (context.Database.CanConnect())`:
1. Add the SQL DDL block to `context.Database.ExecuteSqlRaw(@"...")`:
```sql
IF OBJECT_ID(N'[dbo].[TaiKhoanAdmin]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TaiKhoanAdmin] (
        [IDTaiKhoan] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TenDangNhap] NVARCHAR(100) NOT NULL UNIQUE,
        [MatKhauHash] NVARCHAR(500) NOT NULL,
        [MuoiBaoMat] NVARCHAR(100) NOT NULL,
        [HoTen] NVARCHAR(150) NULL,
        [Email] NVARCHAR(150) NULL,
        [VaiTro] NVARCHAR(50) NOT NULL DEFAULT N'Admin',
        [TrangThaiHoatDong] BIT NOT NULL DEFAULT 1,
        [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [LanDangNhapCuoi] DATETIME2 NULL
    );
END;
```
2. Call `SeedAdmin(context);` at the end of `Seed(context)`:
```csharp
SeedAdmin(context);
```

### 4.5 New Methods: `SeedAdmin`, `GetAdminByUsername`, `ValidateAdminCredentials`
```csharp
/// <summary>
/// Khởi tạo tài khoản quản trị viên mặc định vào SQL Server nếu chưa tồn tại.
/// Nếu database offline hoặc không truyền context, đồng bộ đảm bảo bộ nhớ in-memory có sẵn admin.
/// </summary>
public static void SeedAdmin(ApplicationDbContext? context = null)
{
    if (context != null)
    {
        try
        {
            if (context.Database.CanConnect())
            {
                // Đảm bảo bảng TaiKhoanAdmin tồn tại (Idempotent DDL)
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID(N'[dbo].[TaiKhoanAdmin]', N'U') IS NULL
                    BEGIN
                        CREATE TABLE [dbo].[TaiKhoanAdmin] (
                            [IDTaiKhoan] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [TenDangNhap] NVARCHAR(100) NOT NULL UNIQUE,
                            [MatKhauHash] NVARCHAR(500) NOT NULL,
                            [MuoiBaoMat] NVARCHAR(100) NOT NULL,
                            [HoTen] NVARCHAR(150) NULL,
                            [Email] NVARCHAR(150) NULL,
                            [VaiTro] NVARCHAR(50) NOT NULL DEFAULT N'Admin',
                            [TrangThaiHoatDong] BIT NOT NULL DEFAULT 1,
                            [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
                            [LanDangNhapCuoi] DATETIME2 NULL
                        );
                    END;
                ");

                // Kiểm tra tài khoản admin mặc định
                bool adminExists = context.TaiKhoanAdmins.Any(a => a.TenDangNhap == "admin");
                if (!adminExists)
                {
                    var defaultAdmins = GetDefaultAdmins();
                    foreach (var adm in defaultAdmins)
                    {
                        var dbAdmin = CloneAdmin(adm);
                        dbAdmin.IDTaiKhoan = 0; // Để SQL Server tự sinh ID Identity
                        context.TaiKhoanAdmins.Add(dbAdmin);
                    }
                    context.SaveChanges();
                }
            }
        }
        catch
        {
            // SQL Server offline: bắt lỗi để không làm gián đoạn ứng dụng
        }
    }

    // Đảm bảo bộ nhớ In-Memory luôn có tài khoản admin
    lock (_syncLock)
    {
        if (_inMemoryAdmins.Count == 0)
        {
            _inMemoryAdmins.AddRange(GetDefaultAdmins());
        }
    }
}

/// <summary>
/// Lấy thông tin tài khoản admin theo tên đăng nhập (không phân biệt chữ hoa thường).
/// Hỗ trợ dual-mode: truy vấn SQL Server trước, nếu offline hoặc không có context thì truy vấn từ In-Memory store.
/// Trả về bản sao đối tượng (clone) để tránh bị đột biến dữ liệu ngoài ý muốn.
/// </summary>
public static TaiKhoanAdmin? GetAdminByUsername(string username, ApplicationDbContext? context = null)
{
    if (string.IsNullOrWhiteSpace(username))
    {
        return null;
    }

    string normalized = username.Trim();

    if (context != null)
    {
        try
        {
            if (context.Database.CanConnect())
            {
                var dbAdmin = context.TaiKhoanAdmins
                    .AsNoTracking()
                    .FirstOrDefault(a => a.TenDangNhap.ToLower() == normalized.ToLower());

                if (dbAdmin != null)
                {
                    return CloneAdmin(dbAdmin);
                }
            }
        }
        catch
        {
            // Database gặp sự cố: chuyển xuống In-Memory fallback
        }
    }

    lock (_syncLock)
    {
        if (_inMemoryAdmins.Count == 0)
        {
            _inMemoryAdmins.AddRange(GetDefaultAdmins());
        }

        var inMemAdmin = _inMemoryAdmins.FirstOrDefault(a =>
            string.Equals(a.TenDangNhap, normalized, StringComparison.OrdinalIgnoreCase));

        return inMemAdmin != null ? CloneAdmin(inMemAdmin) : null;
    }
}

/// <summary>
/// Xác thực thông tin đăng nhập admin (tên đăng nhập và mật khẩu).
/// Kiểm tra mật khẩu bằng thuật toán PBKDF2 HMAC-SHA256 chống timing attack.
/// Nếu thông tin hợp lệ, tự động cập nhật thời điểm đăng nhập cuối (LanDangNhapCuoi).
/// </summary>
public static bool ValidateAdminCredentials(string username, string password, ApplicationDbContext? context = null)
{
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
    {
        return false;
    }

    var admin = GetAdminByUsername(username, context);
    if (admin == null || !admin.TrangThaiHoatDong)
    {
        return false;
    }

    bool isValid = BaoMatMatKhau.KiemTraMatKhau(password, admin.MatKhauHash, admin.MuoiBaoMat);
    if (!isValid)
    {
        return false;
    }

    // Cập nhật LanDangNhapCuoi khi đăng nhập thành công
    var now = DateTime.Now;

    // Cập nhật trong In-Memory Store
    lock (_syncLock)
    {
        var inMem = _inMemoryAdmins.FirstOrDefault(a =>
            string.Equals(a.TenDangNhap, username.Trim(), StringComparison.OrdinalIgnoreCase));
        if (inMem != null)
        {
            inMem.LanDangNhapCuoi = now;
        }
    }

    // Cập nhật trong CSDL nếu có context
    if (context != null)
    {
        try
        {
            if (context.Database.CanConnect())
            {
                var dbAdmin = context.TaiKhoanAdmins.FirstOrDefault(a =>
                    a.TenDangNhap.ToLower() == username.Trim().ToLower());
                if (dbAdmin != null)
                {
                    dbAdmin.LanDangNhapCuoi = now;
                    context.SaveChanges();
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi ghi nhật ký đăng nhập nếu CSDL tạm thời mất kết nối
        }
    }

    return true;
}
```

### 4.6 In-Memory Admin CRUD & Management Methods
```csharp
#region In-Memory Admin Store Methods (Phục vụ Demo & Offline Fallback)

public static List<TaiKhoanAdmin> GetInMemoryAdmins()
{
    lock (_syncLock)
    {
        if (_inMemoryAdmins.Count == 0)
        {
            _inMemoryAdmins.AddRange(GetDefaultAdmins());
        }
        return _inMemoryAdmins.Select(a => CloneAdmin(a)).ToList();
    }
}

public static TaiKhoanAdmin? GetInMemoryAdminById(int id)
{
    lock (_syncLock)
    {
        if (_inMemoryAdmins.Count == 0)
        {
            _inMemoryAdmins.AddRange(GetDefaultAdmins());
        }
        var a = _inMemoryAdmins.FirstOrDefault(x => x.IDTaiKhoan == id);
        return a == null ? null : CloneAdmin(a);
    }
}

public static TaiKhoanAdmin AddInMemoryAdmin(TaiKhoanAdmin item)
{
    lock (_syncLock)
    {
        while (_inMemoryAdmins.Any(a => a.IDTaiKhoan == _nextAdminId))
        {
            _nextAdminId++;
        }
        item.IDTaiKhoan = _nextAdminId++;
        item.NgayTao = DateTime.Now;
        var clone = CloneAdmin(item);
        _inMemoryAdmins.Add(clone);
        return CloneAdmin(clone);
    }
}

public static bool UpdateInMemoryAdmin(TaiKhoanAdmin item)
{
    lock (_syncLock)
    {
        var existing = _inMemoryAdmins.FirstOrDefault(x => x.IDTaiKhoan == item.IDTaiKhoan);
        if (existing == null) return false;

        existing.TenDangNhap = item.TenDangNhap;
        if (!string.IsNullOrEmpty(item.MatKhauHash))
        {
            existing.MatKhauHash = item.MatKhauHash;
        }
        if (!string.IsNullOrEmpty(item.MuoiBaoMat))
        {
            existing.MuoiBaoMat = item.MuoiBaoMat;
        }
        existing.HoTen = item.HoTen;
        existing.Email = item.Email;
        existing.VaiTro = item.VaiTro;
        existing.TrangThaiHoatDong = item.TrangThaiHoatDong;
        existing.LanDangNhapCuoi = item.LanDangNhapCuoi;
        return true;
    }
}

public static bool DeleteInMemoryAdmin(int id)
{
    lock (_syncLock)
    {
        var existing = _inMemoryAdmins.FirstOrDefault(x => x.IDTaiKhoan == id);
        if (existing == null) return false;
        _inMemoryAdmins.Remove(existing);
        return true;
    }
}

#endregion
```

---

## 5. Verification Method

### 5.1 Build Verification
- Command:
  ```powershell
  dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen.slnx
  ```
- **Pass Condition**: `0 Error(s)`.

### 5.2 Automated Test Suite Regression Check
- Command:
  ```powershell
  dotnet run --project tests/E2ETests/E2ETests.csproj -- --all
  ```
- **Pass Condition**: All 477 existing tests pass with 0 regressions.

### 5.3 Method Contract & Dual-Mode Unit Tests
Once implemented, verification test cases should assert:
1. **Reflection & Method Signature**:
   - `DbInitializer.SeedAdmin(ApplicationDbContext)` exists.
   - `DbInitializer.GetAdminByUsername(string, ApplicationDbContext)` exists.
   - `DbInitializer.ValidateAdminCredentials(string, string, ApplicationDbContext)` exists.
2. **In-Memory Store Default State**:
   - `DbInitializer.ResetInMemoryStore()` populates `admin` with `Role = "Admin"`.
   - `DbInitializer.ValidateAdminCredentials("admin", "Admin@123")` returns `true`.
   - `DbInitializer.ValidateAdminCredentials("admin", "WrongPassword")` returns `false`.
   - `DbInitializer.ValidateAdminCredentials("nonexistent", "Admin@123")` returns `false`.
3. **Immutability & Clone Isolation**:
   - Mutating a property on the object returned from `GetAdminByUsername` or `GetInMemoryAdmins` does not alter the internal store.
4. **Invalidation Conditions**:
   - `ValidateAdminCredentials("admin", "Admin@123")` returning false in offline mode.
   - Plain-text password being stored in `TaiKhoanAdmin.MatKhauHash`.
   - Any break in existing Tier 1-5 test suites.
