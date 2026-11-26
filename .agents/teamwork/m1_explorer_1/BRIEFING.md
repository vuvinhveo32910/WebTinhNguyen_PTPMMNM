# BRIEFING — 2026-11-07T09:37:30Z

## Mission
Investigate the exact HTML/Razor and JavaScript structure required to add a responsive Hamburger Button to the top header (`md:hidden`) and an off-canvas drawer on both `_Layout.cshtml` and `_CuaHangLayout.cshtml`.

## 🔒 My Identity
- Archetype: teamwork_explorer
- Roles: Public Header Hamburger & Mobile Drawer Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_1\
- Original parent: 55581585-523d-44d0-807c-aeb70f9a76e5
- Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure

## 🔒 Key Constraints
- Read-only investigation — do NOT implement directly into codebase.
- Preserves all existing test invariant IDs: `#themeToggleBtnMobile`, `#floatingCartBtn`, `#floatingCartCount`, `"CuaHang"`, `"hidden md:"`, `"fixed bottom"`, `"md:hidden"`.
- Ensure 100% theme isolation for `_CuaHangLayout.cshtml` (light-mode lock, no dark mode toggle).
- Zero changes to `/admin` or `_AdminLayout.cshtml`.

## Current Parent
- Conversation ID: 55581585-523d-44d0-807c-aeb70f9a76e5
- Updated: 2026-11-07T09:37:30Z

## Investigation State
- **Explored paths**:
  - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (Header, bottom nav, scripts, theme toggle)
  - `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` (Header, bottom nav, light mode lock)
  - `tests/E2ETests/Tier1/Tier1_StorefrontTests.cs` (F05 & F06 assertions)
  - `tests/E2ETests/Tier5/Tier5_ChallengerM4_2ThemeHarness.cs` (Theme toggle and anti-FOUC assertions)
  - `tests/E2ETests/Tier5/Tier5_ChallengerM4OrderHarness.cs` (Cart button assertions)
  - `tests/adversarial_store_isolation_test.js` & `tests/adversarial_final_stress_harness.js` (Store theme isolation tests)
- **Key findings**:
  - Top header currently has no hamburger button on mobile (`md:hidden`) in either layout.
  - Liquid bottom navigation has `z-index: 9999`, requiring the drawer backdrop to be `z-[10000]` and drawer panel to be `z-[10001]`.
  - `_Layout.cshtml` mobile header currently only renders `#themeToggleBtnMobile` wrapped in `@if (ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang")`.
  - Mobile header needs a container `class="flex items-center gap-2 md:hidden"` containing `#themeToggleBtnMobile` (gated) and `#mobileMenuToggleBtn` (always present).
  - `_CuaHangLayout.cshtml` header needs an `md:hidden` container with cart shortcut and `#mobileMenuToggleBtn`, strictly light-mode with zero dark mode code.
  - Drawer (`#mobileDrawer`) must include `#mobileDrawerBackdrop`, `#mobileNavClose`, all public navigation links, campaign year submenu for Ký ức, and CTA button.
  - Full keyboard accessibility (ESC key to close), backdrop click to close, body scroll-lock during drawer open.
- **Unexplored areas**: None. Scope fully covered.

## Key Decisions Made
- Drawer backdrop positioned at `z-[10000]` and drawer at `z-[10001]` to ensure proper layering over `z-index: 9999` bottom navigation.
- Preserved all verbatim string assertions required by Tier 1 and Tier 5 tests.
- Provided complete before/after Razor and JavaScript specifications.

## Artifact Index
- DISPATCH.md — Task assignment
- BRIEFING.md — Persistent state
- progress.md — Liveness heartbeat
- handoff.md — Comprehensive investigation and specification report
