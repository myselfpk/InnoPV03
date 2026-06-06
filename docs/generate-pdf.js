'use strict';
const PDFDocument = require('pdfkit');
const fs = require('fs');
const path = require('path');

const OUTPUT_DIR = path.join(__dirname);
const OUT_FILE = path.join(OUTPUT_DIR, 'InnoPV_User_Manual.pdf');

// ─── Color palette ──────────────────────────────────────────────────────────
const C = {
  primary:   '#1B3A6B',   // dark navy
  secondary: '#2E86C1',   // medium blue
  accent:    '#E74C3C',   // red for warnings
  success:   '#1E8449',   // green
  light:     '#EBF5FB',   // light blue bg
  lightGrey: '#F2F3F4',
  midGrey:   '#99A3A4',
  darkGrey:  '#2C3E50',
  white:     '#FFFFFF',
  black:     '#000000',
  amber:     '#D4AC0D',
};

const doc = new PDFDocument({
  size: 'A4',
  margins: { top: 60, bottom: 60, left: 55, right: 55 },
  info: {
    Title: 'InnoPV – Pharmacovigilance Software – User Manual',
    Author: 'InnoPV System',
    Subject: 'Complete User Manual for InnoPV PV Case Management System',
    Keywords: 'InnoPV, pharmacovigilance, user manual, PV case management',
  },
  bufferPages: true,
  autoFirstPage: false,
});

const stream = fs.createWriteStream(OUT_FILE);
doc.pipe(stream);

let pageCount = 0;

// ─── Helpers ─────────────────────────────────────────────────────────────────
function newPage(bg) {
  doc.addPage();
  pageCount++;
  if (bg) {
    doc.rect(0, 0, doc.page.width, doc.page.height).fill(bg);
  }
}

function headerBand(title, subtitle) {
  doc.rect(0, 0, doc.page.width, 80).fill(C.primary);
  doc.fillColor(C.white).fontSize(22).font('Helvetica-Bold')
     .text(title, 55, 22, { width: doc.page.width - 110 });
  if (subtitle) {
    doc.fontSize(10).font('Helvetica').fillColor('#BDC3C7')
       .text(subtitle, 55, 50, { width: doc.page.width - 110 });
  }
  doc.fillColor(C.darkGrey);
  return 100;
}

function sectionTitle(text, y) {
  doc.rect(55, y, doc.page.width - 110, 26).fill(C.primary);
  doc.fillColor(C.white).fontSize(12).font('Helvetica-Bold')
     .text(text, 63, y + 7, { width: doc.page.width - 126 });
  doc.fillColor(C.darkGrey);
  return y + 34;
}

function subTitle(text, y) {
  doc.fontSize(11).font('Helvetica-Bold').fillColor(C.secondary)
     .text(text, 55, y);
  doc.moveDown(0.2);
  return doc.y;
}

function bodyText(text, x, y, opts) {
  doc.fontSize(10).font('Helvetica').fillColor(C.darkGrey)
     .text(text, x || 55, y || doc.y, opts || { width: doc.page.width - 110 });
  return doc.y;
}

function bullet(text, y, indent) {
  const ix = indent || 65;
  doc.fontSize(10).font('Helvetica').fillColor(C.darkGrey)
     .text('•  ' + text, ix, y || doc.y, { width: doc.page.width - ix - 55 });
  return doc.y;
}

function hLine(y, color) {
  doc.moveTo(55, y || doc.y).lineTo(doc.page.width - 55, y || doc.y)
     .strokeColor(color || C.midGrey).lineWidth(0.5).stroke();
  return (y || doc.y) + 6;
}

function tableLike(rows, startY, col1W, col2W) {
  let y = startY || doc.y;
  const x1 = 55, x2 = x1 + col1W, lineH = 18;
  rows.forEach((row, i) => {
    const bg = i % 2 === 0 ? C.lightGrey : C.white;
    const rowH = 22;
    doc.rect(x1, y, col1W + col2W, rowH).fill(bg);
    doc.fillColor(C.primary).fontSize(9.5).font('Helvetica-Bold')
       .text(row[0], x1 + 4, y + 6, { width: col1W - 8 });
    doc.fillColor(C.darkGrey).fontSize(9.5).font('Helvetica')
       .text(row[1], x2 + 4, y + 6, { width: col2W - 8 });
    y += rowH;
  });
  return y + 4;
}

function roleTag(role, x, y) {
  const colors = {
    'Admin': C.accent,
    'PV Associate': C.secondary,
    'PV Manager': C.success,
    'Medical Reviewer': C.amber,
  };
  const bg = colors[role] || C.midGrey;
  const w = role.length * 6.8 + 14;
  doc.roundedRect(x, y - 1, w, 16, 4).fill(bg);
  doc.fillColor(C.white).fontSize(8.5).font('Helvetica-Bold')
     .text(role, x + 7, y + 2, { width: w - 14 });
  doc.fillColor(C.darkGrey);
  return x + w + 6;
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  COVER PAGE                                                              ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage();

// Deep navy gradient simulation via two rectangles
doc.rect(0, 0, doc.page.width, doc.page.height).fill(C.primary);
doc.rect(0, doc.page.height - 120, doc.page.width, 120).fill('#162E55');

// Decorative accent bar
doc.rect(0, 220, 8, 300).fill(C.secondary);

// Software logo text
doc.fillColor('#2E86C1').fontSize(60).font('Helvetica-Bold').text('Inno', 55, 200);
doc.fillColor(C.white).fontSize(60).font('Helvetica-Bold').text('PV', 180, 200);

// Subtitle line
doc.fillColor('#BDC3C7').fontSize(14).font('Helvetica')
   .text('Pharmacovigilance Case Management System', 55, 270, { width: 480 });

// Divider
doc.moveTo(55, 300).lineTo(420, 300).strokeColor('#2E86C1').lineWidth(2).stroke();

// Main title
doc.fillColor(C.white).fontSize(28).font('Helvetica-Bold')
   .text('User Manual', 55, 320);

doc.fillColor('#BDC3C7').fontSize(12).font('Helvetica')
   .text('Complete guide for all user roles and system features', 55, 360, { width: 460 });

// Info box bottom
doc.rect(55, 430, 480, 110).fill('#162E55');
doc.rect(55, 430, 4, 110).fill(C.secondary);

const infoItems = [
  ['Document Version', 'v1.0'],
  ['Date', 'June 2026'],
  ['System', 'InnoPV – Pharmacovigilance Software'],
  ['Audience', 'Admin / PV Associate / PV Manager / Medical Reviewer'],
];
infoItems.forEach((item, i) => {
  doc.fillColor('#BDC3C7').fontSize(9).font('Helvetica-Bold')
     .text(item[0], 70, 441 + i * 22);
  doc.fillColor(C.white).fontSize(9).font('Helvetica')
     .text(': ' + item[1], 195, 441 + i * 22);
});

// Footer
doc.fillColor('#7F8C8D').fontSize(8).font('Helvetica')
   .text('CONFIDENTIAL – FOR INTERNAL USE ONLY', 55, doc.page.height - 50,
     { width: doc.page.width - 110, align: 'center' });

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  TABLE OF CONTENTS                                                       ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
let y = headerBand('Table of Contents', 'InnoPV User Manual');

const toc = [
  ['1.', 'Introduction & System Overview', '3'],
  ['2.', 'User Roles & Access Rights', '4'],
  ['3.', 'Login & Authentication', '5'],
  ['4.', 'Dashboard', '6'],
  ['5.', 'Case Inbox', '7'],
  ['6.', 'PV Case Management', '8'],
  ['7.', 'Case Data Entry (Detailed)', '9'],
  ['8.', 'Case Workflow & Actions', '11'],
  ['9.', 'Checklist Capture', '12'],
  ['10.', 'Case Follow-Up', '13'],
  ['11.', 'Duplicate Check', '14'],
  ['12.', 'Case History & Audit Trail', '15'],
  ['13.', 'Case Complete Report (PDF/Word/Excel)', '15'],
  ['14.', 'Regulatory Submission Management', '16'],
  ['15.', 'Reports & Analytics', '17'],
  ['16.', 'SLA Alerts', '18'],
  ['17.', 'Admin – User Management', '19'],
  ['18.', 'Admin – Master Data Management', '20'],
  ['19.', 'Admin – Checklist Management', '21'],
  ['20.', 'Profile & Password Management', '22'],
  ['21.', 'Case Status Flow Diagram', '23'],
  ['22.', 'Role-wise Feature Matrix', '24'],
];

toc.forEach((item, i) => {
  const bg = i % 2 === 0 ? C.lightGrey : C.white;
  doc.rect(55, y, doc.page.width - 110, 20).fill(bg);
  doc.fillColor(C.primary).fontSize(10).font('Helvetica-Bold')
     .text(item[0], 62, y + 5, { width: 30 });
  doc.fillColor(C.darkGrey).fontSize(10).font('Helvetica')
     .text(item[1], 94, y + 5, { width: 380 });
  doc.fillColor(C.secondary).fontSize(10).font('Helvetica-Bold')
     .text(item[2], doc.page.width - 80, y + 5, { width: 30, align: 'right' });
  y += 20;
});

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE 3 – INTRODUCTION                                                   ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('1. Introduction & System Overview', 'InnoPV User Manual');

y = sectionTitle('What is InnoPV?', y);
y = bodyText(
  'InnoPV is a web-based Pharmacovigilance (PV) Case Management System designed to manage the complete lifecycle of adverse event cases from initial intake to regulatory submission and closure. It supports ICH E2B reporting standards and provides structured workflows for multi-user, multi-role environments.',
  55, y + 4
);
y = doc.y + 10;

y = sectionTitle('Key Capabilities', y);
const keyFeatures = [
  'Centralized case intake and data entry for adverse events',
  'Structured multi-level workflow: PV Associate → PV Manager → Medical Reviewer',
  'Automated validity & seriousness assessment with due-date calculation',
  'Duplicate case detection and resolution',
  'Role-based checklists for quality assurance at every stage',
  'Follow-up management for additional case information',
  'Regulatory submission tracking (E2B, MedWatch, CIOMS, etc.)',
  'Complete case reports in PDF, Word, and Excel formats',
  'SLA (Service Level Agreement) monitoring and email alerts',
  'Full audit trail and case history for compliance',
  'Admin panel for user, master data, and checklist management',
];
y += 4;
keyFeatures.forEach(f => { y = bullet(f, y) + 2; });
y += 6;

y = sectionTitle('Technology & Access', y);
const techRows = [
  ['Platform', 'Web-based (browser accessible) – no installation required'],
  ['Supported Browsers', 'Chrome, Edge, Firefox (latest versions recommended)'],
  ['Authentication', 'Email + Password with account lockout protection'],
  ['Session', 'Cookie-based session with "Remember Me" option'],
  ['Data Security', 'Role-based access control (RBAC), CSRF protection, audit trails'],
];
y = tableLike(techRows, y + 4, 160, 320);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE 4 – USER ROLES                                                     ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('2. User Roles & Access Rights', 'InnoPV User Manual');

y = sectionTitle('Overview of Roles', y);
y = bodyText('InnoPV has four predefined roles. Each user is assigned exactly one role.', 55, y + 4);
y = doc.y + 8;

const roles = [
  {
    role: 'Admin',
    color: C.accent,
    desc: 'System administrator with full access. Manages users, master data, checklists, and has visibility over all cases.',
    powers: [
      'Full access to all modules and all cases',
      'Create, edit, activate/deactivate users',
      'Assign any role to users',
      'Manage master data (products, studies, sponsors, common options)',
      'Manage checklists (create/edit/version)',
      'View admin dashboard with system-wide statistics',
      'Assign cases to any role or user',
      'Access all reports and export data',
      'Trigger SLA alerts',
    ],
    cannot: ['N/A – Admin has full system access'],
  },
  {
    role: 'PV Associate',
    color: C.secondary,
    desc: 'Primary data entry user. Creates new cases, enters all case data sections, and progresses cases through initial workflow stages.',
    powers: [
      'Create new PV cases (intake form)',
      'View and manage own created cases + cases assigned to them',
      'Enter all case data: patient, reporter, product, adverse event, lab details, attachments, concomitant medications',
      'Perform duplicate check',
      'Complete PV Associate checklist',
      'Submit case to PV Manager',
      'Add and process follow-ups',
      'View case history and audit trail',
      'Download complete case report (PDF/Word/Excel)',
    ],
    cannot: [
      'Cannot assign cases to other users',
      'Cannot access Admin panel, master data, or user management',
      'Cannot access regulatory submission module',
      'Cannot trigger SLA alerts',
      'Cannot view Admin or PV Manager dashboard',
    ],
  },
  {
    role: 'PV Manager',
    color: C.success,
    desc: 'Supervises case quality. Reviews submitted cases, assigns to Medical Reviewer or returns to PV Associate, manages regulatory submissions.',
    powers: [
      'View all cases in PV Manager inbox',
      'Review case data and assign checklist',
      'Forward case to Medical Reviewer',
      'Return case to PV Associate with comments',
      'Manage case assignment (assign cases to users)',
      'Manage regulatory submissions',
      'View SLA alerts and send email notifications',
      'Access case reports and CSV export',
      'Access regulatory submission report',
      'View PV Manager dashboard',
      'Initiate case closure validation',
    ],
    cannot: [
      'Cannot create new cases (intake)',
      'Cannot access Admin panel, master data, or user management',
      'Cannot manage checklists',
    ],
  },
  {
    role: 'Medical Reviewer',
    color: C.amber,
    desc: 'Performs medical review of cases forwarded by PV Manager. Approves or returns cases.',
    powers: [
      'View cases in Medical Reviewer inbox',
      'Review full case data (read access to all sections)',
      'Complete Medical Reviewer checklist',
      'Medically approve a case',
      'Return case to PV Manager with comments',
      'Add follow-up entries',
      'Manage regulatory submissions',
      'View case history and audit trail',
      'Download complete case report',
      'Initiate case closure validation',
      'View Medical Reviewer dashboard',
    ],
    cannot: [
      'Cannot create new cases',
      'Cannot assign cases',
      'Cannot access Admin panel or master data',
      'Cannot trigger SLA alerts',
      'Cannot access case reports / CSV export',
    ],
  },
];

roles.forEach(r => {
  if (doc.y > doc.page.height - 180) { newPage(C.white); y = 60; }
  const boxY = doc.y + 6;
  doc.rect(55, boxY, doc.page.width - 110, 18).fill(r.color);
  doc.fillColor(C.white).fontSize(12).font('Helvetica-Bold')
     .text(r.role + ' Role', 63, boxY + 3, { width: 400 });
  doc.fillColor(C.darkGrey).fontSize(9.5).font('Helvetica')
     .text(r.desc, 55, doc.y + 6, { width: doc.page.width - 110 });
  doc.moveDown(0.3);
  doc.fillColor(r.color).fontSize(9.5).font('Helvetica-Bold').text('  Can Do:', 55, doc.y);
  r.powers.forEach(p => { bullet(p, doc.y + 2, 68); });
  if (r.cannot && r.cannot[0] !== 'N/A – Admin has full system access') {
    doc.fillColor(C.accent).fontSize(9.5).font('Helvetica-Bold').text('  Restrictions:', 55, doc.y + 4);
    r.cannot.forEach(c => { bullet(c, doc.y + 2, 68); });
  }
  doc.moveDown(0.6);
});

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – LOGIN & AUTHENTICATION                                           ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('3. Login & Authentication', 'InnoPV User Manual');

y = sectionTitle('How to Login', y);
const loginSteps = [
  'Open InnoPV in your web browser.',
  'Enter your registered Email address in the Email field.',
  'Enter your Password in the Password field.',
  'Optionally check "Remember Me" to stay logged in on the device.',
  'Click the Login button.',
  'On successful login, you will be redirected to your role-specific Dashboard.',
];
loginSteps.forEach((s, i) => {
  if (i === 0) y += 4;
  y = bullet(`Step ${i+1}: ${s}`, y) + 2;
});
y = doc.y + 10;

y = sectionTitle('First Login – Password Change', y);
y = bodyText(
  'If you are logging in for the first time (or if Admin has reset your password), the system will prompt you to change your password immediately before accessing any other feature. You must set a new password that is different from the temporary/current password.',
  55, y + 4
);
y = doc.y + 10;

y = sectionTitle('Security Features', y);
const secRows = [
  ['Account Lockout', 'Account is locked after multiple consecutive failed login attempts. Contact Admin to unlock.'],
  ['Inactive Account', 'If your account is deactivated by Admin, login is blocked with a message.'],
  ['Password Policy', 'New password must differ from current password. Complexity enforced by system policy.'],
  ['CSRF Protection', 'All forms are protected against Cross-Site Request Forgery attacks.'],
  ['Session Security', 'Sessions are cookie-based and expire on logout or browser close (unless "Remember Me" is used).'],
  ['Forgot Password', 'Use the "Forgot Password" link on the login page – a reset link is sent to your registered email.'],
];
y = tableLike(secRows, y + 4, 160, 320);
y += 10;

y = sectionTitle('Logout', y);
y = bodyText('Click your profile/name in the top navigation bar and select "Logout". Always log out when leaving your workstation unattended.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – DASHBOARD                                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('4. Dashboard', 'InnoPV User Manual');

y = sectionTitle('Role-Specific Dashboards', y);
y = bodyText('After login, each user is redirected to their role-specific dashboard. The dashboard provides a real-time snapshot of the system.', 55, y + 4);
y = doc.y + 8;

const dashboards = [
  {
    role: 'Admin Dashboard',
    color: C.accent,
    items: [
      'Total cases count, open vs closed cases',
      'Cases by status (bar/pie chart)',
      'Total users count, active users',
      'Users by role (chart)',
      'Recent 8 cases list with status and due dates',
      'Recent 8 users registered',
      'Overdue cases count with due date alerts',
    ],
  },
  {
    role: 'PV Manager Dashboard',
    color: C.success,
    items: [
      'Cases pending PV Manager review',
      'Cases forwarded to Medical Reviewer',
      'Overdue cases and SLA status',
      'Monthly case trend charts',
      'Cases by seriousness (Serious / Non-Serious)',
      'Cases by validity (Valid / Invalid)',
    ],
  },
  {
    role: 'PV Associate Dashboard',
    color: C.secondary,
    items: [
      'Cases assigned to me',
      'My cases by status',
      'Draft cases pending completion',
      'Cases due in next 7 days',
      'Returned cases requiring action',
    ],
  },
  {
    role: 'Medical Reviewer Dashboard',
    color: C.amber,
    items: [
      'Cases pending medical review',
      'Medically approved cases',
      'Cases returned to PV Manager',
      'Review pending SLA status',
    ],
  },
];

dashboards.forEach(d => {
  if (doc.y > doc.page.height - 120) { newPage(C.white); y = 60; }
  const bY = doc.y + 6;
  doc.rect(55, bY, 6, d.items.length * 14 + 26).fill(d.color);
  doc.rect(61, bY, doc.page.width - 116, 18).fill(d.color);
  doc.fillColor(C.white).fontSize(10.5).font('Helvetica-Bold')
     .text(d.role, 70, bY + 4, { width: 400 });
  d.items.forEach((item, ii) => {
    bullet(item, bY + 22 + ii * 14, 75);
  });
  doc.y = bY + d.items.length * 14 + 30;
});

y = doc.y + 6;
y = sectionTitle('PV Analytics Dashboard', y);
y = bodyText('All roles can access the PV Analytics Dashboard (/PvDashboard/Index) which shows system-wide analytics including: total/open/closed cases, serious vs non-serious breakdown, valid vs invalid counts, overdue cases, due-soon cases (next 7 days), monthly case receipt trend (last 12 months), and cases by assigned role.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – CASE INBOX                                                       ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('5. Case Inbox', 'InnoPV User Manual');

y = sectionTitle('What is the Case Inbox?', y);
y = bodyText('The Case Inbox is your personalized list of cases that require your attention. It shows only the cases relevant to your role and current assignment. Navigate to Case Inbox from the top navigation.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Inbox Filters by Role', y);
const inboxRows = [
  ['Admin', 'Sees ALL cases in the system (no filter applied)'],
  ['PV Associate', 'Draft, DataEntryInProgress, ValidityPending, InvalidFollowUpRequired, DuplicateCheckPending, PvAssociateChecklistPending, ReturnedByPvManager, AdditionalInformationRequired, Reopened'],
  ['PV Manager', 'SubmittedToPvManager, PvManagerReviewPending, PvManagerChecklistPending, ResubmittedToPvManager, ReturnedByMedicalReviewer'],
  ['Medical Reviewer', 'ForwardedToMedicalReviewer, MedicalReviewPending, MedicalReviewerChecklistPending, MedicallyApproved'],
];
y = tableLike(inboxRows, y + 4, 130, 350);
y += 8;

y = sectionTitle('Inbox Columns', y);
const inboxCols = [
  ['Case No', 'Unique auto-generated case identifier'],
  ['Source', 'Source of the adverse event report'],
  ['Receipt Date', 'Date the case was received'],
  ['Reporter', 'Initial reporter name'],
  ['Patient ID', 'Patient identifier'],
  ['Product', 'Suspect product name'],
  ['Event', 'Adverse event term'],
  ['Valid', 'Whether the case meets validity criteria (Yes/No)'],
  ['Serious', 'Whether the event is serious (Yes/No)'],
  ['Due Date', 'Case processing deadline (highlighted if overdue)'],
  ['Status', 'Current workflow status'],
  ['Actions', 'Links to open case, enter data, workflow, history'],
];
y = tableLike(inboxCols, y + 4, 140, 340);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – PV CASE MANAGEMENT                                               ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('6. PV Case Management', 'InnoPV User Manual');

y = sectionTitle('Creating a New Case (Intake)', y);
y = bodyText('Only Admin and PV Associate can create new cases. Navigate to PV Cases → New Case.', 55, y + 4);
y = doc.y + 6;

const intakeFields = [
  ['Case Source', 'Source of the report (e.g., Spontaneous, Clinical Trial, Literature)'],
  ['Receipt Date', 'Date case was received. Cannot be a future date.'],
  ['Initial Reporter Name', 'Name of the person/entity reporting the event'],
  ['Patient Identifier', 'Anonymized patient identifier'],
  ['Suspect Product Name', 'Name of the product suspected to cause the event'],
  ['Adverse Event Term', 'Medical term describing the adverse event'],
  ['Is Patient Identifiable?', 'Yes/No – one of 4 validity criteria'],
  ['Is Reporter Identifiable?', 'Yes/No – one of 4 validity criteria'],
  ['Suspect Product Available?', 'Yes/No – one of 4 validity criteria'],
  ['Adverse Event Available?', 'Yes/No – one of 4 validity criteria'],
  ['Is Serious?', 'Yes/No – determines due date (7 days if Serious, 15 days if Non-Serious)'],
  ['Narrative', 'Free-text description of the case'],
];
y = tableLike(intakeFields, y + 4, 170, 310);
y += 8;

y = sectionTitle('Case Validity & Due Date Calculation', y);
y = bodyText('A case is automatically determined as Valid if ALL four criteria are met: Patient Identifiable AND Reporter Identifiable AND Suspect Product Available AND Adverse Event Available. If any criterion is missing, the case is set to "Invalid – Follow-Up Required" status.', 55, y + 4);
y = doc.y + 4;
y = bodyText('Due Date: Serious cases = Receipt Date + 7 calendar days. Non-Serious cases = Receipt Date + 15 calendar days.', 55, y);
y = doc.y + 8;

y = sectionTitle('Case List (PV Cases)', y);
y = bodyText('Admins see all cases. PV Associates see only cases they created or cases assigned to them. Cases are sorted by most recent first. Click on a Case No to open the case detail.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – CASE DATA ENTRY                                                  ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('7. Case Data Entry (Detailed)', 'InnoPV User Manual');

y = sectionTitle('Overview', y);
y = bodyText('After creating a case, detailed data is entered across multiple sections. All roles (Admin, PV Associate, PV Manager, Medical Reviewer) can view case data. Edit is restricted based on case status – closed/finalized/submitted/duplicate/invalid cases are read-only.', 55, y + 4);
y = doc.y + 8;

const sections = [
  {
    name: '7.1 Patient Details',
    fields: [
      'Patient Initials, Patient Identifier, Date of Birth, Age (value + unit)',
      'Gender, Weight (kg), Height (cm)',
      'Is Pregnant (Yes/No + remarks)',
      'Relevant Medical History, Allergy History, Other Patient Information',
    ],
  },
  {
    name: '7.2 Reporter Details',
    fields: [
      'Reporter Name, Reporter Type (Doctor, Nurse, Pharmacist, Consumer, etc.)',
      'Organization, Address, City, State, Country, Postal Code',
      'Phone, Email, Fax',
      'Is Reporter the Prescriber? (Yes/No)',
    ],
  },
  {
    name: '7.3 Suspect Product Details (1 or more)',
    fields: [
      'Product Name (linked to Product Master), Batch No, Expiry Date',
      'Indication, Route of Administration, Dose, Dose Unit, Frequency',
      'Start Date, Stop Date, Duration',
      'Action Taken (Drug Withdrawn, Dose Reduced, etc.)',
      'Rechallenge / De-challenge information',
    ],
  },
  {
    name: '7.4 Adverse Event Details (1 or more)',
    fields: [
      'Event Term (MedDRA term), Event Description',
      'Onset Date, Resolution Date, Duration',
      'Outcome (Recovered, Fatal, Not Recovered, Unknown, etc.)',
      'Seriousness criteria (Life Threatening, Hospitalization, Death, Congenital Anomaly, etc.)',
      'Causality Assessment, Severity',
    ],
  },
  {
    name: '7.5 Concomitant Medications (optional)',
    fields: [
      'Drug Name, Indication, Dose, Route, Frequency',
      'Start Date, Stop Date',
    ],
  },
  {
    name: '7.6 Lab Details (optional)',
    fields: [
      'Test Name, Test Date, Result Value, Unit, Normal Range',
      'Interpretation (Normal / Abnormal)',
    ],
  },
  {
    name: '7.7 Attachments',
    fields: [
      'Upload supporting documents (medical records, forms, etc.)',
      'File type, original filename, uploaded by and date are recorded',
      'Files are stored securely on the server',
    ],
  },
];

sections.forEach(s => {
  if (doc.y > doc.page.height - 100) { newPage(C.white); }
  doc.moveDown(0.3);
  doc.fillColor(C.secondary).fontSize(10.5).font('Helvetica-Bold').text(s.name, 55, doc.y);
  s.fields.forEach(f => { bullet(f, doc.y + 2, 68); });
  doc.moveDown(0.2);
});

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – WORKFLOW                                                         ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('8. Case Workflow & Actions', 'InnoPV User Manual');

y = sectionTitle('Complete Case Workflow', y);
const wfSteps = [
  { step: 'Step 1 – Intake', who: 'PV Associate / Admin', desc: 'Create case via intake form. System auto-assigns Case No, sets validity, calculates due date. Status = DataEntryInProgress (valid) or InvalidFollowUpRequired.' },
  { step: 'Step 2 – Data Entry', who: 'PV Associate / Admin', desc: 'Complete all data sections: Patient, Reporter, Product, Adverse Event, Concomitant Meds, Lab, Attachments. Status remains DataEntryInProgress.' },
  { step: 'Step 3 – Duplicate Check', who: 'PV Associate / Admin', desc: 'System searches for potential duplicate cases based on patient, product, event. Associate reviews and either clears or marks as duplicate. Status = DuplicateCheckPending.' },
  { step: 'Step 4 – PV Associate Checklist', who: 'PV Associate', desc: 'Complete the role-specific quality checklist. All mandatory items must be checked. Status = PvAssociateChecklistPending.' },
  { step: 'Step 5 – Submit to PV Manager', who: 'PV Associate', desc: 'Once checklist is complete, case is submitted to PV Manager. Status = SubmittedToPvManager. Email notification sent to PV Managers.' },
  { step: 'Step 6 – PV Manager Review', who: 'PV Manager / Admin', desc: 'PV Manager reviews case data. Can: (a) Forward to Medical Reviewer, or (b) Return to PV Associate with comments.' },
  { step: 'Step 7 – PV Manager Checklist', who: 'PV Manager', desc: 'PV Manager completes their quality checklist. Status = PvManagerChecklistPending.' },
  { step: 'Step 8 – Forward to Medical Reviewer', who: 'PV Manager / Admin', desc: 'After checklist, case is forwarded for medical review. Status = ForwardedToMedicalReviewer.' },
  { step: 'Step 9 – Medical Review', who: 'Medical Reviewer', desc: 'Medical Reviewer reviews full case. Completes Medical Reviewer checklist. Status = MedicalReviewPending / MedicalReviewerChecklistPending.' },
  { step: 'Step 10 – Medical Approval / Return', who: 'Medical Reviewer', desc: 'Medical Reviewer either: (a) Approves case medically → Status = MedicallyApproved, or (b) Returns to PV Manager with comments → Status = ReturnedByMedicalReviewer.' },
  { step: 'Step 11 – Case Finalization', who: 'PV Manager / Admin', desc: 'After medical approval, case is finalized. Status = CaseFinalized.' },
  { step: 'Step 12 – Regulatory Submission', who: 'PV Manager / Admin / Medical Reviewer', desc: 'Create submission record(s) for regulatory authority. Track submission status: Pending → Submitted → Acknowledgement Received. Status = Submitted.' },
  { step: 'Step 13 – Case Closure', who: 'Admin / PV Manager', desc: 'After all submissions are complete, validate closure criteria and close the case. Status = CaseClosed.' },
];

wfSteps.forEach(ws => {
  if (doc.y > doc.page.height - 65) { newPage(C.white); doc.y = 60; }
  const startY = doc.y + 4;
  doc.rect(55, startY, 4, 40).fill(C.secondary);
  doc.fillColor(C.primary).fontSize(10).font('Helvetica-Bold')
     .text(ws.step, 64, startY + 2, { width: 200 });
  doc.fillColor(C.success).fontSize(8.5).font('Helvetica-Bold')
     .text('[' + ws.who + ']', 64, startY + 14, { width: 300 });
  doc.fillColor(C.darkGrey).fontSize(9).font('Helvetica')
     .text(ws.desc, 64, doc.y + 2, { width: doc.page.width - 124 });
  doc.y += 6;
});

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – CHECKLIST                                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('9. Checklist Capture', 'InnoPV User Manual');

y = sectionTitle('Purpose of Checklists', y);
y = bodyText('Checklists are quality assurance tools that ensure all required steps are completed before a case progresses to the next stage. Different checklists apply to different roles and stages. Checklists are configurable by Admin.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('How to Complete a Checklist', y);
const clSteps = [
  'Open the case from your Case Inbox.',
  'Click "Checklist" from the case action menu.',
  'The system loads the appropriate checklist for your role and current stage.',
  'Read each checklist item carefully.',
  'Check the checkbox next to each item when you have verified it.',
  'Add remarks in the Remarks field if needed.',
  'Mandatory items (marked with *) MUST be checked before you can proceed.',
  'Click "Save & Submit" to save your checklist responses and advance the workflow.',
];
clSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Checklist Types', y);
const clTypes = [
  ['PV Associate Checklist', 'PV Associate', 'Case data completeness and quality check before submission to PV Manager'],
  ['PV Manager Checklist', 'PV Manager', 'Review quality and regulatory compliance before forwarding to Medical Reviewer'],
  ['Medical Reviewer Checklist', 'Medical Reviewer', 'Medical accuracy and approval criteria verification'],
];
y += 4;
clTypes.forEach((ct, i) => {
  const bg = i % 2 === 0 ? C.lightGrey : C.white;
  doc.rect(55, y, doc.page.width - 110, 28).fill(bg);
  doc.fillColor(C.primary).fontSize(9.5).font('Helvetica-Bold').text(ct[0], 60, y + 4, { width: 170 });
  doc.fillColor(C.secondary).fontSize(9.5).font('Helvetica-Bold').text(ct[1], 235, y + 4, { width: 90 });
  doc.fillColor(C.darkGrey).fontSize(9.5).font('Helvetica').text(ct[2], 330, y + 4, { width: 190 });
  y += 28;
});
y += 8;

y = sectionTitle('Checklist Versioning', y);
y = bodyText('Admin can create new versions of checklists (with EffectiveFrom date). The system always uses the latest active checklist for each role/stage combination. Checklist responses are permanently saved per case per version for audit purposes.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – FOLLOW-UP                                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('10. Case Follow-Up Management', 'InnoPV User Manual');

y = sectionTitle('What is a Follow-Up?', y);
y = bodyText('A follow-up is additional information received for an existing case after initial intake. This could be updated patient status, new lab results, reporter clarifications, etc. Follow-ups are tracked separately and can update the validity status of a case.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Adding a Follow-Up', y);
const fuSteps = [
  'Open the case from Case Inbox.',
  'Click "Follow-Up" from the case action menu.',
  'On the Follow-Up list, click "Add Follow-Up".',
  'Enter: Receipt Date, Source, Received From, Description, Additional Information.',
  'Click Save. A follow-up number (FU-001, FU-002, etc.) is auto-assigned.',
  'Follow-ups cannot be added to duplicate or invalid cases.',
];
fuSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Processing a Follow-Up', y);
y = bodyText('Once a follow-up is reviewed and the case data is updated accordingly, mark the follow-up as "Processed" with remarks. This keeps the follow-up log clean and auditable.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Follow-Up Fields', y);
const fuFields = [
  ['Follow-Up No', 'Auto-generated sequential number (FU-001, FU-002...)'],
  ['Receipt Date', 'Date additional information was received'],
  ['Source', 'Where the information came from'],
  ['Received From', 'Name/entity that provided the information'],
  ['Description', 'Summary of the follow-up information'],
  ['Additional Information', 'Detailed notes'],
  ['Is Processed', 'Whether the case data has been updated based on this follow-up'],
  ['Processed Remarks', 'Notes on what changes were made to the case'],
];
y = tableLike(fuFields, y + 4, 160, 320);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – DUPLICATE CHECK                                                  ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('11. Duplicate Check', 'InnoPV User Manual');

y = sectionTitle('Purpose', y);
y = bodyText('The Duplicate Check module prevents multiple records being created for the same adverse event case. The system automatically finds potential duplicates based on matching patient identifiers, suspect products, and adverse event terms.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('How to Perform a Duplicate Check', y);
const dcSteps = [
  'Open the case and click "Duplicate Check" from the action menu.',
  'The system displays a list of potential duplicate cases based on matching criteria.',
  'Review each candidate case by clicking its Case No to open it.',
  'If a duplicate is found: select the duplicate case, enter remarks, and click "Mark as Duplicate".',
  'The current case is set to status "MarkedAsDuplicate" and a link is created to the original case.',
  'If no duplicate found: click "No Duplicate Found – Continue" to proceed.',
];
dcSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Duplicate Matching Criteria', y);
y = bodyText('The system checks cases with the same: patient identifier AND suspect product name AND adverse event term. Cases that are already marked as duplicate, invalid, or closed are excluded from candidates.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Access', y);
y = bodyText('Available to: Admin, PV Associate, PV Manager. Medical Reviewer does not perform duplicate checks.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – HISTORY / COMPLETE REPORT                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('12–13. Case History, Audit Trail & Complete Report', 'InnoPV User Manual');

y = sectionTitle('12. Case History & Audit Trail', y);
y = bodyText('All roles can view the complete history of any case they have access to. Navigate to Case Inbox → Open Case → History.', 55, y + 4);
y = doc.y + 6;

y = subTitle('Workflow Comments', y);
y = bodyText('Every action (submit, return, forward, approve) generates a comment record showing: action type, from role, to role, comment text, performed by, timestamp.', 55, y);
y = doc.y + 6;

y = subTitle('Audit Trail', y);
y = bodyText('The system records every change to every field in the case. Each audit trail entry shows: entity modified, field name, old value, new value, performed by user, timestamp. This provides a complete 21 CFR Part 11 compliant audit trail.', 55, y);
y = doc.y + 10;

y = sectionTitle('13. Case Complete Report (PDF / Word / Excel)', y);
y = bodyText('All roles can generate a complete case report that includes all sections of a case in one document. Navigate to Case Inbox → Open Case → Complete Report.', 55, y + 4);
y = doc.y + 6;

const reportFormats = [
  ['PDF Export', 'Generates a comprehensive formatted PDF report of the entire case suitable for printing or archiving'],
  ['Word (DOCX)', 'Generates a Microsoft Word document for further editing or submission preparation'],
  ['Excel (XLSX)', 'Generates a structured Excel file with all case data in tabular format for analysis'],
];
y = tableLike(reportFormats, y + 4, 140, 340);
y += 8;

y = sectionTitle('Case Complete Report – Index View', y);
y = bodyText('The Report Index page allows filtering cases by status, seriousness, date range, product, and event. You can view summary statistics (Total, Serious, Non-Serious, Valid, Invalid, Closed) and then select individual cases to export their complete report.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – REGULATORY SUBMISSION                                            ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('14. Regulatory Submission Management', 'InnoPV User Manual');

y = sectionTitle('Overview', y);
y = bodyText('After a case is medically approved and finalized, regulatory submissions must be created and tracked. Access: Admin, PV Manager, Medical Reviewer.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Creating a Regulatory Submission', y);
const rsFields = [
  ['Submission Type', 'E.g., 15-Day Expedited, Periodic, Annual Safety Report'],
  ['Recipient Authority', 'Regulatory body (e.g., CDSCO, FDA, EMA, WHO)'],
  ['Submission Format', 'E.g., E2B (XML), MedWatch, CIOMS, NCA Form'],
  ['Due Date', 'Submission deadline based on regulatory requirements'],
  ['Submission Status', 'SubmissionPending → Submitted → AcknowledgementPending → AcknowledgementReceived'],
  ['Submitted Date', 'Actual date submission was sent'],
  ['Acknowledgement Date', 'Date regulatory body acknowledged receipt'],
  ['Reference No', 'Acknowledgement reference number from authority'],
  ['Remarks', 'Additional notes on the submission'],
  ['File Attachment', 'Upload the submission document (E2B XML, PDF, etc.)'],
];
y = tableLike(rsFields, y + 4, 170, 310);
y += 8;

y = sectionTitle('Submission Status Flow', y);
const ssFlow = [
  'SubmissionPending → Initial state when submission record is created',
  'Submitted → Submission has been sent to regulatory authority',
  'AcknowledgementPending → Awaiting acknowledgement from authority',
  'AcknowledgementReceived → Authority has acknowledged the submission',
  'Rejected → Submission was rejected; requires resubmission',
  'Cancelled → Submission was cancelled (case may be invalid/duplicate)',
];
ssFlow.forEach(s => { y = bullet(s, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Overdue Submissions', y);
y = bodyText('Submissions are automatically flagged as "Overdue" when the Due Date has passed and the status is still SubmissionPending or AcknowledgementPending. Overdue count is visible on the submission index page.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – REPORTS                                                          ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('15. Reports & Analytics', 'InnoPV User Manual');

y = sectionTitle('Case Report (Access: Admin + PV Manager)', y);
y = bodyText('Provides a filterable list of all cases with summary statistics. Navigate to Reports → Case Report.', 55, y + 4);
y = doc.y + 6;

y = subTitle('Filter Options', y);
const caseReportFilters = [
  'Date range (Receipt Date from – to)',
  'Case status',
  'Is Serious (Yes/No)',
  'Is Valid (Yes/No)',
  'Product name',
  'Event term',
];
caseReportFilters.forEach(f => { y = bullet(f, y) + 2; });
y = doc.y + 4;

y = subTitle('Summary Statistics', y);
y = bodyText('Total Count, Valid Count, Invalid Count, Serious Count, Non-Serious Count, Closed Count, Overdue Count.', 55, y);
y = doc.y + 4;

y = subTitle('Export to CSV', y);
y = bodyText('Click "Export CSV" to download all filtered cases in CSV format. File is named with timestamp: PV_Case_Report_YYYYMMDD_HHmmss.csv', 55, y);
y = doc.y + 10;

y = sectionTitle('Regulatory Submission Report (Access: Admin + PV Manager)', y);
y = bodyText('Provides a filterable list of all regulatory submissions. Navigate to Reports → Submission Report.', 55, y + 4);
y = doc.y + 6;

y = subTitle('Filter Options', y);
const subFilters = [
  'Case number, Submission type, Recipient authority',
  'Submission status, Date range',
  'Overdue only filter',
];
subFilters.forEach(f => { y = bullet(f, y) + 2; });
y = doc.y + 4;

y = subTitle('Summary Statistics', y);
y = bodyText('Total, Pending, Submitted, Acknowledgement Pending, Acknowledgement Received, Overdue, Rejected, Cancelled.', 55, y);
y = doc.y + 4;

y = subTitle('Export to CSV', y);
y = bodyText('Click "Export CSV" to download submission report. File: Regulatory_Submission_Report_YYYYMMDD_HHmmss.csv', 55, y);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – SLA ALERTS                                                       ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('16. SLA Alerts', 'InnoPV User Manual');

y = sectionTitle('What are SLA Alerts?', y);
y = bodyText('SLA (Service Level Agreement) Alerts are automated email notifications sent to relevant users when cases are approaching their due date or have already become overdue. This ensures timely case processing and regulatory compliance.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Access', y);
y = bodyText('SLA Alerts module is accessible to Admin and PV Manager only. Navigate to SLA Alerts from the main menu.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Alert Types', y);
const alertTypes = [
  ['Due Soon Alerts', 'Sent for cases whose due date falls within the next 3 days (configurable). Reminds users to process the case before deadline.'],
  ['Overdue Alerts', 'Sent for cases whose due date has already passed and the case is not yet closed. Escalation notification.'],
  ['All Alerts', 'Sends both Due Soon and Overdue alerts in a single batch operation.'],
];
y = tableLike(alertTypes, y + 4, 160, 320);
y += 8;

y = sectionTitle('How to Send Alerts', y);
const alertSteps = [
  'Navigate to SLA Alerts from the main navigation menu.',
  'The page shows a preview of cases that would receive alerts (Due Soon and Overdue counts).',
  'Click "Send Due Soon Alerts" to send reminders for cases nearing deadline.',
  'Click "Send Overdue Alerts" to send escalation notices for overdue cases.',
  'Click "Send All Alerts" to process both types in one action.',
  'A success message confirms the number of cases processed and email recipients.',
];
alertSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Email Recipients', y);
y = bodyText('Alert emails are sent to the currently assigned user for each case, and to PV Managers. The email includes the Case No, product, event, due date, and current status.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – ADMIN USER MANAGEMENT                                            ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('17. Admin – User Management', 'InnoPV User Manual');

y = sectionTitle('Overview (Admin only)', y);
y = bodyText('Admin can manage all system users from Admin → User Management. This includes creating new users, editing profiles, activating/deactivating accounts, and unlocking locked accounts.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('User List', y);
const ulCols = [
  ['Full Name', 'User\'s full name'],
  ['Email', 'Login email address (also username)'],
  ['Mobile No', 'Contact number'],
  ['Designation', 'Job title/designation'],
  ['Department', 'Department (default: PV)'],
  ['Role', 'Assigned system role'],
  ['Is Active', 'Whether user can log in'],
  ['Must Change Password', 'Forced password change flag on next login'],
  ['Lockout End', 'If locked, when lockout expires'],
  ['Actions', 'Edit, Activate/Deactivate, Unlock, Reset Password'],
];
y = tableLike(ulCols, y + 4, 160, 320);
y += 8;

y = sectionTitle('Creating a New User', y);
const createUserSteps = [
  'Navigate to Admin → User Management → Create User.',
  'Enter Full Name, Email, Mobile No, Designation, Department.',
  'Select Role from dropdown (Admin, PV Associate, PV Manager, Medical Reviewer).',
  'The system auto-generates a temporary password.',
  'Check "Send Credentials by Email" to email login details to the user.',
  'Click Create. User account is created with MustChangePassword = true.',
  'User must change password on first login.',
];
createUserSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Editing a User', y);
y = bodyText('Click Edit on the user row. Admin can update: Full Name, Mobile No, Designation, Department, Role. To deactivate a user, toggle the Is Active flag – the user will be blocked from login immediately.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Unlocking an Account', y);
y = bodyText('If a user account is locked due to multiple failed login attempts, Admin can unlock it from User Management → click Unlock next to the user. The lockout end date is cleared and the user can log in again.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – ADMIN MASTER DATA                                                ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('18. Admin – Master Data Management', 'InnoPV User Manual');

y = sectionTitle('Overview (Admin only)', y);
y = bodyText('Master Data provides the dropdown values used throughout the application. Navigate to Admin → Master Data. Keeping master data accurate ensures consistent data entry across cases.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Common Master Options', y);
y = bodyText('These are the dropdown lists used in case data entry forms:', 55, y + 4);
y = doc.y + 4;

const commonMasters = [
  ['Reporter Type', 'Types of reporters (e.g., Physician, Pharmacist, Nurse, Patient, Consumer, Sponsor)'],
  ['Outcome', 'Adverse event outcomes (e.g., Recovered, Fatal, Not Recovered, Recovering, Unknown, Sequelae)'],
  ['Route of Administration', 'Drug routes (e.g., Oral, IV, IM, SC, Topical, Inhalation)'],
  ['Frequency', 'Dosing frequency (e.g., OD, BD, TDS, QID, Weekly, Monthly)'],
  ['Causality', 'Causality assessment values (e.g., Certain, Probable, Possible, Unlikely, Unclassifiable)'],
  ['Seriousness Criteria', 'ICH E2A serious criteria (e.g., Death, Life-Threatening, Hospitalization, Disability, Congenital Anomaly)'],
  ['Severity Grade', 'Event severity (e.g., Mild, Moderate, Severe, Life-Threatening, Fatal)'],
  ['Action Taken', 'Action taken with suspect product (e.g., Drug Withdrawn, Dose Reduced, Dose Increased, No Change)'],
  ['Case Source', 'Case source types (e.g., Spontaneous, Clinical Trial, Literature, Post-Marketing, Regulatory Authority)'],
];
y = tableLike(commonMasters, y + 4, 160, 320);
y += 8;

y = sectionTitle('Product Master', y);
y = bodyText('Manage the list of pharmaceutical products. Each product has: Product Name, Generic Name, Strength, Dosage Form, Category, Is Active flag. Products appear in the Suspect Product Details dropdown.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Study Master & Sponsor Master', y);
y = bodyText('Study Master manages clinical study records (Study Code, Study Title, Phase, Status). Sponsor Master manages pharmaceutical company/sponsor information. These are used for clinical trial cases.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – ADMIN CHECKLIST MANAGEMENT                                       ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('19. Admin – Checklist Management', 'InnoPV User Manual');

y = sectionTitle('Overview (Admin only)', y);
y = bodyText('Admin can configure checklists for each role and workflow stage. Checklists drive the quality gate at each workflow step. Navigate to Admin → Checklists.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Creating a Checklist Master', y);
const chkSteps = [
  'Navigate to Admin → Checklists → Create Checklist.',
  'Enter Checklist Name (e.g., "PV Associate Data Quality Checklist v2.0").',
  'Select Applicable Role (PV Associate / PV Manager / Medical Reviewer).',
  'Select Applicable Stage (stage where checklist is triggered).',
  'Enter Version No (e.g., "1.0", "2.0").',
  'Set Effective From date (when this checklist version becomes active).',
  'Set Is Active = true to make it the current checklist for that role/stage.',
  'Save the master, then add checklist items.',
];
chkSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Adding Checklist Items', y);
const chkItemFields = [
  ['Item Text', 'The checklist question or criteria statement (e.g., "Is patient identifier present?")'],
  ['Is Mandatory', 'If Yes, user CANNOT submit without checking this item'],
  ['Display Order', 'Sequence number for ordering items in the checklist'],
  ['Is Active', 'Whether this item appears in the checklist'],
];
y = tableLike(chkItemFields, y + 4, 140, 340);
y += 8;

y = sectionTitle('Checklist Versioning Rules', y);
y = bodyText('Only ONE checklist should be active per Role + Stage combination. When creating a new version, deactivate the old one. Historical checklist responses are preserved with the checklist master version they were based on.', 55, y + 4);

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – PROFILE & PASSWORD                                               ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('20. Profile & Password Management', 'InnoPV User Manual');

y = sectionTitle('View My Profile', y);
y = bodyText('Click on your name in the top navigation bar → My Profile. View your: Full Name, Email, Mobile No, Designation, Department, Role.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Change Password', y);
const pwSteps = [
  'Navigate to My Profile → Change Password.',
  'Enter your Current Password.',
  'Enter a New Password (must differ from current).',
  'Confirm the New Password.',
  'Click Change Password.',
  'Password is updated immediately. You remain logged in.',
];
pwSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });
y = doc.y + 8;

y = sectionTitle('Forced Password Change', y);
y = bodyText('If your account was newly created or password was reset by Admin, you will be automatically redirected to the Change Password screen on login. You cannot access any other page until you change your password.', 55, y + 4);
y = doc.y + 8;

y = sectionTitle('Forgot Password (Self-Service)', y);
const fpSteps = [
  'On the login page, click "Forgot Password?"',
  'Enter your registered email address.',
  'A password reset link is sent to your email.',
  'Click the link in the email (link is time-limited for security).',
  'Enter and confirm your new password.',
  'Login with the new password.',
];
fpSteps.forEach((s, i) => { y = bullet(`Step ${i+1}: ${s}`, y) + 2; });

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – STATUS FLOW                                                      ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('21. Case Status Flow Reference', 'InnoPV User Manual');

y = sectionTitle('Complete Status Flow', y);
y += 4;

const statuses = [
  { status: 'Draft', color: '#95A5A6', desc: 'Case saved but not yet processed' },
  { status: 'DataEntryInProgress', color: C.secondary, desc: 'Data entry ongoing (after valid intake)' },
  { status: 'ValidityPending', color: C.amber, desc: 'Validity criteria under review' },
  { status: 'InvalidFollowUpRequired', color: C.accent, desc: 'Case fails validity; follow-up needed' },
  { status: 'DuplicateCheckPending', color: '#8E44AD', desc: 'Awaiting duplicate check by PV Associate' },
  { status: 'PvAssociateChecklistPending', color: C.secondary, desc: 'PV Associate completing quality checklist' },
  { status: 'SubmittedToPvManager', color: C.success, desc: 'Case submitted to PV Manager inbox' },
  { status: 'PvManagerReviewPending', color: C.success, desc: 'PV Manager reviewing the case' },
  { status: 'PvManagerChecklistPending', color: C.success, desc: 'PV Manager completing quality checklist' },
  { status: 'ReturnedByPvManager', color: C.accent, desc: 'Returned to PV Associate for corrections' },
  { status: 'ResubmittedToPvManager', color: C.secondary, desc: 'PV Associate resubmitted after corrections' },
  { status: 'ForwardedToMedicalReviewer', color: C.amber, desc: 'Forwarded for medical review' },
  { status: 'MedicalReviewPending', color: C.amber, desc: 'Medical Reviewer reviewing case' },
  { status: 'MedicalReviewerChecklistPending', color: C.amber, desc: 'Medical Reviewer completing checklist' },
  { status: 'ReturnedByMedicalReviewer', color: C.accent, desc: 'Returned to PV Manager for corrections' },
  { status: 'AdditionalInformationRequired', color: '#E67E22', desc: 'More information requested' },
  { status: 'MedicallyApproved', color: C.success, desc: 'Case medically approved by Medical Reviewer' },
  { status: 'CaseFinalized', color: C.success, desc: 'Case finalized, ready for submission' },
  { status: 'SubmissionPending', color: '#E67E22', desc: 'Regulatory submission pending' },
  { status: 'Submitted', color: C.success, desc: 'Submitted to regulatory authority' },
  { status: 'AcknowledgementPending', color: C.amber, desc: 'Awaiting acknowledgement from authority' },
  { status: 'CaseClosed', color: '#2C3E50', desc: 'Case fully closed' },
  { status: 'Reopened', color: '#E67E22', desc: 'Previously closed case reopened' },
  { status: 'MarkedAsDuplicate', color: '#7F8C8D', desc: 'Case identified as duplicate of another' },
  { status: 'MarkedAsInvalid', color: '#7F8C8D', desc: 'Case marked as invalid and archived' },
  { status: 'OnHold', color: '#7F8C8D', desc: 'Case placed on hold' },
];

const colW1 = 180, colW2 = 300;
statuses.forEach((s, i) => {
  if (doc.y > doc.page.height - 30) { newPage(C.white); y = 60; }
  const rowH = 20, rowY = doc.y;
  const bg = i % 2 === 0 ? C.lightGrey : C.white;
  doc.rect(55, rowY, colW1 + colW2, rowH).fill(bg);
  doc.roundedRect(57, rowY + 4, 12, 12, 2).fill(s.color);
  doc.fillColor(C.primary).fontSize(9).font('Helvetica-Bold')
     .text(s.status, 74, rowY + 5, { width: colW1 - 20 });
  doc.fillColor(C.darkGrey).fontSize(9).font('Helvetica')
     .text(s.desc, 55 + colW1, rowY + 5, { width: colW2 - 8 });
  doc.y = rowY + rowH;
});

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  PAGE – ROLE MATRIX                                                      ║
// ╚══════════════════════════════════════════════════════════════════════════╝
newPage(C.white);
y = headerBand('22. Role-wise Feature Matrix', 'InnoPV User Manual');

y += 4;

const features = [
  ['Feature / Module', 'Admin', 'PV Assoc.', 'PV Mgr', 'Med.Rev.'],
  ['Login & Authentication', '✔', '✔', '✔', '✔'],
  ['Dashboard (Role-specific)', '✔', '✔', '✔', '✔'],
  ['PV Analytics Dashboard', '✔', '✔', '✔', '✔'],
  ['Case Inbox', '✔ (All)', '✔ (Own)', '✔ (Stage)', '✔ (Stage)'],
  ['Create New Case (Intake)', '✔', '✔', '✘', '✘'],
  ['Case Data Entry (Patient)', '✔', '✔', 'Read', 'Read'],
  ['Case Data Entry (Reporter)', '✔', '✔', 'Read', 'Read'],
  ['Case Data Entry (Product)', '✔', '✔', 'Read', 'Read'],
  ['Case Data Entry (Adverse Ev.)', '✔', '✔', 'Read', 'Read'],
  ['Case Data Entry (Conc. Meds)', '✔', '✔', 'Read', 'Read'],
  ['Case Data Entry (Lab)', '✔', '✔', 'Read', 'Read'],
  ['Case Attachments', '✔', '✔', 'Read', 'Read'],
  ['Duplicate Check', '✔', '✔', '✔', '✘'],
  ['PV Associate Checklist', '✔', '✔', '✘', '✘'],
  ['PV Manager Checklist', '✔', '✘', '✔', '✘'],
  ['Medical Reviewer Checklist', '✔', '✘', '✘', '✔'],
  ['Case Follow-Up', '✔', '✔', '✔', '✔'],
  ['Case Assignment', '✔', '✘', '✔', '✘'],
  ['Regulatory Submission', '✔', '✘', '✔', '✔'],
  ['Case Closure Validation', '✔', '✘', '✔', '✔'],
  ['Case History & Audit Trail', '✔', '✔', '✔', '✔'],
  ['Case Report (CSV)', '✔', '✘', '✔', '✘'],
  ['Submission Report (CSV)', '✔', '✘', '✔', '✘'],
  ['Complete Case Report (PDF)', '✔', '✔', '✔', '✔'],
  ['SLA Alerts', '✔', '✘', '✔', '✘'],
  ['User Management', '✔', '✘', '✘', '✘'],
  ['Master Data Management', '✔', '✘', '✘', '✘'],
  ['Checklist Management', '✔', '✘', '✘', '✘'],
  ['Change My Password', '✔', '✔', '✔', '✔'],
  ['View My Profile', '✔', '✔', '✔', '✔'],
];

const colWidths = [200, 65, 65, 65, 65];
const headers = features[0];

// Header row
doc.rect(55, y, colWidths.reduce((a, b) => a + b, 0), 22).fill(C.primary);
let cx = 55;
headers.forEach((h, i) => {
  doc.fillColor(C.white).fontSize(9).font('Helvetica-Bold').text(h, cx + 4, y + 6, { width: colWidths[i] - 8 });
  cx += colWidths[i];
});
y += 22;

const checkColor = (val) => {
  if (val === '✔') return C.success;
  if (val === '✘') return C.accent;
  return C.amber;
};

features.slice(1).forEach((row, ri) => {
  if (doc.y > doc.page.height - 22) { newPage(C.white); y = doc.y; }
  const rowH = 18;
  const bg = ri % 2 === 0 ? C.lightGrey : C.white;
  doc.rect(55, y, colWidths.reduce((a, b) => a + b, 0), rowH).fill(bg);
  let cxi = 55;
  row.forEach((cell, ci) => {
    const color = ci === 0 ? C.darkGrey : checkColor(cell);
    const bold = ci === 0 ? 'Helvetica' : 'Helvetica-Bold';
    doc.fillColor(color).fontSize(ci === 0 ? 9 : 10).font(bold)
       .text(cell, cxi + 4, y + 4, { width: colWidths[ci] - 8, align: ci === 0 ? 'left' : 'center' });
    cxi += colWidths[ci];
  });
  y += rowH;
});

// ─── Footer on all pages ─────────────────────────────────────────────────────
const totalPages = doc.bufferedPageRange().count;
for (let i = 0; i < totalPages; i++) {
  doc.switchToPage(i);
  if (i === 0) continue; // skip cover
  doc.rect(0, doc.page.height - 32, doc.page.width, 32).fill(C.primary);
  doc.fillColor('#7F8C8D').fontSize(8).font('Helvetica')
     .text('InnoPV – Pharmacovigilance Case Management System | Confidential | v1.0 | June 2026',
       55, doc.page.height - 20, { width: doc.page.width - 160 });
  doc.fillColor(C.white).fontSize(8).font('Helvetica-Bold')
     .text(`Page ${i} of ${totalPages - 1}`, doc.page.width - 100, doc.page.height - 20,
       { width: 80, align: 'right' });
}

doc.flushPages();
doc.end();

stream.on('finish', () => {
  console.log('✅ PDF generated: ' + OUT_FILE);
});
stream.on('error', err => {
  console.error('❌ Error:', err);
});
