# Survey 3 Task Assignment

## Objective
Investigate existing test infrastructure and formulate the automated browser / mobile viewport testing strategy for `D:\web_tinh_nguyen` to verify 100% responsive compliance across mobile viewports (375px - 430px).

## Scope
- Investigate existing test projects (`tests/`, `tests/E2ETests/`, etc.): what frameworks are used (.NET test SDK, Playwright, Selenium, xUnit, WebApplicationFactory, bUnit, etc.), how tests are executed (`dotnet test`).
- Check how browser tests or integration tests run in this environment (headless browsers, Playwright dotnet, Puppeteer, Selenium, or WebApplicationFactory with HTML/DOM assertions).
- Check requirements in `ORIGINAL_REQUEST.md`:
  - Acceptance Criteria 1: "Bộ kiểm thử tự động trên trình duyệt (automated browser tests) kiểm tra trên các kích thước viewport di động tiêu chuẩn (chiều rộng từ 375px đến 430px) vượt qua 100% cho toàn bộ các trang công khai bên ngoài."
  - Acceptance Criteria 2: "Không có trang công khai nào bị thanh cuộn ngang ngoài ý muốn (`document.scrollingElement.scrollWidth <= window.innerWidth`) trên các viewport di động từ 375px trở lên."
  - Acceptance Criteria 3: "Menu điều hướng trên di động hiển thị gọn gàng (dạng hamburger hoặc drawer di động) và đóng/mở hoạt động chính xác khi tương tác trên màn hình nhỏ."
  - Acceptance Criteria 4: "Văn bản, hình ảnh, bảng dữ liệu và form nhập liệu trên các trang người dùng co giãn phù hợp, không bị che khuất, méo mó hoặc tràn lề màn hình di động."
  - System Integrity: All admin views/controllers/styles intact, `dotnet build` succeeds, all existing tests pass.
- Determine the exact tooling, packages, test runner mechanism, and viewport test cases (e.g. 375px, 390px, 412px, 430px) to rigorously and automatically verify these acceptance criteria.

## Artifacts to Read
- `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- `tests/` directory and `.csproj` files in `tests/`
- Existing test cases and runner scripts

## Output Requirement
Write full findings report to `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_3/handoff.md`.
Report:
1. Current test suite status, frameworks, execution commands, and pass rates.
2. Capability and architecture for automated browser testing (Playwright / Puppeteer / Selenium / WebApplicationFactory).
3. Test suite design for mobile viewport testing (375px - 430px, horizontal scroll checks, hamburger menu toggle test, form/table rendering).
4. Safety and isolation of admin tests (ensure admin regression tests pass).
5. Recommended test execution plan and test project structure.


## 2026-11-07T09:13:06Z
You are teamwork_explorer instance 3.
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_3\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_3\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Your role: Browser Testing & Automated Verification Explorer.
Investigate existing tests in `tests/`, test execution tooling (`dotnet test`), and formulate the automated browser testing strategy for mobile viewports (375px to 430px).
Check how we can run automated browser tests for:
1. Viewport rendering across 375px - 430px (100% pass).
2. Zero horizontal scrolling (`document.scrollingElement.scrollWidth <= window.innerWidth`).
3. Mobile navigation menu toggle (hamburger/drawer open/close).
4. Responsive scaling of text, images, tables, and form inputs.
5. Absolute regression safety for /admin and existing 518+ unit/integration tests.

Write your findings and evidence-based report to:
D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_3\handoff.md
Update progress.md in your directory periodically.
When finished, send a message to caller with a brief summary referencing handoff.md.
