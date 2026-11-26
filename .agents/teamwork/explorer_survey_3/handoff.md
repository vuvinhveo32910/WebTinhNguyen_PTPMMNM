# Handoff Report: Database, Models & Auth Infra Survey (explorer_survey_3)

## 1. Observation

### 1.1 Database Connectivity & SQL Server Configuration
- **Configuration Files**:
  - `WEB_TinhNguyen/appsettings.json` (lines 1-12):
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=DESKTOP-C5LJ9BM\\SQL2025_DEV;Database=TinhNguyen;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*"
    }
    ```
  - `WEB_TinhNguyen/appsettings.Development.json` (lines 1-12): Contains the identical connection string.
- **SQL Server Instance & Database**:
  - Target Server/Instance: `DESKTOP-C5LJ9BM\SQL2025_DEV`
  - Target Database: `TinhNguyen`
  - Authentication: Windows Integrated Security (`Integrated Security=True`), `TrustServerCertificate=True`, `MultipleActiveResultSets=true`.
  - Machine Environment: SQL Server installation directory exists at `C:\Program Files\Microsoft SQL Server\MSSQL17.SQL2025_DEV` and `MSSQL16.SQLEXPRESS`. Windows sandbox restrictions prevent direct service manipulation, making the project's existing dual-mode persistence architecture essential.

### 1.2 Data Access Layer & DbContext Architecture
- **Framework & Package Versions** (`WEB_TinhNguyen/WEB_TinhNguyen.csproj`, lines 10-15):
  - Target Framework: `net10.0`
  - `Microsoft.EntityFrameworkCore`: `10.0.12`
  - `Microsoft.EntityFrameworkCore.SqlServer`: `10.0.12`
  - `Microsoft.EntityFrameworkCore.Tools`: `10.0.12`
- **DbContext Definition** (`WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`, lines 7-52):
  - Class: `public class ApplicationDbContext : DbContext`
  - Declares 25 `DbSet` properties covering campaign, home config, countdown, memories, news, partners, store, orders, and payment QR.
  - In `OnConfiguring` (lines 53-57):
    `optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));`
  - In `OnModelCreating` (lines 59-219): Maps entities to table names, configures decimal precision (`decimal(18,2)`), and ignores legacy columns.
- **EF Core Migrations**:
  - 13 migrations exist in `WEB_TinhNguyen/Migrations/` ranging from `20261006032230_AddBannerTable.cs` to `20261104000000_XoaCotMauVaVietHoaTenBangCot.cs`.
  - `ApplicationDbContextModelSnapshot.cs` (1442 lines) captures the snapshot up to the latest migration.
- **Reference SQL Scripts**:
  - `D:\web_tinh_nguyen\SQLQuery1.sql` (132 lines): Contains initial DDL scripts for tables `ChienDich`, `ChangChienDich`, `DangKyTNV`, `NhaTaiTro`, `TinTuc`, `ThuVienAnh`, `CauHoiThuongGap`, and `CauHinhTrangChu`.

### 1.3 User / Account / Admin Entity Inventory
- **Codebase Search**:
  - Executed recursive search across all `.cs` files in `D:\web_tinh_nguyen\WEB_TinhNguyen` for classes matching `User`, `Account`, `TaiKhoan`, `NguoiDung`, `AdminUser`, or `QuanTriVien`.
  - Result: **Zero existing user/account/identity entity classes exist in the project.**
  - All occurrences of the string `TaiKhoan` in the codebase are strictly related to bank accounts (`SoTaiKhoan`, `TenChuTaiKhoan`) in `CauHinhThanhToanQR`, `KenhDongGop`, and `VietQrHelper`.
  - No `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package is referenced.

### 1.4 Password Hashing Mechanism
- **Current State**:
  - Zero password hashing classes, interfaces, or utilities currently exist in `WEB_TinhNguyen`.
- **Framework Capability**:
  - The project targets .NET 10 via `<Project Sdk="Microsoft.NET.Sdk.Web">`, which references the `Microsoft.AspNetCore.App` shared framework.
  - `Microsoft.AspNetCore.Identity.PasswordHasher<TUser>` and `System.Security.Cryptography.Rfc2898DeriveBytes` (PBKDF2 with HMAC-SHA256) are natively available out of the box with zero additional package dependencies.

### 1.5 Database Initialization & Seeding Pattern
- **Startup Flow in `WEB_TinhNguyen/Program.cs`** (lines 88-109):
  ```csharp
  // Khởi tạo dữ liệu mẫu ban đầu (DbInitializer Seed Data) với cơ chế phòng vệ khi offline
  try
  {
      using var scope = app.Services.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      if (context.Database.CanConnect())
      {
          try
          {
              context.Database.Migrate();
          }
          catch (Exception migrateEx)
          {
              app.Logger.LogWarning(migrateEx, "Bỏ qua bước tự động Migrate do cơ sở dữ liệu vật lý đã tồn tại trước đó.");
          }
      }
      DbInitializer.Initialize(context);
  }
  catch (Exception ex)
  {
      app.Logger.LogWarning(ex, "Database initialization warning: SQL Server có thể đang offline, hệ thống sẽ sử dụng dữ liệu mẫu in-memory dự phòng.");
  }
  ```
- **Seeding & Idempotent DDL Mechanism in `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`**:
  - In `DbInitializer.Seed(ApplicationDbContext context)` (lines 253-383):
    1. Checks `if (context.Database.CanConnect())`.
    2. Runs idempotent raw SQL DDL using `context.Database.ExecuteSqlRaw(@"IF OBJECT_ID(N'[dbo].[TableName]', N'U') IS NULL BEGIN CREATE TABLE ... END")`.
    3. Checks `if (!context.Set<T>().Any())` and inserts seed records.
    4. Wraps in `try/catch` to safely fall back to in-memory store if database is unreachable.
  - In-Memory Fallback Store (lines 392-906): Provides thread-safe in-memory collections protected by `_syncLock` for offline execution, testing, and demo resilience.

### 1.6 Current Compilation & Test Suite Status
- `dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen\WEB_TinhNguyen.csproj`: Builds cleanly in 6.55s, 0 errors, 4 NU1902/NU1903 transitive package warnings.
- `dotnet run --project tests/E2ETests/E2ETests.csproj`: All 477 tests pass (Tier 1: 161, Tier 2: 28, Tier 3: 18, Tier 4: 12, Tier 5: 258) in 10.33s with 0 failures and 0 skips.

---

## 2. Logic Chain

### 2.1 Entity Design: `NguoiDung`
- **Premise**: User requirement R1 requires creating a user/admin table structure in SQL Server and storing a default admin account with username `admin` and secure hashed password `Admin@123`.
- **Inference**: A dedicated domain model `NguoiDung` must be created in `WEB_TinhNguyen.MoHinh` following the project's Vietnamese root naming convention.
- **Contract Definition**:
  - `IDNguoiDung`: `int`, Primary Key Identity
  - `TenDangNhap`: `string`, Required, Max 100, Unique index (default `admin`)
  - `MatKhauHash`: `string`, Required, Max 500 (stores PBKDF2 hashed password string, never plain-text)
  - `HoTen`: `string?`, Max 200 (e.g. `Quản trị viên Hệ thống`)
  - `Email`: `string?`, Max 200 (e.g. `admin@tinhnguyen.vn`)
  - `VaiTro`: `string`, Max 50, Default `Admin`
  - `TrangThai`: `bool`, Default `true` (Active)
  - `NgayTao`: `DateTime`, Default `DateTime.Now`
  - `LanDangNhapCuoi`: `DateTime?`

### 2.2 DbContext Integration & Idempotent Schema Creation
- **Premise**: In production, SQL Server may run existing migrations or direct scripts. Using only EF migrations could fail if physical tables exist or if migrations drift.
- **Inference**: Follow the established pattern used for `CauHinhThanhToanQR` and `SanPhamGayQuy`:
  1. Add `public DbSet<NguoiDung> NguoiDungs { get; set; }` to `ApplicationDbContext.cs`.
  2. In `OnModelCreating`, configure `entity.ToTable("NguoiDung")` and unique index on `TenDangNhap`.
  3. In `DbInitializer.Seed()`, execute idempotent DDL:
     ```sql
     IF OBJECT_ID(N'[dbo].[NguoiDung]', N'U') IS NULL
     BEGIN
         CREATE TABLE [dbo].[NguoiDung] (
             [IDNguoiDung] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
             [TenDangNhap] NVARCHAR(100) NOT NULL CONSTRAINT [UQ_NguoiDung_TenDangNhap] UNIQUE,
             [MatKhauHash] NVARCHAR(500) NOT NULL,
             [HoTen] NVARCHAR(200) NULL,
             [Email] NVARCHAR(200) NULL,
             [VaiTro] NVARCHAR(50) NOT NULL DEFAULT N'Admin',
             [TrangThai] BIT NOT NULL DEFAULT 1,
             [NgayTao] DATETIME2 NOT NULL DEFAULT GETDATE(),
             [LanDangNhapCuoi] DATETIME2 NULL
         );
     END;
     ```
  4. This guarantees that table `NguoiDung` is automatically created on startup in SQL Server even if `context.Database.Migrate()` encounters pre-existing objects.

### 2.3 Password Hashing Design: `PasswordHelper`
- **Premise**: Storing plain-text passwords violates acceptance criteria. The hashing algorithm must be secure, resilient against brute-force and rainbow table attacks, and maintainable without fragile external dependencies.
- **Inference**: Use `Microsoft.AspNetCore.Identity.PasswordHasher<string>` (or `PasswordHasher<NguoiDung>`).
  - Standard: PBKDF2 with HMAC-SHA256, 128-bit cryptographically secure random salt, 100,000 iterations.
  - Output: Format Version 3 Base64 string (`AQAAAAIAAA...`).
  - Verification: `VerifyHashedPassword` uses constant-time byte comparison (`CryptographicOperations.FixedTimeEquals`) to prevent timing side-channel attacks.
  - Helper Service / Utility: `PasswordHelper.HashPassword(string password)` and `PasswordHelper.VerifyPassword(string hashedPassword, string providedPassword)`.
  - Zero external NuGet package needed; 100% native to .NET 10.

### 2.4 Automatic Admin Seeding Flow
- **Premise**: On application startup, the default admin account (`admin` / `Admin@123`) must be automatically created in the database if it does not already exist.
- **Inference**:
  - In `DbInitializer.Seed(ApplicationDbContext context)`:
    ```csharp
    if (!context.NguoiDungs.Any(u => u.TenDangNhap == "admin"))
    {
        var admin = new NguoiDung
        {
            TenDangNhap = "admin",
            HoTen = "Quản trị viên Hệ thống",
            Email = "admin@tinhnguyen.vn",
            VaiTro = "Admin",
            TrangThai = true,
            NgayTao = DateTime.Now,
            MatKhauHash = PasswordHelper.HashPassword("Admin@123")
        };
        context.NguoiDungs.Add(admin);
        context.SaveChanges();
    }
    ```
  - In `DbInitializer` In-Memory Store:
    Add `_inMemoryUsers` collection with the pre-seeded `admin` record so in-memory and offline testing modes authenticate successfully without needing live SQL Server.
  - Provide helper method `DbInitializer.FindUser(ApplicationDbContext context, string username)` that queries SQL Server when connected and falls back to `_inMemoryUsers` when offline.

### 2.5 Authentication & Authorization Middleware Flow
- **Premise**: Requirement R2 & R3 specify that accessing any `/admin` route or add/edit/delete actions when unauthenticated must return HTTP 302 redirect to `/admin/login`.
- **Inference**:
  1. In `Program.cs`:
     Register Cookie Authentication:
     ```csharp
     builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
         .AddCookie(options =>
         {
             options.LoginPath = "/admin/login";
             options.LogoutPath = "/admin/logout";
             options.AccessDeniedPath = "/admin/login";
             options.ExpireTimeSpan = TimeSpan.FromHours(8);
             options.SlidingExpiration = true;
             options.Cookie.Name = ".WEB_TinhNguyen.AdminAuth";
             options.Cookie.HttpOnly = true;
             options.Cookie.SameSite = SameSiteMode.Lax;
         });
     ```
  2. Middleware Pipeline Order:
     ```csharp
     app.UseRouting();
     app.UseSession();
     app.UseAuthentication(); // Must precede UseAuthorization
     app.UseAuthorization();
     ```
  3. Controller Protection:
     - Apply `[Authorize(Roles = "Admin")]` to all Admin controllers:
       `AdminController`, `AdminTrangChuController`, `AdminChuongTrinhController`, `AdminGayQuyController`, `AdminCauHinhQRController`, `AdminKyUcController`, `AdminBanToChucController`, `AdminTinTucController`, `AdminHieuUngController`, `AdminSanPhamController`, `AdminDonHangController`.
     - In `AdminController`:
       - `[HttpGet("/admin/login")]` with `[AllowAnonymous]` -> renders `/GiaoDien/Admin/Login.cshtml`.
       - `[HttpPost("/admin/login")]` with `[AllowAnonymous]` -> validates credentials, issues `ClaimsPrincipal` via `HttpContext.SignInAsync()`, redirects to `/admin` (or `returnUrl`).
       - `[HttpGet("/admin/logout")]` / `[HttpPost("/admin/logout")]` -> calls `HttpContext.SignOutAsync()`, clears session, redirects to `/admin/login`.
       - `[HttpGet("/admin")]` -> renders Admin dashboard (`Index.cshtml`).
  4. Behavior Verification:
     - When unauthenticated, any GET/POST to `/admin`, `/AdminTrangChu`, `/AdminChuongTrinh`, etc., is challenged by the Cookie Authentication handler and issued a `302 Found` with `Location: /admin/login?ReturnUrl=...`.
     - When authenticated as admin, all routes return `200 OK`.

---

## 3. Caveats

1. **Sandboxed Windows Environment**:
   - In this execution environment, loopback network bindings and service controller permissions are sandboxed.
   - Therefore, the application MUST strictly maintain its dual-mode persistence architecture: SQL Server DDL/seeding when online, and `DbInitializer` in-memory user store when offline or during automated in-memory E2E tests.
2. **View Engine Structure**:
   - The admin views are located in `WEB_TinhNguyen/GiaoDien/` rather than standard `Views/`. The Razor ViewEngine is configured via `VietnameseViewLocationExpander` to resolve `/GiaoDien/{1}/{0}.cshtml`.
   - The login view should be created at `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml` and specify `Layout = null;` to render as a dedicated login page without sidebar clutter.
3. **Admin Layout Navigation**:
   - `_AdminLayout.cshtml` line 343 contains an inert logout anchor: `<a href="#" title="Đăng xuất" ...>`. This must be updated to target the logout action (`href="@Url.Action("Logout", "Admin", new { area = "" })"`).
4. **Existing Test Suite Compatibility**:
   - The 477 existing tests in `tests/E2ETests` instantiate controllers directly in C# using mock contexts (`CreateOfflineDbContext`). Direct C# method invocation bypasses ASP.NET Core filter pipeline and continues to return `ViewResult`.
   - Adding `[Authorize]` attributes will not break existing unit/tier tests. New tests should be added to specifically verify authentication, password hashing, and redirection.

---

## 4. Conclusion

1. **Database Schema**:
   - Table name: `NguoiDung`
   - Primary key: `IDNguoiDung INT IDENTITY(1,1)`
   - Columns: `TenDangNhap NVARCHAR(100) NOT NULL UNIQUE`, `MatKhauHash NVARCHAR(500) NOT NULL`, `HoTen NVARCHAR(200) NULL`, `Email NVARCHAR(200) NULL`, `VaiTro NVARCHAR(50) NOT NULL DEFAULT 'Admin'`, `TrangThai BIT NOT NULL DEFAULT 1`, `NgayTao DATETIME2 NOT NULL DEFAULT GETDATE()`, `LanDangNhapCuoi DATETIME2 NULL`.
2. **Default Admin Seed Data**:
   - Username: `admin`
   - Password: `Admin@123`
   - Stored Hash: PBKDF2 with HMAC-SHA256, salted (never plain-text)
   - Role: `Admin`
   - Seeded automatically on startup via `DbInitializer.Seed()` with idempotent `IF OBJECT_ID(...) IS NULL` DDL and in-memory fallback.
3. **Security & Hashing Infrastructure**:
   - Built-in `Microsoft.AspNetCore.Identity.PasswordHasher<string>` provides enterprise-grade PBKDF2 hashing with zero external dependencies.
   - ASP.NET Core Cookie Authentication handles session tickets, login paths, and automatic HTTP 302 redirection to `/admin/login`.
4. **Route Protection**:
   - Applying `[Authorize(Roles = "Admin")]` to all 11 Admin controllers guarantees complete protection of `/admin` and all C-U-D operations.
   - Anonymous access is explicitly granted only to `/admin/login`.

---

## 5. Verification Method

### 5.1 Build Verification
Execute the project build command:
```powershell
dotnet build D:\web_tinh_nguyen\WEB_TinhNguyen\WEB_TinhNguyen.csproj
```
Expected output: 0 errors, build succeeds cleanly.

### 5.2 Regression Test Execution
Execute the existing test suite:
```powershell
dotnet run --project tests/E2ETests/E2ETests.csproj
```
Expected output: All 477 existing tests pass with 0 failures.

### 5.3 Automated Auth Verification Test Suite Design
Implement `tests/E2ETests/Tier1/Tier1_AdminAuthTests.cs` to verify:
1. `NguoiDung` entity contract: Properties, attributes, and uniqueness constraints.
2. `PasswordHelper` verification:
   - `HashPassword("Admin@123")` does not contain `"Admin@123"` (not plain text).
   - `VerifyPassword(hash, "Admin@123")` returns `true`.
   - `VerifyPassword(hash, "wrong_password")` returns `false`.
3. `DbInitializer` seeding verification:
   - Default user `admin` exists in both SQL Server seed routine and in-memory store.
   - Stored password hash validates against `Admin@123`.
4. Authorization attribute verification:
   - `AdminController`, `AdminTrangChuController`, `AdminChuongTrinhController`, `AdminGayQuyController`, `AdminCauHinhQRController`, `AdminKyUcController`, `AdminBanToChucController`, `AdminTinTucController`, `AdminHieuUngController`, `AdminSanPhamController`, `AdminDonHangController` all have `[Authorize]` attributes.
   - `AdminController.Login` has `[AllowAnonymous]`.
5. Cookie Authentication configuration check in `Program.cs`:
   - `AddAuthentication` and `AddCookie` registered with `LoginPath = "/admin/login"`.
   - `app.UseAuthentication()` precedes `app.UseAuthorization()`.
6. Login & Logout action lifecycle:
   - Invalid credentials return login view with error message.
   - Valid credentials create auth cookie and redirect to `/admin`.
   - Logout clears auth ticket and redirects to `/admin/login`.

### 5.4 Invalidation Conditions
- Storing `Admin@123` in plain-text inside `NguoiDung.MatKhauHash` invalidates Acceptance Criteria.
- Any admin route returning 200 OK without valid admin authentication cookie/session invalidates R3.
- Failure of any existing 477 tests invalidates non-regression requirement.
