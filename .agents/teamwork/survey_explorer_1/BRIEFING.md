# BRIEFING — 2026-11-07T09:23:00Z

## Mission
Investigate public layout files, header, footer, navigation bar (desktop vs mobile, hamburger/drawer toggle), viewport configuration, and CSS/Tailwind infrastructure for mobile responsiveness while safeguarding /admin.

## 🔒 My Identity
- Archetype: explorer
- Roles: Public Layout & Styling Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\
- Original parent: 55581585-523d-44d0-807c-aeb70f9a76e5
- Milestone: M1 - Survey & Technical Architecture

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Safeguard /admin: 100% untouched and isolated
- Mobile target viewport: 375px to 430px, no unintended horizontal scroll

## Current Parent
- Conversation ID: 55581585-523d-44d0-807c-aeb70f9a76e5
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (public main layout)
  - `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` (storefront layout)
  - `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` (admin layout - isolated)
  - `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml` (admin login - isolated)
  - `WEB_TinhNguyen/GiaoDien/_ViewStart.cshtml`, `WEB_TinhNguyen/Areas/CuaHang/Views/_ViewStart.cshtml`
  - `Styles/app.css`, `tailwind.config.js`, `package.json`, `WEB_TinhNguyen.csproj`
  - `WEB_TinhNguyen/GiaoDien/Home/*.cshtml` (all 9 public views)
  - `WEB_TinhNguyen/Areas/CuaHang/Views/**/*.cshtml` (storefront views)
  - `tests/E2ETests/` (536 E2E tests, verified 536/536 pass)
  - `WEB_TinhNguyen/wwwroot/js/script.js` (unreferenced prototype JS)
- **Key findings**:
  1. Admin segregation: 100% isolated. Admin uses `_AdminLayout.cshtml` and CDN Tailwind, does NOT link `site.css` or `Styles/app.css`. All 11 admin modules are untouched.
  2. Public layouts (`_Layout.cshtml` & `_CuaHangLayout.cshtml`) have NO hamburger menu button and NO mobile drawer. Desktop `<nav>` is hidden with `hidden md:flex`, mobile top header only has logo + theme toggle (or logo only in CuaHang).
  3. Mobile navigation relies exclusively on a fixed liquid bottom bar (`.navigation.fixed.bottom-[15px]`) with 7 items crammed into 375px (~51px each) and overflowing 60px circular indicator.
  4. Floating Cart button (`#floatingCartBtn`) fixed at `bottom-24` directly above bottom nav causes visual crowding and overlap on small screens (and collides with wish button on `LoiChuc.cshtml`).
  5. Viewport meta tags contain `maximum-scale=1, user-scalable=0` which disables pinch-to-zoom.
  6. Neither `html` nor `body` has `overflow-x: hidden` or `overflow-x: clip`, leaving public pages vulnerable to horizontal scroll blowout on 375px-430px viewports if any unconstrained child or transform overflows.
  7. Existing tests assert specific strings in `_Layout.cshtml` (`CuaHang`, `hidden md:`, `fixed bottom`, `md:hidden`, `floatingCartBtn`, `themeToggleBtn`, etc.) which must be preserved.
- **Unexplored areas**: None within Survey 1 scope.

## Key Decisions Made
- Fully documented evidence for 6 required architectural report sections.
- Formulated concrete implementation recommendations for responsive header + mobile drawer + non-breaking bottom bar preservation.

## Artifact Index
- D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\DISPATCH.md — Task assignment
- D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\progress.md — Liveness & progress tracker
- D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\handoff.md — Final investigation report
