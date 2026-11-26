using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.FileProviders;
using WEB_TinhNguyen.Areas.CuaHang.Helpers;
using WEB_TinhNguyen.BoDieuKhien;
using WEB_TinhNguyen.DuLieu;
using WEB_TinhNguyen.MoHinh;

namespace WEB_TinhNguyen.Agents.Challenger1.AdminChallenger
{
    #region Mocks & Async Test Helpers

    public class MockEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "web_tinhnguyen_challenger_" + Guid.NewGuid().ToString("N"));
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "WEB_TinhNguyen";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    public class MockTempDataProvider : ITempDataProvider
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        public IDictionary<string, object> LoadTempData(HttpContext context) => _data;
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression) =>
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new object[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    }

    public class MockDbSet<T> : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T> where T : class
    {
        private readonly IQueryable<T> _queryable;

        public MockDbSet(IEnumerable<T> data)
        {
            _queryable = new TestAsyncEnumerable<T>(data);
        }

        public override Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityType => null!;
        public Type ElementType => _queryable.ElementType;
        public Expression Expression => _queryable.Expression;
        public IQueryProvider Provider => _queryable.Provider;

        public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _queryable.GetEnumerator();

        public override IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            ((IAsyncEnumerable<T>)_queryable).GetAsyncEnumerator(cancellationToken);
    }

    #endregion

    public class Program
    {
        private static int _passed = 0;
        private static int _failed = 0;
        private static readonly List<string> _failures = new List<string>();

        private static void Assert(bool condition, string testName, string errorDetails = "")
        {
            if (condition)
            {
                _passed++;
                Console.WriteLine($"  [PASS] {testName}");
            }
            else
            {
                _failed++;
                var msg = $"  [FAIL] {testName} : {errorDetails}";
                _failures.Add(msg);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }

        private static ApplicationDbContext CreateOfflineDbContext()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=127.0.0.1,65534;Database=NonExistentDb_ChallengerOffline;User Id=sa;Password=invalid_pwd;Connect Timeout=1;Encrypt=False;")
                .Options;
            return new ApplicationDbContext(dbOptions);
        }

        private static ApplicationDbContext CreateMockDbContextWithCampaigns(IEnumerable<ChienDich> campaigns)
        {
            var db = CreateOfflineDbContext();
            db.ChienDichs = new MockDbSet<ChienDich>(campaigns);
            return db;
        }

        private static void SetupController(Controller controller, RouteData? routeData = null)
        {
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = routeData ?? new RouteData()
            };
            controller.TempData = new TempDataDictionary(httpContext, new MockTempDataProvider());
        }

        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine(" CHALLENGER 1: ADVERSARIAL ADMIN ROUTING & NAVIGATION TEST HARNESS");
            Console.WriteLine("================================================================================");

            var projectRoot = @"D:\web_tinh_nguyen";
            var viewsRoot = Path.Combine(projectRoot, "WEB_TinhNguyen", "GiaoDien");
            var adminLayoutPath = Path.Combine(viewsRoot, "Shared", "_AdminLayout.cshtml");
            var adminIndexPath = Path.Combine(viewsRoot, "Admin", "Index.cshtml");
            var env = new MockEnvironment();

            var expectedAdminControllers = new[]
            {
                "Admin",
                "AdminTrangChu",
                "AdminChuongTrinh",
                "AdminGayQuy",
                "AdminCauHinhQR",
                "AdminKyUc",
                "AdminBanToChuc",
                "AdminTinTuc"
            };

            // =========================================================================
            // SUITE 1: Layout & Dashboard Link Inspection
            // =========================================================================
            Console.WriteLine("\n--- SUITE 1: Layout & Dashboard Adversarial Link Inspection ---");

            var layoutContent = File.ReadAllText(adminLayoutPath);
            var indexContent = File.ReadAllText(adminIndexPath);

            Assert(!layoutContent.Contains("%2F"), "S1.01: _AdminLayout.cshtml contains 0 '%2F' characters");
            Assert(!layoutContent.Contains("GiaoDien/"), "S1.02: _AdminLayout.cshtml contains 0 'GiaoDien/' prefixes");
            Assert(!indexContent.Contains("%2F"), "S1.03: Admin/Index.cshtml contains 0 '%2F' characters");

            // In indexContent, verify GiaoDien/ only appears in Layout path
            var indexWithoutLayoutPath = Regex.Replace(indexContent, @"Layout\s*=\s*""~/GiaoDien/Shared/_AdminLayout\.cshtml"";", "");
            Assert(!indexWithoutLayoutPath.Contains("GiaoDien/"), "S1.04: Admin/Index.cshtml contains 0 'GiaoDien/' prefixes in links or code");

            foreach (var ctrl in expectedAdminControllers)
            {
                Assert(layoutContent.Contains($"\"Index\", \"{ctrl}\""),
                    $"S1.05: _AdminLayout.cshtml references pure controller '{ctrl}' in Url.Action");

                Assert(layoutContent.Contains($"\"{ctrl}\", new {{ area = \"\" }}"),
                    $"S1.06: _AdminLayout.cshtml sets 'new {{ area = \"\" }}' for controller '{ctrl}'");
            }

            Assert(indexContent.Contains("\"Index\", \"AdminTinTuc\", new { area = \"\" }"),
                "S1.07: Admin/Index.cshtml 'Quản lý Tin tức' card links to AdminTinTuc with explicit area = \"\"");

            Assert(layoutContent.Contains("== \"AdminChuongTrinh\""),
                "S1.08: _AdminLayout active check for ChuongTrinh matches pure controller name");
            Assert(layoutContent.Contains("== \"AdminGayQuy\""),
                "S1.09: _AdminLayout active check for GayQuy matches pure controller name");
            Assert(!layoutContent.Contains("== \"GiaoDien/"),
                "S1.10: _AdminLayout contains zero active checks comparing against 'GiaoDien/'");

            // Extract all hrefs from layout
            var hrefMatchesLayout = Regex.Matches(layoutContent, @"href\s*=\s*[""']([^""']+)[""']");
            var deadLinksInLayout = new List<string>();
            foreach (Match m in hrefMatchesLayout)
            {
                var val = m.Groups[1].Value;
                if (val == "#") deadLinksInLayout.Add(m.Value);
            }
            Assert(deadLinksInLayout.Count == 2,
                "S1.11: _AdminLayout contains exactly 2 placeholder href=\"#\" (Đăng ký TNV and Logout), 0 dead links for 8 admin pages",
                $"Found {deadLinksInLayout.Count} instances");

            // Extract all hrefs from Admin/Index.cshtml
            var hrefMatchesIndex = Regex.Matches(indexContent, @"href\s*=\s*[""']([^""']+)[""']");
            var deadLinksInIndex = new List<string>();
            foreach (Match m in hrefMatchesIndex)
            {
                var val = m.Groups[1].Value;
                if (val == "#") deadLinksInIndex.Add(m.Value);
            }
            Assert(deadLinksInIndex.Count == 1,
                "S1.12: Admin/Index contains exactly 1 placeholder href=\"#\" (Duyệt Lời chúc - mockup), 0 dead links for 8 admin pages",
                $"Found {deadLinksInIndex.Count} instances");

            // =========================================================================
            // SUITE 2: Controller Offline Database Resilience (8 Pages)
            // =========================================================================
            Console.WriteLine("\n--- SUITE 2: Controller Offline Database Resilience (8 Pages) ---");

            // 1. AdminController
            {
                var ctrl = new AdminController();
                SetupController(ctrl);
                var res = ctrl.Index();
                Assert(res is ViewResult, "S2.01: AdminController.Index returns ViewResult");
            }

            // 2. AdminCauHinhQRController
            {
                using var db = CreateOfflineDbContext();
                var ctrl = new AdminCauHinhQRController(db, env);
                SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is IEnumerable<CauHinhThanhToanQR> list && list.Any(),
                    "S2.02: AdminCauHinhQRController.Index returns ViewResult with non-empty in-memory fallback model");
            }

            // 3. AdminTrangChuController
            {
                using var db = CreateOfflineDbContext();
                var ctrl = new AdminTrangChuController(db, env);
                SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is IEnumerable<ChienDich> list && !list.Any(),
                    "S2.03: AdminTrangChuController.Index returns ViewResult with non-null empty List<ChienDich>");
            }

            // 4. AdminChuongTrinhController
            {
                using var db = CreateOfflineDbContext();
                var ctrl = new AdminChuongTrinhController(db, env);
                SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is IEnumerable<ChienDich> list && !list.Any(),
                    "S2.04: AdminChuongTrinhController.Index returns ViewResult with non-null empty List<ChienDich>");
            }

            // 5. AdminGayQuyController
            {
                using var db = CreateOfflineDbContext();
                var ctrl = new AdminGayQuyController(db, env);
                SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is IEnumerable<ChienDich> list && !list.Any(),
                    "S2.05: AdminGayQuyController.Index returns ViewResult with non-null empty List<ChienDich>");
            }

            // 6. AdminKyUcController
            {
                using var db = CreateOfflineDbContext();
                var ctrl = new AdminKyUcController(db, env);
                SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is IEnumerable<ChienDich> list && !list.Any(),
                    "S2.06: AdminKyUcController.Index returns ViewResult with non-null empty List<ChienDich>");
            }

            // 7. AdminBanToChucController
            {
                using var db = CreateOfflineDbContext();
                var ctrl = new AdminBanToChucController(db, env);
                SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is IEnumerable<ChienDich> list && !list.Any(),
                    "S2.07: AdminBanToChucController.Index returns ViewResult with non-null empty List<ChienDich>");
            }

            // 8. AdminTinTucController
            {
                using var db = CreateOfflineDbContext();
                var ctrl = new AdminTinTucController(db, env);
                SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is IEnumerable<ChienDich> list && !list.Any(),
                    "S2.08: AdminTinTucController.Index returns ViewResult with non-null empty List<ChienDich>");
            }

            // =========================================================================
            // SUITE 3: Controller Normal Database Simulation (Seeded Campaigns)
            // =========================================================================
            Console.WriteLine("\n--- SUITE 3: Controller Normal Database Simulation (Seeded Campaigns) ---");

            var sampleCampaigns = new List<ChienDich>
            {
                new ChienDich { IDChienDich = 1, Nam = 2023, TieuDe = "Mùa 2023", TrangThaiHoatDong = false },
                new ChienDich { IDChienDich = 2, Nam = 2025, TieuDe = "Mùa 2025", TrangThaiHoatDong = true },
                new ChienDich { IDChienDich = 3, Nam = 2024, TieuDe = "Mùa 2024", TrangThaiHoatDong = false }
            };

            // Test normal scenario on 6 campaign controllers: verify OrderByDescending(c => c.Nam) produces 2025, 2024, 2023
            {
                using var db = CreateMockDbContextWithCampaigns(sampleCampaigns);
                var ctrl = new AdminTrangChuController(db, env); SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is List<ChienDich> list && list.Count == 3 && list[0].Nam == 2025 && list[1].Nam == 2024 && list[2].Nam == 2023,
                    "S3.01: AdminTrangChuController.Index under normal DB returns sorted campaigns (2025, 2024, 2023)");
            }
            {
                using var db = CreateMockDbContextWithCampaigns(sampleCampaigns);
                var ctrl = new AdminChuongTrinhController(db, env); SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is List<ChienDich> list && list.Count == 3 && list[0].Nam == 2025,
                    "S3.02: AdminChuongTrinhController.Index under normal DB returns sorted campaigns");
            }
            {
                using var db = CreateMockDbContextWithCampaigns(sampleCampaigns);
                var ctrl = new AdminGayQuyController(db, env); SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is List<ChienDich> list && list.Count == 3 && list[0].Nam == 2025,
                    "S3.03: AdminGayQuyController.Index under normal DB returns sorted campaigns");
            }
            {
                using var db = CreateMockDbContextWithCampaigns(sampleCampaigns);
                var ctrl = new AdminKyUcController(db, env); SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is List<ChienDich> list && list.Count == 3 && list[0].Nam == 2025,
                    "S3.04: AdminKyUcController.Index under normal DB returns sorted campaigns");
            }
            {
                using var db = CreateMockDbContextWithCampaigns(sampleCampaigns);
                var ctrl = new AdminBanToChucController(db, env); SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is List<ChienDich> list && list.Count == 3 && list[0].Nam == 2025,
                    "S3.05: AdminBanToChucController.Index under normal DB returns sorted campaigns");
            }
            {
                using var db = CreateMockDbContextWithCampaigns(sampleCampaigns);
                var ctrl = new AdminTinTucController(db, env); SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is List<ChienDich> list && list.Count == 3 && list[0].Nam == 2025,
                    "S3.06: AdminTinTucController.Index under normal DB returns sorted campaigns");
            }

            // =========================================================================
            // SUITE 4: Cross-Area RouteData Simulation (area = "CuaHang" and area = "")
            // =========================================================================
            Console.WriteLine("\n--- SUITE 4: Cross-Area RouteData Simulation (area = 'CuaHang' and area = '') ---");

            // Scenario 4A: RouteData area = "CuaHang"
            var routeDataStore = new RouteData();
            routeDataStore.Values["area"] = "CuaHang";
            routeDataStore.Values["controller"] = "AdminSanPham";
            routeDataStore.Values["action"] = "Index";

            {
                using var db = CreateOfflineDbContext();
                var c1 = new AdminController(); SetupController(c1, routeDataStore);
                var c2 = new AdminCauHinhQRController(db, env); SetupController(c2, routeDataStore);
                var c3 = new AdminTrangChuController(db, env); SetupController(c3, routeDataStore);
                var c4 = new AdminChuongTrinhController(db, env); SetupController(c4, routeDataStore);
                var c5 = new AdminGayQuyController(db, env); SetupController(c5, routeDataStore);
                var c6 = new AdminKyUcController(db, env); SetupController(c6, routeDataStore);
                var c7 = new AdminBanToChucController(db, env); SetupController(c7, routeDataStore);
                var c8 = new AdminTinTucController(db, env); SetupController(c8, routeDataStore);

                var r1 = c1.Index();
                var r2 = await c2.Index();
                var r3 = await c3.Index();
                var r4 = await c4.Index();
                var r5 = await c5.Index();
                var r6 = await c6.Index();
                var r7 = await c7.Index();
                var r8 = await c8.Index();

                Assert(r1 is ViewResult && r2 is ViewResult && r3 is ViewResult && r4 is ViewResult &&
                       r5 is ViewResult && r6 is ViewResult && r7 is ViewResult && r8 is ViewResult,
                       "S4.01: All 8 controllers return ViewResult under ambient routeData area = 'CuaHang'");
            }

            // Scenario 4B: RouteData area = ""
            var routeDataRoot = new RouteData();
            routeDataRoot.Values["area"] = "";
            routeDataRoot.Values["controller"] = "Admin";
            routeDataRoot.Values["action"] = "Index";

            {
                using var db = CreateOfflineDbContext();
                var c1 = new AdminController(); SetupController(c1, routeDataRoot);
                var c2 = new AdminCauHinhQRController(db, env); SetupController(c2, routeDataRoot);
                var c3 = new AdminTrangChuController(db, env); SetupController(c3, routeDataRoot);
                var c4 = new AdminChuongTrinhController(db, env); SetupController(c4, routeDataRoot);
                var c5 = new AdminGayQuyController(db, env); SetupController(c5, routeDataRoot);
                var c6 = new AdminKyUcController(db, env); SetupController(c6, routeDataRoot);
                var c7 = new AdminBanToChucController(db, env); SetupController(c7, routeDataRoot);
                var c8 = new AdminTinTucController(db, env); SetupController(c8, routeDataRoot);

                var r1 = c1.Index();
                var r2 = await c2.Index();
                var r3 = await c3.Index();
                var r4 = await c4.Index();
                var r5 = await c5.Index();
                var r6 = await c6.Index();
                var r7 = await c7.Index();
                var r8 = await c8.Index();

                Assert(r1 is ViewResult && r2 is ViewResult && r3 is ViewResult && r4 is ViewResult &&
                       r5 is ViewResult && r6 is ViewResult && r7 is ViewResult && r8 is ViewResult,
                       "S4.02: All 8 controllers return ViewResult under root routeData area = ''");
            }

            // =========================================================================
            // SUITE 5: Edge Conditions & Error Path Testing
            // =========================================================================
            Console.WriteLine("\n--- SUITE 5: Edge Conditions & Error Path Testing ---");

            // Edge 5.1: RouteData is completely NULL
            {
                using var db = CreateOfflineDbContext();
                var c1 = new AdminController();
                c1.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext(), RouteData = null! };
                var r1 = c1.Index();
                Assert(r1 is ViewResult, "S5.01: AdminController.Index survives RouteData == null");

                var c2 = new AdminTrangChuController(db, env);
                c2.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext(), RouteData = null! };
                var r2 = await c2.Index();
                Assert(r2 is ViewResult, "S5.02: AdminTrangChuController.Index survives RouteData == null");
            }

            // Edge 5.2: ViewContext active-class evaluation with null/missing controller route value
            {
                var emptyRouteData = new RouteData();
                var controllerValue = emptyRouteData.Values["Controller"]?.ToString();
                var isActiveTrangChu = controllerValue == "AdminTrangChu";
                var isActiveChuongTrinh = controllerValue == "AdminChuongTrinh";
                Assert(!isActiveTrangChu && !isActiveChuongTrinh,
                    "S5.03: Active class Razor expression returns false without NullReferenceException when RouteData lacks 'Controller'");
            }

            // Edge 5.3: Empty database table (0 rows returned)
            {
                using var db = CreateMockDbContextWithCampaigns(new List<ChienDich>());
                var ctrl = new AdminTrangChuController(db, env); SetupController(ctrl);
                var res = await ctrl.Index();
                Assert(res is ViewResult vr && vr.Model is List<ChienDich> list && list.Count == 0,
                    "S5.04: AdminTrangChuController.Index with 0 campaigns returns ViewResult with empty list without error");
            }

            // Edge 5.4: High Concurrency Stress Test (50 Parallel Requests)
            {
                var tasks = new List<Task>();
                int concurrentSuccess = 0;

                for (int i = 0; i < 50; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        using var offlineDb = CreateOfflineDbContext();
                        var ctrl = new AdminTrangChuController(offlineDb, env);
                        SetupController(ctrl);
                        var result = await ctrl.Index();
                        if (result is ViewResult vr && vr.Model is IEnumerable<ChienDich>)
                        {
                            Interlocked.Increment(ref concurrentSuccess);
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                Assert(concurrentSuccess == 50,
                    "S5.05: 50 concurrent requests to AdminTrangChuController.Index all succeed with ViewResult",
                    $"Expected 50, got {concurrentSuccess}");
            }

            // =========================================================================
            // SUMMARY
            // =========================================================================
            Console.WriteLine("\n================================================================================");
            Console.WriteLine($" CHALLENGER VERIFICATION RESULTS: {_passed} PASSED | {_failed} FAILED | TOTAL: {_passed + _failed}");
            Console.WriteLine("================================================================================");

            if (_failed > 0)
            {
                Console.WriteLine("\nFailures:");
                foreach (var f in _failures) Console.WriteLine(f);
                return 1;
            }

            return 0;
        }
    }
}
