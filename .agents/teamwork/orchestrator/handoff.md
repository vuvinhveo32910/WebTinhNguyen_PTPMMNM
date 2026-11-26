# Orchestrator Soft Handoff (State Dump) — Generation 1 to Generation 2

## 1. Milestone State
- **Overall Mission**: Đồng bộ và tối ưu hóa toàn bộ giao diện người dùng bên ngoài của dự án web hiện tại (`D:\web_tinh_nguyen`) để hiển thị trực quan, dễ nhìn và thuận tiện thao tác trên trình duyệt di động theo chuẩn responsive web (viewport 375px - 430px, không tràn ngang `scrollWidth <= innerWidth`, menu hamburger/drawer, co giãn linh hoạt), giữ nguyên hoàn toàn khu vực quản trị (`/admin`), đạt 100% kiểm thử tự động.
- **Milestone Overview**:
  - **Survey & Architecture Mapping**: [DONE] (3 Explorers completed, `PROJECT.md` authored).
  - **Milestone 4 (E2E Testing Track)**: [DONE / TEST_READY] (`TEST_INFRA.md`, `TEST_READY.md`, `tests/run_mobile_viewport_browser_tests.js`, Tier 1 & Tier 4 C# tests).
  - **Milestone 1 (Public Layouts & Navigation Infrastructure)**: [DONE - Gate PASS]
    - Modified: `_Layout.cshtml`, `_CuaHangLayout.cshtml`, `Styles/app.css`, `LoiChuc.cshtml`.
    - Gate passed unconditionally: Reviewer 1 (APPROVE), Reviewer 2 (APPROVE), Challenger 1 (CONFIRM), Challenger 2 (CONFIRM), Forensic Auditor (CLEAN).
  - **Milestone 2 (Public Home Views Responsive Optimization)**: [IN_PROGRESS - Investigation Phase COMPLETED]
    - All 3 Explorers (`m2_explorer_1`, `m2_explorer_2`, `m2_explorer_3`) have delivered comprehensive handoff reports with exact before/after code replacement specifications.
    - **READY FOR WORKER IMPLEMENTATION & GATE VERIFICATION**.
  - **Milestone 3 (Storefront, Cart & Checkout Views)**: [PLANNED]
  - **Milestone 5 (Final E2E Pass & Adversarial Hardening)**: [PLANNED]

---

## 2. Active Subagents
- Cumulative spawn count: **16 / 16** (Threshold reached).
- Active/running subagents: **NONE**. All 16 subagents (`survey_explorer_1..3`, `e2e_test_writer_1`, `m1_explorer_1..3`, `m1_worker_1`, `m1_reviewer_1..2`, `m1_challenger_1..2`, `m1_auditor_1`, `m2_explorer_1..3`) are in `idle` state and have delivered their handoff reports.

---

## 3. Pending Decisions & Immediate Next Steps for Successor

### Immediate Next Step: Dispatch Milestone 2 Worker (`m2_worker_1`)
The successor orchestrator must dispatch a `teamwork_worker` with Role "Home Views Responsive Worker" to apply the synthesized changes across the 8 Home views (`WEB_TinhNguyen/GiaoDien/Home/`), `_Layout.cshtml`, and `tests/run_mobile_viewport_browser_tests.js`.

#### Concrete Specifications for `m2_worker_1`:
1. **`WEB_TinhNguyen/GiaoDien/Home/TinTuc.cshtml`**:
   - Hero title (line 93-96): Replace inline `font-size: @(Model?.Hero_CoChuTieuDe ?? "64px")` with `font-size: clamp(1.875rem, 6vw, @(Model?.Hero_CoChuTieuDe ?? "64px"))` and `leading-[1.2]`.
   - Card titles (line 166-169): Replace `font-size: @(news.CoChuTieuDe ?? "18px")` with `font-size: clamp(1rem, 2.5vw, @(news.CoChuTieuDe ?? "18px"))` + `line-clamp-2 break-words`.
   - Like button (line 184-191): Upgrade to `min-h-[40px] min-w-[40px]`.
   - Modal close button (line 225-230): Upgrade to `h-10 w-10 min-w-[40px] min-h-[40px]`.
   - Modal FB link (line 243-246): Add `min-h-[44px]`.

2. **`WEB_TinhNguyen/GiaoDien/Home/GayQuy.cshtml`**:
   - Hero title (line 68-71): Replace inline `font-size: @(Model?.Hero_CoChuTieuDe ?? "48px")` with `font-size: clamp(1.625rem, 5.5vw, @(Model?.Hero_CoChuTieuDe ?? "48px"))`.
   - Section headings (lines 107, 278, 303): `text-2xl font-black leading-tight sm:text-3xl md:text-4xl lg:text-5xl`.
   - Donation cards (lines 153 & 225): `w-full max-w-full sm:max-w-[360px] p-5 sm:p-7`.
   - Bank details (lines 174-192 & 246-249): Add `break-all` on `@effectiveAccNum` and `@effectiveMemo`, `shrink-0` on labels (`Ngân hàng:`, `Số tài khoản:`, `Chủ TK:`, `Cú pháp:`). **Resolves E2E test `M4_F06_T01`!**
   - Sponsor buttons (line 329): Replace `w-44` with `w-[calc(50%-0.75rem)] max-w-[176px] sm:w-56 h-24 sm:h-32`.
   - Modal close buttons (lines 348 & 365): Upgrade to `h-10 w-10 min-w-[40px] min-h-[40px]`.
   - Modal note (line 357): Add `break-all`.

3. **`WEB_TinhNguyen/GiaoDien/Home/Index.cshtml`**:
   - Countdown card (line 152): `p-4 sm:p-8 md:p-14 rounded-3xl sm:rounded-[2.5rem]`.
   - Slide (line 150): `px-2 sm:px-6 md:px-12`.
   - Countdown title (line 167): `text-2xl sm:text-4xl md:text-6xl lg:text-7xl` (or `text-3xl sm:text-5xl md:text-7xl`).
   - Countdown boxes (line 187-204): `flex-nowrap sm:flex-wrap justify-center gap-2 sm:gap-3 md:gap-5 max-w-full`, each box `w-16 sm:w-20 md:w-28 flex-1 min-w-[62px] max-w-[80px] sm:max-w-none py-3 px-2 sm:p-4`, numbers `text-2xl sm:text-3xl md:text-5xl`, labels `text-[9px] sm:text-[10px] md:text-xs`.
   - CTA buttons (lines 209-215): `w-full sm:w-auto text-center px-6 sm:px-8 py-3 sm:py-3.5`.
   - "Về dự án" (line 244): `text-2xl sm:text-3xl md:text-4xl lg:text-6xl`.
   - Section headings (lines 280, 329, 374, 418, 460): `text-2xl sm:text-3xl md:text-4xl lg:text-5xl`.
   - News card title (line 343): `clamp(1rem, 2.5vw, @(n.KichCoTieuDe ?? "18px"))` + `line-clamp-2 break-words`.
   - News "Đọc tiếp" and Like buttons (lines 349-357): `min-h-[40px] min-w-[40px]`.
   - Modal close button (line 483): `w-10 h-10 min-w-[40px] min-h-[40px]`.
   - Sponsor logo tooltips (line 393): `max-w-[200px] truncate` (was `whitespace-nowrap`).

4. **`WEB_TinhNguyen/GiaoDien/Home/BanToChuc.cshtml`**:
   - Main title H1 (line 264): `text-2xl sm:text-4xl md:text-5xl`.
   - Cấp 1 card (line 294, 315, 327): `p-5 sm:px-10 sm:py-7`, name pill `max-w-full truncate`.
   - Cấp 1, 2, 3 Facebook icon buttons (lines 305-309, 413-416, 527-531): Upgrade to `h-10 w-10 min-w-[40px] min-h-[40px]` with `-bottom-1 -right-1` on avatar, `shadow-md hover:scale-110`.
   - Cấp 3 container (line 495): `p-3 sm:p-6 md:p-8 rounded-2xl sm:rounded-3xl`.
   - Cấp 3 member card (line 504): `p-3 sm:p-4 rounded-xl sm:rounded-2xl`.
   - `#btnShowMoreMembers` (line 540) & CTA (line 559): `min-h-[44px]`.

5. **`WEB_TinhNguyen/GiaoDien/Home/KyUc.cshtml`**:
   - Hero section (lines 20, 40): `px-4 sm:px-6 py-20 sm:py-32`, title `text-2xl sm:text-4xl md:text-6xl lg:text-7xl`.
   - Summary rich text (lines 52, 62): `px-4 sm:px-6`, card `p-4 sm:p-8 md:p-10 rounded-2xl sm:rounded-3xl`.
   - Section headings (lines 56, 76, 104): `text-2xl sm:text-3xl md:text-4xl lg:text-5xl`.
   - Gallery (line 85): `w-full @(img.LaAnhDoc ? "h-full min-h-[220px]" : "aspect-square")`.
   - Sponsor cards (lines 123, 126): `h-24 w-[150px] sm:h-32 sm:w-56 gap-3 sm:gap-5 md:gap-6 p-3 sm:p-6`.

6. **`WEB_TinhNguyen/GiaoDien/Home/ChuongTrinh.cshtml`**:
   - Hero section (lines 17, 26): `px-4 sm:px-6`, title `text-2xl sm:text-4xl lg:text-6xl`.
   - Hero floating photo (line 56): `translate-x-0 sm:translate-x-3` and `border-2 sm:border-4`.
   - Roadmap heading (line 75): `text-2xl sm:text-3xl lg:text-5xl`.
   - Timeline cards (line 106): `p-4 sm:p-6 rounded-2xl sm:rounded-3xl`.
   - Timeline connecting line (line 118, 123): `flex-1 min-h-[1.5rem] w-0.5 my-1 bg-emerald-200 dark:bg-emerald-900` with container `self-stretch shrink-0`.

7. **`WEB_TinhNguyen/GiaoDien/Home/LoiChuc.cshtml` & `ComingSoon.cshtml`**:
   - `LoiChuc.cshtml` (line 35): Title `text-2xl sm:text-4xl md:text-5xl`.
   - `ComingSoon.cshtml` (lines 74, 78): `text-2xl sm:text-3xl md:text-5xl` and `text-2xl md:text-4xl`.

8. **`WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml`**:
   - Header logo `<a>` (line 475): Add `max-w-[calc(100%-96px)] flex-1`.
   - Header title `<span>` (line 482): `class="min-w-0 flex-1 block leading-tight overflow-hidden"`.
   - Mobile buttons `<div>` (line 543): `class="flex items-center gap-2 md:hidden shrink-0"`.

9. **`tests/run_mobile_viewport_browser_tests.js`**:
   - Line 223-224: Replace double-replace with `layout = layout.replace(/~?\/css\/site\.css/, cssUrl);`.

10. **Build & Verification after Worker**:
    - `dotnet build` -> 0 errors.
    - `npm run build:css` -> builds Tailwind CSS.
    - `dotnet run --project tests/E2ETests/E2ETests.csproj` -> **552/552 PASS (100%)** (with `M4_F06_T01` passing).
    - `node tests/run_mobile_viewport_browser_tests.js` -> 100% PASS on all Home views across 375px, 390px, 412px, 430px.

---

### M2 Verification Gate Execution
After Worker completes:
1. Spawn 2 Reviewers (`teamwork_reviewer`): `m2_reviewer_1`, `m2_reviewer_2`.
2. Spawn 2 Challengers (`teamwork_challenger`): `m2_challenger_1`, `m2_challenger_2`.
3. Spawn 1 Forensic Auditor (`teamwork_auditor`): `m2_auditor_1`.
4. Gate check: If all APPROVE/CONFIRM/CLEAN, mark M2 DONE in `PROJECT.md` and `GATE_STATUS.md`.
5. Proceed to Milestone 3 (Storefront & Cart).

---

## 4. Key Constraints & Commitments
1. **Zero Modifications to Admin**: Absolutely no files under `WEB_TinhNguyen/GiaoDien/Admin*`, `WEB_TinhNguyen/BoDieuKhien/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Controllers/Admin*`, `WEB_TinhNguyen/Areas/CuaHang/Views/Admin*`, or `_AdminLayout.cshtml` may be touched or modified.
2. **Dispatch-Only Orchestrator**: The orchestrator must NEVER write code directly, never run build/test commands, and never explore code directly. Delegate all technical execution to subagents.
3. **Forensic Audit Veto**: A Forensic Auditor verdict of `INTEGRITY VIOLATION` is a strict, unconditional binary veto.
4. **Mandatory Worker Warning**: Every worker dispatch MUST include the verbatim integrity warning:
   > DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.
5. **Mandatory Authoritative Request Path**: Every subagent dispatch MUST include the path to `ORIGINAL_REQUEST.md` (`D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`).

---

## 5. Key Artifacts
- User Request: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- Project Plan & Feature Inventory: `D:\web_tinh_nguyen\PROJECT.md`
- E2E Test Infra: `D:\web_tinh_nguyen\TEST_INFRA.md`
- E2E Test Ready: `D:\web_tinh_nguyen\TEST_READY.md`
- Gate Status: `D:\web_tinh_nguyen\.agents\teamwork\orchestrator\GATE_STATUS.md`
- M2 Explorer 1 Report: `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_1\handoff.md`
- M2 Explorer 2 Report: `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\handoff.md`
- M2 Explorer 3 Report: `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\handoff.md`
- Parent Conversation ID: `5600af65-871f-41d3-a6b8-02719bfd993c`
