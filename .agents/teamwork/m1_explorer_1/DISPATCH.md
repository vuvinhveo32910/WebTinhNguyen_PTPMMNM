# Milestone 1 - Explorer 1 Task Assignment

## Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure
**Focus**: Header Mobile Hamburger Button & Off-Canvas Mobile Drawer Navigation for `_Layout.cshtml` and `_CuaHangLayout.cshtml`.

## Inputs & Context
- Authoritative User Request: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
- Survey 1 Report: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\handoff.md`
- Survey 2 Report: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\handoff.md`

## Mission
1. Investigate the exact HTML/Razor and JavaScript structure required to add a responsive Hamburger Button to the top header (`md:hidden`) on both `_Layout.cshtml` and `_CuaHangLayout.cshtml`.
2. Design the Off-Canvas Mobile Drawer (`#mobileDrawer`) with:
   - Backdrop overlay (`#mobileDrawerBackdrop`).
   - Close button (`#mobileNavClose`).
   - Navigation links to all public routes: Trang chủ, Chương trình, Đóng góp, Ban tổ chức, Tin tức, Ký ức (with campaign year submenu), Cửa hàng gây quỹ.
   - Clean keyboard accessibility (ESC key to close), backdrop click to close.
3. Strict Constraints:
   - Preserves all existing test invariant IDs: `#themeToggleBtnMobile`, `#floatingCartBtn`, `#floatingCartCount`, `"CuaHang"`, `"hidden md:"`, `"fixed bottom"`, `"md:hidden"`.
   - In `_CuaHangLayout.cshtml`: Maintain light-mode lock (do NOT include dark mode toggle).
   - Zero changes to `/admin` or `_AdminLayout.cshtml`.
4. Output detailed recommendations and code specifications to `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_1\handoff.md`.


## 2026-11-07T09:32:12Z
[Message] timestamp=2026-11-07T09:32:12Z sender=55581585-523d-44d0-807c-aeb70f9a76e5 priority=MESSAGE_PRIORITY_HIGH
Role: Public Header Hamburger & Mobile Drawer Explorer.
Investigate the exact HTML/Razor and JavaScript structure required to add a responsive Hamburger Button to the top header (md:hidden) and an off-canvas drawer on both _Layout.cshtml and _CuaHangLayout.cshtml.
Ensure you preserve all test invariant IDs (#themeToggleBtnMobile, #floatingCartBtn, #floatingCartCount, "CuaHang", "hidden md:", "fixed bottom", "md:hidden").
Ensure 100% theme isolation for _CuaHangLayout.cshtml (no dark mode toggle).
Zero changes to /admin.
Write report to D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_1\handoff.md
