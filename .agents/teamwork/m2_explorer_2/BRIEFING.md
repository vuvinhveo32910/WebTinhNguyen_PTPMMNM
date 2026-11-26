# BRIEFING — 2026-11-07T10:30:00Z

## Mission
Investigate container paddings, card grids, countdown boxes, and sponsor logos across WEB_TinhNguyen/GiaoDien/Home/ (Index, BanToChuc, KyUc, ChuongTrinh), and formulate responsive sizing solutions to guarantee document.scrollingElement.scrollWidth <= window.innerWidth on 375px-430px viewports with zero admin modifications.

## 🔒 My Identity
- Archetype: teamwork_explorer
- Roles: Home Views Grids, Cards & Scaling Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\
- Original parent: 55581585-523d-44d0-807c-aeb70f9a76e5
- Milestone: M2 - Public Home Views Responsive Optimization

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Zero modifications to /admin
- Investigate container paddings, card grids, countdown boxes, and sponsor logos across WEB_TinhNguyen/GiaoDien/Home/ (Index, BanToChuc, KyUc, ChuongTrinh)
- Guarantee document.scrollingElement.scrollWidth <= window.innerWidth on 375px-430px viewports
- Must follow 5-component handoff report protocol

## Current Parent
- Conversation ID: 55581585-523d-44d0-807c-aeb70f9a76e5
- Updated: 2026-11-07T10:30:00Z

## Investigation State
- **Explored paths**:
  - `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`
  - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`
  - `Styles/app.css` & `WEB_TinhNguyen/wwwroot/css/site.css`
  - `tests/run_mobile_viewport_browser_tests.js`
- **Key findings**:
  1. Countdown timer in Index: `p-8` + 4 * `w-20` (80px) + gap-3 (12px) = 356px needed vs 263px available at 375px. Forces 2x2 wrapping. Needs `p-4 sm:p-8`, `w-16 sm:w-20 md:w-28`, `gap-2 sm:gap-3 md:gap-5` to fit 1 row cleanly at 280px <= 311px available.
  2. Sponsor logos in KyUc: `w-44` (176px) + gap-5 (20px) = 372px needed vs 335px available at 375px/390px. Causes awkward 1-card-per-row stack. Needs `w-[150px] sm:w-56`, `gap-3 sm:gap-5` to fit 2 per row symmetrically at 312px <= 343px.
  3. BanToChuc card grids: Level 3 member cards have 80px total padding (`p-6` wrapper + `p-4` card), leaving only 151px for member text, truncating names. Facebook button is 24px-28px (below touch standard). Needs `p-3 sm:p-6` wrapper, `p-3 sm:p-4` card, expanding text space to 183px (+21.2%), and `h-8 w-8` touch targets.
  4. ChuongTrinh hero floating photo has `translate-x-3` + `scale-105` risking border clip on 375px, and timeline line has fixed `h-8` (32px) which disconnects on multi-line descriptions. Needs `translate-x-0 sm:translate-x-3` and dynamic `flex-1 min-h-[1.5rem]` timeline line.
  5. Critical Test Runner Defect: `tests/run_mobile_viewport_browser_tests.js` lines 223-224 had a double replacement bug (`layout.replace('/css/site.css', cssUrl)`) that corrupted the CSS link into `wwwrootfile:///...`, preventing `site.css` from loading during headless test runs!
  6. Header Logo/Title Defect in `_Layout.cshtml`: Unconstrained `<span>` title pushing `<div class="flex items-center gap-2 md:hidden">` out to 492px (+119px excess) on 375px viewports.
- **Unexplored areas**: None within the scope of M2 Home views.

## Key Decisions Made
- Formulated exact mathematical specifications and class replacements for all 4 Home views.
- Documented both the view layout fixes and the test harness fix in `handoff.md`.

## Artifact Index
- `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\handoff.md` — Final analysis and handoff report
- `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\progress.md` — Liveness heartbeat and step tracking
