# Dispatch Task for Explorer 1 (Survey: Project Structure, DB & Layout)

## Objective
Investigate the project structure, ASP.NET Core version, database context (DbContext, models, connection strings, migrations), and existing layout/navigation components (`_Layout.cshtml`, header/nav/sidebar).

## Scope Boundaries
Read-only analysis. Do NOT modify source code files.

## Input Information
- ORIGINAL_REQUEST.md path: `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/ORIGINAL_REQUEST.md`
- Project root: `/home/hutech/Vinh_Offline/WEB_TinhNguyen`

## Output Requirements
Produce a comprehensive report in your working directory (`/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_survey_1/handoff.md`).
Report should detail:
1. Solution and project architecture (.NET version, project references, NuGet packages).
2. Existing DbContext, database provider (SQL Server, SQLite, MySQL, InMemory, etc.), connection strings in appsettings.json, and existing entities/tables.
3. Front-end layouts, navbar/slide bar structure in `Views/Shared/_Layout.cshtml` or partial views, CSS/JS libraries used.
4. How to run `dotnet build` and any compilation prerequisites or existing warnings/errors.


## 2026-11-06T07:37:24Z
[Message] timestamp=2026-11-06T07:37:24Z sender=3ebaa881-f84a-47af-9d40-eda26cf6ddcc priority=MESSAGE_PRIORITY_HIGH
content=You are teamwork_explorer_survey_1.
Your working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1\
Authoritative user request: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Read ORIGINAL_REQUEST.md before starting work.
You are a read-only exploration agent. You MUST NOT modify any source code or execute disruptive operations.

YOUR MISSION:
Perform a comprehensive survey of the backend architecture, database models, and Admin subsystem of the application at D:\web_tinh_nguyen.
Specifically investigate:
1. Technology stack (.NET version, ASP.NET Core MVC/Razor Pages/Blazor, Entity Framework Core version, database type e.g. SQLite, SQL Server, PostgreSQL, etc.).
2. Project structure, solution file, existing DbContext(s), existing entities/models (especially anything related to Payment, Bank, Order, Donation, Campaign, Settings/Configuration).
3. Database migrations workflow (EF Core migrations, seed data, ensure created).
4. Existing Admin area/controllers/views/routes/pages. How navigation, layouts, and CRUD forms are built. How authorization/authentication works in Admin.
5. File upload handling (e.g. wwwroot/uploads, static file serving, image storage conventions).
6. Existing QR generation utilities or libraries (VietQR, QRCoder, external API, etc.) if any.
7. How dynamic configuration can be modeled cleanly (e.g., PaymentConfig entity with BankName, AccountNumber, AccountHolder, BankCode/Bin, QrImageUrl, Note/Instructions, IsActive, UsageType e.g. Store vs Donation vs Default).

OUTPUT:
Write your comprehensive findings and evidence report to:
D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1\handoff.md
Update your progress.md periodically.
When finished, send a brief completion message to your parent.


## 2026-11-07T05:10:13Z
[Message] timestamp=2026-11-07T05:10:13Z sender=3007f0a1-1a91-4596-8f2a-fc29176756e7 priority=MESSAGE_PRIORITY_HIGH
content=You are teamwork_explorer_survey_1.
Working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1

MANDATORY FIRST STEP:
Read ORIGINAL_REQUEST.md at: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
Also read PROJECT.md at: D:\web_tinh_nguyen\PROJECT.md

OBJECTIVE:
Investigate all 11 admin functional modules and navigation layout in the existing codebase for Granular Admin Permissions (R1, R2, R3):
1. Trang chủ (Dashboard / Home)
2. Chương trình (Programs)
3. Gây quỹ (Fundraising)
4. Cấu hình QR (QR Configuration)
5. Ký ức (Memories)
6. Ban tổ chức (Organizers)
7. Tin tức (News)
8. Hiệu ứng (Effects)
9. Quản lý sản phẩm (Products)
10. Đơn hàng (Orders)
11. Quản lý tài khoản quản trị (Admin Accounts)

TASKS:
1. Locate and inspect the controllers and routes for all 10 existing modules (controllers in WEB_TinhNguyen/BoDieuKhien/ and WEB_TinhNguyen/Areas/CuaHang/Controllers/). Enumerate their controller classes, actions, routes, and current [Authorize] decoration.
2. Determine where and how the 11th module (Quản lý tài khoản quản trị) should be implemented (e.g., AdminTaiKhoanController or actions in AdminController), its routes, actions for listing, creating new admin accounts, and assigning permissions.
3. Inspect WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml:
   - Enumerate all menu navigation items, links, and icons.
   - Analyze how user identity/claims or helper methods can be used to dynamically show/hide menu items based on the logged-in admin's permissions (R3).
4. Identify any restrictions (e.g. locked views in Areas/CuaHang/Views/) and compatibility requirements.

DELIVERABLES:
- Write comprehensive analysis to D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1\analysis.md
- Write handoff report to D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_1\handoff.md
- Send completion message to parent orchestrator via send_message.
