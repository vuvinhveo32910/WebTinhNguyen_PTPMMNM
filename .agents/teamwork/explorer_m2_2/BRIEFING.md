# BRIEFING — 2026-11-07T03:00:00Z

## Mission
Investigate and design the exact Razor view for Admin Login (`WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml`) and updates to Admin Layout (`WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`) for Milestone 2.

## 🔒 My Identity
- Archetype: explorer
- Roles: Login View & Layout Explorer
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_2
- Original parent: 8cd06411-5097-4e95-b532-893861716fb8
- Milestone: M2 - Admin Auth UI, Pipeline & Route Protection

## 🔒 Key Constraints
- Read-only investigation — do NOT implement / modify source code directly
- Output structured findings and exact Razor markup to handoff.md
- Ensure NO files under `WEB_TinhNguyen/Areas/CuaHang/Views/` are touched
- Notify parent upon completion via send_message

## Current Parent
- Conversation ID: 8cd06411-5097-4e95-b532-893861716fb8
- Updated: not yet

## Investigation State
- **Explored paths**:
  - `WEB_TinhNguyen/MoHinh/TaiKhoanAdmin.cs` (reviewed `AdminLoginViewModel` structure: TenDangNhap, MatKhau, GhiNhoDangNhap, ReturnUrl)
  - `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` (inspected lines 320-357: topbar, user badge at 333-341, logout anchor at 343)
  - `WEB_TinhNguyen/GiaoDien/Admin/Index.cshtml` (inspected design language: green gradient `#1b5e20` to `#4caf50`, Be Vietnam Pro, FontAwesome 6, card styles)
  - `WEB_TinhNguyen/GiaoDien/AdminTrangChu/Index.cshtml` and `AdminCauHinhQR/Index.cshtml` (inspected error/alert banners, button styles)
  - `tests/E2ETests/Tier1/Tier1_AdminRoutingTests.cs` and `Tier5/Tier5_Challenger2_AdminAdversarialHarness.cs` (verified test constraints on `_AdminLayout.cshtml` and admin views)
- **Key findings**:
  - `Login.cshtml` does not yet exist. Should use `Layout = null` with full standalone HTML/Tailwind shell to eliminate admin sidebar clutter.
  - Palette alignment: volunteer emerald/green (`#1b5e20`, `#4caf50`, emerald-600, emerald-700), dark slate background accents (`#161a2d`), font Be Vietnam Pro, FontAwesome 6 icons.
  - `_AdminLayout.cshtml` line 338 has hardcoded `Admin Hệ thống` and line 343 has `href="#"`. These need targeted updates to bind `@(User.Identity?.Name ?? "Quản trị viên")` and `href="/admin/logout"`.
- **Unexplored areas**: None for M2-2 scope.

## Key Decisions Made
- `Login.cshtml` designed with `Layout = null`, complete self-contained HTML/Tailwind shell, CSRF token, alert error banner rendering model and field validation errors, input fields with icons, password show/hide toggle, "Ghi nhớ đăng nhập" checkbox, hidden ReturnUrl input, and volunteer branding.
- `_AdminLayout.cshtml` exact snippet ready for replacement at lines 330-348.

## Artifact Index
- DISPATCH.md — incoming instructions log
- BRIEFING.md — situational awareness
- progress.md — liveness heartbeat
- handoff.md — comprehensive specifications and Razor markup for implementer
