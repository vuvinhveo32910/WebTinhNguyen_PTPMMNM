# E2E Testing Track - Test Writer 1 Task Assignment

## Objective
Design and implement the Automated Browser Testing infrastructure and test cases for mobile viewports (375px to 430px) according to the user requirements in `ORIGINAL_REQUEST.md`.

## Inputs & Context
- Authoritative User Request: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
- Survey 3 Report: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_3\handoff.md`

## Mission & Architecture
1. **Create `TEST_INFRA.md`** at project root documenting:
   - Test philosophy (opaque-box, requirement-driven, 4-tier methodology: Feature, Boundary, Pairwise, Real-World).
   - Viewport matrix: 375px (iPhone SE), 390px (iPhone 14/15), 412px (Samsung Galaxy/Pixel), 430px (iPhone Pro Max).
   - Pass/fail criteria:
     * 100% pass across all mobile viewports for all public pages.
     * Zero horizontal scroll: `document.scrollingElement.scrollWidth <= window.innerWidth`.
     * Mobile navigation menu toggle (hamburger button visible, click opens drawer, click closes drawer).
     * Responsive images, tables, forms, and touch targets.
     * 100% preservation of `/admin` area (0 admin files modified).
2. **Implement Standalone Headless Chrome Test Runner**:
   - Location: `tests/run_mobile_viewport_browser_tests.js`.
   - Uses local Chrome (`C:\Program Files\Google\Chrome\Application\chrome.exe`) with iframe viewport emulation and `--headless=new --no-sandbox --disable-gpu --allow-file-access-from-files`.
   - Iterates over all 11 public pages at 375px, 390px, 412px, 430px.
   - Measures `scrollWidth` vs `innerWidth`, clicks `#mobileNavToggle` and asserts drawer visibility, verifies form inputs and images.
   - Outputs structured, colored terminal results and summary table.
3. **Extend C# E2E Test Suite in `tests/E2ETests/`**:
   - Add new `[E2ETest]` assertions verifying viewport tags, mobile nav markup, touch targets >= 40px, and admin boundary immutability so that `dotnet run --project tests/E2ETests/E2ETests.csproj` includes these tests.
4. **Publish `TEST_READY.md`** when tests are ready to execute.
5. Report results to `D:\web_tinh_nguyen\.agents\teamwork\e2e_test_writer_1\handoff.md`.


## 2026-11-07T09:32:34Z
You are teamwork_test_writer instance 1.
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\e2e_test_writer_1\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\e2e_test_writer_1\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Your role: E2E Testing Track Test Writer.
Your mission:
1. Create `TEST_INFRA.md` at project root according to the template in instructions.
2. Implement the standalone automated browser test runner in `tests/run_mobile_viewport_browser_tests.js`:
   - Uses local Google Chrome (`C:\Program Files\Google\Chrome\Application\chrome.exe`) with iframe viewport emulation and flags `--headless=new --no-sandbox --disable-gpu --allow-file-access-from-files`.
   - Iterates across all 4 canonical mobile viewports: 375px, 390px, 412px, 430px.
   - Tests all 11 public pages for zero horizontal scroll (`document.scrollingElement.scrollWidth <= window.innerWidth`).
   - Tests mobile hamburger toggle (#mobileNavToggle) click opening and closing #mobileDrawer.
   - Verifies images, tables, and form inputs co-exist properly without overflow.
3. Extend C# E2E test suite in `tests/E2ETests/` with mobile viewport assertions and admin preservation checks.
4. Publish `TEST_READY.md` when the test suite is ready to run.
5. Zero modifications to any source code files under `WEB_TinhNguyen/GiaoDien/Admin*` or `/admin` routes.

Write your handoff report to:
D:\web_tinh_nguyen\.agents\teamwork\e2e_test_writer_1\handoff.md
Update progress.md in your directory periodically.
When finished, send a message to caller with a brief summary referencing handoff.md.
