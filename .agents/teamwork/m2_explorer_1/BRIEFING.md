# BRIEFING — 2026-11-07T10:22:00Z

## Mission
Investigate oversized headings, hero titles, and inline font sizes in all 8 public Home views (WEB_TinhNguyen/GiaoDien/Home/) and formulate responsive typography solutions for 375px-430px viewports with zero /admin impact.

## 🔒 My Identity
- Archetype: teamwork_explorer
- Roles: Home Views Typography & Headings Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1
- Original parent: 55581585-523d-44d0-807c-aeb70f9a76e5
- Milestone: M2 - Public Home Views Responsive Optimization

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Zero modifications to /admin files or routes
- Ensure typography solutions balance readability, eliminate line-breaking/overflow on 375px-430px viewports, and preserve desktop appearance
- All reports and metadata stay inside D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\

## Current Parent
- Conversation ID: 55581585-523d-44d0-807c-aeb70f9a76e5
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
  - `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\handoff.md`
  - `WEB_TinhNguyen/GiaoDien/Home/` (9 view files: Index, ChuongTrinh, GayQuy, BanToChuc, TinTuc, KyUc, LoiChuc, ComingSoon, Privacy)
  - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` & Tailwind configurations
  - `tests/run_mobile_viewport_browser_tests.js` & `tests/E2ETests/Tier1/Tier1_MobileViewportTests.cs`
- **Key findings**:
  - TinTuc.cshtml:94 hardcoded inline `style="font-size: @(Model?.Hero_CoChuTieuDe ?? "64px")"` causes 300px+ height title and line breaking on 375px. Solution: `clamp(1.875rem, 6vw, @(Model?.Hero_CoChuTieuDe ?? "64px"))`.
  - GayQuy.cshtml:69 hardcoded inline `style="font-size: @(Model?.Hero_CoChuTieuDe ?? "48px")"` causes 336px uppercase "YÊU THƯƠNG" which overflows 327px inner width. Solution: `clamp(1.625rem, 5.5vw, @(Model?.Hero_CoChuTieuDe ?? "48px"))`.
  - Index.cshtml:167 `text-5xl md:text-7xl` in countdown card with `p-8` leaves only 279px usable width. Solution: `text-2xl sm:text-4xl md:text-6xl lg:text-7xl` with `p-5 sm:p-8 md:p-14`.
  - Index.cshtml:244 "Về dự án" `text-4xl md:text-5xl lg:text-6xl` (36px base on 53 characters). Solution: `text-2xl sm:text-3xl md:text-4xl lg:text-6xl`.
  - Section headings across Index, GayQuy, ChuongTrinh, KyUc, BanToChuc: normalized from `text-3xl` / `text-4xl` base to `text-2xl sm:text-3xl md:text-4xl lg:text-5xl`.
  - Automated browser test matrix confirms failures on 375px/390px for Index, GayQuy, KyUc, BanToChuc, TinTuc, and LoiChuc.
  - Zero /admin files involved (100% isolated).
- **Unexplored areas**:
  - Grid card widths, sponsor logos, and image aspect ratios belong to Layout/Storefront explorer peers.

## Key Decisions Made
- Use CSS `clamp(min, preferred_vw, max)` for dynamic database-bound font-sizes (`Model?.Hero_CoChuTieuDe`, `news.CoChuTieuDe`) to protect mobile while retaining admin desktop customization.
- Use standardized Tailwind responsive scale (`text-2xl sm:text-4xl md:text-5xl lg:text-7xl` for H1, `text-2xl sm:text-3xl md:text-4xl lg:text-5xl` for H2) for static headings.

## Artifact Index
- `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\handoff.md` — Final 5-component report
- `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\progress.md` — Execution heartbeat and progress log
- `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\BRIEFING.md` — Persistent working memory
- `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\DISPATCH.md` — Incoming assignment logs
