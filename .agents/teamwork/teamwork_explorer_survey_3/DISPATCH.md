# Dispatch Task for Explorer 3 (Survey: Storefront, Cart, Seed Data & Build/Test)

## Objective
Investigate storefront capabilities, shopping cart implementation patterns (session/cookie), fundraising product domain entities & seed data mechanisms, and build/run verification.

## Scope Boundaries
Read-only analysis. Do NOT modify source code files.

## Input Information
- ORIGINAL_REQUEST.md path: `/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/ORIGINAL_REQUEST.md`
- Project root: `/home/hutech/Vinh_Offline/WEB_TinhNguyen`

## Output Requirements
Produce a comprehensive report in your working directory (`/home/hutech/Vinh_Offline/WEB_TinhNguyen/.agents/teamwork/teamwork_explorer_survey_3/handoff.md`).
Report should detail:
1. Is Session or Cookie-based state configured in `Program.cs` (`AddSession`, `UseSession`, `DistributedMemoryCache`)?
2. Entity modeling requirements for Fundraising Products (Id, Name, Price, Description, ImageUrl, Quantity, Campaign/Cause info, etc.) and Orders/Donations (Customer info, order items, total amount, status).
3. How seed data is currently handled in the project (e.g. `HasData` in EF Core model builder, or a `DbInitializer`/`SeedData` class called at startup).
4. Verify project compilation (`dotnet build`) status and document exact build commands and results.
5. Identify edge cases and UI/UX requirements for storefront, product detail, cart, and checkout.


## 2026-11-03T12:30:41Z
Received message from Orchestrator:
You are teamwork_explorer_survey_3.
Investigate storefront capabilities, shopping cart implementation patterns (session/cookie), fundraising product domain entities & seed data mechanisms, and build/run verification (`dotnet build`).


## 2026-11-03T12:38:19Z
**Context**: Survey execution for Storefront & Build
**Content**: Note that in this Linux sandbox environment, running `dotnet build` requires `XDG_RUNTIME_DIR=/tmp` (e.g. `XDG_RUNTIME_DIR=/tmp dotnet build WEB_TinhNguyen.slnx`) to avoid the named mutex lock error on `/run/user/1000`. Please update your progress.md and complete your handoff report when ready.
**Action**: Continue survey and submit handoff.md.


## 2026-11-06T07:37:40Z
You are teamwork_explorer_survey_3.
Your working directory: D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_3\
Authoritative user request: D:\web_tinh_nguyen\.agents\teamwork\ORIGINAL_REQUEST.md

Read ORIGINAL_REQUEST.md before starting work.
You are a read-only exploration agent. You MUST NOT modify any source code or execute disruptive operations.

YOUR MISSION:
Perform a comprehensive survey of the Donation / Fundraising / Campaign (Ủng hộ / Gây quỹ / Quyên góp) subsystem at D:\web_tinh_nguyen.
Specifically investigate:
1. All controllers, views, viewmodels, Razor pages, or client components related to Donation / Fundraising / Campaigns.
2. Where and how QR codes, bank accounts, transfer syntax (cú pháp đóng góp), and payment instructions are currently displayed in the Donation subsystem.
3. Identify every single hardcoded bank account, QR image path, bank name, account holder, and transfer message in the Donation subsystem.
4. How campaign details or general donation options are passed to the donation/payment view.
5. How VietQR is generated or if static images are used. Can dynamic parameters (campaign code, donor note, bank details) be integrated?
6. What changes are needed to switch from hardcoded values to dynamic configuration from the database/service.

OUTPUT:
Write your comprehensive findings and evidence report to:
D:\web_tinh_nguyen\.agents\teamwork\teamwork_explorer_survey_3\handoff.md
Update your progress.md periodically.
When finished, send a brief completion message to your parent.
