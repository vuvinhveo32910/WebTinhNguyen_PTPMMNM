# Milestone 1 - Forensic Auditor Task Assignment

## Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure
**Assigned Agent**: `teamwork_auditor` instance 1 (`m1_auditor_1`)
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m1_auditor_1\`

## Authoritative User Request
Path: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
Worker Handoff: `D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\handoff.md`

## Forensic Integrity Mission
Conduct an exhaustive forensic integrity verification to verify that the implementation is genuine and free of any cheating, facade implementations, or integrity violations.
Check specifically:
1. Static Analysis:
   - Check file diffs for `_Layout.cshtml`, `_CuaHangLayout.cshtml`, `Styles/app.css`, and `LoiChuc.cshtml`.
   - Verify all markup, styles, and scripts are genuine functional logic.
   - Verify NO test-specific string short-circuiting or hardcoded dummy returns were introduced.
2. Admin Immutability Audit:
   - Perform `git status` and file hash checks on `WEB_TinhNguyen/GiaoDien/Admin*`, `WEB_TinhNguyen/BoDieuKhien/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*`, and `_AdminLayout.cshtml`.
   - Verify EXACTLY ZERO admin files were touched or altered.
3. Test Tampering Audit:
   - Verify that NO test files in `tests/` or harness scripts were weakened, commented out, or bypassed.
4. Runtime Execution Audit:
   - Verify build and tests run genuinely and pass:
     * `npm run build:css`
     * `dotnet build`
     * `dotnet run --project tests/E2ETests/E2ETests.csproj`
     * `node tests/adversarial_store_isolation_test.js`
     * `node tests/adversarial_final_stress_harness.js`

State your binary verdict explicitly: `CLEAN` or `INTEGRITY VIOLATION` in `D:\web_tinh_nguyen\.agents\teamwork\m1_auditor_1\handoff.md`.


## 2026-11-07T09:58:05Z
From: 55581585-523d-44d0-807c-aeb70f9a76e5
You are teamwork_auditor instance 1 (m1_auditor_1).
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\m1_auditor_1\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\m1_auditor_1\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md
Worker Handoff path: D:\web_tinh_nguyen\.agents\teamwork\m1_worker_1\handoff.md

Conduct exhaustive forensic integrity audit:
1. Static analysis of diffs in _Layout.cshtml, _CuaHangLayout.cshtml, Styles/app.css, LoiChuc.cshtml to ensure genuine implementation with 0 dummy code.
2. Admin immutability audit: verify 0 admin files were touched or altered.
3. Test tampering audit: verify 0 test suites or assertions were weakened or bypassed.
4. Runtime execution validation: execute build and tests genuinely.
State binary verdict: CLEAN or INTEGRITY VIOLATION in:
D:\web_tinh_nguyen\.agents\teamwork\m1_auditor_1\handoff.md
When finished, send a message to caller with a brief summary referencing handoff.md.
