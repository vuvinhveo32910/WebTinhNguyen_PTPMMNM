# Milestone 1 - Reviewer 2 Task Assignment

## Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure
**Assigned Agent**: `teamwork_reviewer` instance 2 (`m1_reviewer_2`)
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_2\`

## Authoritative User Request
Path: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
Worker Handoff: `D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\handoff.md`

## Mission
1. Independently review the changes implemented by `m1_worker_1` in `_Layout.cshtml`, `_CuaHangLayout.cshtml`, `Styles/app.css`, and `LoiChuc.cshtml`.
2. Inspect:
   - Code cleanliness, semantics, and accessibility (ARIA attributes: `aria-expanded`, `aria-controls`, `aria-label`, `role="dialog"`, `aria-modal="true"`).
   - Keyboard interaction (ESC key closes drawer, focus management).
   - Invariant preservation: `#themeToggleBtnMobile`, `#floatingCartBtn`, `#floatingCartCount`, `"fixed bottom"`, `"md:hidden"`, `"CuaHang"`.
   - Storefront theme isolation: Confirm 0 occurrences of theme toggle or dark classes in `_CuaHangLayout.cshtml`.
   - Admin area preservation: Confirm 0 modifications to `/admin` files.
3. Run builds and tests:
   - `npm run build:css`
   - `dotnet build`
   - `dotnet run --project tests/E2ETests/E2ETests.csproj`
   - `node tests/adversarial_store_isolation_test.js`
   - `node tests/adversarial_final_stress_harness.js`
4. State your verdict explicitly: `APPROVE` or `REQUEST_CHANGES` with concrete rationale in `D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_2\handoff.md`.


## 2026-11-07T09:57:22Z
You are teamwork_reviewer instance 2 (m1_reviewer_2).
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_2\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_2\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
Worker Handoff path: D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\handoff.md

Review code cleanliness, ARIA semantics, keyboard interaction, theme isolation, and test invariants.
Run builds and test suites. Verify admin preservation.
State verdict: APPROVE or REQUEST_CHANGES in:
D:\web_tinh_nguyen\.agents\teamwork\m1_reviewer_2\handoff.md
When finished, send a message to caller with a brief summary referencing handoff.md.
