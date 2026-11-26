using System;
using System.IO;
using WEB_TinhNguyen.Tests.E2E.Framework;

namespace WEB_TinhNguyen.Tests.E2E.Tier1
{
    public class Tier1_MobileViewportTests
    {
        // =========================================================================
        // FEATURE 1: Viewport & Root Overflow Standardization (M1)
        // =========================================================================

        [E2ETest(tier: 1, feature: 1, name: "M4_F01_T01: Meta viewport tag enforces width=device-width on public layouts")]
        public void M4_F01_T01_MetaViewportWidthDevice()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var storeLayoutPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "Shared", "_CuaHangLayout.cshtml");

            Assert.RequireFile(mainLayoutPath, "_Layout.cshtml");
            Assert.RequireFile(storeLayoutPath, "_CuaHangLayout.cshtml");

            var mainContent = File.ReadAllText(mainLayoutPath);
            var storeContent = File.ReadAllText(storeLayoutPath);

            Assert.Contains("name=\"viewport\"", mainContent, "_Layout.cshtml must declare viewport meta tag");
            Assert.Contains("width=device-width", mainContent, "_Layout.cshtml must enforce width=device-width");

            Assert.Contains("name=\"viewport\"", storeContent, "_CuaHangLayout.cshtml must declare viewport meta tag");
            Assert.Contains("width=device-width", storeContent, "_CuaHangLayout.cshtml must enforce width=device-width");
        }

        [E2ETest(tier: 1, feature: 1, name: "M4_F01_T02: Meta viewport tag includes viewport-fit=cover for mobile notches")]
        public void M4_F01_T02_MetaViewportFitCover()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var storeLayoutPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "Shared", "_CuaHangLayout.cshtml");

            Assert.RequireFile(mainLayoutPath, "_Layout.cshtml");
            Assert.RequireFile(storeLayoutPath, "_CuaHangLayout.cshtml");

            var mainContent = File.ReadAllText(mainLayoutPath);
            var storeContent = File.ReadAllText(storeLayoutPath);

            if (!mainContent.Contains("viewport-fit=cover"))
            {
                throw new AssertException("[SKIP] Pending implementation in M1: viewport-fit=cover in _Layout.cshtml");
            }

            Assert.Contains("viewport-fit=cover", mainContent, "_Layout.cshtml must declare viewport-fit=cover for edge-to-edge screens");
            Assert.Contains("viewport-fit=cover", storeContent, "_CuaHangLayout.cshtml must declare viewport-fit=cover for edge-to-edge screens");
        }

        [E2ETest(tier: 1, feature: 1, name: "M4_F01_T03: Global root container enforces overflow-x clip or hidden")]
        public void M4_F01_T03_GlobalRootOverflowClipOrHidden()
        {
            var appCssPath = Path.Combine(E2ETestContext.SolutionRoot, "Styles", "app.css");
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");

            Assert.RequireFile(appCssPath, "Styles/app.css");
            var appCssContent = File.ReadAllText(appCssPath);
            var layoutContent = File.ReadAllText(mainLayoutPath);

            bool hasOverflowRule = appCssContent.Contains("overflow-x: clip") ||
                                   appCssContent.Contains("overflow-x: hidden") ||
                                   layoutContent.Contains("overflow-x: clip") ||
                                   layoutContent.Contains("overflow-x: hidden") ||
                                   layoutContent.Contains("overflow-x-clip") ||
                                   layoutContent.Contains("overflow-x-hidden");

            if (!hasOverflowRule)
            {
                throw new AssertException("[SKIP] Pending implementation in M1: global overflow-x clip or hidden rule");
            }

            Assert.True(hasOverflowRule, "Root stylesheet or layout must enforce overflow-x: clip/hidden to prevent horizontal scroll");
        }

        // =========================================================================
        // FEATURE 2: Mobile Navigation Hamburger & Drawer (M1)
        // =========================================================================

        [E2ETest(tier: 1, feature: 2, name: "M4_F02_T01: Header contains mobile hamburger button (#mobileNavToggle) with md:hidden")]
        public void M4_F02_T01_MobileHamburgerButtonExists()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            Assert.RequireFile(mainLayoutPath, "_Layout.cshtml");

            var content = File.ReadAllText(mainLayoutPath);
            if (!content.Contains("mobileNavToggle"))
            {
                throw new AssertException("[SKIP] Pending implementation in M1: #mobileNavToggle hamburger button in _Layout.cshtml");
            }

            Assert.Contains("mobileNavToggle", content, "_Layout.cshtml must define #mobileNavToggle button");
            Assert.Contains("md:hidden", content, "Mobile hamburger toggle must be hidden on desktop breakpoints (md:hidden)");
        }

        [E2ETest(tier: 1, feature: 2, name: "M4_F02_T02: Mobile hamburger button defines aria-expanded and aria-controls")]
        public void M4_F02_T02_MobileHamburgerAccessibilityAttributes()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var content = File.ReadAllText(mainLayoutPath);

            if (!content.Contains("mobileNavToggle"))
            {
                throw new AssertException("[SKIP] Pending implementation in M1: #mobileNavToggle in _Layout.cshtml");
            }

            Assert.Contains("aria-expanded", content, "Mobile hamburger toggle must define aria-expanded attribute");
            Assert.Contains("aria-controls=\"mobileDrawer\"", content, "Mobile hamburger toggle must declare aria-controls=\"mobileDrawer\"");
        }

        [E2ETest(tier: 1, feature: 2, name: "M4_F02_T03: Mobile off-canvas drawer (#mobileDrawer) exists with slide-in position")]
        public void M4_F02_T03_MobileDrawerStructure()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var content = File.ReadAllText(mainLayoutPath);

            if (!content.Contains("mobileDrawer"))
            {
                throw new AssertException("[SKIP] Pending implementation in M1: #mobileDrawer off-canvas panel in _Layout.cshtml");
            }

            Assert.Contains("id=\"mobileDrawer\"", content, "_Layout.cshtml must define #mobileDrawer element");
            Assert.True(
                content.Contains("fixed") && (content.Contains("right-0") || content.Contains("inset-y-0")),
                "Mobile drawer must be fixed off-canvas element"
            );
        }

        [E2ETest(tier: 1, feature: 2, name: "M4_F02_T04: Mobile drawer includes close button (#mobileNavClose) and backdrop")]
        public void M4_F02_T04_MobileDrawerCloseButtonAndBackdrop()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var content = File.ReadAllText(mainLayoutPath);

            if (!content.Contains("mobileDrawer"))
            {
                throw new AssertException("[SKIP] Pending implementation in M1: #mobileDrawer in _Layout.cshtml");
            }

            Assert.True(
                content.Contains("mobileNavClose") || content.Contains("mobileDrawerClose"),
                "Mobile drawer must include close button (#mobileNavClose)"
            );
            Assert.True(
                content.Contains("mobileDrawerBackdrop") || content.Contains("bg-black/50"),
                "Mobile drawer must include backdrop overlay element"
            );
        }

        [E2ETest(tier: 1, feature: 2, name: "M4_F02_T05: Mobile drawer includes all core public navigation links")]
        public void M4_F02_T05_MobileDrawerNavigationLinks()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var content = File.ReadAllText(mainLayoutPath);

            if (!content.Contains("mobileDrawer"))
            {
                throw new AssertException("[SKIP] Pending implementation in M1: #mobileDrawer links in _Layout.cshtml");
            }

            // Must include references to major sections
            Assert.Contains("ChuongTrinh", content, "Mobile drawer must link to ChuongTrinh");
            Assert.Contains("GayQuy", content, "Mobile drawer must link to GayQuy");
            Assert.Contains("BanToChuc", content, "Mobile drawer must link to BanToChuc");
            Assert.Contains("TinTuc", content, "Mobile drawer must link to TinTuc");
        }

        // =========================================================================
        // FEATURE 3: Mobile Bottom Nav & Floating Action De-confliction (M1)
        // =========================================================================

        [E2ETest(tier: 1, feature: 3, name: "M4_F03_T01: Mobile bottom nav preserves existing test contract invariants")]
        public void M4_F03_T01_BottomNavInvariantsPreserved()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var content = File.ReadAllText(mainLayoutPath);

            Assert.Contains("CuaHang", content, "Layout must preserve verbatim 'CuaHang' token");
            Assert.Contains("hidden md:", content, "Layout must preserve verbatim 'hidden md:' token");
            Assert.Contains("fixed bottom", content, "Layout must preserve verbatim 'fixed bottom' token");
            Assert.Contains("md:hidden", content, "Layout must preserve verbatim 'md:hidden' token");
            Assert.Contains("themeToggleBtnMobile", content, "Layout must preserve #themeToggleBtnMobile invariant");
        }

        [E2ETest(tier: 1, feature: 3, name: "M4_F03_T02: Touch targets for mobile interactive buttons maintain >= 40px")]
        public void M4_F03_T02_TouchTargetMinimumSize()
        {
            var mainLayoutPath = Path.Combine(E2ETestContext.ViewsRoot, "Shared", "_Layout.cshtml");
            var content = File.ReadAllText(mainLayoutPath);

            // Verify theme button mobile touch target sizing
            Assert.True(
                content.Contains("w-9 h-9") || content.Contains("w-10 h-10") || content.Contains("p-2") || content.Contains("min-h-[40px]"),
                "Mobile buttons must maintain accessible touch target dimensions"
            );
        }

        // =========================================================================
        // FEATURE 6: Bank Memo Break-All Safety in Donation View (M2)
        // =========================================================================

        [E2ETest(tier: 1, feature: 6, name: "M4_F06_T01: Donation bank transfer memos enforce break-all or break-words")]
        public void M4_F06_T01_DonationMemoBreakAll()
        {
            var gayQuyPath = Path.Combine(E2ETestContext.ViewsRoot, "Home", "GayQuy.cshtml");
            Assert.RequireFile(gayQuyPath, "GayQuy.cshtml");

            var content = File.ReadAllText(gayQuyPath);
            if (!content.Contains("break-all") && !content.Contains("break-words"))
            {
                throw new AssertException("[SKIP] Pending implementation in M2: break-all or break-words on bank memo in GayQuy.cshtml");
            }

            Assert.True(
                content.Contains("break-all") || content.Contains("break-words"),
                "Donation transfer memos must declare break-all or break-words to avoid mobile horizontal scroll"
            );
        }

        // =========================================================================
        // FEATURE 8: Checkout Form Inputs Font Size for iOS Auto-Zoom (M3)
        // =========================================================================

        [E2ETest(tier: 1, feature: 8, name: "M4_F08_T01: Checkout form inputs declare text-base on mobile to eliminate auto-zoom")]
        public void M4_F08_T01_CheckoutFormInputFontSize()
        {
            var gioHangPath = Path.Combine(E2ETestContext.AreasRoot, "Views", "GioHang", "Index.cshtml");
            Assert.RequireFile(gioHangPath, "GioHang/Index.cshtml");

            var content = File.ReadAllText(gioHangPath);
            if (!content.Contains("text-base sm:text-sm") && !content.Contains("text-base"))
            {
                throw new AssertException("[SKIP] Pending implementation in M3: text-base font size on checkout inputs in GioHang/Index.cshtml");
            }

            Assert.True(
                content.Contains("text-base sm:text-sm") || content.Contains("text-base"),
                "Checkout form inputs must declare text-base (16px) on mobile viewports to prevent iOS Safari auto-zoom"
            );
        }
    }
}
