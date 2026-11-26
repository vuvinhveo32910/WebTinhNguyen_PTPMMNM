# BRIEFING — 2026-11-07T02:23:30Z

## Mission
Comprehensive survey of the existing codebase at D:\web_tinh_nguyen (tech stack, project structure, dependencies, app configuration, auth setup, build status) to inform Admin Auth and feature implementation.

## 🔒 My Identity
- Archetype: explorer
- Roles: Architecture & Tech Stack Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\explorer_survey_1
- Original parent: 8cd06411-5097-4e95-b532-893861716fb8
- Milestone: Survey & Architecture Discovery

## 🔒 Key Constraints
- Read-only investigation — do NOT implement or modify source code
- Files for content delivery, messages for coordination
- Handoff report in handoff.md following 5-component format

## Current Parent
- Conversation ID: 8cd06411-5097-4e95-b532-893861716fb8
- Updated: 2026-11-07T02:23:30Z

## Investigation State
- **Explored paths**:
  - `ORIGINAL_REQUEST.md`, `PROJECT.md`, `TEST_INFRA.md`, `TEST_READY.md`, `SQLQuery1.sql`
  - `WEB_TinhNguyen.slnx`, `WEB_TinhNguyen/WEB_TinhNguyen.csproj`, `package.json`
  - `WEB_TinhNguyen/Program.cs`, `appsettings.json`, `appsettings.Development.json`
  - `WEB_TinhNguyen/DuLieu/ApplicationDbContext.cs`, `WEB_TinhNguyen/MoHinh/Entities.cs`
  - `WEB_TinhNguyen/Areas/CuaHang/Helpers/DbInitializer.cs`
  - Controllers in `WEB_TinhNguyen/BoDieuKhien/` and `WEB_TinhNguyen/Areas/CuaHang/Controllers/`
  - Views in `WEB_TinhNguyen/GiaoDien/` and `WEB_TinhNguyen/Areas/CuaHang/Views/`
  - `tests/E2ETests/`, `tests/M1Challenger/`, `tests/M3Challenger2/`
- **Key findings**:
  - Tech Stack: ASP.NET Core .NET 10.0 (`net10.0`), EF Core 10.0.12, Razor View Engine, Tailwind CSS 3.4.3.
  - Auth Status: Completely missing. No Cookie Auth, No Identity, No JWT, No `app.UseAuthentication()`, zero `[Authorize]` attributes.
  - User/Admin Model: None exists in DB or code.
  - Pipeline Ordering Contract: `UseRouting()` -> `UseSession()` -> `UseAuthorization()`. `CH19` verifies this strictly. `UseAuthentication()` must be inserted between `UseSession()` and `UseAuthorization()`.
  - Dual-Mode Persistence: SQL Server raw idempotent DDL + In-Memory fallback store in `DbInitializer.cs`.
  - Critical Test Constraints: `E2ETests` (477 passing tests); `M3Challenger2` asserts SHA-256 hashes of 12 views under `Areas/CuaHang/Views/` (must not touch these views!).
  - Build Status: `dotnet build` succeeds (0 errors, 47 warnings).
- **Unexplored areas**: None. Codebase survey complete.

## Key Decisions Made
- Recommend Cookie Authentication (`CookieAuthenticationDefaults.AuthenticationScheme`) with custom `TaiKhoanAdmin` entity, PBKDF2 / SHA-256 password hashing with salt, and dual-mode persistence (SQL Server raw DDL + In-Memory store).
- Recommend protecting all admin controllers via `[Authorize]` and mapping `/admin/login` / `/admin/logout` in `AdminController.cs`.

## Artifact Index
- handoff.md — Comprehensive Survey Report
- progress.md — Liveness & step tracking
- DISPATCH.md — Task assignment log
