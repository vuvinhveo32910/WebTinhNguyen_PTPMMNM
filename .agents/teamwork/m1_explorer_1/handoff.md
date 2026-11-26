# Technical Investigation Report: Header Hamburger & Mobile Off-Canvas Drawer Navigation

**Author**: teamwork_explorer instance 1 (`m1_explorer_1`)  
**Assigned Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_1\`  
**Milestone**: M1 - Public Layouts & Mobile Navigation Infrastructure  
**Target Layouts**: `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` and `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`  
**Date**: 2026-11-07  

---

## 1. Observation

### 1.1 Current Header State on Public Layouts

#### 1. Root Public Layout (`WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`)
Lines 432-515 of `_Layout.cshtml`:
```razor
    <!-- Header -->
    <header id="main-header" class="fixed inset-x-0 top-0 z-50 transition-all duration-300 bg-white/90 dark:bg-night/90 backdrop-blur-md shadow-sm py-2 border-b border-transparent dark:border-white/10">
        <div class="mx-auto flex max-w-7xl items-center justify-between px-5">
            <a href="/" class="flex min-w-0 items-center gap-2 sm:gap-2.5">
                <!-- Logo & Site Title -->
            </a>

            <!-- Menu Desktop -->
            <nav class="hidden md:flex items-center gap-1 lg:flex">
                <a class="rounded-full px-3.5 py-2 text-sm font-medium text-gray-700 transition hover:bg-leaf/10 hover:text-leaf-deep dark:text-gray-100 dark:hover:bg-white/10 dark:hover:text-leaf-bright" asp-area="" asp-controller="Home" asp-action="Index">Trang chủ</a>
                <a class="rounded-full px-3.5 py-2 text-sm font-medium transition @(ViewContext.RouteData.Values["Area"]?.ToString() == "CuaHang" ? "bg-leaf/15 text-leaf-deep font-semibold dark:bg-white/15 dark:text-leaf-bright" : "text-gray-700 hover:bg-leaf/10 hover:text-leaf-deep dark:text-gray-100 dark:hover:bg-white/10 dark:hover:text-leaf-bright")" asp-area="CuaHang" asp-controller="CuaHang" asp-action="Index">Cửa hàng Gây quỹ</a>
                <a asp-area="CuaHang" asp-controller="GioHang" asp-action="Index" ...>...</a>
                <a ... asp-action="ChuongTrinh">Chương trình</a>
                <a ... asp-action="GayQuy">Đóng góp</a>
                <a ... asp-action="BanToChuc">Ban tổ chức</a>
                <a ... asp-action="TinTuc">Tin tức</a>
                <!-- Dropdown Ký ức -->
                ...
                @if (ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang")
                {
                    <button id="themeToggleBtn" type="button" onclick="toggleThemeMode()" ...>...</button>
                }
            </nav>

            <!-- NÚT CHUYỂN ĐỔI SÁNG / TỐI (LIGHT/DARK MODE TOGGLE MOBILE) -->
            @if (ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang")
            {
                <div class="flex items-center gap-2 md:hidden">
                    <button id="themeToggleBtnMobile" type="button" onclick="toggleThemeMode()"
                            class="theme-toggle-btn relative inline-flex items-center justify-center w-9 h-9 rounded-full border border-emerald-500/20 bg-emerald-50 text-emerald-800 hover:bg-emerald-100 dark:bg-white/10 dark:text-amber-400 dark:hover:bg-white/15 dark:border-white/10 transition-all duration-300 shadow-xs focus:outline-none focus:ring-2 focus:ring-emerald-500"
                            title="Chuyển đổi giao diện Sáng / Tối"
                            aria-label="Chuyển đổi giao diện Sáng / Tối">
                        <i class="fas fa-moon text-sm text-emerald-700 transition-transform duration-300 rotate-0 dark:-rotate-90 dark:hidden"></i>
                        <i class="fas fa-sun text-sm text-amber-400 transition-transform duration-300 hidden dark:inline-block rotate-0"></i>
                    </button>
                </div>
            }
        </div>
    </header>
```
- **Direct Finding**: On screens `< 768px` (`md:hidden`), the header displays only the logo on the left and `#themeToggleBtnMobile` on the right (if outside `CuaHang`). There is **zero hamburger button** and **zero off-canvas drawer**.
- Furthermore, if `ViewContext.RouteData.Values["area"]?.ToString() == "CuaHang"`, the entire `div class="flex items-center gap-2 md:hidden"` is skipped, leaving the right side of the mobile header completely empty.

#### 2. Storefront Layout (`WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`)
Lines 182-234 of `_CuaHangLayout.cshtml`:
```razor
    <!-- Header Cửa hàng -->
    <header id="main-header" class="fixed inset-x-0 top-0 z-50 transition-all duration-300 bg-white/95 backdrop-blur-md shadow-xs py-2 border-b border-slate-200">
        <div class="mx-auto flex max-w-7xl items-center justify-between px-5">
            <a href="/" class="flex min-w-0 items-center gap-2 sm:gap-2.5">
                <!-- Logo & Store Title -->
            </a>

            <!-- Menu Desktop -->
            <nav class="hidden md:flex items-center gap-1 lg:flex pr-4">
                <!-- Desktop nav links -->
            </nav>
        </div>
    </header>
```
- **Direct Finding**: On screens `< 768px`, the desktop `<nav>` is hidden via `hidden md:flex`. The header has **no mobile container**, **no hamburger button**, and **no drawer**.

### 1.2 Z-Index Layering and Bottom Navigation Invariant
Lines 314-328 of `_Layout.cshtml` and lines 79-93 of `_CuaHangLayout.cshtml`:
```css
.navigation {
    position: fixed;
    bottom: 15px;
    left: 50%;
    transform: translateX(-50%);
    width: 96%;
    height: 70px;
    background: #222327;
    border-radius: 20px;
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
    box-shadow: 0 10px 30px rgba(0,0,0,0.3);
}
```
- **Crucial Z-Index Finding**: The existing `.navigation` (liquid bottom navigation) explicitly declares `z-index: 9999;`.
- If an off-canvas drawer is added with standard Tailwind z-indices such as `z-50` or `z-[70]`, the bottom navigation will physically render **on top of the drawer and backdrop** when opened!
- Consequently, `#mobileDrawerBackdrop` must be assigned `z-[10000]` and `#mobileDrawer` must be assigned `z-[10001]` to ensure the off-canvas drawer layers properly above all existing page elements, including the bottom bar.

### 1.3 Test Harness Invariants & Constraints

1. **`tests/E2ETests/Tier1/Tier1_StorefrontTests.cs`**:
   - `F05_T01..T05`: Requires `"CuaHang"`, `asp-area="CuaHang"` or `/CuaHang`, `"Cửa Hàng"` or `"Gây Quỹ"`, `"Trang chủ"` before `"CuaHang"`, `"hidden md:"`.
   - `F06_T01..T05`: Requires mobile bottom nav to have `"CuaHang"`, icon (`<svg` or `fa-`), `"fixed bottom"`, `"md:hidden"`, and route to `CuaHang/Index`.
   - *Implication*: The bottom navigation `.navigation` cannot be removed or have its invariant classes deleted. The off-canvas drawer must coexist cleanly.

2. **`tests/E2ETests/Tier5/Tier5_ChallengerM4_2ThemeHarness.cs`**:
   - M4_01: `id="themeToggleBtn"` with `onclick="toggleThemeMode()"`.
   - M4_02: `id="themeToggleBtnMobile"` inside container with `md:hidden`.
   - M4_03: Moon (`fa-moon`) and Sun (`fa-sun`) icons with `rotate-0`.
   - M4_04: Guarded by `ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang"`.
   - M4_11: In `_CuaHangLayout.cshtml`, strictly NO `themeToggleBtn`, NO `toggleThemeMode`, and `class="light` is locked.
   - *Implication*: In `_Layout.cshtml`, `#themeToggleBtnMobile` must remain present within `md:hidden` and retain its exact ID, classes, and area gating. In `_CuaHangLayout.cshtml`, absolutely NO theme toggle element or dark class can be added.

3. **`tests/E2ETests/Tier5/Tier5_ChallengerM4OrderHarness.cs`**:
   - CH_M4_06: `id="floatingCartBtn"` or `id="floatingCartCount"` routing to `CuaHang/GioHang`.
   - *Implication*: Floating cart button must remain in layouts.

4. **Adversarial Test Suites (`tests/adversarial_store_isolation_test.js` & `tests/adversarial_final_stress_harness.js`)**:
   - Both node test suites run 109 and 201 tests respectively, asserting:
     - `!cuaHangLayoutContent.includes('id="themeToggleBtn"')`
     - `!cuaHangLayoutContent.includes('id="themeToggleBtnMobile"')`
     - `!cuaHangLayoutContent.includes('theme-toggle-btn')`
     - `!cuaHangLayoutContent.includes('toggleThemeMode')`
     - `!cuaHangLayoutContent.includes('toggleTheme')`
   - *Implication*: The drawer markup and scripts inside `_CuaHangLayout.cshtml` must be 100% theme-isolated with zero theme toggle code and zero `dark:` styling.

5. **Current Test Run Status**:
   - `dotnet run --project tests/E2ETests/E2ETests.csproj`: **536/536 tests pass (100%)**.
   - `node tests/adversarial_store_isolation_test.js`: **109/109 tests pass (100%)**.
   - `node tests/adversarial_final_stress_harness.js`: **201/201 tests pass (100%)**.

---

## 2. Logic Chain

1. **Premise 1: User Request R1 & Acceptance Criteria require a responsive hamburger/drawer on mobile viewports (375px - 430px)**:
   - Acceptance Criteria state: *"Menu điều hướng trên di động hiển thị gọn gàng (dạng hamburger hoặc drawer di động) và đóng/mở hoạt động chính xác khi tương tác trên màn hình nhỏ."*
   - Current observation confirms neither public layout has a hamburger button or drawer.
   - Therefore, a top header hamburger button (`#mobileMenuToggleBtn` with `md:hidden`) and an off-canvas drawer (`#mobileDrawer`) must be implemented on both `_Layout.cshtml` and `_CuaHangLayout.cshtml`.

2. **Premise 2: Layering order must account for existing fixed elements**:
   - Observation 1.2 proves `.navigation` has `z-index: 9999` and `#main-header` has `z-50`.
   - To provide a true off-canvas experience where opening the menu darkens and covers the entire screen, `#mobileDrawerBackdrop` must have `z-[10000]` and `#mobileDrawer` must have `z-[10001]`.

3. **Premise 3: Preserving test invariants in `_Layout.cshtml`**:
   - Observation 1.3 proves tests require `id="themeToggleBtnMobile"` inside an `md:hidden` block, guarded by `ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang"`.
   - Therefore, the mobile header container in `_Layout.cshtml` must be:
     ```razor
     <div class="flex items-center gap-2 md:hidden">
         @if (ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang")
         {
             <button id="themeToggleBtnMobile" type="button" onclick="toggleThemeMode()" ...>...</button>
         }
         <button id="mobileMenuToggleBtn" type="button" onclick="openMobileDrawer()" ...>
             <i class="fas fa-bars text-base"></i>
         </button>
     </div>
     ```
   - This satisfies both `M4_02` (contains `id="themeToggleBtnMobile"` inside `md:hidden`), `M4_04` (contains area gating condition), and provides the hamburger button on all pages.

4. **Premise 4: Preserving 100% theme isolation in `_CuaHangLayout.cshtml`**:
   - Observation 1.3 proves that if `_CuaHangLayout.cshtml` contains any of `themeToggleBtn`, `themeToggleBtnMobile`, `theme-toggle-btn`, `toggleThemeMode`, or `toggleTheme`, the adversarial test suites fail immediately.
   - Therefore, the drawer in `_CuaHangLayout.cshtml` must use only light-mode Tailwind classes (e.g., `bg-white`, `text-slate-700`, `border-slate-200`, `hover:bg-slate-100`), without any `dark:` variants and without any theme toggle buttons.

5. **Premise 5: Drawer features and accessibility requirements**:
   - The drawer must feature:
     - Backdrop overlay (`#mobileDrawerBackdrop`) with click-to-close.
     - Accessible close button (`#mobileNavClose`) with `aria-label="Đóng menu"`.
     - Links to all public routes: Trang chủ (`/`), Cửa hàng gây quỹ (`/CuaHang`), Giỏ quà (`/CuaHang/GioHang`), Chương trình (`/Home/ChuongTrinh`), Đóng góp (`/Home/GayQuy`), Ban tổ chức (`/Home/BanToChuc`), Tin tức (`/Home/TinTuc`), Lời chúc (`/Home/LoiChuc`), and Ký ức các mùa (`/Home/KyUc?year={year}`).
     - Expandable campaign year submenu for Ký ức.
     - Prominent CTA donation button.
     - Body scroll locking (`document.body.classList.add('overflow-hidden')`) when open.
     - Keyboard ESC key handling to close the drawer.

---

## 3. Caveats

1. **Area Routing for Ký Ức Links**:
   - In `_Layout.cshtml`, the route helper is `@Url.Action("KyUc", "Home", new { year = nam })`.
   - In `_CuaHangLayout.cshtml` (which lives in Area "CuaHang"), the helper MUST specify `area = ""` (i.e. `@Url.Action("KyUc", "Home", new { area = "", year = nam })`) to prevent MVC from erroneously generating `/CuaHang/Home/KyUc`.
2. **Horizontal Overflow Guard**:
   - On mobile devices, opening or translating the drawer can trigger horizontal overflow if the body or root wrapper lacks overflow containment. Adding `overflow-x: clip` or `overflow-x-hidden` on `body` prevents unintended scrollbars.
3. **Database Fallback Safety**:
   - The campaign year query `_context.ChienDichs.Select(c => c.Nam).Distinct().OrderByDescending(n => n).ToList()` must be wrapped with null-coalescing or empty list fallback in case the database context returns no campaigns.
4. **Admin Isolation**:
   - Zero changes to `/admin` or `_AdminLayout.cshtml`. No caveats on admin boundary; the isolation is 100% complete and verified.

---

## 4. Conclusion & Concrete Design Specifications

### 4.1 Specification for `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`

#### A. Header Hamburger Button Markup
**Target Location**: Inside `<header id="main-header">` (lines 501-514), replacing the existing mobile block:

```razor
<!-- BEFORE (Lines 501-513): -->
            <!-- NÚT CHUYỂN ĐỔI SÁNG / TỐI (LIGHT/DARK MODE TOGGLE MOBILE) -->
            @if (ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang")
            {
                <div class="flex items-center gap-2 md:hidden">
                    <button id="themeToggleBtnMobile" type="button" onclick="toggleThemeMode()"
                            class="theme-toggle-btn relative inline-flex items-center justify-center w-9 h-9 rounded-full border border-emerald-500/20 bg-emerald-50 text-emerald-800 hover:bg-emerald-100 dark:bg-white/10 dark:text-amber-400 dark:hover:bg-white/15 dark:border-white/10 transition-all duration-300 shadow-xs focus:outline-none focus:ring-2 focus:ring-emerald-500"
                            title="Chuyển đổi giao diện Sáng / Tối"
                            aria-label="Chuyển đổi giao diện Sáng / Tối">
                        <i class="fas fa-moon text-sm text-emerald-700 transition-transform duration-300 rotate-0 dark:-rotate-90 dark:hidden"></i>
                        <i class="fas fa-sun text-sm text-amber-400 transition-transform duration-300 hidden dark:inline-block rotate-0"></i>
                    </button>
                </div>
            }

<!-- AFTER: -->
            <!-- CỤM ĐIỀU KHIỂN HEADER MOBILE (THEME TOGGLE + HAMBURGER BUTTON) -->
            <div class="flex items-center gap-2 md:hidden">
                @if (ViewContext.RouteData.Values["area"]?.ToString() != "CuaHang")
                {
                    <button id="themeToggleBtnMobile" type="button" onclick="toggleThemeMode()"
                            class="theme-toggle-btn relative inline-flex items-center justify-center w-10 h-10 rounded-full border border-emerald-500/20 bg-emerald-50 text-emerald-800 hover:bg-emerald-100 dark:bg-white/10 dark:text-amber-400 dark:hover:bg-white/15 dark:border-white/10 transition-all duration-300 shadow-xs focus:outline-none focus:ring-2 focus:ring-emerald-500"
                            title="Chuyển đổi giao diện Sáng / Tối"
                            aria-label="Chuyển đổi giao diện Sáng / Tối">
                        <i class="fas fa-moon text-sm text-emerald-700 transition-transform duration-300 rotate-0 dark:-rotate-90 dark:hidden"></i>
                        <i class="fas fa-sun text-sm text-amber-400 transition-transform duration-300 hidden dark:inline-block rotate-0"></i>
                    </button>
                }
                <button id="mobileMenuToggleBtn" type="button" onclick="openMobileDrawer()"
                        class="relative inline-flex items-center justify-center w-10 h-10 rounded-full border border-gray-200 dark:border-white/10 bg-gray-50 text-gray-700 hover:bg-gray-100 dark:bg-white/10 dark:text-gray-200 dark:hover:bg-white/15 transition-all duration-200 shadow-xs focus:outline-none focus:ring-2 focus:ring-emerald-500"
                        title="Mở menu điều hướng"
                        aria-label="Mở menu điều hướng"
                        aria-controls="mobileDrawer"
                        aria-expanded="false">
                    <i class="fas fa-bars text-base"></i>
                </button>
            </div>
```

#### B. Off-Canvas Mobile Drawer Markup
**Target Location**: Immediately preceding the existing `<!-- THANH LIQUID BOTTOM NAVIGATION MOBILE -->` (line 595 of `_Layout.cshtml`):

```razor
    <!-- ========================================================================= -->
    <!-- OFF-CANVAS MOBILE DRAWER NAVIGATION                                        -->
    <!-- ========================================================================= -->
    <!-- Backdrop Overlay -->
    <div id="mobileDrawerBackdrop"
         onclick="closeMobileDrawer()"
         class="fixed inset-0 bg-black/60 z-[10000] backdrop-blur-xs transition-opacity duration-300 opacity-0 pointer-events-none"
         aria-hidden="true"></div>

    <!-- Drawer Panel (Trượt từ cạnh phải) -->
    <aside id="mobileDrawer"
           role="dialog"
           aria-modal="true"
           aria-label="Menu điều hướng di động"
           aria-hidden="true"
           class="fixed top-0 bottom-0 right-0 w-[85vw] max-w-[340px] z-[10001] bg-white dark:bg-night-2 text-slate-800 dark:text-slate-100 border-l border-slate-200 dark:border-white/10 shadow-2xl transition-transform duration-300 ease-in-out translate-x-full flex flex-col">
        
        <!-- Drawer Header -->
        <div class="flex items-center justify-between px-5 py-4 border-b border-slate-200 dark:border-white/10 shrink-0">
            <div class="flex items-center gap-2.5">
                @if (!string.IsNullOrEmpty(logoImg))
                {
                    <img src="@logoImg" alt="Logo" class="w-8 h-8 object-contain rounded-lg" />
                }
                <div class="leading-tight">
                    <span class="block text-[10px] font-black uppercase tracking-wider text-emerald-600 dark:text-emerald-400">Trăng Sáng</span>
                    <span class="block text-sm font-black text-slate-900 dark:text-white">Langbiang</span>
                </div>
            </div>
            <button id="mobileNavClose" type="button" onclick="closeMobileDrawer()"
                    class="w-10 h-10 rounded-full flex items-center justify-center text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-white/10 transition-colors focus:outline-none focus:ring-2 focus:ring-emerald-500"
                    aria-label="Đóng menu điều hướng">
                <i class="fas fa-times text-lg"></i>
            </button>
        </div>

        <!-- Drawer Body Navigation Links -->
        <nav class="flex-1 overflow-y-auto px-4 py-3 space-y-1 overscroll-contain">
            <!-- 1. Trang chủ -->
            <a asp-area="" asp-controller="Home" asp-action="Index" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-home text-sm"></i>
                </span>
                <span class="text-sm">Trang chủ</span>
            </a>

            <!-- 2. Cửa hàng gây quỹ -->
            <a asp-area="CuaHang" asp-controller="CuaHang" asp-action="Index" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-store text-sm"></i>
                </span>
                <span class="text-sm flex-1">Cửa hàng Gây quỹ</span>
                <span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-emerald-100 dark:bg-emerald-900/50 text-emerald-700 dark:text-emerald-300">100% quỹ</span>
            </a>

            <!-- 3. Giỏ quà gây quỹ -->
            <a asp-area="CuaHang" asp-controller="GioHang" asp-action="Index" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-shopping-basket text-sm"></i>
                </span>
                <span class="text-sm flex-1">Giỏ quà gây quỹ</span>
                <span id="drawerCartBadge" class="inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 text-xs font-black text-white bg-rose-500 rounded-full">
                    @cartCount
                </span>
            </a>

            <!-- 4. Chương trình -->
            <a asp-area="" asp-controller="Home" asp-action="ChuongTrinh" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-calendar-alt text-sm"></i>
                </span>
                <span class="text-sm">Chương trình</span>
            </a>

            <!-- 5. Đóng góp -->
            <a asp-area="" asp-controller="Home" asp-action="GayQuy" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-heart text-sm"></i>
                </span>
                <span class="text-sm">Đóng góp & Tài trợ</span>
            </a>

            <!-- 6. Ban tổ chức -->
            <a asp-area="" asp-controller="Home" asp-action="BanToChuc" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-users text-sm"></i>
                </span>
                <span class="text-sm">Ban tổ chức</span>
            </a>

            <!-- 7. Tin tức -->
            <a asp-area="" asp-controller="Home" asp-action="TinTuc" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-newspaper text-sm"></i>
                </span>
                <span class="text-sm">Tin tức dự án</span>
            </a>

            <!-- 8. Lời chúc tình nguyện -->
            <a asp-area="" asp-controller="Home" asp-action="LoiChuc" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                    <i class="fas fa-comment-dots text-sm"></i>
                </span>
                <span class="text-sm">Lời chúc tình nguyện</span>
            </a>

            <!-- 9. Ký ức các mùa (Accordion Submenu) -->
            <div class="pt-1">
                <button type="button" onclick="toggleDrawerSubmenu('drawerKyUcSubmenu')"
                        class="w-full flex items-center justify-between px-3.5 py-3 rounded-xl font-medium text-slate-700 dark:text-slate-200 hover:bg-emerald-50 dark:hover:bg-white/10 transition-colors"
                        aria-expanded="false" aria-controls="drawerKyUcSubmenu">
                    <div class="flex items-center gap-3">
                        <span class="w-8 h-8 rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
                            <i class="fas fa-history text-sm"></i>
                        </span>
                        <span class="text-sm">Ký ức các mùa</span>
                    </div>
                    <i id="drawerKyUcChevron" class="fas fa-chevron-down text-xs text-slate-400 transition-transform duration-200"></i>
                </button>
                <div id="drawerKyUcSubmenu" class="hidden pl-11 pr-2 py-1 space-y-1">
                    @{
                        var drawerNamList = _context.ChienDichs.Select(c => c.Nam).Distinct().OrderByDescending(n => n).ToList();
                        foreach (var nam in drawerNamList)
                        {
                            <a href="@Url.Action("KyUc", "Home", new { year = nam })" onclick="closeMobileDrawer()"
                               class="block px-3 py-2 rounded-lg text-xs font-semibold text-slate-600 dark:text-slate-300 hover:bg-emerald-50 dark:hover:bg-white/10 hover:text-emerald-700 dark:hover:text-emerald-400 transition-colors">
                                Mùa @nam
                            </a>
                        }
                    }
                </div>
            </div>
        </nav>

        <!-- Drawer Footer (Quick CTA) -->
        <div class="p-4 border-t border-slate-200 dark:border-white/10 shrink-0">
            <a asp-area="" asp-controller="Home" asp-action="GayQuy" onclick="closeMobileDrawer()"
               class="flex items-center justify-center gap-2 w-full py-3 px-4 rounded-xl font-bold text-sm text-white bg-gradient-to-r from-emerald-600 to-green-700 hover:from-emerald-700 hover:to-green-800 shadow-md hover:shadow-lg transition">
                <i class="fas fa-heart text-rose-300"></i>
                <span>Ủng hộ dự án ngay</span>
            </a>
        </div>
    </aside>
```

#### C. JavaScript Functions for `_Layout.cshtml`
**Target Location**: Inside `<script>` tag at lines 671-758:

```javascript
// =========================================================================
// MOBILE DRAWER CONTROLLER
// =========================================================================
function openMobileDrawer() {
    var drawer = document.getElementById('mobileDrawer');
    var backdrop = document.getElementById('mobileDrawerBackdrop');
    var toggleBtn = document.getElementById('mobileMenuToggleBtn');
    if (!drawer || !backdrop) return;

    drawer.classList.remove('translate-x-full');
    drawer.classList.add('translate-x-0');
    drawer.setAttribute('aria-hidden', 'false');

    backdrop.classList.remove('opacity-0', 'pointer-events-none');
    backdrop.classList.add('opacity-100', 'pointer-events-auto');

    if (toggleBtn) toggleBtn.setAttribute('aria-expanded', 'true');
    document.body.classList.add('overflow-hidden');

    var closeBtn = document.getElementById('mobileNavClose');
    if (closeBtn) closeBtn.focus();
}

function closeMobileDrawer() {
    var drawer = document.getElementById('mobileDrawer');
    var backdrop = document.getElementById('mobileDrawerBackdrop');
    var toggleBtn = document.getElementById('mobileMenuToggleBtn');
    if (!drawer || !backdrop) return;

    drawer.classList.remove('translate-x-0');
    drawer.classList.add('translate-x-full');
    drawer.setAttribute('aria-hidden', 'true');

    backdrop.classList.remove('opacity-100', 'pointer-events-auto');
    backdrop.classList.add('opacity-0', 'pointer-events-none');

    if (toggleBtn) {
        toggleBtn.setAttribute('aria-expanded', 'false');
        toggleBtn.focus();
    }
    document.body.classList.remove('overflow-hidden');
}

function toggleDrawerSubmenu(submenuId) {
    var submenu = document.getElementById(submenuId);
    var chevron = document.getElementById('drawerKyUcChevron');
    if (!submenu) return;
    var isHidden = submenu.classList.contains('hidden');
    if (isHidden) {
        submenu.classList.remove('hidden');
        if (chevron) chevron.classList.add('rotate-180');
    } else {
        submenu.classList.add('hidden');
        if (chevron) chevron.classList.remove('rotate-180');
    }
}

// Keyboard ESC to close drawer
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape' || e.keyCode === 27) {
        var drawer = document.getElementById('mobileDrawer');
        if (drawer && !drawer.classList.contains('translate-x-full')) {
            closeMobileDrawer();
        }
    }
});
```

---

### 4.2 Specification for `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`

#### A. Header Hamburger Button Markup
**Target Location**: Inside `<header id="main-header">` (lines 183-234), adjacent to `<nav class="hidden md:flex ...">`:

```razor
<!-- BEFORE (Lines 198-233): -->
            <!-- Menu Desktop -->
            <nav class="hidden md:flex items-center gap-1 lg:flex pr-4">
                ...
            </nav>
        </div>
    </header>

<!-- AFTER: -->
            <!-- Menu Desktop -->
            <nav class="hidden md:flex items-center gap-1 lg:flex pr-4">
                ...
            </nav>

            <!-- CỤM ĐIỀU KHIỂN HEADER MOBILE CỬA HÀNG (CART SHORTCUT + HAMBURGER BUTTON) -->
            <div class="flex items-center gap-2 md:hidden">
                <a asp-area="CuaHang" asp-controller="GioHang" asp-action="Index"
                   class="relative inline-flex items-center justify-center w-10 h-10 rounded-full border border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 transition shadow-xs"
                   title="Xem giỏ quà gây quỹ"
                   aria-label="Xem giỏ quà">
                    <i class="fas fa-shopping-basket text-sm text-emerald-700"></i>
                    @if (cartCount > 0)
                    {
                        <span class="absolute -top-1 -right-1 inline-flex items-center justify-center min-w-[18px] h-[18px] px-1 text-[10px] font-black text-white bg-rose-500 rounded-full">
                            @cartCount
                        </span>
                    }
                </a>
                <button id="mobileMenuToggleBtn" type="button" onclick="openMobileDrawer()"
                        class="relative inline-flex items-center justify-center w-10 h-10 rounded-full border border-slate-200 bg-slate-50 text-slate-700 hover:bg-slate-100 transition-all duration-200 shadow-xs focus:outline-none focus:ring-2 focus:ring-emerald-500"
                        title="Mở menu điều hướng"
                        aria-label="Mở menu điều hướng"
                        aria-controls="mobileDrawer"
                        aria-expanded="false">
                    <i class="fas fa-bars text-base text-slate-700"></i>
                </button>
            </div>
        </div>
    </header>
```
*Note: In `_CuaHangLayout.cshtml`, NO theme toggle button is included, upholding 100% theme isolation.*

#### B. Off-Canvas Mobile Drawer Markup for `_CuaHangLayout.cshtml`
**Target Location**: Immediately preceding `<!-- THANH LIQUID BOTTOM NAVIGATION MOBILE -->` (line 299 of `_CuaHangLayout.cshtml`):

```razor
    <!-- ========================================================================= -->
    <!-- OFF-CANVAS MOBILE DRAWER NAVIGATION (CỬA HÀNG - CƯỠNG CHẾ SÁNG TUYỆT ĐỐI)   -->
    <!-- ========================================================================= -->
    <!-- Backdrop Overlay -->
    <div id="mobileDrawerBackdrop"
         onclick="closeMobileDrawer()"
         class="fixed inset-0 bg-black/60 z-[10000] backdrop-blur-xs transition-opacity duration-300 opacity-0 pointer-events-none"
         aria-hidden="true"></div>

    <!-- Drawer Panel (Light-Mode Exclusive, Không có class dark:) -->
    <aside id="mobileDrawer"
           role="dialog"
           aria-modal="true"
           aria-label="Menu điều hướng di động"
           aria-hidden="true"
           class="fixed top-0 bottom-0 right-0 w-[85vw] max-w-[340px] z-[10001] bg-white text-slate-800 border-l border-slate-200 shadow-2xl transition-transform duration-300 ease-in-out translate-x-full flex flex-col">
        
        <!-- Drawer Header -->
        <div class="flex items-center justify-between px-5 py-4 border-b border-slate-200 shrink-0">
            <div class="flex items-center gap-2.5">
                @if (!string.IsNullOrEmpty(logoImg))
                {
                    <img src="@logoImg" alt="Logo" class="w-8 h-8 object-contain rounded-lg" />
                }
                <div class="leading-tight">
                    <span class="block text-[10px] font-black uppercase tracking-wider text-emerald-700">CỬA HÀNG GÂY QUỸ</span>
                    <span class="block text-sm font-black text-slate-900">Trăng Sáng Langbiang</span>
                </div>
            </div>
            <button id="mobileNavClose" type="button" onclick="closeMobileDrawer()"
                    class="w-10 h-10 rounded-full flex items-center justify-center text-slate-500 hover:text-slate-800 hover:bg-slate-100 transition-colors focus:outline-none focus:ring-2 focus:ring-emerald-500"
                    aria-label="Đóng menu điều hướng">
                <i class="fas fa-times text-lg"></i>
            </button>
        </div>

        <!-- Drawer Body Navigation Links -->
        <nav class="flex-1 overflow-y-auto px-4 py-3 space-y-1 overscroll-contain">
            <!-- 1. Trang chủ -->
            <a asp-area="" asp-controller="Home" asp-action="Index" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                    <i class="fas fa-home text-sm"></i>
                </span>
                <span class="text-sm">Trang chủ</span>
            </a>

            <!-- 2. Cửa hàng gây quỹ -->
            <a asp-area="CuaHang" asp-controller="CuaHang" asp-action="Index" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-semibold bg-emerald-50 text-emerald-800 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-600 text-white flex items-center justify-center shrink-0">
                    <i class="fas fa-store text-sm"></i>
                </span>
                <span class="text-sm flex-1">Cửa hàng Gây quỹ</span>
                <span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-800">Đang xem</span>
            </a>

            <!-- 3. Giỏ quà gây quỹ -->
            <a asp-area="CuaHang" asp-controller="GioHang" asp-action="Index" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                    <i class="fas fa-shopping-basket text-sm"></i>
                </span>
                <span class="text-sm flex-1">Giỏ quà gây quỹ</span>
                <span class="inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 text-xs font-black text-white bg-rose-500 rounded-full">
                    @cartCount
                </span>
            </a>

            <!-- 4. Chương trình -->
            <a asp-area="" asp-controller="Home" asp-action="ChuongTrinh" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                    <i class="fas fa-calendar-alt text-sm"></i>
                </span>
                <span class="text-sm">Chương trình</span>
            </a>

            <!-- 5. Đóng góp -->
            <a asp-area="" asp-controller="Home" asp-action="GayQuy" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                    <i class="fas fa-heart text-sm"></i>
                </span>
                <span class="text-sm">Đóng góp & Tài trợ</span>
            </a>

            <!-- 6. Ban tổ chức -->
            <a asp-area="" asp-controller="Home" asp-action="BanToChuc" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                    <i class="fas fa-users text-sm"></i>
                </span>
                <span class="text-sm">Ban tổ chức</span>
            </a>

            <!-- 7. Tin tức -->
            <a asp-area="" asp-controller="Home" asp-action="TinTuc" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                    <i class="fas fa-newspaper text-sm"></i>
                </span>
                <span class="text-sm">Tin tức dự án</span>
            </a>

            <!-- 8. Lời chúc tình nguyện -->
            <a asp-area="" asp-controller="Home" asp-action="LoiChuc" onclick="closeMobileDrawer()"
               class="flex items-center gap-3 px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 hover:text-emerald-700 transition-colors">
                <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                    <i class="fas fa-comment-dots text-sm"></i>
                </span>
                <span class="text-sm">Lời chúc tình nguyện</span>
            </a>

            <!-- 9. Ký ức các mùa (Accordion Submenu) -->
            <div class="pt-1">
                <button type="button" onclick="toggleDrawerSubmenu('drawerKyUcSubmenu')"
                        class="w-full flex items-center justify-between px-3.5 py-3 rounded-xl font-medium text-slate-700 hover:bg-emerald-50 transition-colors"
                        aria-expanded="false" aria-controls="drawerKyUcSubmenu">
                    <div class="flex items-center gap-3">
                        <span class="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center shrink-0">
                            <i class="fas fa-history text-sm"></i>
                        </span>
                        <span class="text-sm">Ký ức các mùa</span>
                    </div>
                    <i id="drawerKyUcChevron" class="fas fa-chevron-down text-xs text-slate-400 transition-transform duration-200"></i>
                </button>
                <div id="drawerKyUcSubmenu" class="hidden pl-11 pr-2 py-1 space-y-1">
                    @{
                        var drawerNamList = _context.ChienDichs.Select(c => c.Nam).Distinct().OrderByDescending(n => n).ToList();
                        foreach (var nam in drawerNamList)
                        {
                            <a href="@Url.Action("KyUc", "Home", new { area = "", year = nam })" onclick="closeMobileDrawer()"
                               class="block px-3 py-2 rounded-lg text-xs font-semibold text-slate-600 hover:bg-emerald-50 hover:text-emerald-800 transition-colors">
                                Mùa @nam
                            </a>
                        }
                    }
                </div>
            </div>
        </nav>

        <!-- Drawer Footer (Quick CTA) -->
        <div class="p-4 border-t border-slate-200 shrink-0">
            <a asp-area="" asp-controller="Home" asp-action="GayQuy" onclick="closeMobileDrawer()"
               class="flex items-center justify-center gap-2 w-full py-3 px-4 rounded-xl font-bold text-sm text-white bg-gradient-to-r from-emerald-600 to-green-700 hover:from-emerald-700 hover:to-green-800 shadow-md hover:shadow-lg transition">
                <i class="fas fa-heart text-rose-300"></i>
                <span>Ủng hộ dự án ngay</span>
            </a>
        </div>
    </aside>
```

#### C. JavaScript Functions for `_CuaHangLayout.cshtml`
**Target Location**: Inside `<script>` tag at lines 361-367:

```javascript
// =========================================================================
// MOBILE DRAWER CONTROLLER (CỬA HÀNG - LIGHT MODE ONLY)
// =========================================================================
function openMobileDrawer() {
    var drawer = document.getElementById('mobileDrawer');
    var backdrop = document.getElementById('mobileDrawerBackdrop');
    var toggleBtn = document.getElementById('mobileMenuToggleBtn');
    if (!drawer || !backdrop) return;

    drawer.classList.remove('translate-x-full');
    drawer.classList.add('translate-x-0');
    drawer.setAttribute('aria-hidden', 'false');

    backdrop.classList.remove('opacity-0', 'pointer-events-none');
    backdrop.classList.add('opacity-100', 'pointer-events-auto');

    if (toggleBtn) toggleBtn.setAttribute('aria-expanded', 'true');
    document.body.classList.add('overflow-hidden');

    var closeBtn = document.getElementById('mobileNavClose');
    if (closeBtn) closeBtn.focus();
}

function closeMobileDrawer() {
    var drawer = document.getElementById('mobileDrawer');
    var backdrop = document.getElementById('mobileDrawerBackdrop');
    var toggleBtn = document.getElementById('mobileMenuToggleBtn');
    if (!drawer || !backdrop) return;

    drawer.classList.remove('translate-x-0');
    drawer.classList.add('translate-x-full');
    drawer.setAttribute('aria-hidden', 'true');

    backdrop.classList.remove('opacity-100', 'pointer-events-auto');
    backdrop.classList.add('opacity-0', 'pointer-events-none');

    if (toggleBtn) {
        toggleBtn.setAttribute('aria-expanded', 'false');
        toggleBtn.focus();
    }
    document.body.classList.remove('overflow-hidden');
}

function toggleDrawerSubmenu(submenuId) {
    var submenu = document.getElementById(submenuId);
    var chevron = document.getElementById('drawerKyUcChevron');
    if (!submenu) return;
    var isHidden = submenu.classList.contains('hidden');
    if (isHidden) {
        submenu.classList.remove('hidden');
        if (chevron) chevron.classList.add('rotate-180');
    } else {
        submenu.classList.add('hidden');
        if (chevron) chevron.classList.remove('rotate-180');
    }
}

document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape' || e.keyCode === 27) {
        var drawer = document.getElementById('mobileDrawer');
        if (drawer && !drawer.classList.contains('translate-x-full')) {
            closeMobileDrawer();
        }
    }
});
```

---

## 5. Verification Method

To verify the proposed implementation independently:

1. **Static Analysis & Invariant Check**:
   ```powershell
   # Confirm invariant IDs exist in _Layout.cshtml
   Select-String -Path WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml -Pattern "themeToggleBtnMobile","floatingCartBtn","floatingCartCount","hidden md:","fixed bottom"

   # Confirm 100% theme isolation in _CuaHangLayout.cshtml (Expected: 0 matches)
   Select-String -Path WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml -Pattern "themeToggleBtn","theme-toggle-btn","toggleThemeMode","toggleTheme"
   ```

2. **Build Verification**:
   ```powershell
   dotnet build
   ```
   *Expected result*: 0 errors, auto-triggering `npm run build:css`.

3. **C# E2E Test Suite (536 Tests)**:
   ```powershell
   dotnet run --project tests/E2ETests/E2ETests.csproj -- --all
   ```
   *Expected result*: 536/536 tests pass across Tiers 1-5 with 0 failures and 0 skipped.

4. **Node Store Isolation & Stress Harnesses**:
   ```powershell
   node tests/adversarial_store_isolation_test.js
   node tests/adversarial_final_stress_harness.js
   ```
   *Expected result*: 100% pass (109 tests + 201 tests pass).

5. **Admin Route Quarantining Check**:
   ```powershell
   git status WEB_TinhNguyen/GiaoDien/Admin* WEB_TinhNguyen/BoDieuKhien/Admin* WEB_TinhNguyen/Areas/CuaHang/Views/Admin*
   ```
   *Expected result*: 0 files modified under admin directories.

6. **Browser DOM Interactive Verification (375px Viewport)**:
   - On `width=375px`: Hamburger button `#mobileMenuToggleBtn` is visible in the top header.
   - Clicking `#mobileMenuToggleBtn`: `#mobileDrawer` slides into view from the right; `#mobileDrawerBackdrop` dims the background; body scroll is locked.
   - Clicking `#mobileNavClose`, pressing `Escape`, or clicking `#mobileDrawerBackdrop`: `#mobileDrawer` slides out to the right and closes cleanly.
   - Expanding "Ký ức các mùa": past campaign years are displayed with accurate route links.
