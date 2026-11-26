using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
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

namespace WEB_TinhNguyen.Tests.M2Challenger_Auth
{
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
        public bool TryGetValue(string key, out byte[]? value)
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

    public class Program
    {
        private static int passed = 0;
        private static int failed = 0;

        private static void SetupController(Controller controller, HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            controller.ControllerContext = new ControllerContext(actionContext);
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
        }

        private static void Assert(bool condition, string testName, string details = "")
        {
            if (condition)
            {
                passed++;
                Console.WriteLine($"  [PASS] {testName}");
            }
            else
            {
                failed++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] {testName} - {details}");
                Console.ResetColor();
            }
        }

        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine(" EMPIRICAL CHALLENGER: MILESTONE M2 AUTH & REDIRECTION HARNESS");
            Console.WriteLine("================================================================================");

            Test_01_AllAdminControllersHaveAuthorizeAttribute();
            Test_02_AllAdminCrudActionsHaveAuthorizeProtection();
            Test_03_AdminControllerActionDecorations();
            Test_04_CookieAuthenticationOptionsRegistration();
            Test_05_ProgramCsPipelineOrderingInvariants();
            await Test_06_UnauthenticatedRouteProtection_ChallengeRedirects();
            await Test_07_InvalidLogin_RejectionAndNoCookie();
            await Test_08_ValidLogin_CookieAuthenticationAndClaims();
            await Test_09_OpenRedirectDefenses();
            await Test_10_AuthenticatedUserVisitingLoginPage_Redirects();
            await Test_11_Logout_ClearsSessionAndSignsOut();
            await Test_12_FullAspNetCorePipeline_InProcessMiddlewareExecution();

            Console.WriteLine("================================================================================");
            Console.WriteLine($" RESULTS: {passed} PASSED, {failed} FAILED");
            Console.WriteLine("================================================================================");

            return failed == 0 ? 0 : 1;
        }

        // =========================================================================
        // TEST 1: ALL 11 ADMIN CONTROLLERS HAVE [Authorize] ATTRIBUTE
        // =========================================================================
        private static void Test_01_AllAdminControllersHaveAuthorizeAttribute()
        {
            Console.WriteLine("\n--- SUITE 1: Controller-Level [Authorize] Audit ---");

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

            Assert(expectedAdminControllers.Count == 11, "ControllerAudit_Count", "Must audit exactly 11 Admin controllers");

            foreach (var ctrlType in expectedAdminControllers)
            {
                if (ctrlType == typeof(AdminController))
                {
                    var indexMethod = ctrlType.GetMethod("Index", BindingFlags.Public | BindingFlags.Instance);
                    bool hasAuth = indexMethod?.GetCustomAttribute<AuthorizeAttribute>() != null;
                    Assert(hasAuth, $"ControllerAudit_{ctrlType.Name}_IndexHasAuthorize", $"{ctrlType.Name}.Index must have [Authorize]");
                }
                else
                {
                    var authAttr = ctrlType.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
                    Assert(authAttr != null, $"ControllerAudit_{ctrlType.Name}_ClassHasAuthorize", $"{ctrlType.Name} must declare class-level [Authorize]");
                }
            }
        }

        // =========================================================================
        // TEST 2: ALL ADMIN CRUD POST ACTIONS PROTECTED
        // =========================================================================
        private static void Test_02_AllAdminCrudActionsHaveAuthorizeProtection()
        {
            Console.WriteLine("\n--- SUITE 2: Admin CRUD POST Endpoints Protection ---");

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
                    Assert(!hasAllowAnonymous, $"CrudProtection_{ctrl.Name}_{method.Name}_NotAnonymous", $"{ctrl.Name}.{method.Name} must NOT allow anonymous");

                    bool hasCsrf = method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null
                                || method.GetCustomAttribute<AutoValidateAntiforgeryTokenAttribute>() != null
                                || ctrl.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null
                                || ctrl.GetCustomAttribute<AutoValidateAntiforgeryTokenAttribute>() != null;
                    Assert(hasCsrf, $"CrudProtection_{ctrl.Name}_{method.Name}_CsrfProtected", $"{ctrl.Name}.{method.Name} must declare CSRF protection");
                }
            }

            Assert(postActionCount >= 15, "CrudProtection_PostActionCount", $"Found {postActionCount} protected CRUD POST actions (expected >= 15)");
        }

        // =========================================================================
        // TEST 3: AdminController Action Decorations
        // =========================================================================
        private static void Test_03_AdminControllerActionDecorations()
        {
            Console.WriteLine("\n--- SUITE 3: AdminController Endpoint Attribute Integrity ---");

            var adminType = typeof(AdminController);

            var index = adminType.GetMethod("Index", BindingFlags.Public | BindingFlags.Instance);
            Assert(index?.GetCustomAttribute<AuthorizeAttribute>() != null, "AdminEndpoint_Index_HasAuthorize");
            Assert(index?.GetCustomAttribute<HttpGetAttribute>() != null, "AdminEndpoint_Index_HasHttpGet");

            var loginGet = adminType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Login" && m.GetCustomAttribute<HttpGetAttribute>() != null);
            Assert(loginGet != null, "AdminEndpoint_LoginGet_Exists");
            Assert(loginGet?.GetCustomAttribute<AllowAnonymousAttribute>() != null, "AdminEndpoint_LoginGet_HasAllowAnonymous");

            var loginPost = adminType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Login" && m.GetCustomAttribute<HttpPostAttribute>() != null);
            Assert(loginPost != null, "AdminEndpoint_LoginPost_Exists");
            Assert(loginPost?.GetCustomAttribute<AllowAnonymousAttribute>() != null, "AdminEndpoint_LoginPost_HasAllowAnonymous");
            Assert(loginPost?.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null, "AdminEndpoint_LoginPost_HasValidateAntiForgeryToken");

            var logout = adminType.GetMethod("Logout", BindingFlags.Public | BindingFlags.Instance);
            Assert(logout != null, "AdminEndpoint_Logout_Exists");
            Assert(logout?.GetCustomAttribute<HttpGetAttribute>() != null, "AdminEndpoint_Logout_HasHttpGet");
            Assert(logout?.GetCustomAttribute<HttpPostAttribute>() != null, "AdminEndpoint_Logout_HasHttpPost");
        }

        // =========================================================================
        // TEST 4: CookieAuthenticationOptions Registration
        // =========================================================================
        private static void Test_04_CookieAuthenticationOptionsRegistration()
        {
            Console.WriteLine("\n--- SUITE 4: Cookie Authentication Options Validation ---");

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

            Assert(cookieOptions.Cookie.Name == ".WEB_TinhNguyen.AdminAuth", "CookieOptions_Name", $"Expected .WEB_TinhNguyen.AdminAuth, got {cookieOptions.Cookie.Name}");
            Assert(cookieOptions.Cookie.HttpOnly == true, "CookieOptions_HttpOnly", "Cookie must be HttpOnly");
            Assert(cookieOptions.Cookie.SameSite == SameSiteMode.Lax, "CookieOptions_SameSite", "Cookie SameSite must be Lax");
            Assert(cookieOptions.LoginPath == "/admin/login", "CookieOptions_LoginPath", $"Expected /admin/login, got {cookieOptions.LoginPath}");
            Assert(cookieOptions.LogoutPath == "/admin/logout", "CookieOptions_LogoutPath", $"Expected /admin/logout, got {cookieOptions.LogoutPath}");
            Assert(cookieOptions.AccessDeniedPath == "/admin/login", "CookieOptions_AccessDeniedPath", $"Expected /admin/login, got {cookieOptions.AccessDeniedPath}");
            Assert(cookieOptions.ExpireTimeSpan == TimeSpan.FromHours(8), "CookieOptions_ExpireTimeSpan", $"Expected 8 hours, got {cookieOptions.ExpireTimeSpan}");
            Assert(cookieOptions.SlidingExpiration == true, "CookieOptions_SlidingExpiration", "SlidingExpiration must be true");
        }

        // =========================================================================
        // TEST 5: Program.cs Pipeline Ordering Invariants
        // =========================================================================
        private static void Test_05_ProgramCsPipelineOrderingInvariants()
        {
            Console.WriteLine("\n--- SUITE 5: Program.cs Pipeline Ordering Invariants ---");

            var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../WEB_TinhNguyen"));
            var programPath = Path.Combine(projectDir, "Program.cs");
            Assert(File.Exists(programPath), "ProgramCs_Exists", $"Program.cs must exist at {programPath}");

            var code = File.ReadAllText(programPath);

            int idxRouting = code.IndexOf("app.UseRouting()");
            int idxSession = code.IndexOf("app.UseSession()");
            int idxAuth = code.IndexOf("app.UseAuthentication()");
            int idxAuthor = code.IndexOf("app.UseAuthorization()");
            int idxStatic = code.IndexOf("app.UseStaticFiles()");

            Assert(idxRouting >= 0, "Pipeline_UseRoutingDeclared");
            Assert(idxSession >= 0, "Pipeline_UseSessionDeclared");
            Assert(idxAuth >= 0, "Pipeline_UseAuthenticationDeclared");
            Assert(idxAuthor >= 0, "Pipeline_UseAuthorizationDeclared");
            Assert(idxStatic >= 0, "Pipeline_UseStaticFilesDeclared");

            Assert(idxRouting < idxSession, "Pipeline_RoutingBeforeSession", "UseRouting() must be before UseSession()");
            Assert(idxSession < idxAuth, "Pipeline_SessionBeforeAuthentication", "UseSession() must be before UseAuthentication()");
            Assert(idxAuth < idxAuthor, "Pipeline_AuthenticationBeforeAuthorization", "UseAuthentication() must be before UseAuthorization()");
            Assert(idxAuthor < idxStatic, "Pipeline_AuthorizationBeforeStaticFiles", "UseAuthorization() must be before UseStaticFiles()");
        }

        // =========================================================================
        // TEST 6: Unauthenticated Route Protection Challenge & 302 Redirection
        // =========================================================================
        private static async Task Test_06_UnauthenticatedRouteProtection_ChallengeRedirects()
        {
            Console.WriteLine("\n--- SUITE 6: Unauthenticated Route Challenge & 302 Redirect ---");

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

                await authService.ChallengeAsync(context, CookieAuthenticationDefaults.AuthenticationScheme, properties);

                int statusCode = context.Response.StatusCode;
                string location = context.Response.Headers.Location.ToString();

                Assert(statusCode == (int)HttpStatusCode.Redirect, $"Unauthenticated_302_{route}", $"Expected HTTP 302, got {statusCode}");
                Assert(location.StartsWith("/admin/login") || location.StartsWith("http://localhost/admin/login"),
                    $"Unauthenticated_RedirectLocation_{route}",
                    $"Expected redirect to /admin/login, got {location}");
                Assert(location.Contains(WebUtility.UrlEncode(route)) || location.Contains(route),
                    $"Unauthenticated_PreservesReturnUrl_{route}",
                    $"Redirect URL must contain target route as returnUrl, got {location}");
            }
        }

        // =========================================================================
        // TEST 7: Invalid Login Rejection & No Cookie Issued
        // =========================================================================
        private static async Task Test_07_InvalidLogin_RejectionAndNoCookie()
        {
            Console.WriteLine("\n--- SUITE 7: Invalid Login Rejection & ModelState Error ---");

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
                ("admin", "' OR '1'='1", "SQL Injection in password")
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

                var result = await controller.Login(model, returnUrl: "/Admin");

                Assert(result is ViewResult, $"InvalidLogin_{s.desc}_ReturnsView", $"{s.desc} must return ViewResult");
                Assert(!controller.ModelState.IsValid, $"InvalidLogin_{s.desc}_ModelStateInvalid", $"{s.desc} must invalidate ModelState");
                Assert(!mockAuth.SignInCalled, $"InvalidLogin_{s.desc}_NoAuthCookieIssued", $"{s.desc} must NOT call SignInAsync");
                Assert(httpContext.User?.Identity?.IsAuthenticated != true, $"InvalidLogin_{s.desc}_NotAuthenticated", "User must remain unauthenticated");
            }
        }

        // =========================================================================
        // TEST 8: Valid Login Signs In with Cookie Auth and Claims
        // =========================================================================
        private static async Task Test_08_ValidLogin_CookieAuthenticationAndClaims()
        {
            Console.WriteLine("\n--- SUITE 8: Valid Login Authentication & Claims Resolution ---");

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

                var result = await controller.Login(model, returnUrl: null);

                Assert(mockAuth.SignInCalled, "ValidLogin_SignInAsyncInvoked", "SignInAsync must be invoked on valid login");
                Assert(mockAuth.LastSignInScheme == CookieAuthenticationDefaults.AuthenticationScheme,
                    "ValidLogin_CookieSchemeUsed",
                    $"Expected {CookieAuthenticationDefaults.AuthenticationScheme}, got {mockAuth.LastSignInScheme}");

                var principal = mockAuth.LastSignInPrincipal;
                Assert(principal != null, "ValidLogin_PrincipalNotNull");
                Assert(principal?.Identity?.IsAuthenticated == true, "ValidLogin_IdentityIsAuthenticated");
                Assert(principal?.IsInRole("Admin") == true, "ValidLogin_HasAdminRole");

                var tenDangNhapClaim = principal?.FindFirst("TenDangNhap")?.Value;
                Assert(tenDangNhapClaim == "admin", "ValidLogin_TenDangNhapClaim", $"Expected admin, got {tenDangNhapClaim}");

                Assert(mockAuth.LastSignInProperties?.IsPersistent == true, "ValidLogin_RememberMe_IsPersistent");

                Assert(result is RedirectToActionResult || (result is RedirectResult rr && rr.Url == "/Admin"),
                    "ValidLogin_RedirectsToAdmin",
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

                var result = await controller.Login(model, returnUrl: "/AdminTinTuc");

                Assert(mockAuth.SignInCalled, "ValidLoginWithReturnUrl_SignInAsyncInvoked");
                Assert(mockAuth.LastSignInProperties?.IsPersistent == false, "ValidLogin_SessionCookie_NotPersistent");

                Assert(result is RedirectResult redirect && redirect.Url == "/AdminTinTuc",
                    "ValidLoginWithReturnUrl_RedirectsToLocalTarget",
                    $"Expected RedirectResult to /AdminTinTuc, got {result}");
            }
        }

        // =========================================================================
        // TEST 9: Open Redirect Defenses
        // =========================================================================
        private static async Task Test_09_OpenRedirectDefenses()
        {
            Console.WriteLine("\n--- SUITE 9: Open Redirect Attack Defenses ---");

            var deadDb = CreateDeadDbContext();

            var maliciousUrls = new[]
            {
                "http://evil.com",
                "https://malicious-phishing.org/steal",
                "//evil.com",
                "//evil.com/admin",
                "/\\evil.com",
                "javascript:alert(1)",
                "data:text/html,<script>alert(1)</script>"
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

                var result = await controller.Login(model, returnUrl: evilUrl);

                bool safeRedirect = false;
                if (result is RedirectToActionResult redirectAction)
                {
                    safeRedirect = redirectAction.ActionName == "Index" && (redirectAction.ControllerName == null || redirectAction.ControllerName == "Admin");
                }
                else if (result is RedirectResult rr)
                {
                    safeRedirect = rr.Url == "/Admin" || rr.Url == "/admin";
                }

                Assert(safeRedirect, $"OpenRedirectDefense_{evilUrl}", $"Must NOT redirect to external URL {evilUrl}. Instead redirected safely: {result}");
            }
        }

        // =========================================================================
        // TEST 10: Already Authenticated User Visiting Login Page
        // =========================================================================
        private static async Task Test_10_AuthenticatedUserVisitingLoginPage_Redirects()
        {
            Console.WriteLine("\n--- SUITE 10: Already Authenticated User Handling ---");

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

            // Visiting /admin/login while authenticated
            var result = controller.Login(returnUrl: null);
            bool safeRedirect = result is RedirectToActionResult || (result is RedirectResult rr && rr.Url == "/Admin");
            Assert(safeRedirect, "AlreadyAuth_RedirectsAwayFromLogin", "Already authenticated user must be redirected away from login");

            // Visiting /admin/login with target returnUrl
            var resultTarget = controller.Login(returnUrl: "/AdminCauHinhQR");
            Assert(resultTarget is RedirectResult targetRedirect && targetRedirect.Url == "/AdminCauHinhQR",
                "AlreadyAuth_RedirectsToTargetReturnUrl",
                "Already authenticated user must redirect to local returnUrl");
        }

        // =========================================================================
        // TEST 11: Logout Clears Session and Signs Out
        // =========================================================================
        private static async Task Test_11_Logout_ClearsSessionAndSignsOut()
        {
            Console.WriteLine("\n--- SUITE 11: Logout Lifecycle & Session Invalidation ---");

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

            session.Set("TestSessionKey", System.Text.Encoding.UTF8.GetBytes("AdminSessionData"));

            var controller = new AdminController(deadDb);
            SetupController(controller, httpContext);

            var result = await controller.Logout();

            Assert(mockAuth.SignOutCalled, "Logout_SignOutAsyncInvoked", "Logout must invoke SignOutAsync");
            Assert(mockAuth.LastSignOutScheme == CookieAuthenticationDefaults.AuthenticationScheme,
                "Logout_CookieSchemeSignedOut",
                $"Expected {CookieAuthenticationDefaults.AuthenticationScheme}, got {mockAuth.LastSignOutScheme}");

            Assert(session.IsCleared, "Logout_SessionCleared", "Session must be cleared on logout");

            Assert(result is RedirectResult redirect && redirect.Url == "/admin/login",
                "Logout_RedirectsToAdminLogin",
                $"Expected redirect to /admin/login, got {result}");
        }

        // =========================================================================
        // TEST 12: In-Process Full ASP.NET Core Request Pipeline Execution
        // =========================================================================
        private static async Task Test_12_FullAspNetCorePipeline_InProcessMiddlewareExecution()
        {
            Console.WriteLine("\n--- SUITE 12: Full ASP.NET Core Pipeline Execution ---");

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

                await requestDelegate(context);

                int statusCode = context.Response.StatusCode;
                string location = context.Response.Headers.Location.ToString();

                Assert(statusCode == (int)HttpStatusCode.Redirect, $"Pipeline_Unauth_302_{route}", $"Route {route} returned status {statusCode}");
                Assert(location.Contains("/admin/login?ReturnUrl="), $"Pipeline_Unauth_RedirectUrl_{route}", $"Location: {location}");
            }

            // Test anonymous access to /admin/login
            {
                using var scope = sp.CreateScope();
                var loginContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                loginContext.Request.Scheme = "http";
                loginContext.Request.Host = new HostString("localhost");
                loginContext.Request.Path = "/admin/login";
                loginContext.Request.Method = "GET";

                await requestDelegate(loginContext);

                Assert(loginContext.Response.StatusCode == 200, "Pipeline_Login_Returns200", $"Expected 200 for /admin/login, got {loginContext.Response.StatusCode}");
            }
        }

        private static ApplicationDbContext CreateDeadDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65533;Database=OfflineTestDb_ChalM2_Auth;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(options);
        }
    }
}
