# Milestone 1 - Worker Implementation Task Assignment

## Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure
**Assigned Agent**: `teamwork_worker` instance 1 (`m1_worker_1`)
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\`

## MANDATORY INTEGRITY WARNING
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

## Authoritative User Request
Path: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
Scope Document: `D:\web_tinh_nguyen\PROJECT.md`

## Input Specifications & Explorer Findings
Read the complete reports before making changes:
- `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_1\handoff.md` (Header Hamburger & Drawer Markup)
- `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\handoff.md` (Bottom Nav Geometry & Floating Action De-confliction)
- `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_3\handoff.md` (Viewport, Root CSS Overflow Clip & Store Isolation)

## Exclusive Write Ownership
You exclusively own and may edit ONLY the following files:
1. `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`
2. `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`
3. `Styles/app.css`
4. `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml`

STRICT PROHIBITION: You MUST NOT modify any file under `WEB_TinhNguyen/GiaoDien/Admin*`, `WEB_TinhNguyen/BoDieuKhien/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*`, or `_AdminLayout.cshtml`.

## Detailed Implementation Tasks
1. **Viewport Meta Tag Standardization**:
   - In `_Layout.cshtml:35` and `_CuaHangLayout.cshtml:26`:
     Replace `<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0">`
     with `<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">`.

2. **Root CSS Overflow Clip**:
   - In `Styles/app.css`: Add `overflow-x: hidden; overflow-x: clip;` to `html, body` inside `@layer base`.
   - In `<style>` blocks of `_Layout.cshtml` and `_CuaHangLayout.cshtml`: Ensure `html, body { overflow-x: hidden; overflow-x: clip; }`.
   - Add `overflow-x-clip` to `<body>` in both layouts.

3. **Header Hamburger Button & Mobile Drawer on `_Layout.cshtml`**:
   - In header mobile container (adjacent to `#themeToggleBtnMobile`), add `#mobileMenuToggleBtn` (`md:hidden`, `p-2`, `w-10 h-10`, `onclick="openMobileDrawer()"`).
   - Retain `#themeToggleBtnMobile` verbatim inside `@if (ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang")`.
   - Add `#mobileDrawerBackdrop` (`z-[10000]`) and `#mobileDrawer` (`z-[10001]`, `w-[85vw] max-w-[340px]`, `#mobileNavClose`, links to all public routes, campaign year submenu, quick donate CTA, and JavaScript controller functions `openMobileDrawer()`, `closeMobileDrawer()`, `toggleDrawerSubmenu()`, ESC key listener).

4. **Header Hamburger Button & Mobile Drawer on `_CuaHangLayout.cshtml`**:
   - In header mobile container, add `#mobileNavToggle` (`w-10 h-10`, `onclick="openDrawer()"`).
   - Add `#mobileDrawerBackdrop` (`z-[10000]`) and `#mobileDrawer` (`z-[10001]`, strictly light-mode, zero dark classes, zero theme toggle buttons, `#mobileNavClose`, store links, campaign year submenu with `area = ""`, cart CTA, isolated JS functions).

5. **Mobile Bottom Navigation Optimization**:
   - In `_Layout.cshtml` and `_CuaHangLayout.cshtml`:
     Add `@media (max-width: 430px)` rules setting `.navigation { height: 64px; border-radius: 18px; }`, `.navigation .indicator { top: -24px; width: 44px; height: 44px; border-width: 4px; box-sizing: border-box; }`, `.navigation ul li a .icon { font-size: 1.15rem; }`, `.navigation ul li.active a .icon { top: 2px; font-size: 1.25rem; }`, `.navigation ul li.active a .text { font-size: 9px; bottom: 8px; }`.
     Add `max-width: 440px;` to `.navigation`.
   - Preserve all existing verbatim string invariants: `"fixed bottom"`, `"md:hidden"`, `"CuaHang"`, `<svg`/`fa-`, `#floatingCartBtn`, `#floatingCartCount`.

6. **Floating Buttons De-confliction on `GiaoDien/Home/LoiChuc.cshtml`**:
   - At line 109 of `LoiChuc.cshtml`, update the floating wish button class from:
     `fixed bottom-[max(1.25rem,env(safe-area-inset-bottom))] right-5 z-40`
     to:
     `fixed bottom-[10.5rem] md:bottom-24 right-5 z-40`.

## Required Build & Verification Steps
You MUST execute the following verification commands and report the results in your handoff report:
1. `npm run build:css` (recompile Tailwind)
2. `dotnet build` (verify 0 errors)
3. `dotnet run --project tests/E2ETests/E2ETests.csproj` (all 536 baseline tests must pass 100%)
4. `node tests/adversarial_store_isolation_test.js` (all 109 tests must pass 100%)
5. `node tests/adversarial_final_stress_harness.js` (all 201 tests must pass 100%)
6. `git status` on admin folders to confirm 0 admin files modified.

Report all results, file diffs, and verification output to `D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\handoff.md`.


## 2026-11-07T09:45:35Z
You are teamwork_worker instance 1 (m1_worker_1).
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\
Read your task assignment in: D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\DISPATCH.md
Exclusive write ownership:
1. WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml
2. WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml
3. Styles/app.css
4. WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml
STRICT PROHIBITION: You MUST NOT modify any file under WEB_TinhNguyen/GiaoDien/Admin*, WEB_TinhNguyen/BoDieuKhien/Admin*, WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*, WEB_TinhNguyen/Areas/CuaHang/Views/Admin*, or _AdminLayout.cshtml.
