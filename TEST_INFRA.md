# E2E Test Infrastructure Specification: Mobile Viewport Optimization & Admin Area Preservation

## 1. Overview & Test Philosophy
This specification defines the automated end-to-end testing infrastructure, viewport matrix, reference oracles, and acceptance criteria for verifying responsive web optimization across mobile viewports (375px to 430px) for all public user views of `WEB_TinhNguyen`, while strictly preserving the integrity and immutability of the administrative subsystem (`/admin`).

### 1.1 Core Testing Principles
1. **Opaque-Box & Requirement-Driven**: Tests validate system behavior, responsive layouts, view outputs, and DOM interactions through genuine browser rendering contexts, HTTP contracts, and Razor markup structure, rather than implementation internals.
2. **Four-Tier Testing Methodology**:
   - **Tier 1 (Feature & Contract Verification)**: Viewport meta tags, mobile navigation hamburger button (`#mobileNavToggle`), off-canvas drawer (`#mobileDrawer`), bottom nav invariants, and responsive form input styling.
   - **Tier 2 (Boundary & Corner Conditions)**: The strict 375px lower boundary (iPhone SE), extreme Vietnamese text lengths, bank transfer memo line breaking, and table overflow constraints.
   - **Tier 3 (Pairwise & Cross-Feature Integration)**: Mobile navigation drawer interactions with backdrop dismissal, keyboard ESC accessibility, and dark/light mode toggle coexistence.
   - **Tier 4 (Real-World Donor & Shopper Scenarios)**: Complete mobile browsing journeys from Home, Campaign Programs, Donation QR modals, to Storefront browsing and Checkout form interaction.
   - **Tier 5 (Adversarial Hardening & Admin Immutability)**: Rapid navigation toggling, viewport resize simulation, and strict validation of zero modifications to the `/admin` area.
3. **Progressive Testability**: Following the `[SKIP] Pending implementation: ...` convention, tests for features under active milestone development gracefully report pending status without failing builds, and immediately activate once the corresponding layout or view changes are merged.
4. **Dual-Layer Browser Verification Architecture**:
   - **Layer 1: C# E2E Test Suite Extension** (`tests/E2ETests/`): Integrated into the 536-test regression engine executable via `dotnet run --project tests/E2ETests/E2ETests.csproj`.
   - **Layer 2: Standalone Headless Chrome Browser Runner** (`tests/run_mobile_viewport_browser_tests.js`): Real Blink layout engine rendering using local Chrome (`C:\Program Files\Google\Chrome\Application\chrome.exe`) with iframe viewport emulation.
5. **Air-Gapped Offline Resilience**: Zero runtime external package installations or cloud dependencies. Operates entirely using pre-installed .NET 10.0, Node.js v24, and local Google Chrome.

---

## 2. Canonical Mobile Viewport Matrix

To guarantee 100% responsive coverage across the standard mobile spectrum specified in `ORIGINAL_REQUEST.md` (375px to 430px), the automated browser test runner tests four canonical viewport widths:

| Viewport Width | Typical Hardware Devices | Aspect Ratio | Test Focus & Stress Areas |
|---|---|---|---|
| **375px** | iPhone SE (2nd/3rd gen), iPhone 12/13 mini | 375 × 667 / 375 × 812 | Strict lower boundary; header logo truncation, countdown timer card width, 7-item bottom bar density, cart table row wrap. |
| **390px** | iPhone 12, 13, 14, 15, 16 standard | 390 × 844 | Predominant mobile iOS resolution; navigation drawer width, donation card padding, hero typography scaling. |
| **412px** | Samsung Galaxy S20–S24, Google Pixel 7/8 | 412 × 915 | Predominant modern Android resolution; card grids, form inputs, modal popup centering, floating action buttons. |
| **430px** | iPhone 14/15/16 Pro Max, Plus models | 430 × 932 | Upper mobile boundary; maximum mobile card scaling, touch target clearance before tablet breakpoint (`sm:640px` / `md:768px`). |

---

## 3. Public Page Coverage Catalog (11 Public Pages)

The test suite systematically iterates across all 11 public user-facing views:

| # | Route / Page Name | Razor View File | Assigned Layout | Key Elements Inspected |
|---|---|---|---|---|
| 1 | **Home / Index** (`/`) | `WEB_TinhNguyen/GiaoDien/Home/Index.cshtml` | `_Layout.cshtml` | Hero banner, countdown cards, activity cards, sponsor logos, floating wish button. |
| 2 | **Home / Chương Trình** (`/Home/ChuongTrinh`) | `WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml` | `_Layout.cshtml` | Timeline schedule table, activity roadmap, image gallery. |
| 3 | **Home / Gây Quỹ** (`/Home/GayQuy`) | `WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml` | `_Layout.cshtml` | Donation cards, dynamic QR modal (`#qrModal`), bank memo `break-all`, copy buttons. |
| 4 | **Home / Ký Ức** (`/Home/KyUc`) | `WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml` | `_Layout.cshtml` | Multi-year campaign gallery, memory image grid, year selector. |
| 5 | **Home / Ban Tổ Chức** (`/Home/BanToChuc`) | `WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml` | `_Layout.cshtml` | Leadership team cards, avatar images, social contact links. |
| 6 | **Home / Tin Tức** (`/Home/TinTuc`) | `WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml` | `_Layout.cshtml` | Article card grid, news reader modal (`#newsReaderModal`), article headings. |
| 7 | **Home / Lời Chúc** (`/Home/LoiChuc`) | `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml` | `_Layout.cshtml` | Donor guestbook list, wish submission form, pagination controls. |
| 8 | **Home / Privacy** (`/Home/Privacy`) | `WEB_TinhNguyen/GiaoDien/Home/Privacy.cshtml` | `_Layout.cshtml` | Policy document text containers, typography readability. |
| 9 | **Cửa Hàng / Index** (`/CuaHang`) | `WEB_TinhNguyen/Areas/CuaHang/Views/CuaHang/Index.cshtml` | `_CuaHangLayout.cshtml` | Product catalog grid, live search filter bar, charity badges. |
| 10 | **Cửa Hàng / Chi Tiết** (`/CuaHang/ChiTiet/{id}`) | `WEB_TinhNguyen/Areas/CuaHang/Views/CuaHang/ChiTiet.cshtml` | `_CuaHangLayout.cshtml` | Product detail image gallery, price badges, quantity selector, add-to-cart button. |
| 11 | **Cửa Hàng / Giỏ Hàng** (`/CuaHang/GioHang`) | `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/Index.cshtml` | `_CuaHangLayout.cshtml` | Cart item list table, checkout form inputs (font-size >= 16px), place order CTA. |
| 12* | **Cửa Hàng / Đặt Hàng Thành Công** | `WEB_TinhNguyen/Areas/CuaHang/Views/GioHang/DatHangThanhCong.cshtml` | `_CuaHangLayout.cshtml` | Dynamic order QR card, bank transfer instructions, continue shopping CTA. |

*\*Bonus view tested within the Store checkout flow.*

---

## 4. Pass / Fail Criteria & Measurement Contracts

### 4.1 Zero Horizontal Scroll Contract (AC2)
- **Mathematical Formula**:
  $$\text{document.scrollingElement.scrollWidth} \le \text{window.innerWidth}$$
- **Pass Criterion**: Evaluated at all 4 canonical mobile viewports (375px, 390px, 412px, 430px). Absolutely zero horizontal overflow permitted.
- **Root Enforcement**: Top-level containers or body declare `overflow-x: clip` or `overflow-x: hidden`.

### 4.2 Mobile Navigation Hamburger & Drawer Contract (AC3)
- **Hamburger Button**:
  - Element selector: `#mobileNavToggle` (or `#mobileMenuToggleBtn`).
  - Visibility: Displayed on mobile viewports (`md:hidden`), with minimum touch target $\ge 40\text{px} \times 40\text{px}$.
  - Accessibility: Declares `aria-expanded="false"`, `aria-controls="mobileDrawer"`, and descriptive `aria-label`.
- **Off-Canvas Drawer (`#mobileDrawer`)**:
  - Fixed position sliding drawer (`fixed inset-y-0 right-0 z-50 transform transition-transform`).
  - Backdrop overlay (`#mobileDrawerBackdrop`).
  - Close button (`#mobileNavClose`).
  - Interaction Pass Criteria:
    1. Initial state: Drawer closed (`aria-expanded="false"`, hidden or translated off-screen).
    2. Click `#mobileNavToggle`: Drawer transitions to open (`aria-expanded="true"`, visible inside viewport).
    3. Click `#mobileNavClose` or backdrop: Drawer transitions to closed (`aria-expanded="false"`, hidden).

### 4.3 Responsive Media & Content Scaling (AC4)
- **Images**: Every `img` element satisfies $\text{img.offsetWidth} \le \text{window.innerWidth}$ and maintains responsive utility classes (`max-w-full h-auto`).
- **Tables**: All `table` elements either satisfy $\text{table.offsetWidth} \le \text{window.innerWidth}$ OR are enclosed within an explicitly scrollable wrapper container (`overflow-x-auto`).
- **Forms & Inputs**:
  - Form inputs (`<input>`, `<select>`, `<textarea>`) satisfy $\text{input.offsetWidth} \le \text{window.innerWidth}$.
  - iOS Auto-Zoom Prevention: Computed `font-size` on form text inputs must be $\ge 16\text{px}$ (e.g., `text-base` or `text-base sm:text-sm`) on mobile viewports to prevent Apple MobileSafari viewport auto-zoom behavior.
  - Touch Target Sizing: Primary call-to-action buttons and form inputs maintain height $\ge 40\text{px}$ (`min-h-[40px]` or `py-2.5+`).

### 4.4 Invariant Preservation & Administrative Immutability Contract (AC5, R2)
- **Public Navigation Invariants**: Must NOT be broken or removed:
  - `#floatingCartBtn` and `#floatingCartCount`.
  - `#themeToggleBtn` (desktop) and `#themeToggleBtnMobile` (mobile) in `_Layout.cshtml`.
  - Store isolation: Zero theme toggle buttons in `_CuaHangLayout.cshtml`.
  - Verbatim tokens: `"CuaHang"`, `"hidden md:"`, `"fixed bottom"`, `"md:hidden"`.
- **Admin Area Immutability**:
  - Exactly **ZERO** files modified in `WEB_TinhNguyen/GiaoDien/Admin*/**`, `WEB_TinhNguyen/BoDieuKhien/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*/**`, or `_AdminLayout.cshtml`.
  - 100% pass across all 536 baseline E2E tests and 204 admin auth tests.

---

## 5. Automated Browser Test Architecture

```
                                 TEST INFRASTRUCTURE
                                          │
            ┌─────────────────────────────┴─────────────────────────────┐
            ▼                                                           ▼
 LAYER 1: C# E2E Test Suite                               LAYER 2: Standalone Browser Runner
 Location: tests/E2ETests/                                Location: tests/run_mobile_viewport_browser_tests.js
 Engine: dotnet run --project tests/E2ETests/             Engine: node tests/run_mobile_viewport_browser_tests.js
 Scope:                                                   Scope:
 - [E2ETest] attributes integrated into 536+ suite        - Real Blink Chrome headless execution
 - Meta viewport & root overflow markup                   - Canonical viewports: 375px, 390px, 412px, 430px
 - Mobile hamburger & drawer Razor contracts              - Zero horizontal scroll measurement (scrollWidth)
 - Invariant & touch target validation                    - Real DOM click event on #mobileNavToggle
 - Strict admin file immutability audit                   - Form font-size & responsive media inspection
```

### 5.1 Standalone Browser Runner (`tests/run_mobile_viewport_browser_tests.js`)
- **Binary**: Local Google Chrome (`C:\Program Files\Google\Chrome\Application\chrome.exe`).
- **Flags**: `--headless=new --no-sandbox --disable-gpu --allow-file-access-from-files --disable-web-security --virtual-time-budget=3000 --dump-dom`.
- **Emulation Technique**: Generates preprocessed HTML test pages with compiled `site.css` and loads them inside an `<iframe>` container configured to exact viewport dimensions (375px, 390px, 412px, 430px).
- **DOM Evaluation**: Executes in-page JavaScript measuring `window.innerWidth`, `scrollingElement.scrollWidth`, element bounding boxes, click events, and style declarations. Outputs structured JSON report embedded in the DOM.

### 5.2 C# E2E Test Extensions (`tests/E2ETests/`)
- `Tier1/Tier1_MobileViewportTests.cs`: Feature coverage for meta viewport, mobile hamburger button, drawer navigation, bottom bar invariants, break-all transfer memo, and input font sizes.
- `Tier4/Tier4_AdminImmutabilityTests.cs`: Real-world scenario assertions verifying 0 admin files touched, admin layout style independence, and route security.

---

## 6. How to Run the Tests

### 6.1 Execute the Standalone Automated Browser Runner (All Viewports & Pages)
```powershell
node tests/run_mobile_viewport_browser_tests.js
```

### 6.2 Execute Browser Tests for a Specific Viewport (e.g. 375px lower boundary)
```powershell
node tests/run_mobile_viewport_browser_tests.js --viewport 375
```

### 6.3 Execute Browser Tests for a Specific Page
```powershell
node tests/run_mobile_viewport_browser_tests.js --page home_index
```

### 6.4 Execute the C# E2E Test Suite (All Tiers)
```powershell
dotnet run --project tests/E2ETests/E2ETests.csproj -- --all
```

### 6.5 Execute Mobile Viewport Tests Only in C# Suite
```powershell
dotnet run --project tests/E2ETests/E2ETests.csproj -- --filter MobileViewport
```

### 6.6 Verify Admin Authorization & Route Protection
```powershell
dotnet run --project tests/M2Challenger_Auth/M2Challenger_Auth.csproj
```
