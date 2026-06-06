# InnoPV Step-by-Step Implementation Progress

## Completed (Current Iteration)
1. Step 1: Intake mandatory validations + data completeness score
- Added reusable service: `ICaseIntakeValidationService`
- Enforced blocking checks for future receipt date and missing linked data for checked validity criteria
- Added completeness score display on Create, Edit, Index, and Details screens

2. Step 2: Workflow transition guard service
- Added reusable service: `ICaseWorkflowTransitionService`
- Enforced allowed submit/return transitions based on role + current status
- Added defensive checks in workflow submit and return endpoints

3. Step 3: Duplicate check confidence hardening + mandatory decision reason
- Enhanced scoring with exact and partial text matches (patient/product/event/reporter), date proximity, and seriousness match
- Added confidence threshold and confidence banding (High/Medium/Low)
- Enforced mandatory remarks for "No Duplicate" decision via strongly typed model validation
- Updated duplicate check UI to show threshold and confidence band per candidate

4. Step 4: SLA escalation matrix (T-7/T-3/T-1/Overdue) with configurable policy
- Extended SLA settings for configurable escalation days and toggles: T7Days, T3Days, T1Days, SendT7Alerts, SendT3Alerts, SendT1Alerts
- Refactored SLA alert service to build exact escalation buckets and combined due-soon queue
- Added escalation labels to case items and email summary template output
- Updated SLA dashboard UI to show T-7/T-3/T-1/Overdue counts and escalation column in case tables
- Updated alert action success summaries with escalation-wise counts

5. Step 5: Structured submission readiness validator report + blocking gate
- Added submission readiness validation service with required checks and warnings
- Added dedicated validation screen under regulatory submission module
- Added CSV export for validation report (required + warning checks)
- Added blocking gate before create submission and before submit action when required checks fail
- Added quick access button for submission validation from submission index page

6. Step 6: Role-permission matrix centralization for module-level action control
- Added centralized permission action catalog (`PermissionActions`) and role-permission matrix service
- Integrated matrix checks into `CaseSecurityService` for view/edit/workflow/attachment actions
- Added module-level permission guards in Duplicate Check and Regulatory Submission controllers
- Registered matrix service in DI for consistent runtime authorization checks

## Build Verification
- `dotnet build .\InnoPV.Web\InnoPV.Web.csproj` passed after each step

## Next Steps (Planned)
1. Optional hardening: convert `[Authorize(Roles=...)]` strings to policy-based attributes mapped from centralized permission matrix
