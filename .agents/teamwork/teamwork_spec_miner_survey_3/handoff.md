# Handoff Report: Specification Mining & Test Architecture for Granular Admin Authorization (R1-R4)

**From**: `teamwork_spec_miner_survey_3`  
**To**: Orchestrator (`3007f0a1-1a91-4596-8f2a-fc29176756e7`)  
**Artifact**: `D:\web_tinh_nguyen\.agents\teamwork\teamwork_spec_miner_survey_3\handoff.md`  
**Analysis Reference**: `D:\web_tinh_nguyen\.agents\teamwork\teamwork_spec_miner_survey_3\analysis.md`  

---

## 1. Observation

1. **Existing Test Suite Infrastructure & Execution**:
   - `tests/E2ETests/E2ETests.csproj` is an executable project targeting `net10.0` with custom reflection test runner `TestRunnerEngine.cs` and fluent assertion library `Assertions.cs`. It references `Microsoft.AspNetCore.App` and `WEB_TinhNguyen.csproj` without external test framework packages.
   - Executing `dotnet run --project tests\E2ETests\E2ETests.csproj --no-build -- --all` executed **518 tests** in **18.15s**:
     - Tier 1: 161 Passed / 161 Total
     - Tier 2: 28 Passed / 28 Total
     - Tier 3: 18 Passed / 18 Total
     - Tier 4: 12 Passed / 12 Total
     - Tier 5: 299 Passed / 299 Total
     - Result: `OVERALL: 518 Tests | 518 Passed | 0 Failed | 0 Skipped in 18.15s`. Exit code: `0`.
   - Executing `dotnet run --project tests\M2Challenger_Auth\M2Challenger_Auth.csproj --no-build` executed **204 standalone tests** with result: `RESULTS: 204 PASSED, 0 FAILED`.

2. **Auth Simulation Mechanism**:
   - In `Tier5_ChallengerM2_3_AuthPipelineHarness.cs` (lines 52-140) and `M2Challenger_Auth/Program.cs` (lines 33-80), authentication and authorization are tested via:
     - `MockAuthService : IAuthenticationService`: Captures `SignInAsync`, `SignOutAsync`, `ChallengeAsync`, and `ForbidAsync`.
     - `MockSession : ISession` and `MockTempDataProvider : ITempDataProvider`.
     - Direct controller instantiation with `DefaultHttpContext` and `ActionContext`.
     - Dead `DbContext` (`Server=127.0.0.1,65533;Database=OfflineTestDb_ChalM2_3;Connect Timeout=1;Encrypt=False;`) falling back gracefully to `DbInitializer` in-memory thread-safe store (`_inMemoryAdmins`).

3. **Existing Module Controllers & Routes**:
   - Currently, 11 Admin Controllers exist:
     - `AdminController.cs`: Dashboard `/Admin`, `Login`, `Logout`
     - 10 functional modules: `AdminTrangChuController`, `AdminChuongTrinhController`, `AdminGayQuyController`, `AdminCauHinhQRController`, `AdminKyUcController`, `AdminBanToChucController`, `AdminTinTucController`, `AdminHieuUngController`, `AdminSanPhamController` (`/CuaHang/AdminSanPham`), `AdminDonHangController` (`/CuaHang/AdminDonHang`).
   - The 11th module required by `ORIGINAL_REQUEST.md §R1` ("Quản lý tài khoản quản trị") does not yet have a dedicated controller (`AdminTaiKhoanController` or `/AdminTaiKhoan`).

4. **Strict Layout Invariants in Existing Tests**:
   - `Tier1_AdminRoutingTests.cs` (lines 62-115) and `Tier5_Challenger2_AdminAdversarialHarness.cs` (lines 190-278) mandate exact verbatim strings in `_AdminLayout.cshtml`:
     - Must NOT contain `"GiaoDien/"` or `"%2F"`.
     - Must contain verbatim `@Url.Action("Index", "<Controller>", new { area = "" })` for controllers `Admin`, `AdminTrangChu`, `AdminChuongTrinh`, `AdminGayQuy`, `AdminCauHinhQR`, `AdminKyUc`, `AdminBanToChuc`, `AdminTinTuc`.
     - Must contain verbatim: `ViewContext.RouteData.Values["Controller"]?.ToString() == "<Controller>"`.
     - Must contain link to `AdminSanPham` and `AdminDonHang` in `CuaHang` area.

5. **Locked Views Restriction**:
   - `PROJECT.md` line 83 and `Tier5_ChallengerM3_2Harness.cs` enforce that all files under `WEB_TinhNguyen/Areas/CuaHang/Views/` are SHA-256 hash locked and must NOT be edited.

---

## 2. Logic Chain

1. From Observation 1, the project's testing ecosystem relies on an in-process reflection runner (`TestRunnerEngine`) rather than an external CLI like `dotnet test` with xUnit. Therefore, any new automated verification tests for R1-R4 must use `[E2ETest(tier, feature, name)]` attributes and be integrated into `tests/E2ETests/` or dedicated challenger suites.
2. From Observation 2, because unit and integration tests run offline without a live SQL Server instance, all permission management data (R1) must support dual-mode persistence: both SQL Server schema DDL in `DbInitializer.cs` and the synchronized in-memory fallback list `_inMemoryAdmins` (`CloneAdmin`, `GetDefaultAdmins`, `AddInMemoryAdmin`, `UpdateInMemoryAdmin`).
3. From Observation 3 and `ORIGINAL_REQUEST.md §R1-R2`, the platform encompasses exactly 11 functional modules plus the dashboard. Each module's controller endpoints (GET and POST) must be guarded against unauthorized access.
4. When an admin logs in, `AdminController.Login` must generate claims representing their granular permissions. For unauthorized modules, the request must be blocked returning either HTTP 403 Forbidden or HTTP 302 Redirect, satisfying Acceptance Criteria.
5. From Observation 4, when updating `_AdminLayout.cshtml` to dynamically show/hide menu links based on user permissions (R3), the Razor template must wrap the existing link elements within permission checks (`@if (UserCanAccess(...)) { ... }`) rather than deleting or modifying the verbatim link and active-tab expressions, to avoid breaking existing tests `F19_T01-T03` and `CH2_04-CH2_06`.
6. From Observation 5, no modifications can be made to storefront views in `Areas/CuaHang/Views/`. All changes must be strictly confined to admin models, controllers, layout, and new admin account management views.

---

## 3. Caveats

- **View Modifying Restrictions**: While `_AdminLayout.cshtml` can be modified to add conditional rendering for R3, no views inside `Areas/CuaHang/Views/` can be touched.
- **SQL Server vs In-Memory Execution**: In the current test environment, SQL Server runs in an offline/dead-connection state for unit tests. All tests for R1-R4 must verify that the in-memory fallback store properly persists and enforces permissions when the DB is offline.
- **Password Hasher Aliases**: `BaoMatMatKhau` and its alias `PasswordHasher` must be consistently maintained across both namespaces (`WEB_TinhNguyen.DuLieu` and `WEB_TinhNguyen.Areas.CuaHang.Helpers`).

---

## 4. Conclusion

The specification mining and test architecture survey is complete:
1. **Requirements R1-R4 Specifications**: Formally extracted, specifying the 11 modules, PBKDF2 hashing, dual-mode store extension (`[QuyenHan]`), granular authorization filter (HTTP 200 vs 403/Redirect), dynamic menu visibility in `_AdminLayout.cshtml`, and automated test suite.
2. **Test Infrastructure**: Fully mapped. The unified suite contains 518 tests across Tiers 1-5, all passing 100%. `M2Challenger_Auth` contains 204 passing standalone tests.
3. **Verification Plan Formulated**: Detailed 4-Tier test architecture defined in `analysis.md` covering:
   - Tier 1: Unit, Model & Cryptographic Verification (scenarios T1.1 - T1.6)
   - Tier 2: Boundary, Edge Cases & Robustness (scenarios T2.1 - T2.6)
   - Tier 3: Cross-Feature Integration & Enforcement (scenarios T3.1 - T3.6)
   - Tier 4: Real-World E2E Scenarios (scenarios T4.1 - T4.3)
4. **Implementation Safety**: Identified exact verbatim string requirements in `_AdminLayout.cshtml` to guarantee zero regressions during R3 implementation.

---

## 5. Verification Method

To independently verify the findings and baseline test integrity:

1. **Verify Existing Unified Test Suite (518 tests)**:
   ```powershell
   dotnet run --project tests/E2ETests/E2ETests.csproj --no-build -- --all
   ```
   *Expected Output*: `OVERALL: 518 Tests | 518 Passed | 0 Failed | 0 Skipped` (Exit code: 0).

2. **Verify Standalone Auth Challenger (204 tests)**:
   ```powershell
   dotnet run --project tests/M2Challenger_Auth/M2Challenger_Auth.csproj --no-build
   ```
   *Expected Output*: `RESULTS: 204 PASSED, 0 FAILED` (Exit code: 0).

3. **Verify Clean Build**:
   ```powershell
   dotnet build tests/E2ETests/E2ETests.csproj --no-restore --verbosity minimal
   ```
   *Expected Output*: `Build succeeded. 0 Warning(s), 0 Error(s)`.

4. **Review Specification Artifact**:
   Inspect `D:\web_tinh_nguyen\.agents\teamwork\teamwork_spec_miner_survey_3\analysis.md` for the complete specification breakdown, discovered features table, edge cases table, and test tier definitions.
