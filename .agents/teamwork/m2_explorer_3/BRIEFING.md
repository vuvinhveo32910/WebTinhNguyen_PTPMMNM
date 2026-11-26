# BRIEFING — 2026-11-07T10:23:00Z

## Mission
Investigate GayQuy.cshtml (memo break-all, donation card sizing/paddings) and touch targets across Home views (e.g. BanToChuc.cshtml Facebook icon buttons).

## 🔒 My Identity
- Archetype: explorer
- Roles: Touch Targets & Bank Memo Line Break Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3
- Original parent: 55581585-523d-44d0-807c-aeb70f9a76e5
- Milestone: M2 - Public Home Views Responsive Optimization

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Zero modifications to /admin
- Address test M4_F06_T01 (transfer memo line breaking with break-all)
- Ensure all interactive elements have adequate touch target sizing (>= 40px)
- Strictly comply with project guidelines

## Current Parent
- Conversation ID: 55581585-523d-44d0-807c-aeb70f9a76e5
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `tests/E2ETests/Tier1/Tier1_MobileViewportTests.cs` (Feature 6, test `M4_F06_T01`)
  - `tests/run_mobile_viewport_browser_tests.js` (Blink rendering test runner)
  - `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/ComingSoon.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/Privacy.cshtml`
- **Key findings**:
  1. `M4_F06_T01` specifically checks for `"break-all"` or `"break-words"` in `GayQuy.cshtml`. Currently skipped (551/552 passed in E2E tests). Adding `break-all` directly unlocks 100% pass (552/552).
  2. `GayQuy.cshtml` donation memo spans (lines 188-191 & 246-249 and modal line 357) lack `break-all` and label `shrink-0`.
  3. `GayQuy.cshtml` card `max-w-[360px]` with `p-7` (56px padding) leaves only 271px on 375px screens. Update to `p-5 sm:p-7` and `max-w-full sm:max-w-[360px]`.
  4. `GayQuy.cshtml` sponsor buttons (`w-44`) caused +65px horizontal scroll overflow in browser test runner.
  5. `GayQuy.cshtml` modal close buttons are `h-9 w-9` (36px < 40px).
  6. `BanToChuc.cshtml` Facebook buttons are undersized: Cấp 1 `h-8 w-8` (32px), Cấp 2 `h-6 w-6` (24px!), Cấp 3 `h-7 w-7` (28px). All need upgrade to `h-10 w-10 min-w-[40px] min-h-[40px]`.
  7. `Index.cshtml` and `TinTuc.cshtml` Like and "Đọc tiếp" buttons have zero padding (~16px - 20px). Modal close buttons in `TinTuc` are `h-9 w-9` (36px).
  8. Admin boundary 100% isolated: zero modifications to `/admin`.
- **Unexplored areas**: None. Complete investigation of all targets conducted.

## Key Decisions Made
- Fully documented exact line numbers, before/after replacement snippets, and mathematical justifications in `handoff.md`.

## Artifact Index
- D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\DISPATCH.md — Dispatch instructions
- D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\progress.md — Progress log
- D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\handoff.md — Final investigation report
