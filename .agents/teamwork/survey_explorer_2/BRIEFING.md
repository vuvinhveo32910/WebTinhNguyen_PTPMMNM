# BRIEFING — 2026-11-07T09:20:00Z

## Mission
Analyze all external public-facing pages, views, controllers, forms, tables, lists, and modal components in the project to identify horizontal scroll overflow risks, font sizing issues, touch target issues, and responsive display bottlenecks on mobile screens (375px - 430px) without touching any /admin files.

## 🔒 My Identity
- Archetype: explorer
- Roles: Public Views & Responsive Hotspot Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\
- Original parent: 55581585-523d-44d0-807c-aeb70f9a76e5
- Milestone: Survey & Responsive Analysis

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Strictly isolate admin: no modifications or recommendations that impact `/admin` routes or files
- Comply with acceptance criteria: 375px-430px mobile responsiveness, no horizontal overflow (`document.scrollingElement.scrollWidth <= window.innerWidth`)

## Current Parent
- Conversation ID: 55581585-523d-44d0-807c-aeb70f9a76e5
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `WEB_TinhNguyen/BoDieuKhien/` (HomeController + 10 Admin controllers)
  - `WEB_TinhNguyen/GiaoDien/` (Home views, Shared layouts, Admin views)
  - `WEB_TinhNguyen/Areas/CuaHang/` (CuaHangController, GioHangController, Admin controllers & views)
  - `Styles/app.css`, `tailwind.config.js`, `package.json`
  - `tests/` (E2ETests 536 passed, adversarial harnesses 201 + 109 passed)
- **Key findings**:
  - Full inventory of 14 public views and 2 layouts established.
  - Strict Admin boundary confirmed: 12 admin controllers, 12 admin view directories, and `_AdminLayout.cshtml` must remain completely untouched.
  - Absence of hamburger/drawer navigation in header (acceptance criteria explicitly requires hamburger / drawer mobile menu).
  - 7-item bottom navigation bar crowded on 375px screens (~51px per item with 60px indicator).
  - Floating buttons overlapping: bottom nav (15px-85px), cart button (96px), and LoiChuc send wish button (20px).
  - Giant heading fonts: inline 64px (`TinTuc`), 48px (`GayQuy`), 48px/`text-5xl` (`Index`).
  - Input fields on mobile using `text-sm` (14px) which causes iOS WebKit auto-zoom; need `text-base sm:text-sm`.
  - Small touch targets on member Facebook links (24px-28px) and cart action buttons (32px-36px).
  - Long transfer memo text in `GayQuy` and cart row controls in `GioHang` risk overflow if not wrapped properly.
- **Unexplored areas**: None, all public views, controllers, assets, and tests surveyed.

## Key Decisions Made
- Established 6-section detailed findings report mapped to user requirements and test suites.

## Artifact Index
- `handoff.md` — Final survey and responsive analysis report
- `progress.md` — Heartbeat and activity progress
- `DISPATCH.md` — Task assignment
