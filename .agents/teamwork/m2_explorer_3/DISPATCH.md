# Milestone 2 - Explorer 3 Task Assignment

## Milestone: M2 - Public Home Views Responsive Optimization
**Assigned Agent**: `teamwork_explorer` instance 3 (`m2_explorer_3`)
**Working Directory**: `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\`
**Focus**: Touch Targets Sizing & Bank Memo `break-all` in `GayQuy.cshtml` (resolving pending test `M4_F06_T01`).

## Authoritative User Request
Path: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
Survey 2 Report: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\handoff.md`

## Mission
1. Investigate `GayQuy.cshtml`:
   - Transfer memo line breaking: lines 188-191 & 246-249 have `flex justify-between` where `effectiveMemo` (`select-all`) can push the card margin beyond 375px if long.
   - Note test `M4_F06_T01`: Requires `break-all` or `break-words` on transfer memo element in `GayQuy.cshtml`.
   - QR card width: `max-w-[360px]` with `p-7` (56px padding) on a 375px screen with parent `px-6` (48px padding) -> available width is 327px, causing collision. Needs `max-w-full sm:max-w-[360px]` and `p-4 sm:p-7`.
2. Investigate Touch Target sizing across Home views:
   - `BanToChuc.cshtml:414 & 527`: Facebook icon links `h-6 w-6` (24px) and `h-7 w-7` (28px) -> upgrade to minimum `min-w-[40px] min-h-[40px]` or `p-2`.
   - CTA buttons and card links across Home views to ensure >= 40px touch targets.
3. Verify that zero `/admin` files are involved.
4. Output detailed recommendations and code replacement specifications to `D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\handoff.md`.


## 2026-11-07T10:13:08Z
You are teamwork_explorer instance m2_explorer_3.
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Your role: Touch Targets & Bank Memo Line Break Explorer.
Investigate GayQuy.cshtml: transfer memo line breaking with break-all (resolves test M4_F06_T01), donation card max-width and paddings.
Investigate BanToChuc.cshtml: Facebook icon buttons (24px/28px -> >= 40px touch target).
Investigate touch targets across Home views.
Zero modifications to /admin.

Write your report to:
D:\web_tinh_nguyen\.agents\teamwork\m2_explorer_3\handoff.md
Update progress.md in your directory periodically.
When finished, send a message to caller with a brief summary referencing handoff.md.
