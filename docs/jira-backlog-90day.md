# InnoPV Jira-Ready Backlog (90-Day Plan)

## Scope and Assumptions
- Team size: 5-7 members (BE, FE, QA, BA/PV SME)
- Sprint length: 2 weeks
- Planning horizon: 6 sprints (~90 days)
- Story points: Fibonacci (1, 2, 3, 5, 8, 13)
- Compliance objective: inspection-ready traceability and submission readiness

## Epic List
1. EPIC-1: Case Intake and Data Quality Foundation
2. EPIC-2: Workflow, SLA, and Audit Compliance
3. EPIC-3: Coding and Clinical Assessment (MedDRA/WHO-DD/Causality)
4. EPIC-4: Submission Readiness (E2B R3 data model + validation gate)
5. EPIC-5: Security and Privacy Hardening (RBAC + PII masking)
6. EPIC-6: Signal Detection, Reconciliation, and Inspection Readiness

---

## EPIC-1: Case Intake and Data Quality Foundation

### US-101: Create case intake API with mandatory field checks
- Story points: 8
- Description: As a safety processor, I want intake validation so incomplete cases are flagged before triage.
- Acceptance criteria:
  - Given a new case payload, required fields are validated by case type.
  - Validation response returns field-level errors.
  - Case cannot move to Triage when critical fields are missing.
  - Validation events are logged for audit.

### US-102: Implement data completeness scoring
- Story points: 5
- Description: As a team lead, I want a completeness score to prioritize follow-up.
- Acceptance criteria:
  - Each case gets a 0-100 completeness score.
  - Score recalculates on each update.
  - Score breakdown shows missing critical and optional data.

### US-103: Build intake UI validations and error prompts
- Story points: 5
- Description: As a case entry user, I want clear form errors to fix data quickly.
- Acceptance criteria:
  - Field-level and summary validation messages are shown.
  - Submit is blocked for critical missing data.
  - Save Draft works even with partial data.

### US-104: Add duplicate detection candidate list at intake
- Story points: 8
- Description: As an intake user, I want duplicate suggestions to avoid duplicate case creation.
- Acceptance criteria:
  - On create, system computes duplicate confidence score.
  - Top candidate cases are shown with similarity indicators.
  - User can mark as duplicate or continue as new with reason.

---

## EPIC-2: Workflow, SLA, and Audit Compliance

### US-201: Implement case lifecycle state machine
- Story points: 8
- Description: As operations, I want controlled state transitions for process consistency.
- Acceptance criteria:
  - States: New, Triage, Medical Review, QA, Submission Ready, Submitted.
  - Invalid transitions are blocked.
  - Transition history includes user, timestamp, from-state, to-state.

### US-202: Role-based transition permissions
- Story points: 5
- Description: As admin, I want only authorized roles to perform specific transitions.
- Acceptance criteria:
  - Role matrix enforced server-side.
  - Unauthorized actions return 403 with reason.
  - Permission changes are configurable.

### US-203: SLA due-date engine and reminders
- Story points: 8
- Description: As case manager, I want deadline reminders so reports are on time.
- Acceptance criteria:
  - Due dates auto-calculate by case category.
  - Reminder cadence supports T-7, T-3, T-1 and overdue.
  - Alert channel supports in-app and email.

### US-204: Field-level audit trail
- Story points: 8
- Description: As QA/compliance, I need full traceability of data changes.
- Acceptance criteria:
  - Stores entity, field, old value, new value, user, timestamp.
  - Audit logs are immutable and searchable.
  - Export supported by date range and case ID.

---

## EPIC-3: Coding and Clinical Assessment

### US-301: MedDRA coding service integration (v1)
- Story points: 13
- Description: As medical reviewer, I want event terms mapped to MedDRA for standard reporting.
- Acceptance criteria:
  - Search supports LLT/PT lookup.
  - Selected code and version are stored with case event.
  - Manual override requires reason capture.

### US-302: WHO-DD compatible product coding structure
- Story points: 8
- Description: As reviewer, I want suspect/concomitant product coding for consistent analysis.
- Acceptance criteria:
  - Product records support coded drug dictionary references.
  - Suspect vs concomitant classification is mandatory.
  - Dictionary version used is persisted.

### US-303: Seriousness auto-suggestion rules
- Story points: 5
- Description: As triage reviewer, I want seriousness suggestions to speed first assessment.
- Acceptance criteria:
  - Rules suggest seriousness criteria based on event details.
  - User can accept/edit suggestions.
  - Final reviewer decision is always captured.

### US-304: Expectedness check against product labeling
- Story points: 8
- Description: As medical reviewer, I want expectedness check support for reporting classification.
- Acceptance criteria:
  - Label reference and version are linked.
  - Event expected/unexpected suggested by rules.
  - Manual override requires justification.

### US-305: Causality assessment module (WHO-UMC/Naranjo)
- Story points: 13
- Description: As physician reviewer, I want structured causality scoring and rationale.
- Acceptance criteria:
  - Supports WHO-UMC and Naranjo methods.
  - Stores reviewer, score/category, rationale text.
  - Supports medical reviewer second sign-off.

---

## EPIC-4: Submission Readiness

### US-401: Build E2B R3 internal DTO mapping
- Story points: 13
- Description: As submission specialist, I want complete E2B-ready data mapping.
- Acceptance criteria:
  - Internal DTO covers mandatory E2B elements for target markets.
  - Mapping supports null handling and code sets.
  - Versioned mapping configuration available.

### US-402: Pre-submission validation gate
- Story points: 8
- Description: As QA, I want blocking validation before submission packaging.
- Acceptance criteria:
  - Validation report lists errors and warnings.
  - Submission Ready state requires zero blocking errors.
  - Validation report is auditable and exportable.

### US-403: Submission package tracker
- Story points: 5
- Description: As operations, I want visibility into package status lifecycle.
- Acceptance criteria:
  - Statuses: Draft, Validated, Approved, Submitted, Acknowledged.
  - Timestamps captured for each status.
  - Dashboard widget shows pending and failed packages.

---

## EPIC-5: Security and Privacy Hardening

### US-501: Granular RBAC for PV roles
- Story points: 8
- Description: As security admin, I want role-based access by module and action.
- Acceptance criteria:
  - Roles include Intake, Triage, Medical Reviewer, QA, Submitter, Admin.
  - Permissions enforce API and UI-level controls.
  - Role assignment changes are audited.

### US-502: PII masking and secure views
- Story points: 5
- Description: As compliance officer, I want patient identifiers masked by role.
- Acceptance criteria:
  - PII fields masked by default for non-privileged roles.
  - Unmask action is permission-gated and logged.
  - Export files respect masking policy.

### US-503: Access logging and anomaly report
- Story points: 5
- Description: As compliance officer, I want access logs for investigations.
- Acceptance criteria:
  - Logs include user, endpoint, case scope, timestamp, outcome.
  - Daily anomaly summary report available.
  - Retention policy configurable.

---

## EPIC-6: Signal Detection, Reconciliation, Inspection Readiness

### US-601: Implement PRR/ROR metrics pipeline
- Story points: 13
- Description: As signal management team, I want disproportionality metrics for signal detection.
- Acceptance criteria:
  - PRR and ROR computed for product-event combinations.
  - Configurable thresholds for alert generation.
  - Calculation metadata and version retained.

### US-602: Signal dashboard and trend visualization
- Story points: 8
- Description: As safety lead, I want trends by product/event/country for risk review.
- Acceptance criteria:
  - Time-series trends by product-event.
  - Filter by seriousness, geography, date range.
  - Threshold breach is visually highlighted.

### US-603: Source reconciliation report
- Story points: 8
- Description: As operations, I want mismatch detection between intake sources and safety DB.
- Acceptance criteria:
  - Reconciliation report shows missing, mismatched, duplicate records.
  - Report supports scheduled generation.
  - Exceptions can be assigned and tracked.

### US-604: Inspection readiness export pack
- Story points: 5
- Description: As QA, I want one-click export pack for audits/inspections.
- Acceptance criteria:
  - Includes audit trail, validation reports, submission history, CAPA refs.
  - Date-range and case-set filters available.
  - Export action is logged.

---

## Sprint Plan (6 Sprints)

### Sprint 1 (Weeks 1-2) - Foundation Start
- US-101 (8)
- US-102 (5)
- US-201 (8)
- US-501 (8)
- Total: 29 SP

### Sprint 2 (Weeks 3-4) - Workflow and Compliance Base
- US-103 (5)
- US-202 (5)
- US-203 (8)
- US-204 (8)
- Total: 26 SP

### Sprint 3 (Weeks 5-6) - Coding and Duplicates
- US-104 (8)
- US-301 (13)
- US-302 (8)
- Total: 29 SP

### Sprint 4 (Weeks 7-8) - Medical Assessment and Submission Core
- US-303 (5)
- US-304 (8)
- US-305 (13)
- US-401 (13)
- Total: 39 SP
- Note: May split US-401 into two tickets if velocity is below 35.

### Sprint 5 (Weeks 9-10) - Submission and Security Hardening
- US-402 (8)
- US-403 (5)
- US-502 (5)
- US-503 (5)
- US-603 (8)
- Total: 31 SP

### Sprint 6 (Weeks 11-12) - Analytics and Inspection Readiness
- US-601 (13)
- US-602 (8)
- US-604 (5)
- Total: 26 SP

---

## Definition of Done (All Stories)
- Unit tests for business rules and validators
- Integration/API tests for workflow-critical paths
- Audit trail coverage for data changes and approvals
- Security review for role and PII impact
- Demo walkthrough with PV SME and QA sign-off
- Release notes updated

## Non-Functional Backlog (Cross-Cutting)
1. NFR-01: Performance baseline for case list and dashboard APIs (P95 < 2s)
2. NFR-02: Background jobs for reminders, reconciliation, and signal calculations
3. NFR-03: Observability (structured logging, error tracing, health checks)
4. NFR-04: Backup and disaster recovery verification for critical data stores

## Jira Import Tip
- Create issue type mapping:
  - Epic: EPIC-x
  - Story: US-xxx
  - Task/Sub-task: implementation/testing/docs
- Labels suggested:
  - module:intake, module:workflow, module:coding, module:submission, module:security, module:signals
  - compliance:audit, compliance:reporting
  - priority:P1/P2/P3
