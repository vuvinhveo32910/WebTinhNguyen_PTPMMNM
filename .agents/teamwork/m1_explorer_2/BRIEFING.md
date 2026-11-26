# BRIEFING — 2026-11-07T09:39:30Z

## Mission
Investigate 7-item bottom navigation & indicator sizing on 375px screens and floating button de-confliction for M1.

## 🔒 My Identity
- Archetype: teamwork_explorer
- Roles: Bottom Navigation & Floating Buttons Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2
- Original parent: 55581585-523d-44d0-807c-aeb70f9a76e5
- Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Preserve all test invariant strings ("fixed bottom", "md:hidden", "CuaHang", icons, #floatingCartBtn, #floatingCartCount)
- Zero changes to /admin
- Investigate 7-item bottom navigation, indicator sizing on 375px screens, floating cart and wish button de-confliction

## Current Parent
- Conversation ID: 55581585-523d-44d0-807c-aeb70f9a76e5
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (bottom nav, indicator, scripts, floatingCartBtn)
  - `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` (bottom nav, indicator, floatingCartBtn)
  - `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml` (floating wish button)
  - `tests/E2ETests/` (Tier 1-5 test suites, 536 tests passing)
  - `tests/adversarial_final_stress_harness.js` (201 tests passing)
  - `tests/adversarial_store_isolation_test.js` (109 tests passing)
- **Key findings**:
  - On 375px viewport (360px nav width), 7 items get 51.4px slot width. Current indicator has outer diameter 72px (`content-box` 60px + 2x6px borders), overflowing 10.3px outside the capsule on items 0 and 6.
  - Scaling indicator on screens <= 430px to 44px outer diameter (`border-box`, 4px border) completely eliminates clipping, providing +3.7px internal clearance and 20.4px margin to neighbor icons.
  - Only active item shows text label (`opacity: 0` on inactive); scaling active font to 9px fits without collision.
  - Wish button on `LoiChuc.cshtml` at `bottom: 20px` is completely covered by `.navigation` on mobile (<768px) and overlaps `#floatingCartBtn` on desktop (>=768px).
  - Repositioning Wish button to `bottom-[10.5rem] md:bottom-24 right-5` (Strategy A) or `left-5 bottom-24 md:bottom-8` (Strategy B) completely de-conflicts all floating elements.
  - All 17 test invariant string contracts verified and preserved.
  - Zero admin modifications confirmed.
- **Unexplored areas**: None within M1 Explorer 2 scope.

## Key Decisions Made
- Established responsive indicator geometry model (44px outer diameter, 64px bar height on <= 430px).
- Confirmed percentage-based JS indicator math (`calc(${centerPosition}%)`) requires no changes.
- Formulated clear 3-layer floating action button z-index and coordinate specification.
- Handoff report finalized with exact CSS and Razor snippets.

## Artifact Index
- `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\handoff.md` — Final investigation report
- `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\progress.md` — Liveness progress tracker
- `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\DISPATCH.md` — Task assignment log
