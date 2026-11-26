# Investigation Report: Public Layout, Navigation & Styling Architecture

**Explorer Role**: Public Layout & Styling Explorer (Instance 1)  
**Assigned Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\`  
**Date**: 2026-11-07  
**Milestone**: M1 - Survey & Technical Architecture  

---

## 1. Observation

### 1.1 Layout Architecture & Routing Segregation
Direct inspection of view starts and layout files reveals a clear tripartite layout structure:

1. **Root Public Layout (`WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`)**:
   - Bound by default via `WEB_TinhNguyen/GiaoDien/_ViewStart.cshtml`:
     ```razor
     @{
         Layout = "_Layout";
     }
     ```
   - Used by all 9 public views under `WEB_TinhNguyen/GiaoDien/Home/` (`Index`, `ChuongTrinh`, `GayQuy`, `BanToChuc`, `TinTuc`, `KyUc`, `LoiChuc`, `ComingSoon` [has `Layout = null`], `Privacy`).
   - Links compiled CSS (`<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />`, line 44) AND Tailwind CDN (`<script src="https://cdn.tailwindcss.com"></script>`, line 50) with an embedded JavaScript configuration (`tailwind.config = { ... }`, lines 54-120).

2. **Storefront Public Layout (`WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`)**:
   - Bound dynamically via `WEB_TinhNguyen/Areas/CuaHang/Views/_ViewStart.cshtml` (lines 7-15):
     ```razor
     if (controller.StartsWith("Admin", StringComparison.OrdinalIgnoreCase) ||
         viewPath.Contains("/Admin", StringComparison.OrdinalIgnoreCase))
     {
         Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";
     }
     else
     {
         Layout = "~/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml"; // Dedicated storefront _Layout
     }
     ```
   - Used by public storefront views (`CuaHang/Index`, `CuaHang/ChiTiet`, `GioHang/Index`, `GioHang/DatHangThanhCong`).
   - Links compiled CSS (`~/css/site.css`, line 40) and Tailwind CDN (`https://cdn.tailwindcss.com`, line 41).
   - Hard-locked to light mode via inline script (lines 60-69) and `body.cuahang-body` styling (lines 72-76).

3. **Admin Layout (`WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`)**:
   - Used by all admin controllers across root and areas (`AdminController`, `AdminTrangChuController`, `AdminChuongTrinhController`, `AdminGayQuyController`, `AdminCauHinhQRController`, `AdminKyUcController`, `AdminBanToChucController`, `AdminTinTucController`, `AdminHieuUngController`, `AdminTaiKhoanController`, and Area controllers `AdminSanPhamController`, `AdminDonHangController`).
   - Every single Admin Razor view explicitly declares `Layout = "~/GiaoDien/Shared/_AdminLayout.cshtml";` or is routed to it by `_ViewStart.cshtml`.
   - `Admin/Login.cshtml` declares `Layout = null;` and provides a self-contained card layout.

### 1.2 Viewport Configuration
Direct search across all 48 `.cshtml` files in the repository (`Get-ChildItem -Path WEB_TinhNguyen -Filter "*.cshtml" -Recurse | Select-String "viewport"`) reveals:
- `_Layout.cshtml` (line 35):
  `<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0">`
- `_CuaHangLayout.cshtml` (line 26):
  `<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0">`
- `_AdminLayout.cshtml` (line 6):
  `<meta name="viewport" content="width=device-width, initial-scale=1.0" />`
- `Admin/Login.cshtml` (line 10):
  `<meta name="viewport" content="width=device-width, initial-scale=1.0" />`
- `Home/ComingSoon.cshtml` (line 9):
  `<meta name="viewport" content="width=device-width, initial-scale=1.0" />`

Key observation: Public layouts disable pinch-to-zoom (`user-scalable=0, maximum-scale=1`). While this suppresses double-tap zoom delay on old WebKit, it restricts user accessibility and does not include `viewport-fit=cover` for mobile safe areas (notches/home indicator).

### 1.3 CSS & Tailwind Compilation and Runtime Mechanics
1. **Compilation pipeline**:
   - `package.json` defines:
     ```json
     "scripts": {
       "build:css": "tailwindcss -i Styles/app.css -o WEB_TinhNguyen/wwwroot/css/site.css --minify",
       "watch:css": "tailwindcss -i Styles/app.css -o WEB_TinhNguyen/wwwroot/css/site.css --watch"
     }
     ```
   - `WEB_TinhNguyen.csproj` (lines 21-24) hooks directly into the build pipeline:
     ```xml
     <Target Name="TailwindCSS" BeforeTargets="Build">
       <Exec Command="npm run build:css" />
     </Target>
     ```
   - Running `dotnet build` triggers `npm run build:css`, which compiles `Styles/app.css` into `WEB_TinhNguyen/wwwroot/css/site.css` (103 KB minified) in ~1.2s.
2. **Configuration (`tailwind.config.js`)**:
   - `darkMode: 'class'`
   - `content`: Scans `./GiaoDien/**/*.cshtml`, `./WEB_TinhNguyen/GiaoDien/**/*.cshtml`, `./Areas/**/*.cshtml`, `./WEB_TinhNguyen/Areas/**/*.cshtml`, and `wwwroot/js/**/*.js`.
   - `theme.extend`: Volunteer color palette (`volunteer-50` through `950`, `night`, `leaf-bright`, `grass`, `sun`, `sunset`, `sky-soft`, `cream`, `ink`, `leaf-deep`, `leaf`, `forest`), animations (`float`, `pulse-slow`, `heartbeat`, `fade-in-up`), and container queries plugin.
3. **Dual Runtime**:
   - `_Layout.cshtml` and `_CuaHangLayout.cshtml` load both `site.css` (static compiled styles) AND CDN `https://cdn.tailwindcss.com` with inline `tailwind.config = { ... }`.
   - `Styles/app.css` defines base CSS custom properties (`--bg-page`, `--text-main`, `--volunteer-primary`, etc.) for `:root` and `html.dark`.
   - Crucially, neither `Styles/app.css` nor `_Layout.cshtml` sets `overflow-x: hidden` or `overflow-x: clip` on `html` or `body`.

### 1.4 Navigation Header, Footer, and Mobile Navigation Status
1. **Header Navigation (`_Layout.cshtml`, lines 432-515)**:
   - Header is fixed: `<header id="main-header" class="fixed inset-x-0 top-0 z-50 ...">`.
   - Desktop menu: `<nav class="hidden md:flex items-center gap-1 lg:flex">` containing 7 links + "Ký ức" dropdown + theme toggle button (`#themeToggleBtn`).
   - Mobile top bar (`md:hidden`): Contains ONLY the logo + site title on the left, and theme toggle button (`#themeToggleBtnMobile`) on the right.
   - **There is NO hamburger toggle button and NO mobile drawer / slide-out menu in either `_Layout.cshtml` or `_CuaHangLayout.cshtml`.**
2. **Mobile Liquid Bottom Navigation (`_Layout.cshtml` lines 596-642, `_CuaHangLayout.cshtml` lines 299-346)**:
   - `.navigation.fixed.bottom-[15px].md:hidden.lg:hidden` with inline CSS (lines 314-426).
   - Contains 7 fixed items: `Trang chủ`, `Cửa hàng`, `Sự kiện`, `Ký ức`, `Đóng góp`, `Tổ chức`, `Tin tức`, plus an animated circular indicator (`.indicator`, 60px diameter).
   - On a standard 375px mobile screen: `width: 96%` = 360px. Dividing 360px by 7 items yields ~51.4px per item. The 60px circular indicator is wider than each slot. The 10px text labels on `.active` are cramped with minimal tap target spacing.
   - The dropdown for "Ký ức" (which lists past campaign years) is absent from the mobile bottom bar; it only navigates to `year=@currentYear`.
3. **Floating Buttons Collision**:
   - `_Layout.cshtml` line 645: Floating cart button (`#floatingCartBtn`) is placed at `fixed right-5 bottom-24 md:bottom-8 z-40`. On mobile, it floats directly above the 70px bottom nav.
   - `LoiChuc.cshtml` line 109: An additional floating button ("Gửi lời chúc") is fixed at `bottom-[max(1.25rem,env(safe-area-inset-bottom))] right-5 z-40`, directly overlapping the bottom nav and cart button.
4. **Dead Code in Prototype Scripts**:
   - `WEB_TinhNguyen/wwwroot/js/script.js` contains a hamburger drawer implementation (lines 24-44: `.menu-toggle`, `#main-nav`, `is-open`), but this file is NOT linked or referenced in ANY `.cshtml` file in the entire repository.

### 1.5 Admin Style & View Isolation
Direct verification of `/admin` isolation confirms:
1. `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml` does NOT include `~/css/site.css` or `Styles/app.css`. It links ONLY:
   - `<script src="https://cdn.tailwindcss.com"></script>` (line 10)
   - FontAwesome CDN (line 12)
   - Google Fonts CDN (line 14)
   - Embedded `<style>` block (lines 16-226) dedicated to `.sidebar-container`, `.sidebar-header`, `.sidebar-links`.
2. `WEB_TinhNguyen/GiaoDien/Admin/Login.cshtml` declares `Layout = null;` and uses its own self-contained Tailwind CDN script with zero external project CSS dependencies.
3. All admin views are quarantined under `WEB_TinhNguyen/GiaoDien/Admin*` and `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*`.
4. Changes to public views, `_Layout.cshtml`, `_CuaHangLayout.cshtml`, `Styles/app.css`, or `site.css` will have ZERO CSS leakage into `/admin`.

### 1.6 Existing E2E Test Suite Invariants
Running the E2E test harness (`dotnet run --project tests/E2ETests/E2ETests.csproj`) passes **536 out of 536 tests** with 0 failures:
```
================================================================================
                              TEST EXECUTION SUMMARY
================================================================================
  Tier 1: Total: 175 | Passed: 175 | Failed:   0 | Skipped:   0
  Tier 2: Total:  28 | Passed:  28 | Failed:   0 | Skipped:   0
  Tier 3: Total:  18 | Passed:  18 | Failed:   0 | Skipped:   0
  Tier 4: Total:  12 | Passed:  12 | Failed:   0 | Skipped:   0
  Tier 5: Total: 303 | Passed: 303 | Failed:   0 | Skipped:   0
--------------------------------------------------------------------------------
  OVERALL: 536 Tests | 536 Passed | 0 Failed | 0 Skipped in 12.22s
================================================================================
```
The test suite enforces specific verbatim string checks on `_Layout.cshtml`:
- `Tier1_StorefrontTests.cs` (lines 13-140):
  - `F05_T01`: Contains `"CuaHang"`
  - `F05_T02`: Contains `asp-area="CuaHang"` or `/CuaHang`
  - `F05_T03`: Contains `"Cửa Hàng"` or `"Cửa hàng"` or `"Gây Quỹ"`
  - `F05_T04`: Contains `"Trang chủ"` positioned before `"CuaHang"`
  - `F05_T05`: Contains `"hidden md:"`
  - `F06_T01`: Mobile nav contains `"CuaHang"`
  - `F06_T02`: Mobile nav contains `<svg` or `fa-` or `bi-` or `lucide-`
  - `F06_T03`: Contains `"fixed bottom"` or `"bottom-0"` or `"fixed inset-x-0"`
  - `F06_T04`: Contains `"md:hidden"`
  - `F06_T05`: Contains `asp-action="Index"` or `/CuaHang`
- `Tier5_ChallengerM4OrderHarness.cs` (lines 308-318):
  - Declares element with `id="floatingCartBtn"` or `id="floatingCartCount"`
  - Includes SweetAlert2 script `<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>`
- `Tier5_ChallengerM4_2ThemeHarness.cs` (lines 37-145):
  - Declares `id="themeToggleBtn"` (desktop)
  - Declares `id="themeToggleBtnMobile"` (mobile)
  - Inspects `"site_theme"` in localStorage
  - Declares `function toggleThemeMode()`
  - Declares `window.toggleTheme` alias
- `Tier3_CrossFeatureTests.cs`:
  - `C14_AutomaticLayoutSwitching`: `Areas/CuaHang/Views/_ViewStart.cshtml` must contain `_AdminLayout` and `_Layout`.

---

## 2. Logic Chain

1. **Premise 1: User Acceptance Criteria require a responsive mobile navigation menu**:
   - `ORIGINAL_REQUEST.md` specifically states:
     *"Menu điều hướng trên di động hiển thị gọn gàng (dạng hamburger hoặc drawer di động) và đóng/mở hoạt động chính xác khi tương tác trên màn hình nhỏ."*
   - Current observation confirms neither `_Layout.cshtml` nor `_CuaHangLayout.cshtml` has any hamburger button or drawer.
   - Therefore, a hamburger toggle button in the header and an off-canvas drawer must be implemented for public pages.

2. **Premise 2: Existing automated tests enforce the presence of specific mobile nav and bottom elements**:
   - `Tier1_StorefrontTests.cs` requires `_Layout.cshtml` to contain `fixed bottom`, `md:hidden`, `CuaHang`, and icon markup.
   - `Tier5_ChallengerM4OrderHarness.cs` requires `#floatingCartBtn` and `#floatingCartCount`.
   - `Tier5_ChallengerM4_2ThemeHarness.cs` requires `#themeToggleBtnMobile` and `toggleThemeMode()`.
   - Therefore, the mobile drawer must NOT blindly delete or replace these test invariants. The recommended architecture is to introduce the header hamburger button (`#mobileMenuToggleBtn`) and slide-in drawer (`#mobileDrawerNav`) while keeping the existing liquid bottom bar intact (or streamlined with required keywords) so that 100% of the 536 tests pass without regression.

3. **Premise 3: Horizontal scroll prevention (`document.scrollingElement.scrollWidth <= window.innerWidth`) on 375px-430px viewports**:
   - Public layouts currently lack `overflow-x: hidden` / `overflow-x: clip` at the root/body level.
   - Individual views contain absolute positioned decorative SVGs (`TinTuc.cshtml` has SVGs in non-clipped sections) and large padding (`p-8 md:p-14` on countdown cards in `Index.cshtml`).
   - Therefore, adding `overflow-x: clip` (or `overflow-x: hidden` on a top-level wrapper) and tightening mobile paddings (`p-4 sm:p-8`) is necessary and sufficient to prevent unintended horizontal scrollbars on 375px+ screens.

4. **Premise 4: Safeguarding the Admin Subsystem (`/admin`)**:
   - Admin views exclusively use `_AdminLayout.cshtml` and CDN Tailwind, with zero reference to `site.css` or `Styles/app.css`.
   - Therefore, modifying public views, `_Layout.cshtml`, `_CuaHangLayout.cshtml`, or `Styles/app.css` poses 0% risk of CSS or behavioral regression to `/admin`, provided files under `WEB_TinhNguyen/GiaoDien/Admin*` and `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*` remain untouched.

---

## 3. Caveats

1. **Two Public Layouts**: The public application maintains two parallel layouts: `_Layout.cshtml` for general content and `_CuaHangLayout.cshtml` for the fundraising shop. Any layout-level mobile enhancement (such as the hamburger drawer) must be applied symmetrically to both layouts.
2. **CuaHang Light Mode Enforcement**: `_CuaHangLayout.cshtml` deliberately removes dark mode classes. The mobile drawer inside `_CuaHangLayout.cshtml` must respect this constraint and not render a dark mode toggle button.
3. **Database-Driven Inline Font Sizes**: Some views (`Index.cshtml`, `GayQuy.cshtml`, `TinTuc.cshtml`) render inline styles populated from database configuration (e.g., `font-size: @(Model?.Hero_CoChuTieuDe ?? "48px")`). On 375px viewports, these can be oversized if not paired with responsive clamp or max-font scaling.
4. **No Caveats on Admin Isolation**: The boundary between Admin and Public views is 100% strict and verified.

---

## 4. Conclusion

1. **Admin Isolation**: 100% isolated. No styling, views, or controllers in `/admin` are shared with the public layout. Preserving `/admin` is straightforward by establishing a strict boundary: **do not edit any file in `GiaoDien/Admin*`, `Areas/CuaHang/Views/Admin*`, or `_AdminLayout.cshtml`**.
2. **Current Navigation Deficit**: The site has no hamburger menu and no mobile drawer. The 7-item bottom navigation is overly congested on 375px screens and lacks sub-menu navigation (such as campaign years).
3. **Primary Action Items for Public Layout**:
   - **Header**: Add a hamburger button (`#mobileMenuToggleBtn`) with `md:hidden` adjacent to the theme toggle.
   - **Drawer**: Add a clean, accessible mobile drawer (`#mobileDrawerNav`) containing full navigation links, cart status, campaign year sub-links, and quick donation CTA, with backdrop click-to-close and Escape key handling.
   - **Viewport & Overflow**: Upgrade viewport meta tag to `width=device-width, initial-scale=1.0, viewport-fit=cover` and add `overflow-x: clip` on `html, body` or `#public-app-root`.
   - **Floating Buttons**: Adjust z-index and bottom offset on `#floatingCartBtn` to avoid visual collision with bottom navigation and floating CTA buttons.
   - **Dual Layout Consistency**: Mirror the drawer component in `_CuaHangLayout.cshtml` while keeping light-mode lock.
   - **Test Compatibility**: Retain all invariant IDs (`floatingCartBtn`, `floatingCartCount`, `themeToggleBtn`, `themeToggleBtnMobile`) and bottom navigation markers (`fixed bottom`, `md:hidden`, `CuaHang`) to maintain 100% pass rate on all 536 tests.

---

## 5. Verification Method

To independently verify all findings and test proposals:

1. **Build Verification**:
   ```powershell
   dotnet build
   ```
   *Expected result*: Build succeeds with 0 errors, auto-triggering `npm run build:css` via `TailwindCSS` MSBuild target.

2. **Full Regression Test Pass (536/536)**:
   ```powershell
   dotnet run --project tests/E2ETests/E2ETests.csproj
   ```
   *Expected result*: All 536 tests pass across Tiers 1-5 in ~12 seconds.

3. **Admin Isolation Verification**:
   ```powershell
   # Confirm no reference to site.css exists in any admin view or layout
   Get-ChildItem -Path "WEB_TinhNguyen" -Filter "*Admin*.cshtml" -Recurse | Select-String "site.css"
   ```
   *Expected result*: Exactly 0 matches found.

4. **Public Layout Viewport & Navigation Inspection**:
   - Inspect `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` lines 35, 432-515, 596-642.
   - Inspect `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml` lines 26, 183-234, 299-346.
   - Confirm absence of hamburger button and drawer markup.

5. **Invalidation Conditions**:
   - Any modification to `_AdminLayout.cshtml` or `GiaoDien/Admin*` violates constraint R2.
   - Removing `fixed bottom`, `md:hidden`, `CuaHang`, or `#floatingCartBtn` from `_Layout.cshtml` invalidates tests `F06_T01..T05` or `CH_M4_06`.
