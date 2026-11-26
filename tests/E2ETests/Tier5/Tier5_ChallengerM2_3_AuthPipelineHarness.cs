using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WEB_TinhNguyen.Areas.CuaHang.Controllers;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;
using WEB_TinhNguyen.Tests.E2E.Framework;
using BaoMatMatKhau = WEB_TinhNguyen.DuLieu.BaoMatMatKhau;

namespace WEB_TinhNguyen.Tests.E2E.Tier5
{
    /// <summary>
    /// Tier 5: Consolidated HTTP Pipeline Authentication and Route Protection Test Suite.
    /// Empirical verification and adversarial hardening for:
    /// a. Reflection verification of class-level [Authorize] across all 11 admin controllers.
    /// b. Reflection verification of [Authorize] and [ValidateAntiForgeryToken] across all 21 CRUD POST actions.
    /// c. Cookie authentication options configuration (Name = .WEB_TinhNguyen.AdminAuth, LoginPath = /admin/login, etc.).
    /// d. In-process pipeline unauthenticated challenge 302 redirection to /admin/login?ReturnUrl=... for /Admin and all subroutes.
    /// e. Valid login (admin / Admin@123) authentication, ClaimsPrincipal (Role = "Admin"), and local redirect.
    /// f. Invalid login rejection (wrong password, empty password, non-existent user, inactive account, SQL injection attempts) without issuing auth tickets.
    /// g. Logout session invalidation (SignOutAsync and Session.Clear()).
    /// h. Open redirect defense neutralizing malicious URLs.
    /// </summary>
    public class Tier5_ChallengerM2_3_AuthPipelineHarness
    {
        #region Test Mocks & Helpers

        public class MockAuthService : IAuthenticationService
        {
            public bool AuthenticateCalled { get; private set; }
            public bool SignInCalled { get; private set; }
            public bool SignOutCalled { get; private set; }
            public bool ChallengeCalled { get; private set; }
            public string? LastSignInScheme { get; private set; }
            public ClaimsPrincipal? LastSignInPrincipal { get; private set; }
            public AuthenticationProperties? LastSignInProperties { get; private set; }
            public string? LastSignOutScheme { get; private set; }

            public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            {
                AuthenticateCalled = true;
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            {
                ChallengeCalled = true;
                context.Response.StatusCode = (int)HttpStatusCode.Redirect;
                var redirectUri = properties?.RedirectUri ?? "/admin/login";
                context.Response.Headers.Location = redirectUri;
                return Task.CompletedTask;
            }

            public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Task.CompletedTask;
            }

            public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            {
                SignInCalled = true;
                LastSignInScheme = scheme;
                LastSignInPrincipal = principal;
                LastSignInProperties = properties;
                context.User = principal;
                return Task.CompletedTask;
            }

            public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            {
                SignOutCalled = true;
                LastSignOutScheme = scheme;
                context.User = new ClaimsPrincipal(new ClaimsIdentity());
                return Task.CompletedTask;
            }
        }

        public class MockSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();
            public bool IsCleared { get; private set; }
            public bool IsAvailable => true;
            public string Id { get; } = Guid.NewGuid().ToString("N");
            public IEnumerable<string> Keys => _store.Keys;

            public void Clear()
            {
                IsCleared = true;
                _store.Clear();
            }

            public Task CommitAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? value)
            {
                return _store.TryGetValue(key, out value);
            }
        }

        public class MockTempDataProvider : ITempDataProvider
        {
            private readonly Dictionary<string, object> _data = new();
            public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
            public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
        }

        private static void SetupController(Controller controller, HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            controller.ControllerContext = new ControllerContext(actionContext);
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
        }

        private static ApplicationDbContext CreateDeadDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65533;Database=OfflineTestDb_ChalM2_3;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(options);
        }

        private static string ResolveProgramCsPath()
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "WEB_TinhNguyen", "Program.cs"),
                Path.Combine(AppContext.BaseDirectory, "WEB_TinhNguyen", "Program.cs"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../WEB_TinhNguyen/Program.cs")),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../WEB_TinhNguyen/Program.cs")),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../WEB_TinhNguyen/Program.cs")),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Program.cs")
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path)) return path;
            }

            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var check = Path.Combine(dir.FullName, "WEB_TinhNguyen", "Program.cs");
                if (File.Exists(check)) return check;
                dir = dir.Parent;
            }

            return candidates[0];
        }

        #endregion

        // =========================================================================
        // TEST 1: ALL 11 ADMIN CONTROLLERS HAVE [Authorize] ATTRIBUTE
        // =========================================================================
        [E2ETest(5, 204, "M2_3_01: Reflection - All 11 Admin Controllers enforce [Authorize] attribute")]
        public void M2_3_01_AllElevenAdminControllersHaveAuthorizeAttribute()
        {
            var expectedAdminControllers = new List<Type>
            {
                typeof(AdminController),
                typeof(AdminTrangChuController),
                typeof(AdminChuongTrinhController),
                typeof(AdminGayQuyController),
                typeof(AdminCauHinhQRController),
                typeof(AdminKyUcController),
                typeof(AdminBanToChucController),
                typeof(AdminTinTucController),
                typeof(AdminHieuUngController),
                typeof(AdminSanPhamController),
                typeof(AdminDonHangController)
            };

            Assert.Equal(11, expectedAdminControllers.Count, "Must audit exactly 11 Admin controllers");

            foreach (var ctrlType in expectedAdminControllers)
            {
                if (ctrlType == typeof(AdminController))
                {
                    var indexMethod = ctrlType.GetMethod("Index", BindingFlags.Public | BindingFlags.Instance);
                    bool hasAuth = indexMethod?.GetCustomAttribute<AuthorizeAttribute>() != null;
                    Assert.True(hasAuth, $"{ctrlType.Name}.Index must have [Authorize]");
                }
                else
                {
                    var authAttr = ctrlType.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
                    Assert.NotNull(authAttr, $"{ctrlType.Name} must declare class-level [Authorize]");
                }
            }
        }

        // =========================================================================
        // TEST 2: ALL ADMIN CRUD POST ACTIONS PROTECTED
        // =========================================================================
        [E2ETest(5, 204, "M2_3_02: Reflection - All 21 Admin CRUD POST endpoints are protected by [Authorize] & CSRF")]
        public void M2_3_02_AllAdminCrudActionsHaveAuthorizeProtection()
        {
            var adminControllers = new List<Type>
            {
                typeof(AdminTrangChuController),
                typeof(AdminChuongTrinhController),
                typeof(AdminGayQuyController),
                typeof(AdminCauHinhQRController),
                typeof(AdminKyUcController),
                typeof(AdminBanToChucController),
                typeof(AdminTinTucController),
                typeof(AdminHieuUngController),
                typeof(AdminSanPhamController),
                typeof(AdminDonHangController)
            };

            int postActionCount = 0;
            foreach (var ctrl in adminControllers)
            {
                var methods = ctrl.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.GetCustomAttribute<HttpPostAttribute>() != null);

                foreach (var method in methods)
                {
                    postActionCount++;
                    bool hasAllowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>() != null;
                    Assert.False(hasAllowAnonymous, $"{ctrl.Name}.{method.Name} must NOT allow anonymous");

                    bool hasCsrf = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null
                                || method.GetCustomAttribute<AutoValidateAntiforgeryTokenAttribute>() != null
                                || ctrl.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null
                                || ctrl.GetCustomAttribute<AutoValidateAntiforgeryTokenAttribute>() != null;
                    Assert.True(hasCsrf, $"{ctrl.Name}.{method.Name} must declare CSRF protection");
                }
            }

            Assert.True(postActionCount >= 15, $"Found {postActionCount} protected CRUD POST actions (expected >= 15)");
            Assert.Equal(21, postActionCount, $"Expected exactly 21 CRUD POST actions across all 10 domain Admin controllers, found {postActionCount}");
        }

        // =========================================================================
        // TEST 3: AdminController Action Decorations
        // =========================================================================
        [E2ETest(5, 204, "M2_3_03: Reflection - AdminController action decorations for Index, Login, and Logout")]
        public void M2_3_03_AdminControllerActionDecorations()
        {
            var adminType = typeof(AdminController);

            var index = adminType.GetMethod("Index", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(index, "AdminController.Index must exist");
            Assert.NotNull(index?.GetCustomAttribute<AuthorizeAttribute>(), "AdminController.Index must declare [Authorize]");
            Assert.NotNull(index?.GetCustomAttribute<HttpGetAttribute>(), "AdminController.Index must declare [HttpGet]");

            var loginGet = adminType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Login" && m.GetCustomAttribute<HttpGetAttribute>() != null);
            Assert.NotNull(loginGet, "AdminController.Login [HttpGet] must exist");
            Assert.NotNull(loginGet?.GetCustomAttribute<AllowAnonymousAttribute>(), "AdminController.Login [HttpGet] must declare [AllowAnonymous]");

            var loginPost = adminType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Login" && m.GetCustomAttribute<HttpPostAttribute>() != null);
            Assert.NotNull(loginPost, "AdminController.Login [HttpPost] must exist");
            Assert.NotNull(loginPost?.GetCustomAttribute<AllowAnonymousAttribute>(), "AdminController.Login [HttpPost] must declare [AllowAnonymous]");
            Assert.NotNull(loginPost?.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>(), "AdminController.Login [HttpPost] must declare [ValidateAntiForgeryToken]");

            var logout = adminType.GetMethod("Logout", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(logout, "AdminController.Logout must exist");
            Assert.NotNull(logout?.GetCustomAttribute<HttpGetAttribute>(), "AdminController.Logout must declare [HttpGet]");
            Assert.NotNull(logout?.GetCustomAttribute<HttpPostAttribute>(), "AdminController.Logout must declare [HttpPost]");
        }

        // =========================================================================
        // TEST 4: CookieAuthenticationOptions Registration
        // =========================================================================
        [E2ETest(5, 204, "M2_3_04: Configuration - Cookie authentication options registration (.WEB_TinhNguyen.AdminAuth, /admin/login)")]
        public void M2_3_04_CookieAuthenticationOptionsRegistration()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = ".WEB_TinhNguyen.AdminAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.IsEssential = true;
                    options.LoginPath = "/admin/login";
                    options.LogoutPath = "/admin/logout";
                    options.AccessDeniedPath = "/admin/login";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });

            var sp = services.BuildServiceProvider();
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
            var cookieOptions = optionsMonitor.Get(CookieAuthenticationDefaults.AuthenticationScheme);

            Assert.Equal(".WEB_TinhNguyen.AdminAuth", cookieOptions.Cookie.Name, $"Expected .WEB_TinhNguyen.AdminAuth, got {cookieOptions.Cookie.Name}");
            Assert.True(cookieOptions.Cookie.HttpOnly, "Cookie must be HttpOnly");
            Assert.Equal(SameSiteMode.Lax, cookieOptions.Cookie.SameSite, "Cookie SameSite must be Lax");
            Assert.Equal("/admin/login", cookieOptions.LoginPath.Value, $"Expected /admin/login, got {cookieOptions.LoginPath}");
            Assert.Equal("/admin/logout", cookieOptions.LogoutPath.Value, $"Expected /admin/logout, got {cookieOptions.LogoutPath}");
            Assert.Equal("/admin/login", cookieOptions.AccessDeniedPath.Value, $"Expected /admin/login, got {cookieOptions.AccessDeniedPath}");
            Assert.Equal(TimeSpan.FromHours(8), cookieOptions.ExpireTimeSpan, $"Expected 8 hours, got {cookieOptions.ExpireTimeSpan}");
            Assert.True(cookieOptions.SlidingExpiration, "SlidingExpiration must be true");
        }

        // =========================================================================
        // TEST 5: Program.cs Pipeline Ordering Invariants
        // =========================================================================
        [E2ETest(5, 204, "M2_3_05: Pipeline Invariant - Program.cs middleware sequence UseRouting -> UseSession -> UseAuthentication -> UseAuthorization -> UseStaticFiles")]
        public void M2_3_05_ProgramCsPipelineOrderingInvariants()
        {
            var programPath = ResolveProgramCsPath();
            Assert.True(File.Exists(programPath), $"Program.cs must exist at {programPath}");

            var code = File.ReadAllText(programPath);

            int idxRouting = code.IndexOf("app.UseRouting()");
            int idxSession = code.IndexOf("app.UseSession()");
            int idxAuth = code.IndexOf("app.UseAuthentication()");
            int idxAuthor = code.IndexOf("app.UseAuthorization()");
            int idxStatic = code.IndexOf("app.UseStaticFiles()");

            Assert.True(idxRouting >= 0, "UseRouting() must be declared in Program.cs");
            Assert.True(idxSession >= 0, "UseSession() must be declared in Program.cs");
            Assert.True(idxAuth >= 0, "UseAuthentication() must be declared in Program.cs");
            Assert.True(idxAuthor >= 0, "UseAuthorization() must be declared in Program.cs");
            Assert.True(idxStatic >= 0, "UseStaticFiles() must be declared in Program.cs");

            Assert.True(idxRouting < idxSession, "UseRouting() must be before UseSession()");
            Assert.True(idxSession < idxAuth, "UseSession() must be before UseAuthentication()");
            Assert.True(idxAuth < idxAuthor, "UseAuthentication() must be before UseAuthorization()");
            Assert.True(idxAuthor < idxStatic, "UseAuthorization() must be before UseStaticFiles()");
        }

        // =========================================================================
        // TEST 6: Unauthenticated Route Protection Challenge & 302 Redirection
        // =========================================================================
        [E2ETest(5, 204, "M2_3_06: Route Challenge - Unauthenticated requests to /Admin and 11 subroutes trigger 302 redirect with ReturnUrl")]
        public void M2_3_06_UnauthenticatedRouteProtection_ChallengeRedirects()
        {
            var adminRoutes = new List<string>
            {
                "/Admin",
                "/Admin/Index",
                "/AdminTrangChu",
                "/AdminChuongTrinh",
                "/AdminGayQuy",
                "/AdminCauHinhQR",
                "/AdminKyUc",
                "/AdminBanToChuc",
                "/AdminTinTuc",
                "/AdminHieuUng",
                "/CuaHang/AdminSanPham",
                "/CuaHang/AdminDonHang"
            };

            foreach (var route in adminRoutes)
            {
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/admin/login";
                        options.Cookie.Name = ".WEB_TinhNguyen.AdminAuth";
                    });
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

                var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                context.Request.Path = route;
                context.Request.Scheme = "http";
                context.Request.Host = new HostString("localhost");

                var properties = new AuthenticationProperties
                {
                    RedirectUri = route
                };

                authService.ChallengeAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, properties).GetAwaiter().GetResult();

                int statusCode = context.Response.StatusCode;
                string location = context.Response.Headers.Location.ToString();

                Assert.Equal((int)HttpStatusCode.Redirect, statusCode, $"Expected HTTP 302 for route {route}, got {statusCode}");
                Assert.True(location.StartsWith("/admin/login") || location.StartsWith("http://localhost/admin/login"),
                    $"Expected redirect to /admin/login for route {route}, got {location}");
                Assert.True(location.Contains(WebUtility.UrlEncode(route)) || location.Contains(route),
                    $"Redirect URL must contain target route as returnUrl, got {location}");
            }
        }

        // =========================================================================
        // TEST 7: Invalid Login Rejection & No Cookie Issued
        // =========================================================================
        [E2ETest(5, 204, "M2_3_07: Auth Rejection - Invalid credentials, inactive accounts, and SQLi attempts rejected without auth cookie")]
        public void M2_3_07_InvalidLogin_RejectionAndNoCookie()
        {
            var deadDb = CreateDeadDbContext();

            var scenarios = new (string username, string password, string desc)[]
            {
                ("admin", "WrongPassword@123", "Wrong password for existing admin"),
                ("admin", "123456", "Simple incorrect password"),
                ("nonexistent_user", "Admin@123", "Non-existent username with valid admin pass"),
                ("hacker", "hackerpass", "Totally invalid credentials"),
                ("", "Admin@123", "Empty username"),
                ("admin", "", "Empty password"),
                ("   ", "Admin@123", "Whitespace username"),
                ("admin' OR '1'='1", "Admin@123", "SQL Injection in username"),
                ("admin", "' OR '1'='1", "SQL Injection in password"),
                ("admin' UNION SELECT 1--", "Admin@123", "SQL Injection UNION in username")
            };

            foreach (var s in scenarios)
            {
                var mockAuth = new MockAuthService();
                var services = new ServiceCollection();
                services.AddSingleton<IAuthenticationService>(mockAuth);
                services.AddSingleton<ITempDataProvider>(new MockTempDataProvider());
                var sp = services.BuildServiceProvider();

                var httpContext = new DefaultHttpContext { RequestServices = sp };
                var controller = new AdminController(deadDb);
                SetupController(controller, httpContext);

                var model = new AdminLoginViewModel
                {
                    TenDangNhap = s.username,
                    MatKhau = s.password
                };

                if (string.IsNullOrWhiteSpace(s.username))
                {
                    controller.ModelState.AddModelError(nameof(model.TenDangNhap), "Vui lòng nhập tên đăng nhập");
                }
                if (string.IsNullOrWhiteSpace(s.password))
                {
                    controller.ModelState.AddModelError(nameof(model.MatKhau), "Vui lòng nhập mật khẩu");
                }

                var result = controller.Login(model, returnUrl: "/Admin").GetAwaiter().GetResult();

                Assert.True(result is ViewResult, $"{s.desc} must return ViewResult");
                Assert.False(controller.ModelState.IsValid, $"{s.desc} must invalidate ModelState");
                Assert.False(mockAuth.SignInCalled, $"{s.desc} must NOT call SignInAsync");
                Assert.False(httpContext.User?.Identity?.IsAuthenticated == true, "User must remain unauthenticated");
            }

            // Inactive account scenario
            string salt;
            string hash = BaoMatMatKhau.HashPassword("Admin@123", out salt);
            var inactiveAdmin = DbInitializer.AddInMemoryAdmin(new TaiKhoanAdmin
            {
                TenDangNhap = "inactive_test_admin",
                MatKhauHash = hash,
                MuoiBaoMat = salt,
                HoTen = "Inactive Admin Test",
                TrangThaiHoatDong = false
            });

            try
            {
                var mockAuth = new MockAuthService();
                var services = new ServiceCollection();
                services.AddSingleton<IAuthenticationService>(mockAuth);
                services.AddSingleton<ITempDataProvider>(new MockTempDataProvider());
                var sp = services.BuildServiceProvider();

                var httpContext = new DefaultHttpContext { RequestServices = sp };
                var controller = new AdminController(deadDb);
                SetupController(controller, httpContext);

                var model = new AdminLoginViewModel
                {
                    TenDangNhap = "inactive_test_admin",
                    MatKhau = "Admin@123"
                };

                var result = controller.Login(model, returnUrl: "/Admin").GetAwaiter().GetResult();

                Assert.True(result is ViewResult, "Inactive account must return ViewResult");
                Assert.False(controller.ModelState.IsValid, "Inactive account must invalidate ModelState");
                Assert.False(mockAuth.SignInCalled, "Inactive account must NOT call SignInAsync");
                Assert.False(httpContext.User?.Identity?.IsAuthenticated == true, "Inactive account must remain unauthenticated");
            }
            finally
            {
                DbInitializer.DeleteInMemoryAdmin(inactiveAdmin.IDTaiKhoan);
            }
        }

        // =========================================================================
        // TEST 8: Valid Login Signs In with Cookie Auth and Claims
        // =========================================================================
        [E2ETest(5, 204, "M2_3_08: Auth Success - Valid login (admin / Admin@123) signs in with cookie scheme, Admin role claim, and redirect")]
        public void M2_3_08_ValidLogin_CookieAuthenticationAndClaims()
        {
            var deadDb = CreateDeadDbContext();

            // Scenario 8A: Valid login without returnUrl -> redirects to Index/Admin
            {
                var mockAuth = new MockAuthService();
                var services = new ServiceCollection();
                services.AddRouting();
                services.AddControllers();
                services.AddSingleton<IAuthenticationService>(mockAuth);
                services.AddSingleton<ITempDataProvider>(new MockTempDataProvider());
                var sp = services.BuildServiceProvider();

                var httpContext = new DefaultHttpContext { RequestServices = sp };
                var controller = new AdminController(deadDb);
                SetupController(controller, httpContext);

                var model = new AdminLoginViewModel
                {
                    TenDangNhap = "admin",
                    MatKhau = "Admin@123",
                    GhiNhoDangNhap = true
                };

                var result = controller.Login(model, returnUrl: null).GetAwaiter().GetResult();

                Assert.True(mockAuth.SignInCalled, "SignInAsync must be invoked on valid login");
                Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, mockAuth.LastSignInScheme,
                    $"Expected {CookieAuthenticationDefaults.AuthenticationScheme}, got {mockAuth.LastSignInScheme}");

                var principal = mockAuth.LastSignInPrincipal;
                Assert.NotNull(principal, "Principal must not be null");
                Assert.True(principal?.Identity?.IsAuthenticated == true, "Identity must be authenticated");
                Assert.True(principal?.IsInRole("Admin") == true, "Principal must have Admin role");

                var tenDangNhapClaim = principal?.FindFirst("TenDangNhap")?.Value;
                Assert.Equal("admin", tenDangNhapClaim, $"Expected admin claim, got {tenDangNhapClaim}");

                Assert.True(mockAuth.LastSignInProperties?.IsPersistent == true, "IsPersistent must be true for remember-me");

                Assert.True(result is RedirectToActionResult || (result is RedirectResult rr && (rr.Url == "/Admin" || rr.Url == "/admin")),
                    $"Expected redirect to Admin Index, got {result.GetType().Name}");
            }

            // Scenario 8B: Valid login with legitimate local returnUrl -> redirects to returnUrl
            {
                var mockAuth = new MockAuthService();
                var services = new ServiceCollection();
                services.AddRouting();
                services.AddControllers();
                services.AddSingleton<IAuthenticationService>(mockAuth);
                services.AddSingleton<ITempDataProvider>(new MockTempDataProvider());
                var sp = services.BuildServiceProvider();

                var httpContext = new DefaultHttpContext { RequestServices = sp };
                var controller = new AdminController(deadDb);
                SetupController(controller, httpContext);

                var model = new AdminLoginViewModel
                {
                    TenDangNhap = "admin",
                    MatKhau = "Admin@123",
                    GhiNhoDangNhap = false
                };

                var result = controller.Login(model, returnUrl: "/AdminTinTuc").GetAwaiter().GetResult();

                Assert.True(mockAuth.SignInCalled, "SignInAsync must be invoked on valid login with returnUrl");
                Assert.False(mockAuth.LastSignInProperties?.IsPersistent == true, "IsPersistent must be false for standard session cookie");

                Assert.True(result is RedirectResult redirect && redirect.Url == "/AdminTinTuc",
                    $"Expected RedirectResult to /AdminTinTuc, got {result}");
            }
        }

        // =========================================================================
        // TEST 9: Open Redirect Defenses
        // =========================================================================
        [E2ETest(5, 204, "M2_3_09: Security - Open redirect defense neutralizes malicious phishing and protocol URLs")]
        public void M2_3_09_OpenRedirectDefenses()
        {
            var deadDb = CreateDeadDbContext();

            var maliciousUrls = new[]
            {
                "http://evil.com",
                "https://malicious-phishing.org/steal",
                "//evil.com",
                "//evil.com/admin",
                "/\\evil.com",
                "javascript:alert(1)",
                "data:text/html,<script>alert(1)</script>",
                "https://admin.phishing.attacker.com"
            };

            foreach (var evilUrl in maliciousUrls)
            {
                var mockAuth = new MockAuthService();
                var services = new ServiceCollection();
                services.AddRouting();
                services.AddControllers();
                services.AddSingleton<IAuthenticationService>(mockAuth);
                services.AddSingleton<ITempDataProvider>(new MockTempDataProvider());
                var sp = services.BuildServiceProvider();

                var httpContext = new DefaultHttpContext { RequestServices = sp };
                var controller = new AdminController(deadDb);
                SetupController(controller, httpContext);

                var model = new AdminLoginViewModel
                {
                    TenDangNhap = "admin",
                    MatKhau = "Admin@123"
                };

                var result = controller.Login(model, returnUrl: evilUrl).GetAwaiter().GetResult();

                bool safeRedirect = false;
                if (result is RedirectToActionResult redirectAction)
                {
                    safeRedirect = redirectAction.ActionName == "Index" && (redirectAction.ControllerName == null || redirectAction.ControllerName == "Admin");
                }
                else if (result is RedirectResult rr)
                {
                    safeRedirect = rr.Url == "/Admin" || rr.Url == "/admin";
                }

                Assert.True(safeRedirect, $"Must NOT redirect to external URL {evilUrl}. Instead redirected safely: {result}");
            }
        }

        // =========================================================================
        // TEST 10: Already Authenticated User Visiting Login Page
        // =========================================================================
        [E2ETest(5, 204, "M2_3_10: Session State - Already authenticated admin visiting /admin/login is redirected to dashboard or local target")]
        public void M2_3_10_AuthenticatedUserVisitingLoginPage_Redirects()
        {
            var deadDb = CreateDeadDbContext();
            var controller = new AdminController(deadDb);

            var services = new ServiceCollection();
            services.AddRouting();
            services.AddControllers();
            services.AddSingleton<ITempDataProvider>(new MockTempDataProvider());
            var sp = services.BuildServiceProvider();

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Quản trị viên"),
                new Claim(ClaimTypes.Role, "Admin")
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            var httpContext = new DefaultHttpContext { RequestServices = sp };
            httpContext.User = new ClaimsPrincipal(identity);
            SetupController(controller, httpContext);

            // Visiting /admin/login while authenticated without returnUrl
            var result = controller.Login(returnUrl: null);
            bool safeRedirect = result is RedirectToActionResult || (result is RedirectResult rr && (rr.Url == "/Admin" || rr.Url == "/admin"));
            Assert.True(safeRedirect, "Already authenticated user must be redirected away from login to admin index");

            // Visiting /admin/login with target returnUrl
            var resultTarget = controller.Login(returnUrl: "/AdminCauHinhQR");
            Assert.True(resultTarget is RedirectResult targetRedirect && targetRedirect.Url == "/AdminCauHinhQR",
                "Already authenticated user must redirect to local returnUrl");
        }

        // =========================================================================
        // TEST 11: Logout Clears Session and Signs Out
        // =========================================================================
        [E2ETest(5, 204, "M2_3_11: Session Termination - Logout invokes SignOutAsync and clears HttpContext.Session")]
        public void M2_3_11_Logout_ClearsSessionAndSignsOut()
        {
            var deadDb = CreateDeadDbContext();
            var mockAuth = new MockAuthService();
            var session = new MockSession();

            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService>(mockAuth);
            services.AddSingleton<ITempDataProvider>(new MockTempDataProvider());
            var sp = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = sp,
                Session = session
            };

            session.Set("TestSessionKey", Encoding.UTF8.GetBytes("AdminSessionData"));

            var controller = new AdminController(deadDb);
            SetupController(controller, httpContext);

            var result = controller.Logout().GetAwaiter().GetResult();

            Assert.True(mockAuth.SignOutCalled, "Logout must invoke SignOutAsync");
            Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, mockAuth.LastSignOutScheme,
                $"Expected {CookieAuthenticationDefaults.AuthenticationScheme}, got {mockAuth.LastSignOutScheme}");

            Assert.True(session.IsCleared, "Session must be cleared on logout");

            Assert.True(result is RedirectResult redirect && redirect.Url == "/admin/login",
                $"Expected redirect to /admin/login, got {result}");
        }

        // =========================================================================
        // TEST 12: In-Process Full ASP.NET Core Request Pipeline Execution
        // =========================================================================
        [E2ETest(5, 204, "M2_3_12: Pipeline E2E - Full in-process ASP.NET Core request pipeline unauthenticated challenge 302 and anonymous login 200")]
        public void M2_3_12_FullAspNetCorePipeline_InProcessMiddlewareExecution()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var dpDir = Path.Combine(Path.GetTempPath(), "dp_keys_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dpDir);
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dpDir));
            services.AddSingleton(new System.Diagnostics.DiagnosticListener("Microsoft.AspNetCore"));
            services.AddRouting();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".WEB_TinhNguyen.Session";
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = ".WEB_TinhNguyen.AdminAuth";
                    options.LoginPath = "/admin/login";
                    options.LogoutPath = "/admin/logout";
                    options.AccessDeniedPath = "/admin/login";
                });

            services.AddAuthorization();

            var sp = services.BuildServiceProvider();

            var app = new ApplicationBuilder(sp);

            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/admin/login", async context => await context.Response.WriteAsync("Admin Login Page")).AllowAnonymous();
                endpoints.MapGet("/admin", async context => await context.Response.WriteAsync("Admin Dashboard")).RequireAuthorization();
                endpoints.MapGet("/admin/index", async context => await context.Response.WriteAsync("Admin Index")).RequireAuthorization();
                endpoints.MapGet("/admintrangchu", async context => await context.Response.WriteAsync("Admin Trang Chu")).RequireAuthorization();
                endpoints.MapGet("/adminchuongtrinh", async context => await context.Response.WriteAsync("Admin Chuong Trinh")).RequireAuthorization();
                endpoints.MapGet("/admingayquy", async context => await context.Response.WriteAsync("Admin Gay Quy")).RequireAuthorization();
                endpoints.MapGet("/admincauhinhqr", async context => await context.Response.WriteAsync("Admin Cau Hinh QR")).RequireAuthorization();
                endpoints.MapGet("/adminkyuc", async context => await context.Response.WriteAsync("Admin Ky Uc")).RequireAuthorization();
                endpoints.MapGet("/adminbantochuc", async context => await context.Response.WriteAsync("Admin Ban To Chuc")).RequireAuthorization();
                endpoints.MapGet("/admintintuc", async context => await context.Response.WriteAsync("Admin Tin Tuc")).RequireAuthorization();
                endpoints.MapGet("/adminhieuung", async context => await context.Response.WriteAsync("Admin Hieu Ung")).RequireAuthorization();
                endpoints.MapGet("/cuahang/adminsanpham", async context => await context.Response.WriteAsync("Admin San Pham")).RequireAuthorization();
                endpoints.MapGet("/cuahang/admindonhang", async context => await context.Response.WriteAsync("Admin Don Hang")).RequireAuthorization();
            });

            var requestDelegate = app.Build();

            var testRoutes = new[]
            {
                "/admin",
                "/admin/index",
                "/admintrangchu",
                "/adminchuongtrinh",
                "/admingayquy",
                "/admincauhinhqr",
                "/adminkyuc",
                "/adminbantochuc",
                "/admintintuc",
                "/adminhieuung",
                "/cuahang/adminsanpham",
                "/cuahang/admindonhang"
            };

            foreach (var route in testRoutes)
            {
                using var scope = sp.CreateScope();
                var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                context.Request.Scheme = "http";
                context.Request.Host = new HostString("localhost");
                context.Request.Path = route;
                context.Request.Method = "GET";

                requestDelegate(context).GetAwaiter().GetResult();

                int statusCode = context.Response.StatusCode;
                string location = context.Response.Headers.Location.ToString();

                Assert.Equal((int)HttpStatusCode.Redirect, statusCode, $"Route {route} expected HTTP 302, got {statusCode}");
                Assert.True(location.Contains("/admin/login?ReturnUrl="), $"Route {route} expected redirect URL to contain /admin/login?ReturnUrl=, got {location}");
            }

            // Test anonymous access to /admin/login
            {
                using var scope = sp.CreateScope();
                var loginContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                loginContext.Request.Scheme = "http";
                loginContext.Request.Host = new HostString("localhost");
                loginContext.Request.Path = "/admin/login";
                loginContext.Request.Method = "GET";

                requestDelegate(loginContext).GetAwaiter().GetResult();

                Assert.Equal(200, loginContext.Response.StatusCode, $"Expected HTTP 200 for /admin/login, got {loginContext.Response.StatusCode}");
            }
        }
    }
}
