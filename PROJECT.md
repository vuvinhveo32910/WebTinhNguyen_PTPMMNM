# Project: Tối Ưu Hóa Responsive Web Giao Diện Người Dùng Bên Ngoài & Bảo Toàn Khu Vực Admin (/admin)

## Architecture
- **Framework & Runtime**: .NET 10.0 (`net10.0`), ASP.NET Core MVC with Entity Framework Core 10.0.12, Tailwind CSS (compiled via `npm run build:css` into `wwwroot/css/site.css`), and JavaScript.
- **Tripartite Layout System**:
  - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`: Main public layout for all Home views. Supports dark/light mode toggle, fixed header with desktop nav, mobile hamburger button, mobile drawer menu, and mobile bottom nav.
  - `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`: Public storefront layout for fundraising store & cart. Dedicated light-mode lock (no dark mode button, zero theme leakage).
  - `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`: Strictly protected administrative layout. Uses CDN Tailwind and dedicated internal stylesheet. Completely isolated from public layouts.
- **Mobile Navigation Architecture**:
  - Header: Desktop navigation hidden on small screens (`md:flex`), replaced by Mobile Hamburger Toggle (`#mobileNavToggle`).
  - Mobile Drawer (`#mobileDrawer`): Accessible off-canvas drawer with smooth slide-in, backdrop overlay, close button (`#mobileNavClose`), ESC key listener, full route links, and campaign year submenu.
  - Bottom Navigation: Preserves existing invariants (`fixed bottom`, `md:hidden`, `CuaHang`, `#floatingCartBtn`, `#themeToggleBtnMobile`) while eliminating visual overlaps with floating buttons.
- **Viewport & Overflow Control**:
  - Meta Viewport: `<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">`.
  - Global Horizontal Scroll Prevention: `overflow-x: clip` / `overflow-x: hidden` on root containers.
- **Strict Admin Boundary (/admin)**:
  - ZERO changes to `WEB_TinhNguyen/GiaoDien/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*`, `WEB_TinhNguyen/BoDieuKhien/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*`, and `_AdminLayout.cshtml`.
  - 100% preservation of 536 baseline tests and 204 admin auth tests.
- **Dual-Layer Automated Browser Testing**:
  - Layer 1: C# E2E test suite extension in `tests/E2ETests/` for `dotnet run --project tests/E2ETests/E2ETests.csproj`.
  - Layer 2: Standalone Node.js headless Chrome test runner (`tests/run_mobile_viewport_browser_tests.js`) verifying 375px, 390px, 412px, 430px viewports with real Blink rendering context.

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | Viewport & Root Overflow Standardization | Update viewport meta tags to `width=device-width, initial-scale=1.0, viewport-fit=cover` and add global `overflow-x: clip`/`hidden`. | M1 | ORIGINAL_REQUEST §R1 |
| 2 | Mobile Hamburger Button & Drawer Navigation | Implement responsive hamburger button in header and off-canvas drawer with links, backdrop, and keyboard access on `_Layout.cshtml` & `_CuaHangLayout.cshtml`. | M1 | ORIGINAL_REQUEST §R1, §AC3 |
| 3 | Mobile Bottom Nav & Floating Action De-confliction | Rebalance bottom nav spacing on 375px screens and adjust floating button offsets (`#floatingCartBtn`, wish button) to eliminate overlap while keeping test invariants. | M1 | ORIGINAL_REQUEST §R1 |
| 4 | Responsive Typography & Headings Normalization | Normalize oversized inline font sizes (`TinTuc` 64px, `GayQuy` 48px, `Index` text-5xl) using responsive classes (`text-2xl sm:text-4xl md:text-5xl`). | M2 | ORIGINAL_REQUEST §R1, §AC4 |
| 5 | Public Home Views Layout & Card Grid Scaling | Adjust container paddings, card widths, countdown boxes, and sponsor logos across all 8 Home views to fit 375px-430px without horizontal scroll. | M2 | ORIGINAL_REQUEST §R1, §AC2 |
| 6 | Touch Target & Bank Memo Safety in Public Views | Ensure touch targets >= 40px (social buttons, CTAs) and add `break-all` on donation transfer memos in `GayQuy.cshtml`. | M2 | ORIGINAL_REQUEST §R1, §AC4 |
| 7 | Storefront & Product Detail Mobile Optimization | Make product card grid, live search filter bar, and product detail add-to-cart controls fully responsive on mobile screens. | M3 | ORIGINAL_REQUEST §R1, §AC4 |
| 8 | Cart & Checkout Form iOS Auto-Zoom & Overflow Fix | Upgrade checkout form inputs from `text-sm` (14px) to `text-base sm:text-sm` (16px) to eliminate iOS Safari auto-zoom; rebalance cart row width on 375px. | M3 | ORIGINAL_REQUEST §R1, §AC2, §AC4 |
| 9 | Dual-Layer Automated Browser Test Suite | Implement automated browser tests covering 4 viewports (375px, 390px, 412px, 430px) for all 11 public pages, verifying zero horizontal scroll, hamburger toggle, and responsive elements. | M4 | ORIGINAL_REQUEST §R1, §AC1-4 |
| 10 | Admin Isolation & Regression Verification Suite | Verify 100% preservation of all `/admin` routes, controllers, views, styles, and full passing status of 536 baseline tests + 204 admin auth tests. | M5 | ORIGINAL_REQUEST §R2, §AC5-6 |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Public Layouts & Mobile Navigation Infrastructure | `_Layout.cshtml`, `_CuaHangLayout.cshtml`, `Styles/app.css`, `tailwind.config.js`: Viewport update, root overflow clip, mobile hamburger button, slide-in drawer menu, bottom nav & floating button deconfliction, preserving test invariants and CuaHang light-mode lock. | None | DONE |
| M2 | Public Home Views Responsive Optimization | `GiaoDien/Home/` (Index, ChuongTrinh, GayQuy, BanToChuc, TinTuc, KyUc, LoiChuc, Privacy): Responsive headings, countdown padding, bank memo `break-all`, touch targets >= 40px, floating wish button relocation, zero horizontal scroll. | M1 | DONE |
| M3 | Storefront, Cart & Checkout Mobile Optimization | `Areas/CuaHang/Views/` (CuaHang/Index, CuaHang/ChiTiet, GioHang/Index, GioHang/DatHangThanhCong): Input font-size >= 16px to stop iOS auto-zoom, cart item row responsive restructuring on 375px, touch target sizing, store isolation preservation. | M1 | DONE |
| M4 | E2E Testing Track: Automated Mobile Browser Test Suite | `tests/run_mobile_viewport_browser_tests.js` (headless Chrome iframe runner) + `tests/E2ETests/` responsive assertions: 375px, 390px, 412px, 430px viewports across all 11 public pages; zero horizontal scroll verification; hamburger toggle click interaction; publish `TEST_READY.md`. | None | DONE |
| M5 | Final E2E Verification & Adversarial Hardening | Execute 100% of automated browser tests across all viewports; verify `dotnet build` succeeds with 0 errors; verify 536 baseline E2E tests and 204 admin auth tests pass; verify zero changes to `/admin`. | M1, M2, M3, M4 | DONE |

## Interface Contracts
### Mobile Navigation Contract (`_Layout.cshtml` & `_CuaHangLayout.cshtml`)
- **Hamburger Button**:
  - Element ID: `#mobileNavToggle` (or `#mobileMenuToggleBtn`)
  - Classes: `md:hidden inline-flex items-center justify-center p-2 rounded-lg`
  - Attributes: `aria-expanded="false"`, `aria-controls="mobileDrawer"`, `aria-label="Mở menu điều hướng"`
- **Mobile Drawer**:
  - Element ID: `#mobileDrawer`
  - Classes: Off-canvas fixed panel (`fixed inset-y-0 right-0 z-50 w-72 max-w-[85vw] transform transition-transform duration-300 translate-x-full`)
  - Backdrop: `#mobileDrawerBackdrop` (`fixed inset-0 bg-black/50 z-40 hidden`)
  - Close Button: `#mobileNavClose` (`aria-label="Đóng menu"`)
- **Required Invariant IDs & Attributes (Must NOT be removed)**:
  - `#floatingCartBtn` and `#floatingCartCount`
  - `#themeToggleBtn` (desktop) and `#themeToggleBtnMobile` (mobile) in `_Layout.cshtml`
  - Verbatim strings in `_Layout.cshtml`: `"CuaHang"`, `"hidden md:"`, `"fixed bottom"`, `"md:hidden"`
  - Zero theme toggle buttons in `_CuaHangLayout.cshtml`

### Horizontal Scroll Contract
- Every public page must satisfy: `document.scrollingElement.scrollWidth <= window.innerWidth` for any viewport width in `[375px, 430px]`.
- Top-level wrapper or body must enforce `overflow-x: clip` or `overflow-x: hidden`.

### Admin Preservation Contract
- Absolute file immutability for all files matching:
  - `WEB_TinhNguyen/GiaoDien/Admin*/**`
  - `WEB_TinhNguyen/BoDieuKhien/Admin*`
  - `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*`
  - `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*/**`
  - `WEB_TinhNguyen/GiaoDien/Shared/_AdminLayout.cshtml`

## Code Layout
- `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`: Main public layout (Header, Drawer, Content, Bottom Nav, Footer).
- `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`: Storefront layout.
- `Styles/app.css`: Tailwind input CSS (root properties, overflow rules).
- `WEB_TinhNguyen/GiaoDien/Home/`: Public Home views (8 views).
- `WEB_TinhNguyen/Areas/CuaHang/Views/CuaHang/`: Public store views (Index, ChiTiet).
- `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/`: Public cart & checkout views (Index, DatHangThanhCong).
- `tests/run_mobile_viewport_browser_tests.js`: Standalone automated browser test runner using local headless Chrome.
- `tests/E2ETests/Tier1/` & `Tier4/`: C# E2E test extensions for responsive assertions.
- `TEST_READY.md`: Signal that the E2E test suite is complete.
