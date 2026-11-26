## 2026-11-06T12:29:09Z
You are teamwork_worker (Worker 1).
Working directory: D:\web_tinh_nguyen\.agents\teamwork\worker_1

MANDATORY FIRST STEP:
Read the verbatim user request at:
D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

CONTEXT & INPUT DOCUMENTS:
- Project Plan: D:\web_tinh_nguyen\.agents\teamwork\PROJECT.md
- Survey Explorer 1 Report: D:\web_tinh_nguyen\.agents\teamwork\explorer_survey_1\handoff.md
- Survey Explorer 2 Report: D:\web_tinh_nguyen\.agents\teamwork\explorer_survey_2\handoff.md
- Survey Explorer 3 Report: D:\web_tinh_nguyen\.agents\teamwork\explorer_survey_3\handoff.md

YOUR TASKS:
1. Fix navigation and routing in `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`:
   - Replace `"GiaoDien/<Controller>"` with pure controller names in `@Url.Action`:
     - `"GiaoDien/AdminTrangChu"` -> `"AdminTrangChu"`
     - `"GiaoDien/AdminChuongTrinh"` -> `"AdminChuongTrinh"`
     - `"GiaoDien/AdminGayQuy"` -> `"AdminGayQuy"`
     - `"GiaoDien/AdminKyUc"` -> `"AdminKyUc"`
     - `"GiaoDien/AdminBanToChuc"` -> `"AdminBanToChuc"`
     - `"GiaoDien/AdminTinTuc"` -> `"AdminTinTuc"`
   - Add explicit `new { area = "" }` to all root admin action links (`Admin`, `AdminTrangChu`, `AdminChuongTrinh`, `AdminGayQuy`, `AdminCauHinhQR`, `AdminKyUc`, `AdminBanToChuc`, `AdminTinTuc`, `Home`) so navigation from `CuaHang` area never causes ambient area pollution or 404s.
   - Fix active state checks in `_AdminLayout.cshtml` (lines 258, 263) so they check against `"AdminChuongTrinh"` and `"AdminGayQuy"` (not `"GiaoDien/..."`).
2. Fix dead shortcut link in `WEB_TinhNguyen/GiaoDien/Admin/Index.cshtml`:
   - Change `href="#"` for "Quản lý Tin tức" to `@Url.Action("Index", "AdminTinTuc", new { area = "" })`.
3. Enhance controller offline resilience:
   - In `AdminTrangChuController.cs`, `AdminChuongTrinhController.cs`, `AdminGayQuyController.cs`, `AdminKyUcController.cs`, `AdminBanToChucController.cs`, and `AdminTinTucController.cs`, wrap the database query `_context.ChienDichs.OrderByDescending(...).ToListAsync()` in a `try/catch` block that returns `View(new List<ChienDich>())` if the database is offline/unreachable. This guarantees all 8 admin pages reliably return HTTP 200 and render valid HTML under all test and production conditions.
4. Optional UI Polish:
   - Check `GiaoDien/AdminChuongTrinh/Index.cshtml` and `GiaoDien/AdminKyUc/Index.cshtml` to ensure consistent padding/theme with `_AdminLayout.cshtml`.
5. Automated Test Suite:
   - In `tests/E2ETests/`, ensure there are automated tests explicitly verifying:
     a. All 8 Admin routes (`/Admin`, `/AdminTrangChu`, `/AdminChuongTrinh`, `/AdminGayQuy`, `/AdminCauHinhQR`, `/AdminKyUc`, `/AdminBanToChuc`, `/AdminTinTuc`) execute and return ViewResult / HTTP 200.
     b. `_AdminLayout.cshtml` contains zero occurrences of `GiaoDien/` or `%2F` in generated links, and all 8 links point directly to the correct controller actions.
   - Run `dotnet build WEB_TinhNguyen\WEB_TinhNguyen.csproj` to confirm 0 errors.
   - Run `dotnet run --project tests\E2ETests\E2ETests.csproj` to confirm 100% of tests pass.

OUTPUT REQUIREMENTS:
- Record all changes in `D:\web_tinh_nguyen\.agents\teamwork\worker_1\changes.md`
- Document build & test execution results in `D:\web_tinh_nguyen\.agents\teamwork\worker_1\handoff.md`
- Update `D:\web_tinh_nguyen\.agents\teamwork\worker_1\progress.md`
- Send completion message to orchestrator when finished.
