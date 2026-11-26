# Survey 2 Task Assignment

## Objective
Enumerate and analyze all external public-facing pages, views, controllers, forms, tables, lists, and modal components in `D:\web_tinh_nguyen` to identify horizontal scroll overflow risks, font sizing issues, touch target issues, and responsive display bottlenecks on mobile screens (375px - 430px).

## Scope
- Catalog all controllers and views outside `/admin` (e.g. Home, Program/ChuongTrinh, Fundraising/GayQuy, Memory/KyUc, News/TinTuc, About/LienHe, Store/CuaHang public product & cart & order views).
- For each public page:
  - Identify structural containers, grid/flex layouts, fixed width styles (`w-[...]`, fixed px widths, min-width).
  - Identify forms (input inputs, buttons, selects, validation summaries, donation/register forms).
  - Identify tables (tables with many columns that overflow on 375px mobile screens, lack responsive wrappers or card layouts).
  - Identify images, banners, card grids.
  - Highlight specific elements that cause `document.scrollingElement.scrollWidth > window.innerWidth` on 375px-430px viewports.
- Check lock status / constraints (e.g. any file lock rules on views, distinguish public views vs admin views).

## Artifacts to Read
- `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- `WEB_TinhNguyen/GiaoDien/` and `WEB_TinhNguyen/BoDieuKhien/` (controllers and views)
- `WEB_TinhNguyen/Areas/CuaHang/` (check public vs admin routes and views)

## Output Requirement
Write full findings report to `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2/handoff.md`.
Report:
1. Complete inventory of public routes and corresponding Razor view files.
2. Layout & element analysis per page (forms, tables, lists, cards, buttons).
3. Hotspots for horizontal overflow (fixed widths, flex without wrap, wide tables).
4. Touch target and font size analysis on 375px-430px mobile viewports.
5. Verification of admin boundary (which files are `/admin` vs public).
6. Prioritized list of view changes needed for responsive compliance.


## 2026-11-07T09:12:55Z
You are teamwork_explorer instance 2.
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Your role: Public Views & Responsive Hotspot Explorer.
Enumerate all public-facing controllers and Razor views outside /admin (Home, Program, Fundraising, Memory, News, About, Store public catalog/cart/order, etc.).
Analyze layout elements (forms, tables, lists, cards, buttons) for mobile responsiveness on 375px - 430px screens.
Identify elements that risk causing horizontal scrolling (`document.scrollingElement.scrollWidth > window.innerWidth`).
Check admin isolation to ensure no /admin files are touched.

Write your findings and evidence-based report to:
D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\handoff.md
Update progress.md in your directory periodically.
When finished, send a message to caller with a brief summary referencing handoff.md.
