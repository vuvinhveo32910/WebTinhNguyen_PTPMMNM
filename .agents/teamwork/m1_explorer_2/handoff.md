# Investigation Report: Mobile Bottom Navigation & Floating Action Buttons De-confliction

**Explorer Role**: Bottom Navigation & Floating Buttons Explorer (`m1_explorer_2`)  
**Assigned Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\`  
**Date**: 2026-11-07  
**Milestone**: M1 - Public Layouts & Mobile Navigation Infrastructure  

---

## 1. Observation

### 1.1 Architecture and Layout Distribution of Mobile Bottom Navigation
The application employs a custom "Liquid Magic Bottom Navigation" with an animated circular indicator across both public layouts:
1. **Root Public Layout (`WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`)**:
   - Inline styles define `.navigation`, `.indicator`, and list item typography in lines 314–426.
   - HTML markup is rendered at lines 596–642:
     ```html
     <!-- THANH LIQUID BOTTOM NAVIGATION MOBILE -->
     <div class="navigation fixed bottom-[15px] md:hidden lg:hidden">
         <ul>
             <li class="list" data-url="/home/index">...</li>
             <li class="list" data-url="/cuahang">...</li>
             <li class="list" data-url="/home/chuongtrinh">...</li>
             <li class="list" data-url="/home/kyuc">...</li>
             <li class="list" data-url="/home/gayquy">...</li>
             <li class="list" data-url="/home/bantochuc">...</li>
             <li class="list" data-url="/home/tintuc">...</li>
             <div class="indicator"></div>
         </ul>
     </div>
     ```
   - Dynamic JavaScript controller at lines 718–746:
     ```javascript
     const listItems = document.querySelectorAll('.navigation .list');
     const indicator = document.querySelector('.navigation .indicator');
     function setActiveBottomNav() {
         let currentPath = window.location.pathname.toLowerCase();
         if (currentPath === '/' || currentPath === '') currentPath = '/home/index';
         let itemWidth = 100 / listItems.length;
         listItems.forEach((item, index) => {
             item.classList.remove('active');
             let itemPath = item.getAttribute('data-url');
             if (currentPath.includes(itemPath) || (itemPath === '/home/index' && currentPath === '/')) {
                 item.classList.add('active');
                 let centerPosition = (index * itemWidth) + (itemWidth / 2);
                 if (indicator) {
                     indicator.style.left = `calc(${centerPosition}%)`;
                 }
             }
         });
     }
     ```

2. **Storefront Layout (`WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`)**:
   - Inline styles define identical `.navigation`, `.indicator` rules in lines 79–177.
   - HTML markup is rendered at lines 300–346, with `li.list.active` preset on item 2 (`/cuahang`) and `<div class="indicator" style="left: calc(21.4%);"></div>`.
   - The storefront layout does not define `setActiveBottomNav()` in its inline script (lines 362–367 only handle dark mode stripping).

### 1.2 Mathematical Geometry Analysis on 375px Viewports
Direct measurement and CSS property inspection reveal a severe physical clipping and overflow defect on 375px screens (such as iPhone SE, iPhone 12/13 Mini, and standard 360px–375px mobile viewports):

1. **Container Dimensions**:
   - Current CSS: `width: 96%; height: 70px; border-radius: 20px;` (no `max-width` constraint).
   - On a standard **375px** viewport:
     $$\text{Container Width} = 375\text{px} \times 0.96 = 360\text{px}$$
   - On a **360px** viewport (common Android devices):
     $$\text{Container Width} = 360\text{px} \times 0.96 = 345.6\text{px}$$

2. **Per-Item Slot Geometry (7 Items)**:
   - There are 7 items with `flex: 1`:
     $$\text{Item Width (375px)} = \frac{360\text{px}}{7} \approx 51.43\text{px}$$
     $$\text{Item Width (360px)} = \frac{345.6\text{px}}{7} \approx 49.37\text{px}$$

3. **Current Indicator Dimensions vs. Slot Width**:
   - In lines 402–414 (`_Layout.cshtml`) and 158–170 (`_CuaHangLayout.cshtml`):
     ```css
     .navigation .indicator {
         position: absolute;
         top: -36px;
         width: 60px;
         height: 60px;
         background: #ffffff;
         border-radius: 50%;
         border: 6px solid #222327;
         box-sizing: content-box;
         transform: translateX(-50%);
         ...
     }
     ```
   - **Crucial Discovery**: Because `box-sizing: content-box` is specified with `border: 6px solid`, the **total outer diameter** of the indicator is:
     $$\text{Outer Diameter} = 60\text{px} + 6\text{px} + 6\text{px} = 72\text{px}$$
     $$\text{Outer Radius} = 36\text{px}$$
   - **Comparison**:
     - Slot width is only **51.43px**, but the indicator is **72px** wide!
     - The indicator is **20.57px wider** than the slot it sits on ($72 - 51.43 = +20.57\text{px}$).

4. **Container Edge Overflow**:
   - For **Item 0 (`Trang chủ`)**:
     - Center of slot 0 is at $x = \frac{51.43}{2} = 25.71\text{px}$.
     - Left boundary of indicator: $25.71\text{px} - 36\text{px} = -10.29\text{px}$.
     - **Result**: The indicator hangs **10.3px outside the left edge** of `.navigation`, breaking through the rounded container corner.
   - For **Item 6 (`Tin tức`)**:
     - Center of slot 6 is at $x = 360 - 25.71 = 334.29\text{px}$.
     - Right boundary of indicator: $334.29\text{px} + 36\text{px} = 370.29\text{px}$.
     - **Result**: The indicator hangs **10.3px outside the right edge** of `.navigation`.
   - For **Adjacent Icons**:
     - Center-to-center distance between items is $51.43\text{px}$.
     - Indicator outer radius is $36\text{px}$. Distance from center of neighbor item to indicator edge:
       $$51.43\text{px} - 36\text{px} = 15.43\text{px}$$
     - Inactive icon font size is `1.4rem` ($22.4\text{px}$, radius $11.2\text{px}$).
     - Remaining clearance between neighbor icon and indicator ring:
       $$15.43\text{px} - 11.2\text{px} = 4.23\text{px}$$
     - On 360px screens, clearance drops to $13.37\text{px} - 11.2\text{px} = 2.17\text{px}$ (practically touching).

5. **Text Label Visibility Mechanics**:
   - Inspection of lines 382–400 (`_Layout.cshtml`) and 138–156 (`_CuaHangLayout.cshtml`):
     ```css
     .navigation ul li a .text {
         position: absolute;
         bottom: -20px;
         left: 50%;
         transform: translateX(-50%);
         color: #ffffff;
         font-weight: 600;
         font-size: 10px;
         letter-spacing: 0.05em;
         transition: 0.5s cubic-bezier(0.68, -0.55, 0.265, 1.55);
         opacity: 0;
         pointer-events: none;
         white-space: nowrap;
     }
     .navigation ul li.active a .text {
         opacity: 1;
         bottom: 12px;
     }
     ```
   - **Crucial Finding**: Inactive items have `opacity: 0; pointer-events: none;`. **Only the single active item** displays its text label at any one time!
   - Therefore, the 7 labels do *not* collide with each other simultaneously. However, the single active label (e.g., "Trang chủ" ~50px, "Cửa hàng" ~48px, "Đóng góp" ~48px) fills virtually 100% of the 51.4px slot width.

### 1.3 Floating Action Buttons Conflict Analysis
Direct codebase search for fixed-position action elements (`fixed.*bottom`) identified 3 interacting elements on public pages:

| Element | Location | Coordinates | Z-Index | Mobile Behavior (< 768px) | Desktop Behavior (>= 768px) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`.navigation`** | `_Layout.cshtml:596`<br>`_CuaHangLayout.cshtml:300` | `fixed bottom-[15px] left-1/2 -translate-x-1/2 w-[96%]` | `z-index: 9999` (inline CSS) | Spans $y = 15\text{px}$ to $85\text{px}$ (top of indicator reaches $y = 109\text{px}$) | Hidden via `md:hidden lg:hidden` |
| **`#floatingCartBtn`** | `_Layout.cshtml:645`<br>`_CuaHangLayout.cshtml:349` | `fixed right-5 bottom-24 md:bottom-8 z-40` | `z-40` (`z-index: 40`) | Sits at `bottom: 96px`, spans $y = 96\text{px}$ to $144\text{px}$ | Sits at `bottom: 32px`, spans $y = 32\text{px}$ to $80\text{px}$ |
| **Floating Wish Button** | `GiaoDien/Home/LoiChuc.cshtml:109` | `fixed bottom-[max(1.25rem,env(safe-area-inset-bottom))] right-5 z-40` | `z-40` (`z-index: 40`) | Sits at `bottom: 20px`, spans $y = 20\text{px}$ to $76\text{px}$ | Sits at `bottom: 20px`, spans $y = 20\text{px}$ to $76\text{px}$ |

#### Verbatim Conflicts Observed on `GiaoDien/Home/LoiChuc.cshtml`:
1. **Mobile (< 768px) Collision**:
   - The Wish button is placed at `bottom: 20px` ($1.25\text{rem}$) and `right-5` ($20\text{px}$).
   - The `.navigation` bar covers `bottom: 15px` to `85px`, stretching across $96\%$ of screen width.
   - On a 375px viewport, `.navigation` covers $x = 7.5\text{px}$ to $367.5\text{px}$.
   - The Wish button sits at $x = 355\text{px}, y = 20\text{px}$ with `z-index: 40`.
   - `.navigation` has `z-index: 9999`.
   - **Verbatim Error**: The Wish button is **completely covered and blocked behind** the 7th item ("Tin tức") of `.navigation`. Any tap at that location activates the "Tin tức" bottom navigation route instead of the Wish button!
2. **Desktop (>= 768px) Collision**:
   - `.navigation` is hidden (`md:hidden`).
   - `#floatingCartBtn` is at `right-5, bottom-8` ($y = 32\text{px}$ to $80\text{px}$, `z-40`).
   - The Wish button is at `right-5, bottom: 20px` ($y = 20\text{px}$ to $76\text{px}$, `z-40`).
   - **Verbatim Error**: Both buttons occupy the exact same horizontal position (`right-5`) and overlap by $44\text{px}$ vertically ($[32\text{px}, 76\text{px}]$). The buttons clip into each other, creating an unusable UI defect.

### 1.4 Test Suite Baseline and Invariant Preservation
Execution of the test harness confirms 100% baseline pass rate:
- `dotnet run --project tests/E2ETests/E2ETests.csproj`: **536 / 536 passed** (0 failed).
- `node tests/adversarial_final_stress_harness.js`: **201 / 201 passed** (0 failed).
- `node tests/adversarial_store_isolation_test.js`: **109 / 109 passed** (0 failed).
- Total: **846 / 846 passing tests**.

All layout tests enforce exact verbatim string invariants:
1. `Tier1_StorefrontTests.cs` (lines 81–146):
   - `layoutContent.Contains("CuaHang")`
   - `layoutContent.Contains("<svg") || layoutContent.Contains("fa-") || layoutContent.Contains("bi-") || layoutContent.Contains("lucide-")`
   - `layoutContent.Contains("fixed bottom") || layoutContent.Contains("bottom-0") || layoutContent.Contains("fixed inset-x-0")`
   - `layoutContent.Contains("md:hidden")`
   - `layoutContent.Contains("CuaHang") && (layoutContent.Contains("asp-action=\"Index\"") || layoutContent.Contains("/CuaHang"))`
2. `Tier5_ChallengerM4OrderHarness.cs` (lines 305–319):
   - `layoutContent.Contains("floatingCartBtn") || layoutContent.Contains("floatingCartCount")`
   - `layoutContent.Contains("asp-controller=\"GioHang\"") || layoutContent.Contains("CuaHang/GioHang") || layoutContent.Contains("asp-area=\"CuaHang\"")`
   - `layoutContent.Contains("sweetalert2") || layoutContent.Contains("Swal")`
3. `Tier5_ChallengerM4_2ThemeHarness.cs` (lines 33–146):
   - `id="themeToggleBtn"` and `id="themeToggleBtnMobile"`
   - `function toggleThemeMode()`, `localStorage.setItem('site_theme'`, `themeChanged`, `window.toggleTheme`
   - `_CuaHangLayout.cshtml` must enforce `class="light"` and MUST NOT contain `themeToggleBtn` or `toggleThemeMode`.

---

## 2. Logic Chain

1. **Premise 1: 7-Item Liquid Navigation Must Fit on 375px Screens Without Overflow**:
   - Slot width on 375px screen is $51.43\text{px}$.
   - If the circular indicator's total outer diameter is scaled down to **44px** (using `box-sizing: border-box; width: 44px; height: 44px; border: 4px solid ...;`), the outer radius is $22\text{px}$.
   - When item 0 is active (center at $25.71\text{px}$), the left edge is at $25.71 - 22 = \mathbf{+3.71\text{px}}$, which remains completely inside the container's left rounded edge.
   - When item 6 is active (center at $334.29\text{px}$), the right edge is at $334.29 + 22 = \mathbf{356.29\text{px}}$, safely $3.71\text{px}$ inside the 360px container.
   - Distance from active indicator to neighbor icon center is $51.43 - 22 = 29.43\text{px}$. With an inactive icon size of $1.15\text{rem}$ ($18\text{px}$, radius $9\text{px}$), clearance is $29.43 - 9 = \mathbf{20.43\text{px}}$.
   - Therefore, a 44px outer diameter with 4px border completely eliminates clipping, overflows, and collision with neighboring icons on 375px screens.

2. **Premise 2: Navigation Height, Icon Positioning, and Text Scaling**:
   - Setting `.navigation` height to **64px** on screens $\le 430\text{px}$ (from 70px) creates a balanced aspect ratio.
   - Inactive icons centered at `top: 50%; transform: translate(-50%, -50%);` sit at $y = 32\text{px}$.
   - Indicator circle at `top: -24px; height: 44px;` spans $y = -24\text{px}$ to $+20\text{px}$ (vertical center at $-2\text{px}$).
   - Active icon with `top: 0px` (or `top: 2px`) and `font-size: 1.25rem` ($20\text{px}$) centers inside the indicator ring ($36\text{px}$ inner diameter) with $8\text{px}$ border clearance.
   - Active text label with `font-size: 8.5px`–`9px`, `letter-spacing: normal;`, at `bottom: 8px`–`10px` fits within the $51.43\text{px}$ slot with zero overlap.
   - Max-width capping: Adding `max-width: 440px;` prevents the bottom navigation pill from stretching excessively on larger mobile screens/tablets ($430\text{px}$ to $767\text{px}$).

3. **Premise 3: Preserving JavaScript Percentage Mathematics**:
   - The JavaScript calculates indicator position as:
     $$\text{itemWidth} = \frac{100}{\text{listItems.length}} = \frac{100}{7} \approx 14.2857\%$$
     $$\text{centerPosition} = (i \times \text{itemWidth}) + \frac{\text{itemWidth}}{2}$$
   - Because `indicator.style.left` uses `calc(${centerPosition}%)` with CSS `transform: translateX(-50%)`, the horizontal alignment is strictly percentage-based and independent of pixel width.
   - Capping container width or scaling the indicator circle does *not* require any changes to the core JavaScript calculation, guaranteeing 100% backwards compatibility.

4. **Premise 4: Floating Buttons De-confliction Strategy**:
   - **Mobile (< 768px)**:
     - Layer 0: `.navigation` pill at `bottom: 15px`, height $64\text{px}$, top edge at $79\text{px}$ ($15 + 64$).
     - Layer 1: `#floatingCartBtn` at `bottom-24` ($96\text{px}$, $6\text{rem}$), right edge `right-5`. Top edge of bottom nav is $79\text{px}$; bottom edge of cart button is $96\text{px}$. Clearance is $\mathbf{17\text{px}}$ of clear space.
     - Layer 2: On `LoiChuc.cshtml`, the floating wish button can be positioned in one of two non-colliding coordinates:
       - **Strategy A (Vertical Stacking on Right)**: Move Wish button to `bottom-[10.5rem]` ($168\text{px}$) on mobile, and `md:bottom-24` ($96\text{px}$) on desktop.
         - On mobile: Nav ($15\text{px}$–$79\text{px}$) $\to$ Cart ($96\text{px}$–$144\text{px}$) $\to$ Wish ($168\text{px}$–$218\text{px}$). Clear $24\text{px}$ gap between Cart and Wish.
         - On desktop: Cart ($32\text{px}$–$80\text{px}$) $\to$ Wish ($96\text{px}$–$146\text{px}$). Clear $16\text{px}$ gap between Cart and Wish.
       - **Strategy B (Opposite Sides: Left Wish, Right Cart)**: Move Wish button to `left-5 bottom-24 md:bottom-8`.
         - On mobile: Cart button floats at `right-5 bottom-24`, Wish button floats at `left-5 bottom-24`. Both sit $17\text{px}$ above `.navigation`, zero vertical crowding, balanced bilateral ergonomics.
         - On desktop: Cart button at `right-5 bottom-8`, Wish button at `left-8 bottom-8`.
     - **Recommendation**: Both strategies are 100% collision-free. **Strategy A** maintains right-hand thumb accessibility, while **Strategy B** prevents vertical stacking clutter. Detailed code for both is provided below.

5. **Premise 5: Zero Admin Leakage and Test Invariant Preservation**:
   - All proposed changes are strictly confined to:
     - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`
     - `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`
     - `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml`
   - No files in `WEB_TinhNguyen/GiaoDien/Admin*` or `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*` are modified.
   - All invariant tokens (`fixed bottom`, `md:hidden`, `CuaHang`, `floatingCartBtn`, `floatingCartCount`, `site_theme`, `toggleThemeMode`) are 100% preserved.

---

## 3. Caveats

1. **Safe Area Insets on iPhone Devices**:
   - iPhone X and newer utilize a 34px bottom home bar (`env(safe-area-inset-bottom)`).
   - If `.navigation` is fixed at `bottom: 15px`, modern iOS users may find the bottom nav close to the home gesture swipe line.
   - Recommended adjustment: Using `bottom: max(15px, env(safe-area-inset-bottom))` in CSS while preserving the `fixed bottom-[15px]` Tailwind class string in the HTML element to guarantee test invariance.
2. **Body Bottom Padding on Mobile**:
   - Both layouts currently specify:
     ```css
     @media (max-width: 1023px) {
         body { padding-bottom: 100px; }
     }
     ```
   - On `_Layout.cshtml`, with `#floatingCartBtn` at `bottom-24` ($96\text{px}$) and Wish button stacked up to `bottom-[10.5rem]` ($168\text{px}$), pages with floating elements should ensure their last section has sufficient bottom padding (`pb-28` or `pb-32`) so content isn't obstructed when scrolled to the very bottom.
3. **CuaHang Layout Light Mode Lock**:
   - `_CuaHangLayout.cshtml` has an invariant test prohibiting any theme toggle or `toggleThemeMode` references.
   - The indicator styling for `_CuaHangLayout.cshtml` must remain purely in light-mode palette (`#222327` border and `#ffffff` indicator, or emerald accent) without dark mode selectors.

---

## 4. Conclusion & Concrete Code Specifications

### 4.1 Solution for Bottom Navigation & Indicator Sizing (`_Layout.cshtml` & `_CuaHangLayout.cshtml`)

#### CSS Specification to replace lines 314–420 in `_Layout.cshtml`:
```css
/* =====================================================================
   CSS BOTTOM NAVIGATION MƯỢT MÀ & RESPONSIVE ĐA MÀN HÌNH (375PX+)
   ===================================================================== */
.navigation {
    position: fixed;
    bottom: 15px;
    bottom: max(15px, env(safe-area-inset-bottom, 15px));
    left: 50%;
    transform: translateX(-50%);
    width: 96%;
    max-width: 440px;
    height: 70px;
    background: #222327;
    border-radius: 20px;
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
    box-shadow: 0 10px 30px rgba(0,0,0,0.3);
}

html.dark .navigation {
    background: #1a1b1f;
    border: 1px solid rgba(255,255,255,0.05);
}

.navigation ul {
    display: flex;
    width: 100%;
    height: 100%;
    margin: 0;
    padding: 0;
    position: relative;
}

.navigation ul li {
    position: relative;
    list-style: none;
    flex: 1;
    height: 100%;
    z-index: 2;
}

.navigation ul li a {
    position: relative;
    display: block;
    width: 100%;
    height: 100%;
    text-align: center;
    -webkit-tap-highlight-color: transparent;
}

.navigation ul li a .icon {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    font-size: 1.35rem;
    color: rgba(255, 255, 255, 0.5);
    transition: 0.5s cubic-bezier(0.68, -0.55, 0.265, 1.55);
    z-index: 10;
}

.navigation ul li.active a .icon {
    top: 0px;
    color: #222327;
    font-size: 1.45rem;
}

html.dark .navigation ul li.active a .icon {
    color: #1a1b1f;
}

.navigation ul li a .text {
    position: absolute;
    bottom: -20px;
    left: 50%;
    transform: translateX(-50%);
    color: #ffffff;
    font-weight: 600;
    font-size: 10px;
    letter-spacing: 0.02em;
    transition: 0.5s cubic-bezier(0.68, -0.55, 0.265, 1.55);
    opacity: 0;
    pointer-events: none;
    white-space: nowrap;
}

.navigation ul li.active a .text {
    opacity: 1;
    bottom: 12px;
}

.navigation .indicator {
    position: absolute;
    top: -30px;
    width: 54px;
    height: 54px;
    background: #ffffff;
    border-radius: 50%;
    border: 5px solid #222327;
    transition: 0.5s cubic-bezier(0.68, -0.55, 0.265, 1.55);
    transform: translateX(-50%);
    box-sizing: border-box;
    z-index: 1;
}

html.dark .navigation .indicator {
    background: #86efac;
    border-color: #1a1b1f;
}

/* =====================================================================
   TỐI ƯU HÓA ĐẶC BIỆT CHO MÀN HÌNH DI ĐỘNG 360PX - 430PX (IPHONE SE/MINI)
   ===================================================================== */
@media (max-width: 430px) {
    .navigation {
        height: 64px;
        border-radius: 18px;
    }

    .navigation .indicator {
        top: -24px;
        width: 44px;
        height: 44px;
        border-width: 4px;
        box-sizing: border-box;
    }

    .navigation ul li a .icon {
        font-size: 1.15rem;
    }

    .navigation ul li.active a .icon {
        top: 2px;
        font-size: 1.25rem;
    }

    .navigation ul li a .text {
        font-size: 9px;
        letter-spacing: normal;
    }

    .navigation ul li.active a .text {
        bottom: 8px;
    }
}

@media (max-width: 1023px) {
    body {
        padding-bottom: 100px;
    }
}
```

#### Symmetrical Update for `_CuaHangLayout.cshtml` (Lines 79–177):
The identical CSS rules apply, with the light-mode theme preserved (no `html.dark` block needed for CuaHang).
Also, in `_CuaHangLayout.cshtml`, the initial indicator inline style:
```html
<div class="indicator" style="left: calc(21.4%);"></div>
```
is 100% mathematically correct and perfectly centered on Item 2 (`Cửa hàng`), with outer diameter 44px fitting smoothly without clipping.

---

### 4.2 Solution for Floating Buttons De-confliction

#### Coordinate and Z-Index Map:
```
Screen Top
   │
   ▼
┌────────────────────────────────────────────────────────┐
│ Main Page Content                                      │
│                                                        │
│                                                        │
│ [Mobile < 768px]                                       │
│                                                        │
│   Wish Button (Stack A):       right-5 bottom-[10.5rem]│  (y = 168px - 218px, z-40)
│   OR Wish Button (Stack B):    left-5  bottom-24       │  (y = 96px  - 146px, z-40)
│                                                        │
│   Cart Button (#floatingCartBtn): right-5 bottom-24   │  (y = 96px  - 144px, z-40)
│   ┌──────────────────────────────────────────────────┐ │
│   │ .navigation (Liquid Bottom Nav): fixed bottom    │ │  (y = 15px  - 79px,  z-50)
│   └──────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────┘
Screen Bottom
```

#### Code Specification for `GiaoDien/Home/LoiChuc.cshtml` (Line 109):

**Recommended Strategy A (Right-side Vertical Stack)**:
```html
<!-- FLOATING ACTION BUTTON (DE-CONFLICTED & RESPONSIVE) -->
<button type="button" aria-hidden="true" tabindex="-1"
        class="fixed bottom-[10.5rem] md:bottom-24 right-5 z-40 inline-flex min-h-12 sm:min-h-14 cursor-pointer items-center gap-2 px-4 py-3 sm:px-6 sm:py-4 rounded-full bg-gradient-to-r from-leaf-deep to-leaf text-sm font-semibold text-white shadow-xl transition hover:-translate-y-0.5 hover:brightness-110 active:scale-95 focus-visible:ring-2 focus-visible:ring-leaf focus-visible:ring-offset-2">
    <svg viewBox="0 0 24 24" class="h-5 w-5 shrink-0" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
        <path d="M12 20h9"></path>
        <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z"></path>
    </svg>
    <span class="hidden sm:inline">Gửi lời chúc</span>
    <span class="sr-only sm:hidden">Gửi lời chúc</span>
</button>
```

**Alternative Strategy B (Bilateral Placement - Left Wish, Right Cart)**:
```html
<!-- FLOATING ACTION BUTTON (BILATERAL ERGONOMICS) -->
<button type="button" aria-hidden="true" tabindex="-1"
        class="fixed bottom-24 md:bottom-8 left-5 z-40 inline-flex min-h-12 sm:min-h-14 cursor-pointer items-center gap-2 px-4 py-3 sm:px-6 sm:py-4 rounded-full bg-gradient-to-r from-leaf-deep to-leaf text-sm font-semibold text-white shadow-xl transition hover:-translate-y-0.5 hover:brightness-110 active:scale-95 focus-visible:ring-2 focus-visible:ring-leaf focus-visible:ring-offset-2">
    <svg viewBox="0 0 24 24" class="h-5 w-5 shrink-0" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
        <path d="M12 20h9"></path>
        <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z"></path>
    </svg>
    <span class="hidden sm:inline">Gửi lời chúc</span>
    <span class="sr-only sm:hidden">Gửi lời chúc</span>
</button>
```

#### `#floatingCartBtn` in `_Layout.cshtml` & `_CuaHangLayout.cshtml`:
Retain existing verified markup at `_Layout.cshtml:645` and `_CuaHangLayout.cshtml:349`:
```html
<div id="floatingCartBtn" class="fixed right-5 bottom-24 md:bottom-8 z-40">
    <a asp-area="CuaHang" asp-controller="GioHang" asp-action="Index"
       class="flex items-center gap-2.5 px-4 py-3 rounded-full bg-gradient-to-r from-emerald-600 to-green-700 text-white shadow-xl hover:shadow-2xl hover:scale-105 active:scale-95 transition-all group border border-white/20"
       title="Xem giỏ quà gây quỹ">
        <i class="fas fa-shopping-basket text-lg animate-bounce group-hover:scale-110 transition-transform"></i>
        <span class="font-bold text-xs hidden sm:inline tracking-tight">Giỏ quà gây quỹ</span>
        <span id="floatingCartCount" class="w-6 h-6 rounded-full bg-rose-500 text-white font-black text-xs flex items-center justify-center shadow-xs">
            @cartCount
        </span>
    </a>
</div>
```
With the Wish button moved to either `bottom-[10.5rem] md:bottom-24` (Strategy A) or `left-5 bottom-24 md:bottom-8` (Strategy B), `#floatingCartBtn` will **never collide** on any screen size.

---

### 4.3 Preservation Matrix of Test Invariants

| Test Invariant String / Requirement | File Path | Status | Verification Check |
| :--- | :--- | :--- | :--- |
| `"fixed bottom"` | `_Layout.cshtml:596`<br>`_CuaHangLayout.cshtml:300` | **Preserved** | Matches `Tier1_StorefrontTests.F06_T03` |
| `"md:hidden"` | `_Layout.cshtml:596`<br>`_CuaHangLayout.cshtml:300` | **Preserved** | Matches `Tier1_StorefrontTests.F06_T04` |
| `"CuaHang"` | `_Layout.cshtml:605` | **Preserved** | Matches `Tier1_StorefrontTests.F06_T01` |
| `<svg`, `fa-`, `bi-`, `lucide-` | `_Layout.cshtml:606` | **Preserved** | Matches `Tier1_StorefrontTests.F06_T02` |
| `asp-action="Index"` or `/CuaHang` | `_Layout.cshtml:605` | **Preserved** | Matches `Tier1_StorefrontTests.F06_T05` |
| `id="floatingCartBtn"` | `_Layout.cshtml:645`<br>`_CuaHangLayout.cshtml:349` | **Preserved** | Matches `Tier5_ChallengerM4OrderHarness.CH_M4_06` |
| `id="floatingCartCount"` | `_Layout.cshtml:651`<br>`_CuaHangLayout.cshtml:355` | **Preserved** | Matches `Tier5_ChallengerM4OrderHarness.CH_M4_06` |
| `asp-controller="GioHang"` | `_Layout.cshtml:646`<br>`_CuaHangLayout.cshtml:350` | **Preserved** | Matches `Tier5_ChallengerM4OrderHarness.CH_M4_06` |
| `SweetAlert2` / `Swal` | `_Layout.cshtml` | **Preserved** | Matches `Tier5_ChallengerM4OrderHarness.CH_M4_06` |
| `themeToggleBtn`, `themeToggleBtnMobile` | `_Layout.cshtml:489, 508` | **Preserved** | Matches `Tier5_ChallengerM4_2ThemeHarness.M4_01-02` |
| `/cuahang` light mode hard-lock | `_CuaHangLayout.cshtml:180` | **Preserved** | Matches `adversarial_store_isolation_test.js` |
| Zero changes to `/admin` | All Admin controllers/views | **100% Isolated** | Verified |

---

## 5. Verification Method

To independently verify all findings and test proposals:

1. **Verify Existing Test Suites Pass (100% Baseline)**:
   ```powershell
   dotnet run --project tests/E2ETests/E2ETests.csproj
   node tests/adversarial_final_stress_harness.js
   node tests/adversarial_store_isolation_test.js
   ```
   *Expected Result*: 536/536 C# tests pass, 201/201 node stress tests pass, 109/109 node store isolation tests pass.

2. **Verify Invariant Strings in Layout Files**:
   ```powershell
   Get-Content WEB_TinhNguyen\GiaoDien\Shared\_Layout.cshtml | Select-String -Pattern "fixed bottom", "md:hidden", "CuaHang", "floatingCartBtn", "floatingCartCount"
   ```
   *Expected Result*: All 5 strings match directly.

3. **Verify Collision Elimination on `LoiChuc.cshtml`**:
   - Inspect coordinates:
     - `#floatingCartBtn` on `_Layout`: `bottom-24` ($96\text{px}$) on mobile, `bottom-8` ($32\text{px}$) on desktop, `right-5`.
     - Wish Button on `LoiChuc.cshtml`: `bottom-[10.5rem]` ($168\text{px}$) on mobile, `bottom-24` ($96\text{px}$) on desktop, `right-5`.
     - Gap on mobile: $168 - (96 + 48) = 24\text{px}$ clear vertical separation.
     - Gap on desktop: $96 - (32 + 48) = 16\text{px}$ clear vertical separation.
     - `.navigation` top edge on mobile: $79\text{px}$; Cart button bottom: $96\text{px}$ ($17\text{px}$ clear separation). Zero overlap.

4. **Verify Geometry on 375px Screen**:
   - Navigation width: $360\text{px}$. Slot width: $51.43\text{px}$.
   - Indicator outer width: $44\text{px}$ (radius $22\text{px}$).
   - Slot 0 center: $25.71\text{px}$. Left edge: $25.71 - 22 = +3.71\text{px}$ (no container overflow).
   - Slot 6 center: $334.29\text{px}$. Right edge: $334.29 + 22 = 356.29\text{px} < 360\text{px}$ (no container overflow).
   - Neighbor clearance: $51.43 - 22 - 9 = 20.43\text{px}$ (clean spacing).
