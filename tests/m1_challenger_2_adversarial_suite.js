#!/usr/bin/env node
/**
 * M1 CHALLENGER 2 EMPIRICAL ADVERSARIAL STRESS TEST SUITE
 * Milestone 1: Public Layouts & Mobile Navigation Infrastructure
 * 
 * Target Edge Conditions:
 * 1. Sticky positioning on GioHang/Index.cshtml:172 preserved with overflow-x: clip vs broken by overflow: hidden
 * 2. Floating action buttons de-confliction on LoiChuc.cshtml across all viewports
 * 3. Mobile drawer rapid interaction stress (ESC, backdrop, close button, race conditions, z-indices)
 * 4. Storefront theme isolation across URL permutations and hostile script invocations
 */

const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const os = require('os');
const vm = require('vm');

const REPO_ROOT = path.resolve(__dirname, '..');
const PROJECT_DIR = path.join(REPO_ROOT, 'WEB_TinhNguyen');
const SITE_CSS_PATH = path.join(PROJECT_DIR, 'wwwroot', 'css', 'site.css');
const MAIN_LAYOUT_PATH = path.join(PROJECT_DIR, 'GiaoDien', 'Shared', '_Layout.cshtml');
const STORE_LAYOUT_PATH = path.join(PROJECT_DIR, 'Areas', 'CuaHang', 'Views', 'Shared', '_CuaHangLayout.cshtml');
const GIOHANG_VIEW_PATH = path.join(PROJECT_DIR, 'Areas', 'CuaHang', 'Views', 'GioHang', 'Index.cshtml');
const LOICHUC_VIEW_PATH = path.join(PROJECT_DIR, 'GiaoDien', 'Home', 'LoiChuc.cshtml');

const siteCssContent = fs.readFileSync(SITE_CSS_PATH, 'utf8');

let totalAssertions = 0;
let passedAssertions = 0;
let failedAssertions = 0;
const failureList = [];

function assert(condition, message, details = '') {
  totalAssertions++;
  if (condition) {
    passedAssertions++;
    console.log(`  [PASS] ${message}`);
  } else {
    failedAssertions++;
    const fullMsg = details ? `${message} | Details: ${details}` : message;
    failureList.push(fullMsg);
    console.error(`  [FAIL] ${fullMsg}`);
  }
}

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

function cleanRazorDirectives(text) {
  let res = text;
  res = res.replace(/@using\s+[^\r\n]+/g, '');
  res = res.replace(/@inject\s+[^\r\n]+/g, '');
  res = res.replace(/@model\s+[^\r\n]+/g, '');
  res = res.replace(/@addTagHelper\s+[^\r\n]+/g, '');
  res = res.replace(/@\{[\s\S]*?\}/g, '');
  res = res.replace(/@Html\.AntiForgeryToken\(\)/g, '<input type="hidden" name="__RequestVerificationToken" value="mock-token" />');
  res = res.replace(/@Html\.Raw\(([\s\S]*?)\)/g, '$1');
  res = res.replace(/@Url\.Action\([^)]*\)/g, '#');
  res = res.replace(/@ViewData\[[^\]]*\]/g, 'Tinh Nguyen 2026');
  res = res.replace(/@ViewBag\.[a-zA-Z0-9_]+/g, '2026');
  res = res.replace(/@DateTime\.Now\.Year/g, '2026');
  res = res.replace(/@cartCount/g, '3');
  res = res.replace(/@nam/g, '2026');
  res = res.replace(/\sasp-action="[^"]*"/g, '');
  res = res.replace(/\sasp-controller="[^"]*"/g, '');
  res = res.replace(/\sasp-area="[^"]*"/g, '');
  res = res.replace(/\sasp-route-[a-zA-Z0-9_-]+="[^"]*"/g, '');
  res = res.replace(/\sasp-for="([^"]*)"/g, ' name="$1" id="$1"');
  res = res.replace(/\sasp-append-version="[^"]*"/g, '');
  res = res.replace(/@\(([^)]*)\)/g, '$1');
  return res;
}

function runChromeWithReport(chromePath, wrapperHtmlPath, windowSize = '1280,900') {
  return new Promise((resolve) => {
    const fileUrl = 'file:///' + wrapperHtmlPath.replace(/\\/g, '/');
    const args = [
      '--headless=new',
      '--no-sandbox',
      '--disable-gpu',
      `--window-size=${windowSize}`,
      '--allow-file-access-from-files',
      '--disable-web-security',
      '--virtual-time-budget=4000',
      '--dump-dom',
      fileUrl
    ];

    cp.execFile(chromePath, args, { maxBuffer: 20 * 1024 * 1024, timeout: 25000 }, (err, stdout) => {
      if (err && !stdout) {
        return resolve({ success: false, error: err.message });
      }

      const match = (stdout || '').match(/__CHALLENGER_REPORT_START__([\s\S]*?)__CHALLENGER_REPORT_END__/);
      if (match) {
        try {
          const report = JSON.parse(match[1]);
          return resolve({ success: true, report });
        } catch (pe) {
          return resolve({ success: false, error: 'JSON parse error: ' + pe.message });
        }
      }

      const errMatch = (stdout || '').match(/__CHALLENGER_ERROR__([\s\S]*?)__CHALLENGER_ERROR__/);
      if (errMatch) {
        return resolve({ success: false, error: errMatch[1] });
      }

      return resolve({ success: false, error: 'No report marker in Chrome output' });
    });
  });
}

// =============================================================================
// TEST SUITE 1: STICKY POSITIONING ON GIOHANG WITH OVERFLOW-X: CLIP
// =============================================================================
async function testStickyPositioning(chromePath, tempDir) {
  console.log('\n================================================================================');
  console.log(' SUITE 1: EMPIRICAL STICKY POSITIONING VERIFICATION (GioHang/Index.cshtml:172)');
  console.log('================================================================================');

  // 1. Static CSS checks
  assert(siteCssContent.includes('overflow-x:clip') || siteCssContent.includes('overflow-x: clip'), 'Compiled site.css includes overflow-x: clip rule');

  const storeLayout = fs.readFileSync(STORE_LAYOUT_PATH, 'utf8');
  assert(storeLayout.includes('overflow-x: clip'), '_CuaHangLayout.cshtml declares overflow-x: clip on html, body');
  assert(storeLayout.includes('overflow-x-clip'), '_CuaHangLayout.cshtml applies overflow-x-clip class to <body>');

  const appCss = fs.readFileSync(path.join(PROJECT_DIR, '..', 'Styles', 'app.css'), 'utf8');
  assert(appCss.includes('overflow-x: clip'), 'Styles/app.css declares overflow-x: clip under @layer base');

  const giohangView = fs.readFileSync(GIOHANG_VIEW_PATH, 'utf8');
  assert(
    giohangView.includes('sticky top-6'),
    'GioHang/Index.cshtml:172 checkout summary card declares sticky top-6'
  );

  // 2. Synthesize GioHang layout test
  const htmlDoc = `<!DOCTYPE html>
<html lang="vi" class="light">
<head>
<meta charset="utf-8">
<style>${siteCssContent}</style>
<style>
  html, body { overflow-x: hidden; overflow-x: clip; }
</style>
</head>
<body class="light bg-slate-50 text-slate-900 cuahang-body antialiased relative overflow-x-clip">
  <div class="min-h-screen py-8 px-4">
    <div class="max-w-7xl mx-auto">
      <div id="gridRow" class="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        <div class="lg:col-span-7" style="height: 2500px; background: #e2e8f0; border-radius: 1rem; padding: 20px;">
          <h2>Danh sách sản phẩm giỏ hàng (Chiều cao 2500px)</h2>
        </div>
        <div id="stickyCol" class="lg:col-span-5" style="height: 100%;">
          <div id="checkoutStickyCard" class="bg-white rounded-2xl shadow-sm border border-slate-200 p-6 sticky top-6">
            <h2 class="text-xl font-bold mb-4">Thông tin Đặt hàng &amp; Ủng hộ</h2>
            <p>Biểu mẫu đặt hàng cố định</p>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div id="report"></div>
  <script>
    window.addEventListener('load', () => {
      try {
        const card = document.getElementById('checkoutStickyCard');
        const comp = window.getComputedStyle(card);
        const position = comp.position;
        const top = comp.top;

        // Ancestor chain inspection
        let curr = card.parentElement;
        const ancestors = [];
        let brokenByAncestor = false;

        while (curr && curr !== document) {
          const s = window.getComputedStyle(curr);
          const ox = s.overflowX;
          const oy = s.overflowY;
          const isScrollBoundary = (oy === 'auto' || oy === 'scroll' || oy === 'hidden');
          ancestors.push({ tag: curr.tagName, class: curr.className, ox, oy, isScrollBoundary });
          if (curr !== document.documentElement && curr !== document.body && isScrollBoundary) {
            brokenByAncestor = true;
          }
          curr = curr.parentElement;
        }

        const htmlStyle = window.getComputedStyle(document.documentElement);
        const bodyStyle = window.getComputedStyle(document.body);

        const rectBefore = card.getBoundingClientRect();
        window.scrollTo(0, 400);
        const scrollY = window.scrollY || document.documentElement.scrollTop;
        const rectAfter = card.getBoundingClientRect();

        const isPinned = Math.abs(rectAfter.top - 24) <= 4;

        const result = {
          position,
          top,
          htmlOverflowX: htmlStyle.overflowX,
          htmlOverflowY: htmlStyle.overflowY,
          bodyOverflowX: bodyStyle.overflowX,
          bodyOverflowY: bodyStyle.overflowY,
          brokenByAncestor,
          rectBeforeTop: Math.round(rectBefore.top),
          scrollY,
          rectAfterTop: Math.round(rectAfter.top),
          isPinned
        };

        document.getElementById('report').innerText =
          '__CHALLENGER_REPORT_START__' + JSON.stringify(result) + '__CHALLENGER_REPORT_END__';
      } catch (err) {
        document.getElementById('report').innerText =
          '__CHALLENGER_ERROR__' + err.message + '__CHALLENGER_ERROR__';
      }
    });
  </script>
</body>
</html>`;

  const pagePath = path.join(tempDir, 'sticky_verify.html');
  fs.writeFileSync(pagePath, htmlDoc, 'utf8');

  const res = await runChromeWithReport(chromePath, pagePath, '1280,900');
  assert(res.success, 'Headless Chrome evaluated sticky positioning and scroll pinning', res.error);

  if (res.success && res.report) {
    const rep = res.report;
    assert(rep.position === 'sticky', `Computed position is 'sticky' (actual: '${rep.position}')`);
    assert(rep.top === '24px', `Computed top is '24px' (top-6 = 1.5rem = 24px, actual: '${rep.top}')`);
    assert(rep.htmlOverflowX === 'clip', `<html> computed overflow-x is 'clip' (actual: '${rep.htmlOverflowX}')`);
    assert(rep.htmlOverflowY === 'visible', `<html> computed overflow-y is 'visible' (actual: '${rep.htmlOverflowY}')`);
    assert(rep.bodyOverflowX === 'clip', `<body> computed overflow-x is 'clip' (actual: '${rep.bodyOverflowX}')`);
    assert(rep.bodyOverflowY === 'visible', `<body> computed overflow-y is 'visible' (actual: '${rep.bodyOverflowY}')`);
    assert(!rep.brokenByAncestor, 'Zero intermediate ancestor elements create a scroll container breaking sticky context');
    assert(rep.scrollY >= 300, `Viewport was scrolled vertically by ${rep.scrollY}px`);
    assert(rep.isPinned, `Sticky card pinned at top=24px during vertical scrolling (actual top: ${rep.rectAfterTop}px, scrollY: ${rep.scrollY}px)`);
  }

  // 3. Counter-example / Adversarial proof: Verify overflow: hidden DOES break sticky
  const brokenDoc = htmlDoc.replace('html, body { overflow-x: hidden; overflow-x: clip; }', 'html, body { overflow: hidden; }');
  const brokenPagePath = path.join(tempDir, 'sticky_broken_control.html');
  fs.writeFileSync(brokenPagePath, brokenDoc, 'utf8');
  const brokenRes = await runChromeWithReport(chromePath, brokenPagePath, '1280,900');
  if (brokenRes.success && brokenRes.report) {
    assert(!brokenRes.report.isPinned, 'Adversarial Control: confirmed overflow: hidden breaks viewport sticky pinning (isPinned=false)');
  }
}

// =============================================================================
// TEST SUITE 2: FLOATING BUTTONS COLLISION ON LOICHUC.CSHTML
// =============================================================================
async function testFloatingButtonsCollision(chromePath, tempDir) {
  console.log('\n================================================================================');
  console.log(' SUITE 2: FLOATING BUTTONS COLLISION VERIFICATION (LoiChuc.cshtml)');
  console.log('================================================================================');

  const loichucView = fs.readFileSync(LOICHUC_VIEW_PATH, 'utf8');
  assert(
    loichucView.includes('bottom-[10.5rem] md:bottom-24 right-5 z-40'),
    'LoiChuc.cshtml Wish button updated to bottom-[10.5rem] md:bottom-24 right-5 z-40'
  );

  const mainLayout = fs.readFileSync(MAIN_LAYOUT_PATH, 'utf8');
  assert(
    mainLayout.includes('id="floatingCartBtn" class="fixed right-5 bottom-24 md:bottom-8 z-40"'),
    '_Layout.cshtml contains #floatingCartBtn at fixed right-5 bottom-24 md:bottom-8 z-40'
  );

  const testViewports = [
    { width: 375, height: 667, isMobile: true, name: 'iPhone SE (375px)' },
    { width: 390, height: 844, isMobile: true, name: 'iPhone 14/15 (390px)' },
    { width: 412, height: 915, isMobile: true, name: 'Samsung Galaxy (412px)' },
    { width: 430, height: 932, isMobile: true, name: 'iPhone Pro Max (430px)' },
    { width: 600, height: 800, isMobile: true, name: 'Tablet Portrait (600px)' },
    { width: 767, height: 1024, isMobile: true, name: 'Mobile Edge (767px)' },
    { width: 768, height: 1024, isMobile: false, name: 'Tablet / Desktop (768px)' },
    { width: 1024, height: 768, isMobile: false, name: 'Desktop standard (1024px)' },
    { width: 1280, height: 800, isMobile: false, name: 'Desktop Wide (1280px)' },
    { width: 1920, height: 1080, isMobile: false, name: 'Desktop Full HD (1920px)' }
  ];

  let cleanView = cleanRazorDirectives(loichucView);
  let cleanLayout = cleanRazorDirectives(mainLayout);
  cleanLayout = cleanLayout.replace('</head>', `<style>\n${siteCssContent}\n</style>\n</head>`);
  const fullHtml = cleanLayout.replace('@RenderBody()', cleanView);

  const pageHtmlPath = path.join(tempDir, 'loichuc_collision_test.html');
  fs.writeFileSync(pageHtmlPath, fullHtml, 'utf8');
  const pageFileUrl = 'file:///' + pageHtmlPath.replace(/\\/g, '/');

  for (const vp of testViewports) {
    const wrapperHtml = `<!DOCTYPE html>
<html>
<head><meta charset="utf-8"></head>
<body>
<iframe id="testFrame" src="${pageFileUrl}" style="width: ${vp.width}px; height: ${vp.height}px; border: none;"></iframe>
<div id="report"></div>
<script>
  const iframe = document.getElementById('testFrame');
  iframe.onload = () => {
    setTimeout(() => {
      try {
        const doc = iframe.contentDocument || iframe.contentWindow.document;
        const win = iframe.contentWindow;

        const wishBtn = doc.querySelector('button.fixed.z-40') || doc.querySelector('[class*="bottom-[10.5rem]"]');
        const cartBtn = doc.getElementById('floatingCartBtn');
        const nav = doc.querySelector('.navigation.fixed');

        if (!wishBtn) throw new Error('Wish button not found');
        if (!cartBtn) throw new Error('Cart button not found');

        const wishRect = wishBtn.getBoundingClientRect();
        const cartRect = cartBtn.getBoundingClientRect();
        const navRect = nav ? nav.getBoundingClientRect() : null;

        const isNavVisible = nav ? (win.getComputedStyle(nav).display !== 'none') : false;

        function doOverlap(r1, r2) {
          return !(r1.right <= r2.left ||
                   r1.left >= r2.right ||
                   r1.bottom <= r2.top ||
                   r1.top >= r2.bottom);
        }

        const overlapWishCart = doOverlap(wishRect, cartRect);
        const overlapWishNav = isNavVisible ? doOverlap(wishRect, navRect) : false;
        const overlapCartNav = isNavVisible ? doOverlap(cartRect, navRect) : false;

        const gapWishCart = cartRect.top - wishRect.bottom;
        const gapCartNav = isNavVisible ? (navRect.top - cartRect.bottom) : null;

        const report = {
          viewport: ${vp.width},
          isMobile: ${vp.isMobile},
          isNavVisible,
          wishRect: { top: Math.round(wishRect.top), bottom: Math.round(wishRect.bottom) },
          cartRect: { top: Math.round(cartRect.top), bottom: Math.round(cartRect.bottom) },
          navRect: navRect ? { top: Math.round(navRect.top), bottom: Math.round(navRect.bottom) } : null,
          gapWishCart: Math.round(gapWishCart),
          gapCartNav: gapCartNav !== null ? Math.round(gapCartNav) : null,
          overlapWishCart,
          overlapWishNav,
          overlapCartNav
        };

        document.getElementById('report').innerText =
          '__CHALLENGER_REPORT_START__' + JSON.stringify(report) + '__CHALLENGER_REPORT_END__';
      } catch (err) {
        document.getElementById('report').innerText =
          '__CHALLENGER_ERROR__' + err.message + '__CHALLENGER_ERROR__';
      }
    }, 500);
  };
</script>
</body>
</html>`;

    const wrapperPath = path.join(tempDir, `loichuc_vp_${vp.width}.html`);
    fs.writeFileSync(wrapperPath, wrapperHtml, 'utf8');

    const res = await runChromeWithReport(chromePath, wrapperPath, `${vp.width + 50},${vp.height + 50}`);
    assert(res.success, `[${vp.name}] Chrome calculated layout bounding boxes`, res.error);

    if (res.success && res.report) {
      const rep = res.report;
      if (vp.isMobile) {
        assert(!rep.overlapWishCart, `[${vp.name}] Wish button does NOT overlap Cart button (gap: ${rep.gapWishCart}px)`);
        assert(!rep.overlapWishNav, `[${vp.name}] Wish button does NOT overlap mobile navigation bar`);
        assert(!rep.overlapCartNav, `[${vp.name}] Cart button does NOT overlap mobile navigation bar (gap: ${rep.gapCartNav}px)`);
        assert(rep.gapWishCart >= 10, `[${vp.name}] Wish button maintains safe clearance >= 10px above Cart button (${rep.gapWishCart}px)`);
        assert(rep.isNavVisible, `[${vp.name}] Mobile navigation bar is active and visible`);
      } else {
        assert(!rep.isNavVisible, `[${vp.name}] Mobile navigation bar is hidden (display: none)`);
        assert(!rep.overlapWishCart, `[${vp.name}] Wish button does NOT overlap Cart button on desktop (gap: ${rep.gapWishCart}px)`);
        assert(rep.gapWishCart >= 8, `[${vp.name}] Desktop gap between Wish and Cart is >= 8px (${rep.gapWishCart}px)`);
      }
    }
  }
}

// =============================================================================
// TEST SUITE 3: RAPID OPENING/CLOSING OF MOBILE DRAWER & STATE RACES
// =============================================================================
async function testMobileDrawerInteractions(chromePath, tempDir) {
  console.log('\n================================================================================');
  console.log(' SUITE 3: MOBILE DRAWER RAPID INTERACTION & RACE CONDITION STRESS');
  console.log('================================================================================');

  const mainLayout = fs.readFileSync(MAIN_LAYOUT_PATH, 'utf8');
  let cleanLayout = cleanRazorDirectives(mainLayout);
  cleanLayout = cleanLayout.replace('</head>', `<style>\n${siteCssContent}\n</style>\n</head>`);
  const fullHtml = cleanLayout.replace('@RenderBody()', '<div style="height: 1000px; padding: 20px;">Public Body</div>');

  const pageHtmlPath = path.join(tempDir, 'drawer_stress_test.html');
  fs.writeFileSync(pageHtmlPath, fullHtml, 'utf8');
  const pageFileUrl = 'file:///' + pageHtmlPath.replace(/\\/g, '/');

  const wrapperHtml = `<!DOCTYPE html>
<html>
<head><meta charset="utf-8"></head>
<body>
<iframe id="testFrame" src="${pageFileUrl}" style="width: 390px; height: 844px; border: none;"></iframe>
<div id="report"></div>
<script>
  const iframe = document.getElementById('testFrame');
  iframe.onload = () => {
    setTimeout(() => {
      try {
        const doc = iframe.contentDocument || iframe.contentWindow.document;
        const win = iframe.contentWindow;

        const drawer = doc.getElementById('mobileDrawer');
        const backdrop = doc.getElementById('mobileDrawerBackdrop');
        const toggleBtn = doc.getElementById('mobileNavToggle');
        const closeBtn = doc.getElementById('mobileNavClose');

        if (!drawer || !backdrop || !toggleBtn || !closeBtn) {
          throw new Error('Required drawer elements missing from DOM');
        }

        const tests = [];

        // 1. Initial State
        const initialClosed = drawer.classList.contains('translate-x-full') &&
                              !drawer.classList.contains('translate-x-0') &&
                              drawer.getAttribute('aria-hidden') === 'true' &&
                              backdrop.classList.contains('opacity-0') &&
                              backdrop.classList.contains('pointer-events-none') &&
                              !doc.body.classList.contains('overflow-hidden');
        tests.push({ name: 'Initial state strictly closed and non-blocking', pass: initialClosed });

        // 2. Open via toggle click
        toggleBtn.click();
        const openedViaToggle = drawer.classList.contains('translate-x-0') &&
                                !drawer.classList.contains('translate-x-full') &&
                                drawer.getAttribute('aria-hidden') === 'false' &&
                                toggleBtn.getAttribute('aria-expanded') === 'true' &&
                                backdrop.classList.contains('opacity-100') &&
                                doc.body.classList.contains('overflow-hidden');
        tests.push({ name: 'Open via #mobileNavToggle click', pass: openedViaToggle });

        // 3. Close via close button
        closeBtn.click();
        const closedViaBtn = drawer.classList.contains('translate-x-full') &&
                             drawer.getAttribute('aria-hidden') === 'true' &&
                             toggleBtn.getAttribute('aria-expanded') === 'false' &&
                             backdrop.classList.contains('opacity-0') &&
                             !doc.body.classList.contains('overflow-hidden');
        tests.push({ name: 'Close via #mobileNavClose click', pass: closedViaBtn });

        // 4. Open via JS function then close via Backdrop click
        win.openMobileDrawer();
        const reOpened = drawer.classList.contains('translate-x-0');
        backdrop.click();
        const closedViaBackdrop = drawer.classList.contains('translate-x-full') &&
                                  drawer.getAttribute('aria-hidden') === 'true' &&
                                  !doc.body.classList.contains('overflow-hidden');
        tests.push({ name: 'Close via backdrop click', pass: reOpened && closedViaBackdrop });

        // 5. Open via JS function then close via Keyboard ESC
        win.openMobileDrawer();
        const escEvent = new win.KeyboardEvent('keydown', { key: 'Escape', code: 'Escape', keyCode: 27, bubbles: true });
        doc.dispatchEvent(escEvent);
        const closedViaEsc = drawer.classList.contains('translate-x-full') &&
                             drawer.getAttribute('aria-hidden') === 'true' &&
                             !doc.body.classList.contains('overflow-hidden');
        tests.push({ name: 'Close via Keyboard ESC key', pass: closedViaEsc });

        // 6. Adversarial Edge Case: Press ESC when already closed
        let escNoCrash = true;
        try {
          doc.dispatchEvent(escEvent);
        } catch (_) {
          escNoCrash = false;
        }
        const escClosedState = drawer.classList.contains('translate-x-full') && !doc.body.classList.contains('overflow-hidden');
        tests.push({ name: 'Redundant ESC key press when closed causes zero crash or state corruption', pass: escNoCrash && escClosedState });

        // 7. Rapid Sequence Stress: 50 open calls followed by 50 close calls
        let rapidNoCrash = true;
        try {
          for (let i = 0; i < 50; i++) win.openMobileDrawer();
          for (let i = 0; i < 50; i++) win.closeMobileDrawer();
        } catch (_) {
          rapidNoCrash = false;
        }
        const cleanAfterBatch = drawer.classList.contains('translate-x-full') &&
                                !drawer.classList.contains('translate-x-0') &&
                                !doc.body.classList.contains('overflow-hidden');
        tests.push({ name: '50 rapid sequential opens followed by 50 rapid closes', pass: rapidNoCrash && cleanAfterBatch });

        // 8. High-Frequency Alternating Stress: 100 alternating open/close cycles
        let alternatingNoCrash = true;
        try {
          for (let i = 0; i < 100; i++) {
            win.openMobileDrawer();
            win.closeMobileDrawer();
          }
        } catch (_) {
          alternatingNoCrash = false;
        }
        const cleanAfterAlternating = drawer.classList.contains('translate-x-full') &&
                                      !drawer.classList.contains('translate-x-0') &&
                                      !doc.body.classList.contains('overflow-hidden');
        tests.push({ name: '100 alternating open/close cycles under zero delay', pass: alternatingNoCrash && cleanAfterAlternating });

        // 9. Accordion Submenu toggle test
        let accordionPass = false;
        const submenu = doc.getElementById('drawerKyUcSubmenu');
        if (submenu) {
          const initialHidden = submenu.classList.contains('hidden');
          win.toggleDrawerSubmenu('drawerKyUcSubmenu');
          const openedSubmenu = !submenu.classList.contains('hidden');
          win.toggleDrawerSubmenu('drawerKyUcSubmenu');
          const reClosedSubmenu = submenu.classList.contains('hidden');
          accordionPass = initialHidden && openedSubmenu && reClosedSubmenu;
        }
        tests.push({ name: 'Accordion campaign years submenu expands and collapses', pass: accordionPass });

        // 10. Z-Index Layering: Backdrop (10000) and Drawer (10001) vs Bottom Nav (9999)
        const nav = doc.querySelector('.navigation.fixed');
        const navZ = nav ? parseInt(win.getComputedStyle(nav).zIndex || '0', 10) : 9999;
        const backdropZ = parseInt(win.getComputedStyle(backdrop).zIndex || '0', 10);
        const drawerZ = parseInt(win.getComputedStyle(drawer).zIndex || '0', 10);
        const zIndexValid = (drawerZ > backdropZ) && (backdropZ > navZ) && (drawerZ === 10001) && (backdropZ === 10000);
        tests.push({
          name: \`Z-Index layering verified: drawer(\${drawerZ}) > backdrop(\${backdropZ}) > bottomNav(\${navZ})\`,
          pass: zIndexValid
        });

        document.getElementById('report').innerText =
          '__CHALLENGER_REPORT_START__' + JSON.stringify(tests) + '__CHALLENGER_REPORT_END__';
      } catch (err) {
        document.getElementById('report').innerText =
          '__CHALLENGER_ERROR__' + err.message + '__CHALLENGER_ERROR__';
      }
    }, 500);
  };
</script>
</body>
</html>`;

  const wrapperPath = path.join(tempDir, 'drawer_wrapper.html');
  fs.writeFileSync(wrapperPath, wrapperHtml, 'utf8');

  const res = await runChromeWithReport(chromePath, wrapperPath, '500,900');
  assert(res.success, 'Chrome evaluated mobile drawer interactions and stress test harness', res.error);

  if (res.success && res.report && Array.isArray(res.report)) {
    res.report.forEach(t => {
      assert(t.pass, t.name);
    });
  }
}

// =============================================================================
// TEST SUITE 4: STOREFRONT THEME ISOLATION EMPIRICAL STRESS
// =============================================================================
function testStorefrontIsolationStress() {
  console.log('\n================================================================================');
  console.log(' SUITE 4: STOREFRONT THEME ISOLATION STRESS (Adversarial VM Simulation)');
  console.log('================================================================================');

  const cuaHangLayout = fs.readFileSync(STORE_LAYOUT_PATH, 'utf8');
  const mainLayout = fs.readFileSync(MAIN_LAYOUT_PATH, 'utf8');

  // 1. Strict static absence
  assert(!cuaHangLayout.includes('themeToggleBtn'), '_CuaHangLayout contains 0 occurrences of themeToggleBtn');
  assert(!cuaHangLayout.includes('themeToggleBtnMobile'), '_CuaHangLayout contains 0 occurrences of themeToggleBtnMobile');
  assert(!cuaHangLayout.includes('theme-toggle-btn'), '_CuaHangLayout contains 0 occurrences of theme-toggle-btn');
  assert(!cuaHangLayout.includes('toggleThemeMode'), '_CuaHangLayout contains 0 occurrences of toggleThemeMode');
  assert(!cuaHangLayout.includes('toggleTheme'), '_CuaHangLayout contains 0 occurrences of toggleTheme');

  // 2. Extract scripts for dynamic sandbox testing
  const storeScriptRegex = /<!-- CÔ LẬP CỬA HÀNG: CƯỠNG CHẾ LIGHT MODE TUYỆT ĐỐI -->[\s\S]*?<script>([\s\S]*?)<\/script>/i;
  const storeHeadMatch = cuaHangLayout.match(storeScriptRegex);
  assert(Boolean(storeHeadMatch), 'Found head Anti-FOUC guard script in _CuaHangLayout.cshtml');
  const storeHeadScript = storeHeadMatch ? storeHeadMatch[1] : '';

  // Extract toggleThemeMode function accurately
  const startIdx = mainLayout.indexOf('function toggleThemeMode()');
  assert(startIdx !== -1, 'Found toggleThemeMode function in _Layout.cshtml');
  const endMarker = 'window.toggleThemeMode = toggleThemeMode;';
  const endIdx = mainLayout.indexOf(endMarker, startIdx);
  assert(endIdx !== -1, 'Found toggleThemeMode function end in _Layout.cshtml');
  const toggleThemeModeCode = mainLayout.substring(startIdx, endIdx + endMarker.length);

  // Minimal Mock Browser Environment
  class MockClassList {
    constructor(init = []) { this.classes = new Set(init); }
    add(...c) { c.forEach(x => this.classes.add(x)); }
    remove(...c) { c.forEach(x => this.classes.delete(x)); }
    contains(c) { return this.classes.has(c); }
    toString() { return Array.from(this.classes).join(' '); }
  }

  class MockStorage {
    constructor(init = {}) { this.data = { ...init }; }
    getItem(k) { return this.data[k] !== undefined ? this.data[k] : null; }
    setItem(k, v) { this.data[k] = String(v); }
    removeItem(k) { delete this.data[k]; }
  }

  function createEnv(pathname, initialTheme = 'dark', initialClasses = ['dark']) {
    const classList = new MockClassList(initialClasses);
    const storage = new MockStorage({ site_theme: initialTheme });
    const win = {
      location: { pathname, href: `https://test.local${pathname}` },
      localStorage: storage,
      document: {
        documentElement: {
          classList,
          className: classList.toString()
        },
        addEventListener: () => {},
        querySelectorAll: () => []
      },
      tailwind: { config: {} },
      syncInlineColorsForTheme: () => {},
      dispatchEvent: () => {},
      CustomEvent: function(n, o) { this.name = n; this.detail = o ? o.detail : null; },
      console: console
    };
    win.window = win;
    win.globalThis = win;
    return { win, classList, storage };
  }

  const adversarialStoreUrls = [
    '/cuahang',
    '/CUAHANG',
    '/CuaHang',
    '/cuahang/',
    '/CuaHang/',
    '/CUAHANG/',
    '/CuaHang/Index',
    '/cuahang/index',
    '/CUAHANG/INDEX',
    '/CuaHang/ChiTiet/1',
    '/cuahang/chitiet/42',
    '/CUAHANG/CHITIET/99',
    '/CuaHang/GioHang',
    '/cuahang/giohang',
    '/CUAHANG/GIOHANG',
    '/CuaHang/GioHang/DatHangThanhCong',
    '/cuahang/giohang/dathangthanhcong',
    '/CuaHang?category=gift',
    '/cuahang/giohang?step=payment',
    '/CUAHANG#items',
    '/CuaHang/Index#checkout',
    '/CuAhAnG',
    '/cuahang/index?param=/cuahang',
    '/CuaHang/ChiTiet/1?dark=true#reviews'
  ];

  // Test Anti-FOUC head script across all adversarial store URLs
  for (const u of adversarialStoreUrls) {
    const { win, classList, storage } = createEnv(u, 'dark', ['dark']);
    const ctx = vm.createContext(win);
    vm.runInContext(storeHeadScript, ctx);

    assert(
      !classList.contains('dark') && classList.contains('light'),
      `_CuaHangLayout head script enforces light & strips dark on: ${u}`
    );
    assert(
      storage.getItem('site_theme') === 'dark',
      `_CuaHangLayout head script preserves user dark preference in localStorage on: ${u}`
    );
  }

  // Test hostile toggleThemeMode() invocations on store routes
  for (const u of adversarialStoreUrls) {
    const { win, classList, storage } = createEnv(u, 'dark', ['light']);
    const ctx = vm.createContext(win);
    vm.runInContext(toggleThemeModeCode, ctx);

    // Call toggleThemeMode() hostily
    vm.runInContext('if (typeof toggleThemeMode === "function") toggleThemeMode();', ctx);

    assert(
      !classList.contains('dark') && classList.contains('light'),
      `Hostile toggleThemeMode() strictly blocked on: ${u}`
    );
    assert(
      storage.getItem('site_theme') === 'dark',
      `Storage site_theme unmutated after hostile toggle on: ${u}`
    );
  }

  // Verify normal toggleThemeMode() on non-store routes still works
  const normalEnv = createEnv('/Home/Index', 'light', ['light']);
  const normalCtx = vm.createContext(normalEnv.win);
  vm.runInContext(toggleThemeModeCode, normalCtx);
  vm.runInContext('toggleThemeMode();', normalCtx);
  assert(
    normalEnv.classList.contains('dark') && normalEnv.storage.getItem('site_theme') === 'dark',
    'Normal toggleThemeMode() on /Home/Index switches light -> dark and saves site_theme'
  );
  vm.runInContext('toggleThemeMode();', normalCtx);
  assert(
    normalEnv.classList.contains('light') && normalEnv.storage.getItem('site_theme') === 'light',
    'Normal toggleThemeMode() on /Home/Index switches dark -> light and saves site_theme'
  );
}

// =============================================================================
// MAIN RUNNER
// =============================================================================
async function main() {
  console.log('================================================================================');
  console.log(' M1 CHALLENGER 2: EMPIRICAL ADVERSARIAL STRESS TEST HARNESS');
  console.log('================================================================================');

  const chromePath = resolveChromePath();
  if (!chromePath) {
    console.error('FAIL: Chrome executable not found');
    process.exit(1);
  }

  const tempDir = path.join(os.tmpdir(), 'challenger2_m1_' + Date.now());
  fs.mkdirSync(tempDir, { recursive: true });

  try {
    await testStickyPositioning(chromePath, tempDir);
    await testFloatingButtonsCollision(chromePath, tempDir);
    await testMobileDrawerInteractions(chromePath, tempDir);
    testStorefrontIsolationStress();
  } finally {
    try { fs.rmSync(tempDir, { recursive: true, force: true }); } catch (_) {}
  }

  console.log('\n================================================================================');
  console.log(` FINAL RESULTS: ${passedAssertions} PASSED | ${failedAssertions} FAILED | TOTAL: ${totalAssertions}`);
  console.log('================================================================================');

  if (failedAssertions > 0) {
    console.error('\nFAILED ASSERTIONS:');
    failureList.forEach((f, i) => console.error(`  ${i + 1}. ${f}`));
    process.exit(1);
  } else {
    console.log('\nALL EMPIRICAL ADVERSARIAL CHALLENGES COMPLETED WITH 100% SUCCESS.');
    process.exit(0);
  }
}

main().catch(err => {
  console.error('FATAL UNCAUGHT ERROR:', err);
  process.exit(1);
});
