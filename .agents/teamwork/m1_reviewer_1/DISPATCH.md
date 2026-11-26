# Milestone 1 - Reviewer 1 Task Assignment

## Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure
**Assigned Agent**: `teamwork_reviewer` instance 1 (`m1_reviewer_1`)
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_1\`

## Authoritative User Request
Path: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
Worker Handoff: `D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\handoff.md`

## Mission
1. Independently review the changes implemented by `m1_worker_1` in:
   - `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`
   - `WEB_TinhNguyen/Areas/CuaHang/Views/Shared/_CuaHangLayout.cshtml`
   - `Styles/app.css`
   - `WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml`
2. Verify:
   - Viewport meta tag: `<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">`.
   - Root overflow prevention: `overflow-x: clip;` / `overflow-x: hidden;`.
   - Mobile hamburger button (`#mobileNavToggle`) and off-canvas drawer (`#mobileDrawer`, `#mobileDrawerBackdrop`, `#mobileNavClose`).
   - Symmetrical implementation in `_CuaHangLayout.cshtml` with 100% theme isolation (zero dark classes, zero theme toggles).
   - Bottom navigation geometry optimization (44px indicator on <= 430px screens).
   - De-confliction of floating action buttons on `LoiChuc.cshtml`.
   - Zero changes to `/admin` files.
3. Run builds and tests:
   - `npm run build:css`
   - `dotnet build`
   - `dotnet run --project tests/E2ETests/E2ETests.csproj`
   - `node tests/adversarial_store_isolation_test.js`
   - `node tests/adversarial_final_stress_harness.js`
4. State your verdict explicitly: `APPROVE` or `REQUEST_CHANGES` with concrete rationale in `D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_1\handoff.md`.


## 2026-11-07T09:57:16Z
You are teamwork_reviewer instance 1 (m1_reviewer_1).
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_1\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_1\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
Worker Handoff path: D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\handoff.md

Review m1_worker_1's changes in _Layout.cshtml, _CuaHangLayout.cshtml, Styles/app.css, and LoiChuc.cshtml.
Run builds and test suites. Verify admin preservation.
State verdict: APPROVE or REQUEST_CHANGES in:
D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_1\handoff.md
When finished, send a message to caller with a brief summary referencing handoff.md.
