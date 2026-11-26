## 2026-11-07T02:55:05Z
You are explorer_m2_2 (Role: Login View & Layout Explorer).
Your working directory is: D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_2
Your parent conversation ID is: 8cd06411-5097-4e95-b532-893861716fb8

CRITICAL: You MUST read:
- D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
- D:\web_tinh_nguyen\PROJECT.md
- D:\web_tinh_nguyen\.agents\teamwork\worker_m1\handoff.md

Task for Milestone 2 (M2: Admin Auth UI, Pipeline & Route Protection):
Investigate and design the exact Razor view and Layout specifications for:
1. `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml`:
   - Inspect existing admin views in `GiaoDien/Admin/` to align styling, fonts, icons (FontAwesome), and Tailwind CSS classes.
   - Design a responsive, polished admin login page matching the volunteer green/emerald palette:
     - Form bound to `@model WEB_TinhNguyen.MoHinh.AdminLoginViewModel`.
     - CSRF token `@Html.AntiForgeryToken()`.
     - Clear error summary alert banner when `!ViewData.ModelState.IsValid`:
       Display clear message: `"Tên đăng nhập hoặc mật khẩu không chính xác."` or validation errors.
     - Inputs for `TenDangNhap` and `MatKhau` (with password visibility toggle if appropriate).
     - Hidden input for `ReturnUrl`.
     - "Ghi nhớ đăng nhập" checkbox.
     - Submit button with volunteer branding.
     - View layout: Can use a standalone clean layout or `Layout = null` with full HTML/Tailwind shell so login is clean without sidebar clutter.
2. `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`:
   - Inspect lines 330-348:
     - Update logout button (line 343) from `href="#"` to `href="/admin/logout"` (or `@Url.Action("Logout", "Admin")`).
     - Update hardcoded admin name ("Admin Hệ thống", "Quản trị viên") to bind `@(User.Identity?.Name ?? "Quản trị viên")`.
3. RESTRICTION: Ensure NO files under `WEB_TinhNguyen/Areas/CuaHang/Views/` are touched.

Outputs:
Write your structured findings and exact Razor markup to:
`D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_2\handoff.md`

Remember: You are read-only. Do NOT modify source code files. Update progress.md regularly.
Notify parent (Recipient: 8cd06411-5097-4e95-b532-893861716fb8) via send_message when complete.
