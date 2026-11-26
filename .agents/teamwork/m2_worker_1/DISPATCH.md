# DISPATCH: Milestone 2 — Public Home Views Responsive Optimization Worker

## Objective
Apply responsive web optimizations across all 8 public Home views in `WEB_TinhNguyen/GiaoDien/Home/`, the shared layout header in `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`, and the test runner in `tests/run_mobile_viewport_browser_tests.js` to ensure flawless rendering on mobile viewports (375px to 430px) without horizontal scroll (`scrollWidth <= innerWidth`), touch targets >= 40px, and resolve E2E test `M4_F06_T01`.

## Exclusive Write Boundaries
You own and may modify ONLY:
1. `WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml`
2. `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`
3. `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`
4. `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`
5. `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`
6. `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`
7. `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml`
8. `WEB_TinhNguyen/GiaoDien/Home/ComingSoon.cshtml`
9. `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (Header only)
10. `tests/run_mobile_viewport_browser_tests.js` (CSS URL replace bugfix)
11. `Styles/app.css` (only if necessary for reusable utility classes)

ABSOLUTE LOCKDOWN:
DO NOT modify any file under `WEB_TinhNguyen/GiaoDien/Admin*`, `WEB_TinhNguyen/BoDieuKhien/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*`, or `_AdminLayout.cshtml`.

## Mandatory Reading
- Authoritative User Request: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- Orchestrator Detailed Handoff: `D:\web_tinh_nguyen\.agents\teamwork\orchestrator\handoff.md`
- Explorer 1 Report (Typography & Headings): `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\handoff.md`
- Explorer 2 Report (Cards, Grids & Scaling): `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\handoff.md`
- Explorer 3 Report (Touch Targets & Bank Memo): `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\handoff.md`

## Summary of Changes to Apply
1. **`TinTuc.cshtml`**:
   - Hero title (lines 93-96): Replace inline `font-size: @(Model?.Hero_CoChuTieuDe ?? "64px")` with `font-size: clamp(1.875rem, 6vw, @(Model?.Hero_CoChuTieuDe ?? "64px"));` and `leading-[1.2]`.
   - Card title (lines 166-169): Replace `font-size: @(news.CoChuTieuDe ?? "18px")` with `font-size: clamp(1rem, 2.5vw, @(news.CoChuTieuDe ?? "18px"));` + `line-clamp-2 break-words`.
   - Like button (lines 184-191): Add `min-h-[40px] min-w-[40px] p-2 -mr-2`.
   - Modal close button (lines 225-230): Upgrade to `h-10 w-10 min-w-[40px] min-h-[40px]`.
   - Modal FB link (lines 243-246): Add `min-h-[44px]`.

2. **`GayQuy.cshtml`**:
   - Hero title (lines 68-71): Replace inline `font-size: @(Model?.Hero_CoChuTieuDe ?? "48px")` with `font-size: clamp(1.625rem, 5.5vw, @(Model?.Hero_CoChuTieuDe ?? "48px"));`.
   - Section headings (lines 107, 278, 303): `text-2xl font-black leading-tight sm:text-3xl md:text-4xl lg:text-5xl`.
   - Donation cards (lines 153 & 225): `w-full max-w-full sm:max-w-[360px] p-5 sm:p-7`.
   - Bank details (lines 174-192 & 246-249): Add `break-all` on `@effectiveAccNum` and `@effectiveMemo`, `shrink-0` on labels (`Ngân hàng:`, `Số tài khoản:`, `Chủ TK:`, `Cú pháp:`), `items-start justify-between gap-2`. (Resolves E2E test `M4_F06_T01`).
   - Sponsor buttons (line 329): Replace `w-44` with `w-[calc(50%-0.75rem)] max-w-[176px] sm:w-56 h-24 sm:h-32 p-4 sm:p-6`.
   - Modal close buttons (lines 348 & 365): Upgrade to `h-10 w-10 min-w-[40px] min-h-[40px]`.
   - Modal note (line 357): Add `break-all`.

3. **`Index.cshtml`**:
   - Countdown card (line 152): `p-4 sm:p-8 md:p-14 rounded-3xl sm:rounded-[2.5rem]`.
   - Slide (line 150): `px-2 sm:px-6 md:px-12`.
   - Countdown title (line 167): `text-2xl sm:text-4xl md:text-6xl lg:text-7xl font-bold mb-3 sm:mb-4`.
   - Countdown boxes (lines 187-204): `flex-nowrap sm:flex-wrap justify-center gap-2 sm:gap-3 md:gap-5 max-w-full`. Each box `w-16 sm:w-20 md:w-28 flex-1 min-w-[62px] max-w-[80px] sm:max-w-none py-3 px-2 sm:p-4`, numbers `text-2xl sm:text-3xl md:text-5xl`, labels `text-[9px] sm:text-[10px] md:text-xs`.
   - CTA buttons (lines 209-215): `w-full sm:w-auto text-center px-6 sm:px-8 py-3 sm:py-3.5`.
   - "Về dự án" (line 244): `text-2xl sm:text-3xl md:text-4xl lg:text-6xl`.
   - Section headings (lines 280, 329, 374, 418, 460): `text-2xl sm:text-3xl md:text-4xl lg:text-5xl`.
   - News card title (line 343): `font-size: clamp(1rem, 2.5vw, @(n.KichCoTieuDe ?? "18px"));` + `line-clamp-2 break-words`.
   - News "Đọc tiếp" and Like buttons (lines 349-357): `min-h-[40px] min-w-[40px]`.
   - Modal close button (line 483): `w-10 h-10 min-w-[40px] min-h-[40px]`.
   - Sponsor logo tooltips (line 393): `max-w-[200px] truncate` (replace `whitespace-nowrap`).

4. **`BanToChuc.cshtml`**:
   - Main title H1 (line 264): `text-2xl sm:text-4xl md:text-5xl`.
   - Cấp 1 card (lines 294, 315, 327): `p-5 sm:px-10 sm:py-7`, name pill `max-w-full truncate`.
   - Cấp 1, 2, 3 Facebook icon buttons (lines 305-309, 413-416, 527-531): Upgrade to `h-10 w-10 min-w-[40px] min-h-[40px]` with `-bottom-1 -right-1` on avatar.
   - Cấp 3 container (line 495): `p-3 sm:p-6 md:p-8 rounded-2xl sm:rounded-3xl`.
   - Cấp 3 member card (line 504): `p-3 sm:p-4 rounded-xl sm:rounded-2xl`.
   - `#btnShowMoreMembers` (line 540) & CTA (line 559): `min-h-[44px]`.

5. **`KyUc.cshtml`**:
   - Hero section (lines 20, 40): `px-4 sm:px-6 py-20 sm:py-32`, title `text-2xl sm:text-4xl md:text-6xl lg:text-7xl`.
   - Summary rich text (lines 52, 62): `px-4 sm:px-6`, card `p-4 sm:p-8 md:p-10 rounded-2xl sm:rounded-3xl`.
   - Section headings (lines 56, 76, 104): `text-2xl sm:text-3xl md:text-4xl lg:text-5xl`.
   - Gallery (line 85): `w-full @(img.LaAnhDoc ? "h-full min-h-[220px]" : "aspect-square")`.
   - Sponsor cards (lines 123, 126): `h-24 w-[150px] sm:h-32 sm:w-56 gap-3 sm:gap-5 md:gap-6 p-3 sm:p-6`.

6. **`ChuongTrinh.cshtml`**:
   - Hero section (lines 17, 26): `px-4 sm:px-6`, title `text-2xl sm:text-4xl lg:text-6xl`.
   - Hero floating photo (line 56): `translate-x-0 sm:translate-x-3` and `border-2 sm:border-4`.
   - Roadmap heading (line 75): `text-2xl sm:text-3xl lg:text-5xl`.
   - Timeline cards (line 106): `p-4 sm:p-6 rounded-2xl sm:rounded-3xl`.
   - Timeline connecting line (line 118, 123): `flex-1 min-h-[1.5rem] w-0.5 my-1 bg-emerald-200 dark:bg-emerald-900` with container `self-stretch shrink-0`.

7. **`LoiChuc.cshtml` & `ComingSoon.cshtml`**:
   - `LoiChuc.cshtml` (line 35): Title `text-2xl sm:text-4xl md:text-5xl`.
   - `ComingSoon.cshtml` (lines 74, 78): `text-2xl sm:text-3xl md:text-5xl` and `text-2xl md:text-4xl`.

8. **`_Layout.cshtml`**:
   - Header logo `<a>` (line 475): Add `max-w-[calc(100%-96px)] flex-1`.
   - Header title `<span>` (line 482): `class="min-w-0 flex-1 block leading-tight overflow-hidden"`.
   - Mobile buttons `<div>` (line 543): `class="flex items-center gap-2 md:hidden shrink-0"`.

9. **`tests/run_mobile_viewport_browser_tests.js`**:
   - Lines 223-224: Replace with `layout = layout.replace(/~?\/css\/site\.css/, cssUrl);`.

## Required Verification Steps
1. Build: `dotnet build` -> 0 errors.
2. Build CSS: `npm run build:css`.
3. E2E Tests: `dotnet run --project tests/E2ETests/E2ETests.csproj` -> All 552 tests PASS (including `M4_F06_T01`).
4. Automated Browser Tests: `node tests/run_mobile_viewport_browser_tests.js` -> 100% PASS across 375px, 390px, 412px, 430px on all public pages.
5. Admin Immutability Check: `git status WEB_TinhNguyen/GiaoDien/Admin* WEB_TinhNguyen/BoDieuKhien/Admin*` -> Clean!

## Output
Write full report to `.agents/teamwork/m2_worker_1/handoff.md` and message the orchestrator when complete.


## 2026-11-07T10:33:57Z
You are m2_worker_1 (Role: Home Views Responsive Worker).
Your working directory is: D:\web_tinh_nguyen\.agents\teamwork\m2_worker_1

Authoritative User Request: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
Your Dispatch Task Assignment: D:\web_tinh_nguyen\.agents\teamwork\m2_worker_1\DISPATCH.md
Orchestrator Detailed Handoff: D:\web_tinh_nguyen\.agents\teamwork\orchestrator\handoff.md
Explorer Reports with Exact Snippets:
- D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\handoff.md (Typography & Headings)
- D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\handoff.md (Cards, Grids & Scaling)
- D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\handoff.md (Touch Targets & Bank Memo break-all)

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

EXCLUSIVE WRITE BOUNDARIES:
You own and modify ONLY:
1. WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml
2. WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml
3. WEB_TinhNguyen/GiaoDien/Home/Index.cshtml
4. WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml
5. WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml
6. WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml
7. WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml
8. WEB_TinhNguyen/GiaoDien/Home/ComingSoon.cshtml
9. WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml (Header logo and mobile buttons only)
10. tests/run_mobile_viewport_browser_tests.js (Line 223 regex bugfix)
11. Styles/app.css (reusable utility classes if needed)

ABSOLUTE RESTRICTION:
DO NOT touch any file under WEB_TinhNguyen/GiaoDien/Admin*, WEB_TinhNguyen/BoDieuKhien/Admin*, WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*, WEB_TinhNguyen/Areas/CuaHang/Views/Admin*, or _AdminLayout.cshtml.
