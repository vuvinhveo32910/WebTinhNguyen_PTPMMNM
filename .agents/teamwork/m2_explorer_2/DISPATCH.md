# Milestone 2 - Explorer 2 Task Assignment

## Milestone: M2 - Public Home Views Responsive Optimization
**Assigned Agent**: `teamwork_explorer` instance 2 (`m2_explorer_2`)
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\`
**Focus**: Container Paddings, Card Grids, Countdown Boxes & Sponsor Logos Scaling across Home views (`WEB_TinhNguyen/GiaoDien/Home/`).

## Authoritative User Request
Path: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
Survey 2 Report: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\handoff.md`

## Mission
1. Investigate container paddings, card grids, countdown timer boxes, and sponsor logo grids across `WEB_TinhNguyen/GiaoDien/Home/`:
   - `Index.cshtml`: Countdown timer card padding `p-8 md:p-14` and 4 countdown units (`w-20` each + gaps = 356px inside 311px available space at 375px viewport) -> needs responsive sizing (`w-14 sm:w-20`, `p-4 sm:p-8`). Sponsor logos (`w-36 h-36`) scaling.
   - `BanToChuc.cshtml`: Level 1, 2, 3 card grid responsiveness on 375px-430px viewports.
   - `KyUc.cshtml`: Photo gallery grid (`grid-cols-2 sm:grid-cols-3 lg:grid-cols-5`) and sponsor cards.
   - `ChuongTrinh.cshtml`: Roadmap and timeline card widths.
2. Ensure every view satisfies `document.scrollingElement.scrollWidth <= window.innerWidth` across 375px, 390px, 412px, 430px.
3. Verify that zero `/admin` files are involved.
4. Output detailed recommendations and code replacement specifications to `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_2\handoff.md`.
