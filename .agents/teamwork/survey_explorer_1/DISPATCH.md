# Survey 1 Task Assignment

## 2026-11-07T09:12:47Z
You are teamwork_explorer instance 1.
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Your role: Public Layout & Styling Explorer.
Investigate the public layout files, header, footer, navigation bar (desktop vs mobile, hamburger/drawer toggle), viewport configuration, and CSS/Tailwind infrastructure.
Ensure you check how admin is segregated from public styles to ensure /admin is 100% protected.

Write your findings and evidence-based report to:
D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\handoff.md
Update progress.md in your directory periodically.
When finished, send a message to caller with a brief summary referencing handoff.md.

## Objective
Investigate Public Layouts, Shared Navigation/Menu (Header, Footer, Mobile Drawer/Hamburger), Viewport configuration, and CSS/Tailwind infrastructure for the public-facing pages of `D:\web_tinh_nguyen`.

## Scope
- Explore public layout files (e.g. `WEB_TinhNguyen/GiaoDien/Shared/_Layout.cshtml` or similar), header/footer partials, navigation bar.
- Check viewport meta tags (`<meta name="viewport" content="width=device-width, initial-scale=1.0">`).
- Check CSS/Tailwind configuration: how Tailwind is compiled or loaded, classes used, breakpoints, custom CSS files in `wwwroot`.
- Check if mobile menu (hamburger / drawer toggle) currently exists, how it behaves or why it doesn't work on small screens.
- Check if any styling is shared with `/admin` and identify separation boundaries to ensure `/admin` remains 100% untouched.

## Artifacts to Read
- `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- Layout views in `WEB_TinhNguyen/GiaoDien/Shared/`
- `wwwroot/css/`, `tailwind.config.js`, `package.json` if any

## Output Requirement
Write full findings report to `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1/handoff.md`.
Report:
1. Current layout structure for public pages vs admin.
2. Viewport and base CSS configuration.
3. Current navigation header/footer implementation and mobile menu status.
4. Tailwind/CSS compilation & runtime mechanics (utility classes, build pipeline).
5. Specific technical gaps for mobile responsiveness (375px-430px viewports).
6. Concrete recommendations and interface boundaries.
