## 2026-11-07T02:54:56Z
You are explorer_m2_1 (Role: Pipeline & Login/Logout Controller Explorer).
Your working directory is: D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_1
Your parent conversation ID is: 8cd06411-5097-4e95-b532-893861716fb8

CRITICAL: You MUST read:
- D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
- D:\web_tinh_nguyen\PROJECT.md
- D:\web_tinh_nguyen\.agents\teamwork\worker_m1\handoff.md

Task for Milestone 2 (M2: Admin Auth UI, Pipeline & Route Protection):
Investigate and design the exact code specification for:
1. `Program.cs` in `WEB_TinhNguyen/Program.cs`:
   - Cookie Authentication registration:
     `builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(...)`
     - LoginPath = "/admin/login", LogoutPath = "/admin/logout", AccessDeniedPath = "/admin/login"
     - Cookie name = ".WEB_TinhNguyen.AdminAuth", HttpOnly = true, SameSite = Lax, ExpireTimeSpan = 8 hours, SlidingExpiration = true.
   - Pipeline Ordering:
     Crucial constraint: Existing test CH19 asserts `actualSessionLine > actualRoutingLine && actualSessionLine < actualAuthLine`.
     Therefore, the order MUST be:
     `app.UseRouting();` -> `app.UseSession();` -> `app.UseAuthentication();` -> `app.UseAuthorization();`
2. `AdminController.cs` in `WEB_TinhNguyen/BoDieuKhien/AdminController.cs`:
   - Constructor: Ensure parameterless or optional DI `public AdminController(ApplicationDbContext? context = null)` so existing unit tests like `F19_T08` (`new AdminController()`) continue to pass.
   - `[AllowAnonymous] [HttpGet] public IActionResult Login(string? returnUrl = null)`: If already authenticated (`User.Identity?.IsAuthenticated == true`), redirect to local returnUrl or `/Admin`. Otherwise, render view with `AdminLoginViewModel`.
   - `[AllowAnonymous] [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)`:
     - Validate credentials using `DbInitializer.ValidateAdminCredentials(model.TenDangNhap, model.MatKhau, _context)`.
     - On failure: Add model error `"Tên đăng nhập hoặc mật khẩu không chính xác."` and return View.
     - On success: Create Claims (Name, Role "Admin", NameIdentifier), ClaimsIdentity, ClaimsPrincipal, and call `HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties)`. Redirect to local returnUrl or `/Admin`.
   - `[HttpGet, HttpPost] public async Task<IActionResult> Logout()`: Call `HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)`. Clear session. Redirect to `/admin/login`.
   - Protect dashboard: `[Authorize] public IActionResult Index()`.

Outputs:
Write your structured findings and exact C# code specifications to:
`D:\web_tinh_nguyen\.agents\teamwork\explorer_m2_1\handoff.md`

Remember: You are read-only. Do NOT modify source code files. Update progress.md regularly.
Notify parent (Recipient: 8cd06411-5097-4e95-b532-893861716fb8) via send_message when complete.
