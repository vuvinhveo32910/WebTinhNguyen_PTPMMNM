# Milestone 1 - Explorer 2 Task Assignment

## Milestone: M1 - Public Layouts & Mobile Navigation Infrastructure
**Focus**: Mobile Liquid Bottom Navigation Optimization, Indicator Resizing, and Floating Action Button De-confliction.

## Inputs & Context
- Authoritative User Request: `D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md`
- Scope Document: `D:\web_tinh_nguyen\PROJECT.md`
- Survey 1 Report: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_1\handoff.md`
- Survey 2 Report: `D:\web_tinh_nguyen\.agents\teamwork\survey_explorer_2\handoff.md`

## Mission
1. Investigate the 7-item `.navigation.fixed.bottom-[15px]` on `_Layout.cshtml` and `_CuaHangLayout.cshtml`.
   - On 375px screens (360px total width), 7 items get ~51.4px width while the animated `.indicator` is 60px.
   - Design a solution that maintains full compatibility with Tier 1 tests (`fixed bottom`, `md:hidden`, `CuaHang`, icons) while ensuring items and indicators do not clip or cram on 375px screens.
2. Investigate floating buttons positioning:
   - `#floatingCartBtn` on `_Layout.cshtml` and `_CuaHangLayout.cshtml`.
   - Floating Wish button on `GiaoDien/Home/LoiChuc.cshtml` (currently `bottom-[max(1.25rem,...)]` overlapping bottom nav).
   - Propose clear z-index and bottom-offset coordinates so floating elements never collide or obstruct interaction.
3. Output detailed recommendations and code specifications to `D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\handoff.md`.


## 2026-11-07T09:32:20Z
[Message] timestamp=2026-11-07T09:32:20Z sender=55581585-523d-44d0-807c-aeb70f9a76e5 priority=MESSAGE_PRIORITY_HIGH
You are teamwork_explorer instance m1_explorer_2.
Your assigned working directory is: D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\
Read your task description in: D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\DISPATCH.md

MANDATORY: Subagents MUST read the authoritative user request before starting work:
Authoritative user request path: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Your role: Bottom Navigation & Floating Buttons Explorer.
Investigate the 7-item bottom navigation on _Layout.cshtml and _CuaHangLayout.cshtml, indicator sizing on 375px screens, and de-conflict floating buttons (#floatingCartBtn and wish button on LoiChuc.cshtml).
Ensure you preserve all test invariant strings ("fixed bottom", "md:hidden", "CuaHang", icons, #floatingCartBtn, #floatingCartCount).
Zero changes to /admin.

Write your report to:
D:\web_tinh_nguyen\.agents\teamwork\m1_explorer_2\handoff.md
Update progress.md in your directory periodically.
When finished, send a message to caller with a brief summary referencing handoff.md.
