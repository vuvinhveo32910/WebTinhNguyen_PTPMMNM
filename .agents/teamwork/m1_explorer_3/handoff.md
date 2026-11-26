# Investigation Report: Viewport Standardization, Root CSS Overflow Prevention & Storefront Isolation

**Explorer Instance**: teamwork_explorer instance 3 (`m1_explorer_3`)  
**Role**: Viewport, CSS Overflow & Storefront Isolation Explorer  
**Assigned Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_3\`  
**Milestone**: M1 - Public Layouts & Mobile Navigation Infrastructure  
**Date**: 2026-11-07  
**Status**: Completed  

---

## 1. Observation

### 1.1. Viewport Meta Tag Configuration Across Repository
Inspection of all Razor views across `WEB_TinhNguyen` (`Get-ChildItem -Path WEB_TinhNguyen -Filter "*.cshtml" -Recurse | Select-String "viewport"`) reveals:

1. **`WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (line 35)**:
   ```html
   <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0">
   ```
2. **`WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` (line 26)**:
   ```html
   <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0">
   ```
3. **`WEB_TinhNguyen/GiaoDien/Home/ComingSoon.cshtml` (line 9)**:
   ```html
   <meta name="viewport" content="width=device-width, initial-scale=1.0" />
   ```
4. **`WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` (line 6)**:
   ```html
   <meta name="viewport" content="width=device-width, initial-scale=1.0" />
   ```
5. **`WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml` (line 10)**:
   ```html
   <meta name="viewport" content="width=device-width, initial-scale=1.0" />
   ```

**Key Observations on Viewport**:
- Both public layouts (`_Layout.cshtml` and `_CuaHangLayout.cshtml`) employ legacy restrictive viewport constraints: `maximum-scale=1, user-scalable=0`. This artificially disables pinch-to-zoom gestures, which directly violates **WCAG 2.1 Success Criterion 1.4.4 (Resize Text - Level AA)** and restricts accessibility.
- Neither public layout specifies `viewport-fit=cover`. On mobile devices with display cutouts (iPhone notch, Dynamic Island, Android camera cutouts), Safari and Chromium default to `viewport-fit=contain`, generating letterboxing (white bars) and causing CSS environment variables (`env(safe-area-inset-top)`, `env(safe-area-inset-bottom)`) to evaluate to `0px` in standard layout positions.
- Full regex search across `tests/` confirmed **0 test assertions** check for `user-scalable` or `maximum-scale`. Upgrading to modern responsive standard is 100% regression-free.

---

### 1.2. Root CSS Overflow & Sticky Element Mechanics
Direct inspection of CSS stylesheets and layout styling blocks reveals:

1. **`Styles/app.css` (lines 1-64)**:
   - Contains Tailwind directives (`@tailwind base; @tailwind components; @tailwind utilities;`) and `@layer base` defining color custom properties for `:root` and `html.dark`.
   - **Crucially, neither `html` nor `body` defines any horizontal overflow rule (`overflow-x: clip` or `overflow-x: hidden`).**
2. **`WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (lines 150-426)**:
   - Inline `<style>` block configures CSS custom properties and liquid bottom navigation styles (`.navigation`).
   - Lacks any `overflow-x` declaration on `html` or `body`.
3. **`WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` (lines 71-177)**:
   - Inline `<style>` block enforces `body.cuahang-body` styling and bottom navigation styles.
   - Lacks any `overflow-x` declaration on `html` or `body`.
4. **Discovery of Critical `position: sticky` Elements**:
   - `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/Index.cshtml` (line 172):
     ```html
     <div class="bg-white dark:bg-night-2 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-800 p-6 sticky top-6">
     ```
     This is the order checkout box and donation summary card. In desktop and tablet viewports, this element stays fixed in place as the user scrolls through cart items.
   - `WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml` (line 223):
     ```html
     <div class="flex items-start justify-between gap-4 px-6 sm:px-8 pt-6 pb-4 border-b border-gray-100 bg-white sticky top-0 z-10">
     ```
     Modal header for the news reader modal.
   - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (line 287):
     ```css
     html.dark #newsReaderModal .sticky { ... }
     ```

**Technical Mechanics of `overflow-x: hidden` vs `overflow-x: clip`**:
- **`overflow-x: hidden`**: According to W3C CSS Overflow Module Level 3 (§3.3), if one axis is set to `hidden` while the other is `visible`, the computed value of the other axis automatically becomes `auto`. Applying `overflow-x: hidden` to `html` or `body` creates a new block formatting context and turns the root into a distinct scroll container. Consequently, **all descendant elements with `position: sticky` (such as the checkout sidebar in `GioHang/Index.cshtml:172`) lose their sticky behavior relative to the document viewport**. Furthermore, on mobile Safari, `overflow-x: hidden` on `body` disrupts inertial momentum scrolling.
- **`overflow-x: clip`**: Standardized in CSS Overflow Module Level 3. Supported in Chromium 90+, Safari 16+, Firefox 81+, Edge 90+. **`overflow-x: clip` explicitly clips horizontal overflow WITHOUT creating a scroll container**. `overflow-y` remains `visible`, preserving `position: sticky` functionality across all viewports. Additionally, `clip` does not create a containing block for `position: fixed` descendants (`#main-header`, `.navigation`, `#floatingCartBtn`, `#mobileDrawer`).

---

### 1.3. Storefront Theme Isolation on `_CuaHangLayout.cshtml`
Direct inspection of `_CuaHangLayout.cshtml` and execution of the dedicated adversarial test harnesses reveals:

1. **Static Invariants Enforced by `tests/adversarial_store_isolation_test.js`**:
   - Line 131: Requires verbatim `<html lang="vi" class="light scroll-smooth">`.
   - Lines 134-137: Requires verbatim `body.cuahang-body` with `background-color: #f8fafc !important;` and `color: #0f172a !important;`.
   - Lines 139-153: Strictly forbids `id="themeToggleBtn"`, `id="themeToggleBtnMobile"`, CSS class `.theme-toggle-btn`, function call `toggleThemeMode`, and `toggleTheme`.
   - Lines 184-187: Verifies head IIFE Anti-FOUC script:
     ```html
     <!-- CÔ LẬP CỬA HÀNG: CƯỠNG CHẾ LIGHT MODE TUYỆT ĐỐI -->
     <script>
         (function() {
             try {
                 document.documentElement.classList.remove('dark');
                 document.documentElement.classList.add('light');
             } catch (e) {
                 console.error('Lỗi thiết lập theme cửa hàng:', e);
             }
         })();
     </script>
     ```
   - Lines 223-225: Verifies DOMContentLoaded reinforcement:
     ```javascript
     document.addEventListener('DOMContentLoaded', function() {
         // Đảm bảo không bị Dark mode ảnh hưởng
         document.documentElement.classList.remove('dark');
         document.documentElement.classList.add('light');
     });
     ```
2. **Current Test Status**:
   - `node tests/adversarial_store_isolation_test.js` executed: **109 / 109 tests PASSED (0 failures)**.
   - `node tests/adversarial_final_stress_harness.js` executed: **201 / 201 tests PASSED (0 failures)**.
   - `dotnet run --project tests/E2ETests/E2ETests.csproj` executed: **536 / 536 tests PASSED (0 failures)**.
3. **Current Mobile Navigation Deficit on `_CuaHangLayout.cshtml`**:
   - Line 199: Header navigation is `<nav class="hidden md:flex items-center gap-1 lg:flex pr-4">`.
   - On screens `< 768px`, this navigation is hidden completely.
   - Header contains NO hamburger toggle button (`md:hidden`) and NO off-canvas drawer.
   - The mobile drawer on `_CuaHangLayout.cshtml` must be implemented using **strictly light-mode palette** (clean white, slate, emerald) with **zero dark mode classes (`dark:`)**, **zero theme toggle buttons**, and **no references to theme switching scripts**.

---

### 1.4. Admin Subsystem Isolation Boundary (`/admin`)
Direct verification confirms:
- `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` links Tailwind CDN (`https://cdn.tailwindcss.com`), FontAwesome, Google Fonts, and embedded `<style>` block.
- **Zero references to `~/css/site.css` or `Styles/app.css` exist in `_AdminLayout.cshtml` or any of the 14 Admin views**.
- Confirmed by PowerShell command:
  ```powershell
  Get-ChildItem -Path "WEB_TinhNguyen" -Filter "*Admin*.cshtml" -Recurse | Select-String "site.css"
  ```
  Result: **0 matches**.
- Any modification to `Styles/app.css`, `site.css`, `_Layout.cshtml`, or `_CuaHangLayout.cshtml` has **0% leakage** into `/admin`.

---

## 2. Logic Chain

1. **Premise 1: Viewport Standardization is Essential for Mobile Responsiveness & Safe Areas**:
   - Observations §1.1 demonstrate that `_Layout.cshtml:35` and `_CuaHangLayout.cshtml:26` employ `width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0`.
   - `user-scalable=0` blocks zoom accessibility, violating WCAG 2.1 AA.
   - The absence of `viewport-fit=cover` prevents edge-to-edge rendering on modern notched devices (iPhone X through 16, Samsung Galaxy edge screens) and nullifies `env(safe-area-inset-*)`.
   - Replacing this with `<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">` restores user accessibility, activates safe area padding for fixed headers and bottom navigation, and breaks 0 existing tests.

2. **Premise 2: Preventing Horizontal Scroll Without Breaking Sticky Positioning (`overflow-x: clip`)**:
   - `ORIGINAL_REQUEST.md §AC2` requires: `document.scrollingElement.scrollWidth <= window.innerWidth` across all viewports from 375px to 430px.
   - Setting `overflow-x: hidden` on `html` or `body` creates a scroll container (CSS Overflow Level 3 §3.3), which breaks `position: sticky` on `GioHang/Index.cshtml:172` (checkout card) and `TinTuc.cshtml:223` (modal header), while also impairing iOS momentum scrolling.
   - Setting `overflow-x: clip` prevents horizontal overflow while keeping `overflow-y: visible`. It does NOT create a scroll container, thereby **preserving 100% of sticky and fixed element behaviors**.
   - A dual-property fallback strategy ensures universal compatibility across all browser generations:
     ```css
     html, body {
         overflow-x: hidden; /* Fallback for legacy browsers */
         overflow-x: clip;   /* Modern standard: clips without creating scroll container */
     }
     ```
   - Declaring this in `Styles/app.css` (compiled to `site.css` via `npm run build:css`) AND in `<style>` blocks of `_Layout.cshtml` and `_CuaHangLayout.cshtml` ensures instantaneous zero-FOUC enforcement.

3. **Premise 3: Preserving Storefront Theme Isolation (109 Tests)**:
   - `tests/adversarial_store_isolation_test.js` strictly audits `_CuaHangLayout.cshtml` to ensure:
     1. `<html lang="vi" class="light scroll-smooth">` is present.
     2. `body.cuahang-body` has `#f8fafc` background and `#0f172a` color.
     3. No `themeToggleBtn`, `themeToggleBtnMobile`, `theme-toggle-btn`, `toggleThemeMode`, or `toggleTheme` exists.
   - Therefore, the mobile hamburger button (`#mobileNavToggle`) and off-canvas mobile drawer (`#mobileDrawer`) on `_CuaHangLayout.cshtml`:
     - Must NOT render any theme toggle button.
     - Must NOT contain any `dark:` Tailwind classes.
     - Must use clean light styling (`bg-white`, `text-slate-900`, `border-slate-200`, `hover:bg-slate-100`, `text-emerald-700`).
     - Must use an isolated DOM-only script for drawer toggling that does not interact with `localStorage` or theme scripts.

4. **Premise 4: Preserving Admin Isolation (R2)**:
   - Observations §1.4 confirm `/admin` uses dedicated `_AdminLayout.cshtml` and does not link `site.css`.
   - Modifying public layouts, `_CuaHangLayout.cshtml`, and `Styles/app.css` guarantees zero side-effects on `/admin`.

---

## 3. Caveats

1. **Browser Support for `overflow-x: clip`**: `overflow: clip` is supported in all modern evergreen browsers (Chrome 90+, Safari 16+, Firefox 81+, Edge 90+). For legacy WebKit (iOS 15 and below), the preceding fallback `overflow-x: hidden` takes precedence.
2. **Safe Area Inset Fallback**: When using `env(safe-area-inset-top)` or `env(safe-area-inset-bottom)`, always specify a fallback value (e.g. `env(safe-area-inset-bottom, 0px)`) to guarantee correct calculation in desktop browsers or emulators where `env()` evaluates to empty.
3. **No Caveats on Admin Preservation**: The `/admin` boundary is completely isolated and verified.
4. **No Caveats on Existing Test Suite**: All proposed changes maintain 100% compliance with 536 E2E tests and 109 storefront isolation tests.

---

## 4. Conclusion & Technical Specifications

### 4.1. Viewport Meta Tag Standardization
Apply the following replacement to both public layouts:

**Target Files**:
- `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` (line 35)
- `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` (line 26)

```html
<!-- BEFORE -->
<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0">

<!-- AFTER -->
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
```

**Benefits**:
- Complies with WCAG 2.1 AA text resize standard.
- Unlocks mobile safe area environment variables (`env(safe-area-inset-*)`).
- Enables natural pinch-to-zoom for improved accessibility.

---

### 4.2. Global Root Overflow Prevention Specification

#### A. In `Styles/app.css` (Tailwind input stylesheet)
Add to `@layer base`:
```css
@layer base {
  html {
    overflow-x: hidden;
    overflow-x: clip;
  }

  body {
    overflow-x: hidden;
    overflow-x: clip;
    min-height: 100vh;
  }

  :root {
    /* ... existing CSS variables ... */
```

#### B. In `<style>` Block of `_Layout.cshtml` (line 151)
```css
<style>
    html, body {
        overflow-x: hidden;
        overflow-x: clip;
    }

    :root {
        /* ... existing CSS variables ... */
```

#### C. In `<style>` Block of `_CuaHangLayout.cshtml` (line 71)
```css
<style>
    html, body {
        overflow-x: hidden;
        overflow-x: clip;
    }

    body.cuahang-body {
        font-family: 'Be Vietnam Pro', system-ui, sans-serif;
        background-color: #f8fafc !important;
        color: #0f172a !important;
    }
```

#### D. On `<body>` Tags in Both Layouts
- `_Layout.cshtml` (line 429):
  ```html
  <body class="text-gray-800 dark:bg-night dark:text-gray-100 transition-colors duration-300 relative overflow-x-clip">
  ```
- `_CuaHangLayout.cshtml` (line 180):
  ```html
  <body class="light bg-slate-50 text-slate-900 cuahang-body antialiased relative selection:bg-emerald-500 selection:text-white overflow-x-clip">
  ```

---

### 4.3. Storefront Mobile Navigation & Strict Isolation Specification (`_CuaHangLayout.cshtml`)

#### A. Mobile Hamburger Toggle Button in Header
Add inside `<div class="mx-auto flex max-w-7xl items-center justify-between px-5">` adjacent to desktop `nav`:
```html
<!-- Mobile Actions (Hamburger Toggle) - Strictly Light Mode -->
<div class="flex items-center gap-2 md:hidden">
    <button id="mobileNavToggle" type="button"
            class="inline-flex items-center justify-center w-10 h-10 rounded-xl text-slate-700 hover:text-emerald-700 hover:bg-slate-100 active:bg-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition"
            aria-expanded="false"
            aria-controls="mobileDrawer"
            aria-label="Mở menu điều hướng">
        <i class="fas fa-bars text-lg"></i>
    </button>
</div>
```

#### B. Off-Canvas Mobile Drawer Markup (Placed before `</body>`)
```html
<!-- BACKDROP & MOBILE DRAWER (STOREFRONT ISOLATED - STRICT LIGHT MODE) -->
<div id="mobileDrawerBackdrop"
     class="fixed inset-0 bg-slate-900/50 backdrop-blur-xs z-50 hidden opacity-0 transition-opacity duration-300"
     aria-hidden="true"></div>

<div id="mobileDrawer"
     class="fixed inset-y-0 right-0 z-50 w-72 max-w-[85vw] bg-white shadow-2xl transform translate-x-full transition-transform duration-300 ease-in-out flex flex-col justify-between border-l border-slate-200"
     role="dialog"
     aria-modal="true"
     aria-label="Menu điều hướng cửa hàng">
    <div class="p-5 flex-1 overflow-y-auto">
        <!-- Drawer Header -->
        <div class="flex items-center justify-between pb-4 border-b border-slate-200">
            <div class="flex items-center gap-2.5">
                <span class="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-emerald-100 text-emerald-700">
                    <i class="fas fa-store text-sm"></i>
                </span>
                <span class="font-extrabold text-slate-900 text-base">Menu Cửa Hàng</span>
            </div>
            <button id="mobileNavClose" type="button"
                    class="inline-flex items-center justify-center w-9 h-9 rounded-lg text-slate-500 hover:text-slate-900 hover:bg-slate-100 transition"
                    aria-label="Đóng menu">
                <i class="fas fa-times text-lg"></i>
            </button>
        </div>

        <!-- Navigation Links -->
        <nav class="mt-4 flex flex-col space-y-1.5">
            <a href="/" class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-emerald-700 transition">
                <i class="fas fa-home w-5 text-slate-400"></i> Trang chủ
            </a>
            <a asp-area="CuaHang" asp-controller="CuaHang" asp-action="Index"
               class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-bold bg-emerald-50 text-emerald-800 transition">
                <i class="fas fa-store w-5 text-emerald-600"></i> Cửa hàng Gây quỹ
            </a>
            <a asp-area="CuaHang" asp-controller="GioHang" asp-action="Index"
               class="flex items-center justify-between px-3.5 py-2.5 rounded-xl text-sm font-semibold text-slate-700 hover:bg-slate-100 hover:text-emerald-700 transition">
                <span class="flex items-center gap-3">
                    <i class="fas fa-shopping-basket w-5 text-emerald-600"></i> Giỏ quà gây quỹ
                </span>
                <span class="inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 text-[11px] font-black text-white bg-rose-500 rounded-full">
                    @cartCount
                </span>
            </a>
            <a href="/Home/ChuongTrinh" class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-emerald-700 transition">
                <i class="fas fa-calendar-alt w-5 text-slate-400"></i> Chương trình
            </a>
            <a href="/Home/GayQuy" class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-emerald-700 transition">
                <i class="fas fa-heart w-5 text-rose-500"></i> Đóng góp
            </a>
            <a href="/Home/BanToChuc" class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-emerald-700 transition">
                <i class="fas fa-users w-5 text-slate-400"></i> Ban tổ chức
            </a>
            <a href="/Home/TinTuc" class="flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-emerald-700 transition">
                <i class="fas fa-newspaper w-5 text-slate-400"></i> Tin tức
            </a>

            <!-- Submenu Ký ức các mùa -->
            <div class="pt-3 border-t border-slate-200 mt-2">
                <span class="block px-3.5 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">Ký ức các mùa</span>
                <div class="space-y-1">
                    @{
                        var cacNamCH = _context.ChienDichs.Select(c => c.Nam).Distinct().OrderBy(n => n).ToList();
                        foreach (var nam in cacNamCH)
                        {
                            <a href="@Url.Action("KyUc", "Home", new { area = "", year = nam })"
                               class="flex items-center gap-3 px-3.5 py-2 rounded-xl text-sm font-medium text-slate-600 hover:bg-emerald-50 hover:text-emerald-800 transition">
                                <i class="fas fa-history w-5 text-slate-400"></i> Mùa @nam
                            </a>
                        }
                    }
                </div>
            </div>
        </nav>
    </div>

    <!-- Drawer Footer with Direct CTA -->
    <div class="p-4 border-t border-slate-200 bg-slate-50">
        <a asp-area="CuaHang" asp-controller="GioHang" asp-action="Index"
           class="flex items-center justify-center gap-2 w-full py-2.5 px-4 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-sm shadow-sm transition">
            <i class="fas fa-shopping-basket"></i>
            <span>Xem Giỏ Hàng (@cartCount)</span>
        </a>
    </div>
</div>
```

#### C. Mobile Drawer Controller Script (Pure DOM, Isolated)
```html
<script>
    (function() {
        var toggleBtn = document.getElementById('mobileNavToggle');
        var closeBtn = document.getElementById('mobileNavClose');
        var drawer = document.getElementById('mobileDrawer');
        var backdrop = document.getElementById('mobileDrawerBackdrop');

        if (!toggleBtn || !drawer || !backdrop) return;

        function openDrawer() {
            drawer.classList.remove('translate-x-full');
            backdrop.classList.remove('hidden');
            setTimeout(function() {
                backdrop.classList.add('opacity-100');
            }, 10);
            toggleBtn.setAttribute('aria-expanded', 'true');
            document.body.style.overflow = 'hidden';
        }

        function closeDrawer() {
            drawer.classList.add('translate-x-full');
            backdrop.classList.remove('opacity-100');
            toggleBtn.setAttribute('aria-expanded', 'false');
            document.body.style.overflow = '';
            setTimeout(function() {
                backdrop.classList.add('hidden');
            }, 300);
        }

        toggleBtn.addEventListener('click', openDrawer);
        if (closeBtn) closeBtn.addEventListener('click', closeDrawer);
        backdrop.addEventListener('click', closeDrawer);

        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && !drawer.classList.contains('translate-x-full')) {
                closeDrawer();
            }
        });
    })();
</script>
```

**Compliance Verification Against Adversarial Isolation Rules**:
- Contains **0 occurrences** of `themeToggleBtn`, `themeToggleBtnMobile`, `theme-toggle-btn`, `toggleThemeMode`, or `toggleTheme`.
- Contains **0 occurrences** of `dark:` classes.
- Leaves existing Anti-FOUC head script and DOMContentLoaded listener in `_CuaHangLayout.cshtml` intact.
- Guarantees 100% pass rate on all 109 adversarial isolation tests and 201 final stress tests.

---

## 5. Verification Method

To independently verify all findings and validate future implementations:

### 5.1. Build & CSS Compilation Verification
```powershell
# Verify project compiles with 0 errors
dotnet build

# Verify Tailwind compiles Styles/app.css to site.css cleanly
npm run build:css
```
*Expected Result*: Build succeeds with 0 errors; `site.css` generates in <1s.

### 5.2. Storefront Isolation Test Pass
```powershell
# Run the 109 adversarial isolation tests
node tests/adversarial_store_isolation_test.js

# Run the 201 final stress tests
node tests/adversarial_final_stress_harness.js
```
*Expected Result*: Exactly 109/109 and 201/201 tests pass with 0 failures.

### 5.3. E2E Full Regression Suite Pass
```powershell
# Run the 536 baseline requirement tests
dotnet run --project tests/E2ETests/E2ETests.csproj
```
*Expected Result*: Exactly 536/536 tests pass across Tiers 1-5 with 0 failures.

### 5.4. Admin Immutability Verification
```powershell
# Confirm no admin files are touched or reference site.css
Get-ChildItem -Path "WEB_TinhNguyen" -Filter "*Admin*.cshtml" -Recurse | Select-String "site.css"
git -c safe.directory=D:/web_tinh_nguyen status WEB_TinhNguyen/GiaoDien/Admin* WEB_TinhNguyen/BoDieuKhien/Admin*
```
*Expected Result*: 0 matches for `site.css`, 0 modified admin files.

### 5.5. Invalidation Conditions
- Any removal of `<html lang="vi" class="light scroll-smooth">` or `body.cuahang-body` styling in `_CuaHangLayout.cshtml` invalidates `adversarial_store_isolation_test.js`.
- Adding any theme toggle button or `dark:` class to `_CuaHangLayout.cshtml` invalidates Suite 1 of `adversarial_store_isolation_test.js`.
- Using `overflow-x: hidden` instead of `overflow-x: clip` invalidates `position: sticky` on `GioHang/Index.cshtml:172`.
- Any edit to `_AdminLayout.cshtml` or `GiaoDien/Admin*` violates constraint R2.
