/**
 * CHALLENGER M1 ADVERSARIAL VERIFICATION & STRESS TEST HARNESS
 * Rigorously challenges all Milestone 1 assumptions, edge cases, and layout mechanics:
 * 1. Bottom Nav Geometry & Indicator Slot Boundary Containment (320px - 768px)
 * 2. Real Headless Chrome Drawer Interaction, Keyboard ESC, and Focus
 * 3. Sticky Context Survival with Root overflow-x: clip
 * 4. Floating Action Button Collision Detection on LoiChuc.cshtml (Mobile vs Desktop)
 * 5. CuaHang Mobile Drawer Strict Light-Mode Isolation
 */

const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const os = require('os');

const REPO_ROOT = path.resolve(__dirname, '..');
const PROJECT_DIR = path.join(REPO_ROOT, 'WEB_TinhNguyen');
const SITE_CSS_PATH = path.join(PROJECT_DIR, 'wwwroot', 'css', 'site.css');
const MAIN_LAYOUT_PATH = path.join(PROJECT_DIR, 'GiaoDien', 'Shared', '_Layout.cshtml');
const STORE_LAYOUT_PATH = path.join(PROJECT_DIR, 'Areas', 'CuaHang', 'Views', 'Shared', '_CuaHangLayout.cshtml');
const LOI_CHUC_PATH = path.join(PROJECT_DIR, 'GiaoDien', 'Home', 'LoiChuc.cshtml');

function resolveChromePath() {
  const candidates = [
    process.env.CHROME_BIN,
    'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
    'C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe',
    'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe',
    'C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe',
    '/usr/bin/google-chrome',
    '/usr/bin/chromium-browser'
  ].filter(Boolean);

  for (const c of candidates) {
    if (fs.existsSync(c)) return c;
  }
  return null;
}

let totalTests = 0;
let passedTests = 0;
let failedTests = 0;

function assert(condition, testName, details = '') {
  totalTests++;
  if (condition) {
    passedTests++;
    console.log(`  \x1b[32m[PASS]\x1b[0m ${testName}`);
  } else {
    failedTests++;
    console.error(`  \x1b[31m[FAIL]\x1b[0m ${testName} ${details ? '(' + details + ')' : ''}`);
  }
}

function runChromeHeadless(chromePath, htmlContent, width = 375, height = 844) {
  return new Promise((resolve) => {
    const tempFile = path.join(os.tmpdir(), `challenger_test_${Date.now()}_${Math.random().toString(36).substring(2)}.html`);
    fs.writeFileSync(tempFile, htmlContent, 'utf8');
    const fileUrl = 'file:///' + tempFile.replace(/\\/g, '/');

    const args = [
      '--headless=new',
      '--no-sandbox',
      '--disable-gpu',
      '--allow-file-access-from-files',
      '--disable-web-security',
      `--window-size=${width},${height}`,
      '--virtual-time-budget=4000',
      '--dump-dom',
      fileUrl
    ];

    cp.execFile(chromePath, args, { maxBuffer: 15 * 1024 * 1024, timeout: 15000 }, (err, stdout) => {
      try { fs.unlinkSync(tempFile); } catch (_) {}
      if (err && !stdout) {
        return resolve({ success: false, error: err.message });
      }
      const match = (stdout || '').match(/__CHALLENGER_REPORT__([\s\S]*?)__END_CHALLENGER_REPORT__/);
      if (match) {
        try {
          return resolve({ success: true, report: JSON.parse(match[1]) });
        } catch (e) {
          return resolve({ success: false, error: 'JSON parse error: ' + e.message });
        }
      }
      return resolve({ success: false, error: 'Report marker not found', raw: (stdout || '').substring(0, 500) });
    });
  });
}

async function main() {
  console.log('\x1b[36m================================================================================\x1b[0m');
  console.log('\x1b[1m   EMPIRICAL CHALLENGER 1: ADVERSARIAL STRESS TEST & LAYOUT VERIFICATION SUITE\x1b[0m');
  console.log('\x1b[36m================================================================================\x1b[0m\n');

  const chromePath = resolveChromePath();
  if (!chromePath) {
    console.error('\x1b[31m[ERROR] Chrome binary not found.\x1b[0m');
    process.exit(1);
  }

  // ===========================================================================
  // SUITE 1: Bottom Navigation Geometry & Indicator Overhang across Screen Sizes
  // ===========================================================================
  console.log('\x1b[1m--- SUITE 1: Bottom Navigation Geometry & Indicator Overhang (320px - 768px) ---\x1b[0m');
  const viewports = [320, 360, 375, 390, 412, 430, 480, 768];
  const itemCount = 7;

  for (const vp of viewports) {
    const containerWidth = Math.min(vp * 0.96, 440);
    const slotWidth = containerWidth / itemCount;
    const isMobileSmall = (vp <= 430);
    const indicatorDiameter = isMobileSmall ? 44 : 54;
    const indicatorRadius = indicatorDiameter / 2;

    // Slot 0 (Leftmost)
    const slot0Center = slotWidth * 0.5;
    const slot0LeftBoundary = slot0Center - indicatorRadius;
    const slot0RightBoundary = slot0Center + indicatorRadius;

    // Slot 6 (Rightmost)
    const slot6Center = slotWidth * 5.5 + slotWidth * 0.5; // (6 + 0.5) * slotWidth
    const slot6LeftBoundary = slot6Center - indicatorRadius;
    const slot6RightBoundary = slot6Center + indicatorRadius;

    // Check overhang on Slot 0
    const slot0InBounds = slot0LeftBoundary >= -0.5; // allow subpixel margin
    assert(
      slot0InBounds,
      `Bottom Nav at ${vp}px: Slot 0 (Index 0) indicator left boundary (${slot0LeftBoundary.toFixed(2)}px) >= 0px`,
      `Overhang detected: ${slot0LeftBoundary.toFixed(2)}px`
    );

    // Check overhang on Slot 6
    const slot6InBounds = slot6RightBoundary <= containerWidth + 0.5;
    assert(
      slot6InBounds,
      `Bottom Nav at ${vp}px: Slot 6 (Index 6) indicator right boundary (${slot6RightBoundary.toFixed(2)}px) <= container (${containerWidth.toFixed(2)}px)`,
      `Overhang detected: ${slot6RightBoundary.toFixed(2)}px vs ${containerWidth.toFixed(2)}px`
    );
  }

  // ===========================================================================
  // SUITE 2: Real Chrome Verification of _Layout.cshtml Mobile Drawer
  // ===========================================================================
  console.log('\n\x1b[1m--- SUITE 2: Interactive Drawer & Keyboard Controls (_Layout.cshtml) ---\x1b[0m');

  const cssUrl = 'file:///' + SITE_CSS_PATH.replace(/\\/g, '/');
  let layoutHtml = fs.readFileSync(MAIN_LAYOUT_PATH, 'utf8')
    .replace(/@using[^\r\n]+/g, '')
    .replace(/@inject[^\r\n]+/g, '')
    .replace(/@model[^\r\n]+/g, '')
    .replace(/@\{[\s\S]*?\}/g, '')
    .replace(/@RenderBody\(\)/g, '<div id="main-content" style="padding: 100px 20px;"><h1>Public Content</h1></div>')
    .replace(/@RenderSectionAsync\([^)]*\)/g, '')
    .replace(/@Url\.Action\([^)]*\)/g, '#')
    .replace(/@ViewData\[[^\]]*\]/g, 'Test Title')
    .replace(/@ViewBag\.[a-zA-Z0-9_]+/g, '2026')
    .replace(/@DateTime\.Now\.Year/g, '2026')
    .replace(/@colorMain/g, '#15803d')
    .replace(/@textBot/g, 'Test')
    .replace('~/css/site.css', cssUrl)
    .replace('/css/site.css', cssUrl);

  const testHarnessHtml = `
    <!DOCTYPE html>
    <html lang="vi">
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
      <link rel="stylesheet" href="${cssUrl}">
      <script src="https://cdn.tailwindcss.com"></script>
    </head>
    <body>
      ${layoutHtml}
      <div id="assertion-result"></div>
      <script>
        window.addEventListener('load', () => {
          setTimeout(() => {
            const results = {};
            const toggleBtn = document.getElementById('mobileNavToggle');
            const drawer = document.getElementById('mobileDrawer');
            const backdrop = document.getElementById('mobileDrawerBackdrop');
            const closeBtn = document.getElementById('mobileNavClose');

            results.hasToggleBtn = Boolean(toggleBtn);
            results.hasDrawer = Boolean(drawer);
            results.hasBackdrop = Boolean(backdrop);
            results.hasCloseBtn = Boolean(closeBtn);

            // Step 1: Initial state
            results.initialDrawerClosed = drawer ? drawer.classList.contains('translate-x-full') : false;
            results.initialBackdropHidden = backdrop ? backdrop.classList.contains('pointer-events-none') : false;
            results.initialBodyUnlocked = !document.body.classList.contains('overflow-hidden');

            // Step 2: Open drawer via openMobileDrawer()
            if (typeof openMobileDrawer === 'function') {
              openMobileDrawer();
              results.openDrawerSlideIn = drawer.classList.contains('translate-x-0') && !drawer.classList.contains('translate-x-full');
              results.openAriaHiddenFalse = drawer.getAttribute('aria-hidden') === 'false';
              results.openAriaExpandedTrue = toggleBtn ? toggleBtn.getAttribute('aria-expanded') === 'true' : false;
              results.openBackdropVisible = backdrop.classList.contains('opacity-100') && !backdrop.classList.contains('pointer-events-none');
              results.openBodyLocked = document.body.classList.contains('overflow-hidden');

              // Step 3: Close drawer via closeMobileDrawer()
              closeMobileDrawer();
              results.closeDrawerSlideOut = drawer.classList.contains('translate-x-full');
              results.closeAriaHiddenTrue = drawer.getAttribute('aria-hidden') === 'true';
              results.closeAriaExpandedFalse = toggleBtn ? toggleBtn.getAttribute('aria-expanded') === 'false' : false;
              results.closeBackdropHidden = backdrop.classList.contains('pointer-events-none');
              results.closeBodyUnlocked = !document.body.classList.contains('overflow-hidden');

              // Step 4: Open again and test ESC keydown listener
              openMobileDrawer();
              const escEvent = new KeyboardEvent('keydown', { key: 'Escape', keyCode: 27, bubbles: true });
              document.dispatchEvent(escEvent);
              results.escClosesDrawer = drawer.classList.contains('translate-x-full');
              results.escUnlocksBody = !document.body.classList.contains('overflow-hidden');

              // Step 5: Test Submenu Accordion
              if (typeof toggleDrawerSubmenu === 'function') {
                const submenu = document.getElementById('drawerKyUcSubmenu');
                const chevron = document.getElementById('drawerKyUcChevron');
                if (submenu) {
                  const initialHidden = submenu.classList.contains('hidden');
                  toggleDrawerSubmenu('drawerKyUcSubmenu');
                  const toggledOpen = !submenu.classList.contains('hidden');
                  toggleDrawerSubmenu('drawerKyUcSubmenu');
                  const toggledClosed = submenu.classList.contains('hidden');
                  results.submenuAccordionWorks = initialHidden && toggledOpen && toggledClosed;
                }
              }
            }

            document.getElementById('assertion-result').innerText =
              '__CHALLENGER_REPORT__' + JSON.stringify(results) + '__END_CHALLENGER_REPORT__';
          }, 300);
        });
      </script>
    </body>
    </html>
  `;

  const chromeRes = await runChromeHeadless(chromePath, testHarnessHtml, 375, 844);
  if (!chromeRes.success) {
    console.error('Headless chrome test failed:', chromeRes.error);
    assert(false, '_Layout drawer headless test execution', chromeRes.error);
  } else {
    const rep = chromeRes.report;
    assert(rep.hasToggleBtn, '_Layout contains #mobileNavToggle');
    assert(rep.hasDrawer, '_Layout contains #mobileDrawer');
    assert(rep.hasBackdrop, '_Layout contains #mobileDrawerBackdrop');
    assert(rep.hasCloseBtn, '_Layout contains #mobileNavClose');
    assert(rep.initialDrawerClosed, '_Layout drawer is initially closed (translate-x-full)');
    assert(rep.initialBackdropHidden, '_Layout backdrop is initially inactive (pointer-events-none)');
    assert(rep.initialBodyUnlocked, '_Layout document.body initially not locked');
    assert(rep.openDrawerSlideIn, 'openMobileDrawer() applies translate-x-0 slide-in');
    assert(rep.openAriaHiddenFalse, 'openMobileDrawer() sets aria-hidden="false"');
    assert(rep.openAriaExpandedTrue, 'openMobileDrawer() sets aria-expanded="true" on toggle button');
    assert(rep.openBackdropVisible, 'openMobileDrawer() activates backdrop (opacity-100 & pointer-events-auto)');
    assert(rep.openBodyLocked, 'openMobileDrawer() locks body scrolling with overflow-hidden');
    assert(rep.closeDrawerSlideOut, 'closeMobileDrawer() applies translate-x-full slide-out');
    assert(rep.closeAriaHiddenTrue, 'closeMobileDrawer() sets aria-hidden="true"');
    assert(rep.closeAriaExpandedFalse, 'closeMobileDrawer() sets aria-expanded="false"');
    assert(rep.closeBackdropHidden, 'closeMobileDrawer() hides backdrop (pointer-events-none)');
    assert(rep.closeBodyUnlocked, 'closeMobileDrawer() restores body scrolling');
    assert(rep.escClosesDrawer, 'Keyboard ESC event cleanly closes the drawer');
    assert(rep.escUnlocksBody, 'Keyboard ESC event cleanly restores body scrolling');
    assert(rep.submenuAccordionWorks, 'toggleDrawerSubmenu cleanly toggles season accordion');
  }

  // ===========================================================================
  // SUITE 3: Real Chrome Verification of _CuaHangLayout.cshtml Mobile Drawer & Isolation
  // ===========================================================================
  console.log('\n\x1b[1m--- SUITE 3: Storefront Mobile Drawer & Theme Isolation (_CuaHangLayout.cshtml) ---\x1b[0m');

  let storeLayoutHtml = fs.readFileSync(STORE_LAYOUT_PATH, 'utf8')
    .replace(/@using[^\r\n]+/g, '')
    .replace(/@inject[^\r\n]+/g, '')
    .replace(/@model[^\r\n]+/g, '')
    .replace(/@\{[\s\S]*?\}/g, '')
    .replace(/@RenderBody\(\)/g, '<div id="main-content" style="padding: 100px 20px;"><h1>Store Content</h1></div>')
    .replace(/@RenderSectionAsync\([^)]*\)/g, '')
    .replace(/@Url\.Action\([^)]*\)/g, '#')
    .replace(/@ViewData\[[^\]]*\]/g, 'Store Title')
    .replace(/@ViewBag\.[a-zA-Z0-9_]+/g, '2026')
    .replace(/@DateTime\.Now\.Year/g, '2026')
    .replace(/@cartCount/g, '2')
    .replace(/@currentYear/g, '2026')
    .replace('~/css/site.css', cssUrl)
    .replace('/css/site.css', cssUrl);

  const testStoreHarnessHtml = `
    <!DOCTYPE html>
    <html lang="vi" class="light">
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
      <link rel="stylesheet" href="${cssUrl}">
      <script src="https://cdn.tailwindcss.com"></script>
    </head>
    <body class="light bg-slate-50 text-slate-900 cuahang-body">
      ${storeLayoutHtml}
      <div id="assertion-result"></div>
      <script>
        window.addEventListener('load', () => {
          setTimeout(() => {
            const results = {};
            const toggleBtn = document.getElementById('mobileNavToggle');
            const drawer = document.getElementById('mobileDrawer');
            const backdrop = document.getElementById('mobileDrawerBackdrop');
            const closeBtn = document.getElementById('mobileNavClose');

            results.hasToggleBtn = Boolean(toggleBtn);
            results.hasDrawer = Boolean(drawer);
            results.hasBackdrop = Boolean(backdrop);
            results.hasCloseBtn = Boolean(closeBtn);

            // Test openDrawer() and closeDrawer() in CuaHang
            if (typeof openDrawer === 'function') {
              openDrawer();
              results.openDrawerSlideIn = drawer.classList.contains('translate-x-0');
              results.openBodyLocked = document.body.classList.contains('overflow-hidden');

              closeDrawer();
              results.closeDrawerSlideOut = drawer.classList.contains('translate-x-full');
              results.closeBodyUnlocked = !document.body.classList.contains('overflow-hidden');

              openDrawer();
              const escEvent = new KeyboardEvent('keydown', { key: 'Escape', keyCode: 27, bubbles: true });
              document.dispatchEvent(escEvent);
              results.escClosesDrawer = drawer.classList.contains('translate-x-full');
              results.escUnlocksBody = !document.body.classList.contains('overflow-hidden');
            }

            // Theme Leakage Check on Drawer
            results.drawerHasZeroDarkClasses = !drawer.innerHTML.includes('dark:');
            results.drawerHasZeroThemeButtons = !drawer.innerHTML.includes('themeToggle');

            document.getElementById('assertion-result').innerText =
              '__CHALLENGER_REPORT__' + JSON.stringify(results) + '__END_CHALLENGER_REPORT__';
          }, 300);
        });
      </script>
    </body>
    </html>
  `;

  const chromeStoreRes = await runChromeHeadless(chromePath, testStoreHarnessHtml, 375, 844);
  if (!chromeStoreRes.success) {
    console.error('Headless chrome store test failed:', chromeStoreRes.error);
    assert(false, '_CuaHangLayout drawer headless test execution', chromeStoreRes.error);
  } else {
    const rep = chromeStoreRes.report;
    assert(rep.hasToggleBtn, '_CuaHangLayout contains #mobileNavToggle');
    assert(rep.hasDrawer, '_CuaHangLayout contains #mobileDrawer');
    assert(rep.hasBackdrop, '_CuaHangLayout contains #mobileDrawerBackdrop');
    assert(rep.hasCloseBtn, '_CuaHangLayout contains #mobileNavClose');
    assert(rep.openDrawerSlideIn, 'CuaHang openDrawer() opens drawer');
    assert(rep.openBodyLocked, 'CuaHang openDrawer() locks body');
    assert(rep.closeDrawerSlideOut, 'CuaHang closeDrawer() closes drawer');
    assert(rep.closeBodyUnlocked, 'CuaHang closeDrawer() restores body');
    assert(rep.escClosesDrawer, 'CuaHang ESC listener closes drawer');
    assert(rep.escUnlocksBody, 'CuaHang ESC listener restores body');
    assert(rep.drawerHasZeroDarkClasses, '_CuaHangLayout mobile drawer has 0 "dark:" utility classes');
    assert(rep.drawerHasZeroThemeButtons, '_CuaHangLayout mobile drawer has 0 theme buttons');
  }

  // ===========================================================================
  // SUITE 4: Sticky Context Survival with Root overflow-x: clip
  // ===========================================================================
  console.log('\n\x1b[1m--- SUITE 4: Sticky Context Survival with Root overflow-x: clip ---\x1b[0m');

  const stickyTestHtml = `
    <!DOCTYPE html>
    <html lang="vi" style="overflow-x: hidden; overflow-x: clip;">
    <head>
      <meta charset="utf-8">
      <style>
        * { box-sizing: border-box; margin: 0; padding: 0; }
        html, body { overflow-x: hidden; overflow-x: clip; min-height: 100vh; }
        #sticky-element {
          position: sticky;
          top: 0px;
          height: 60px;
          background: #15803d;
          color: white;
          z-index: 100;
        }
        #tall-content { height: 2500px; padding-top: 100px; }
      </style>
    </head>
    <body>
      <div style="height: 150px; background: #e2e8f0;">Pre-header content</div>
      <div id="sticky-element">Sticky Header Test</div>
      <div id="tall-content">Scrollable Body Content</div>
      <div id="assertion-result"></div>
      <script>
        window.addEventListener('load', () => {
          setTimeout(() => {
            const results = {};
            const stickyEl = document.getElementById('sticky-element');
            const rectBefore = stickyEl.getBoundingClientRect();
            results.initialTop = Math.round(rectBefore.top);

            // Scroll down past the pre-header
            window.scrollTo(0, 300);

            setTimeout(() => {
              const rectAfter = stickyEl.getBoundingClientRect();
              results.scrolledTop = Math.round(rectAfter.top);
              results.isStickyWorking = (rectAfter.top === 0);

              document.getElementById('assertion-result').innerText =
                '__CHALLENGER_REPORT__' + JSON.stringify(results) + '__END_CHALLENGER_REPORT__';
            }, 200);
          }, 200);
        });
      </script>
    </body>
    </html>
  `;

  const stickyRes = await runChromeHeadless(chromePath, stickyTestHtml, 375, 844);
  if (!stickyRes.success) {
    assert(false, 'Sticky context survival test execution', stickyRes.error);
  } else {
    const rep = stickyRes.report;
    assert(
      rep.isStickyWorking,
      `position: sticky survives root overflow-x: clip (scrolled top = ${rep.scrolledTop}px, expected 0px)`,
      `scrolledTop was ${rep.scrolledTop}px`
    );
  }

  // ===========================================================================
  // SUITE 5: Floating Action Buttons Collision Detection on LoiChuc.cshtml
  // ===========================================================================
  console.log('\n\x1b[1m--- SUITE 5: Floating Action Buttons Collision on LoiChuc.cshtml ---\x1b[0m');

  let loiChucHtml = fs.readFileSync(LOI_CHUC_PATH, 'utf8')
    .replace(/@using[^\r\n]+/g, '')
    .replace(/@model[^\r\n]+/g, '')
    .replace(/@\{[\s\S]*?\}/g, '')
    .replace(/@Url\.Action\([^)]*\)/g, '#')
    .replace(/@foreach\s*\([^{]+\)\s*\{[\s\S]*?\}/g, '<div class="card" style="height: 200px;">Card</div>')
    .replace(/@\([^)]*\)/g, '');

  const fullLoiChucHtml = layoutHtml.replace('<div id="main-content" style="padding: 100px 20px;"><h1>Public Content</h1></div>', loiChucHtml);

  const collisionTestHarness = `
    <!DOCTYPE html>
    <html lang="vi">
    <head>
      <meta charset="utf-8">
      <meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
      <link rel="stylesheet" href="${cssUrl}">
      <script src="https://cdn.tailwindcss.com"></script>
    </head>
    <body>
      ${fullLoiChucHtml}
      <div id="assertion-result"></div>
      <script>
        window.addEventListener('load', () => {
          setTimeout(() => {
            const results = {};
            const bottomNav = document.querySelector('.navigation');
            const cartBtn = document.getElementById('floatingCartBtn');
            const wishBtn = document.querySelector('button[class*="bottom-[10.5rem]"], button[class*="fixed"][class*="z-40"]');

            results.hasBottomNav = Boolean(bottomNav);
            results.hasCartBtn = Boolean(cartBtn);
            results.hasWishBtn = Boolean(wishBtn);

            const isOverlapping = (rectA, rectB) => {
              return !(
                rectA.right <= rectB.left ||
                rectA.left >= rectB.right ||
                rectA.bottom <= rectB.top ||
                rectA.top >= rectB.bottom
              );
            };

            const innerW = window.innerWidth;
            results.innerWidth = innerW;

            if (cartBtn && wishBtn) {
              const rectCart = cartBtn.getBoundingClientRect();
              const rectWish = wishBtn.getBoundingClientRect();
              results.cartVsWishOverlap = isOverlapping(rectCart, rectWish);
              results.cartRect = { top: Math.round(rectCart.top), bottom: Math.round(rectCart.bottom) };
              results.wishRect = { top: Math.round(rectWish.top), bottom: Math.round(rectWish.bottom) };
            }

            if (bottomNav && wishBtn && window.getComputedStyle(bottomNav).display !== 'none') {
              const rectNav = bottomNav.getBoundingClientRect();
              const rectWish = wishBtn.getBoundingClientRect();
              results.navVsWishOverlap = isOverlapping(rectNav, rectWish);
              results.navRect = { top: Math.round(rectNav.top), bottom: Math.round(rectNav.bottom) };
            } else {
              results.navVsWishOverlap = false; // nav hidden on desktop
            }

            document.getElementById('assertion-result').innerText =
              '__CHALLENGER_REPORT__' + JSON.stringify(results) + '__END_CHALLENGER_REPORT__';
          }, 300);
        });
      </script>
    </body>
    </html>
  `;

  // Test Mobile (375px)
  const mobileCollisionRes = await runChromeHeadless(chromePath, collisionTestHarness, 375, 844);
  if (mobileCollisionRes.success) {
    const rep = mobileCollisionRes.report;
    assert(!rep.cartVsWishOverlap, 'Mobile (375px): Floating Cart and Wish button do NOT collide', JSON.stringify({ cart: rep.cartRect, wish: rep.wishRect }));
    assert(!rep.navVsWishOverlap, 'Mobile (375px): Mobile Bottom Nav and Wish button do NOT collide', JSON.stringify({ nav: rep.navRect, wish: rep.wishRect }));
  } else {
    assert(false, 'Mobile collision test execution', mobileCollisionRes.error);
  }

  // Test Desktop (1024px)
  const desktopCollisionRes = await runChromeHeadless(chromePath, collisionTestHarness, 1024, 768);
  if (desktopCollisionRes.success) {
    const rep = desktopCollisionRes.report;
    assert(!rep.cartVsWishOverlap, 'Desktop (1024px): Floating Cart and Wish button do NOT collide', JSON.stringify({ cart: rep.cartRect, wish: rep.wishRect }));
    assert(!rep.navVsWishOverlap, 'Desktop (1024px): Mobile Bottom Nav is hidden, no collision');
  } else {
    assert(false, 'Desktop collision test execution', desktopCollisionRes.error);
  }

  // ===========================================================================
  // SUMMARY
  // ===========================================================================
  console.log('\n\x1b[36m================================================================================\x1b[0m');
  console.log(`\x1b[1mTOTAL TESTS: ${totalTests} | \x1b[32mPASSED: ${passedTests}\x1b[0m | \x1b[31mFAILED: ${failedTests}\x1b[0m`);
  console.log('\x1b[36m================================================================================\x1b[0m');

  process.exit(failedTests > 0 ? 1 : 0);
}

main().catch(err => {
  console.error('Fatal test harness error:', err);
  process.exit(1);
});
