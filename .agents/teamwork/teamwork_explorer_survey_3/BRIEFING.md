# BRIEFING — 2026-11-06T07:47:00Z

## Mission
Comprehensive survey of the Donation / Fundraising / Campaign (Ủng hộ / Gây quỹ / Quyên góp) subsystem at D:\web_tinh_nguyen to identify all controllers, views, viewmodels, QR display points, hardcoded bank account/QR values, transfer syntax, VietQR generation mechanisms, and architecture needed for dynamic QR configuration.

## 🔒 My Identity
- Archetype: explorer
- Roles: investigation, synthesis
- Working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_3
- Original parent: 3ebaa881-f84a-47af-9d40-eda26cf6ddcc
- Milestone: survey_donation_subsystem

## 🔒 Key Constraints
- Read-only investigation — do NOT implement
- Do NOT modify any source code or execute disruptive operations
- Write findings to handoff.md in own directory

## Current Parent
- Conversation ID: 3ebaa881-f84a-47af-9d40-eda26cf6ddcc
- Updated: 2026-11-06T07:47:00Z

## Investigation State
- **Explored paths**:
  - `BoDieuKhien\HomeController.cs` (GayQuy, Index, ChuongTrinh actions)
  - `BoDieuKhien\AdminGayQuyController.cs` (Index, Edit GET/POST, lazy seeding of KenhDongGop)
  - `MoHinh\Entities.cs` (CauHinhGayQuy, KenhDongGop, NhaTaiTroGayQuy, DonHangGayQuy)
  - `MoHinh\CauHinhTrangChuVM.cs` (CauHinhGayQuyVM, KenhDongGopVM, NhaTaiTroGayQuyVM)
  - `DuLieu\ApplicationDbContext.cs` (DbSets, OnModelCreating mappings)
  - `GiaoDien\Home\GayQuy.cshtml` (Hero, Ways to contribute, #qrModal, #sponsorModal)
  - `GiaoDien\AdminGayQuy\Index.cshtml` & `Edit.cshtml` (Tabs, Repeater, QR inputs, templateChannel)
  - `Areas\CuaHang\Controllers\GioHangController.cs` & `AdminDonHangController.cs`
  - `Areas\CuaHang\Views\GioHang\DatHangThanhCong.cshtml` & `AdminDonHang\ChiTiet.cshtml`
  - `tests\E2ETests\` (Tiers 1-5, ReferenceOracles, 337 tests verified)
- **Key findings**:
  - Two distinct donation/payment subsystems exist: Kênh Đóng Góp (`/Home/GayQuy`) and Cửa Hàng Gây Quỹ (`/CuaHang/GioHang/DatHangThanhCong`).
  - `/Home/GayQuy` uses static images (`kenh.LinkAnhQR`), seeds hardcoded MBBank `0924044283` (NGUYỄN ANH HÀO).
  - `/CuaHang/GioHang/DatHangThanhCong` uses `img.vietqr.io` standard URL but with hardcoded MBBank `0924044283` (NGUYEN THAI VINH).
  - Admin does not currently have a unified QR management system across both channels.
  - Test `Tier1_AdminOrderTests.cs` lines 238 & 244 asserts "0924044283" and "NGUYEN THAI VINH" in `ChiTiet.cshtml`.
- **Unexplored areas**: None within survey scope.

## Key Decisions Made
- Structured complete catalog of all 6 mission questions with verbatim line references and code extracts.
- Formulated recommended database and service design (`CauHinhThanhToanQR` or unified QR service) for R1 & R2 compliance.

## Artifact Index
- DISPATCH.md — incoming mission dispatch
- progress.md — liveness and heartbeat log
- handoff.md — final comprehensive report
