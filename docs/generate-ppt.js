'use strict';
const PptxGenJS = require('pptxgenjs');

const prs = new PptxGenJS();
prs.layout = 'LAYOUT_WIDE'; // 13.33 x 7.5 inches

// ─── Theme colors ────────────────────────────────────────────────────────────
const CLR = {
  navy:    '1B3A6B',
  blue:    '2E86C1',
  green:   '1E8449',
  red:     'E74C3C',
  amber:   'D4AC0D',
  white:   'FFFFFF',
  offWhite:'F2F3F4',
  dark:    '2C3E50',
  mid:     '566573',
  light:   'EBF5FB',
  cyan:    '17A589',
};

// ─── Slide helpers ──────────────────────────────────────────────────────────
function titleSlide(title, subtitle, bg) {
  const slide = prs.addSlide();
  slide.background = { color: bg || CLR.navy };
  // Accent bar left
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 0, w: 0.18, h: 7.5, fill: { color: CLR.blue } });
  // Title
  slide.addText(title, {
    x: 0.4, y: 2.5, w: 12.5, h: 1.4,
    fontSize: 44, bold: true, color: CLR.white,
    fontFace: 'Calibri', align: 'left', valign: 'middle',
  });
  if (subtitle) {
    slide.addText(subtitle, {
      x: 0.4, y: 4.0, w: 12.5, h: 0.6,
      fontSize: 18, color: 'BDC3C7',
      fontFace: 'Calibri', align: 'left',
    });
  }
  // Bottom strip
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 7.1, w: 13.33, h: 0.4, fill: { color: CLR.blue } });
  slide.addText('InnoPV – Pharmacovigilance Case Management System | Confidential', {
    x: 0, y: 7.1, w: 13.33, h: 0.4,
    fontSize: 9, color: CLR.white, align: 'center', valign: 'middle',
    fontFace: 'Calibri',
  });
  return slide;
}

function contentSlide(title, accentColor) {
  const slide = prs.addSlide();
  slide.background = { color: CLR.white };
  // Header band
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 0, w: 13.33, h: 1.0, fill: { color: accentColor || CLR.navy } });
  // Accent bottom of header
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 0.95, w: 13.33, h: 0.08, fill: { color: CLR.blue } });
  // Title text
  slide.addText(title, {
    x: 0.35, y: 0.05, w: 12.6, h: 0.9,
    fontSize: 22, bold: true, color: CLR.white,
    fontFace: 'Calibri', align: 'left', valign: 'middle',
  });
  // Footer
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 7.15, w: 13.33, h: 0.35, fill: { color: '1A2943' } });
  slide.addText('InnoPV User Manual | v1.0 | June 2026', {
    x: 0, y: 7.15, w: 13.33, h: 0.35,
    fontSize: 8, color: '99A3A4', align: 'center', valign: 'middle', fontFace: 'Calibri',
  });
  return slide;
}

function addBullets(slide, items, x, y, w, color, fontSize, indent) {
  const rows = items.map(item => ({
    text: item,
    options: { bullet: { type: 'bullet', indent: indent || 15 }, fontSize: fontSize || 14, color: color || CLR.dark, fontFace: 'Calibri', paraSpaceBefore: 4 }
  }));
  slide.addText(rows, { x, y, w, h: 5.5, valign: 'top' });
}

function roleBox(slide, role, color, x, y, w, h, desc, bullets) {
  slide.addShape(prs.ShapeType.roundRect, { x, y, w, h, fill: { color }, line: { color, width: 0 }, rectRadius: 0.1 });
  slide.addText(role, { x: x + 0.1, y: y + 0.05, w: w - 0.2, h: 0.35, fontSize: 14, bold: true, color: CLR.white, fontFace: 'Calibri' });
  if (desc) {
    slide.addText(desc, { x: x + 0.1, y: y + 0.4, w: w - 0.2, h: 0.5, fontSize: 10, color: CLR.white, fontFace: 'Calibri', wrap: true });
  }
  if (bullets) {
    const rows = bullets.map(b => ({ text: b, options: { bullet: { type: 'bullet', indent: 10 }, fontSize: 9.5, color: 'F8F9FA', fontFace: 'Calibri', paraSpaceBefore: 2 } }));
    slide.addText(rows, { x: x + 0.1, y: y + (desc ? 0.9 : 0.45), w: w - 0.2, h: h - (desc ? 1.0 : 0.55), valign: 'top' });
  }
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 1 – COVER                                                         ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = prs.addSlide();
  slide.background = { color: CLR.navy };
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 0, w: 0.25, h: 7.5, fill: { color: CLR.blue } });
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 6.2, w: 13.33, h: 1.3, fill: { color: '162E55' } });
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 6.18, w: 13.33, h: 0.07, fill: { color: CLR.blue } });

  slide.addText([
    { text: 'Inno', options: { color: CLR.blue, fontSize: 58, bold: true } },
    { text: 'PV', options: { color: CLR.white, fontSize: 58, bold: true } },
  ], { x: 0.5, y: 1.2, w: 8, h: 1.4, fontFace: 'Calibri' });

  slide.addText('Pharmacovigilance Case Management System', {
    x: 0.5, y: 2.55, w: 10, h: 0.5, fontSize: 18, color: 'BDC3C7', fontFace: 'Calibri',
  });
  slide.addShape(prs.ShapeType.line, { x: 0.5, y: 3.15, w: 9, h: 0, line: { color: CLR.blue, width: 2 } });
  slide.addText('User Manual', { x: 0.5, y: 3.3, w: 10, h: 0.9, fontSize: 36, bold: true, color: CLR.white, fontFace: 'Calibri' });
  slide.addText('Complete guide for all user roles and system features', {
    x: 0.5, y: 4.25, w: 10, h: 0.45, fontSize: 15, color: 'BDC3C7', fontFace: 'Calibri',
  });

  const infoItems = [
    ['Version:', 'v1.0'],
    ['Date:', 'June 2026'],
    ['Audience:', 'Admin / PV Associate / PV Manager / Medical Reviewer'],
    ['Classification:', 'Confidential – For Internal Use Only'],
  ];
  infoItems.forEach((item, i) => {
    slide.addText(item[0], { x: 0.5, y: 6.3 + i * 0.22, w: 1.5, h: 0.22, fontSize: 9, bold: true, color: '99A3A4', fontFace: 'Calibri' });
    slide.addText(item[1], { x: 2.0, y: 6.3 + i * 0.22, w: 8, h: 0.22, fontSize: 9, color: CLR.white, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 2 – AGENDA                                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Agenda – What We Will Cover');
  const agendaItems = [
    ['1', 'System Overview & Introduction'],
    ['2', 'User Roles & Access Rights'],
    ['3', 'Login, Authentication & Security'],
    ['4', 'Dashboard & Analytics'],
    ['5', 'Case Inbox'],
    ['6', 'PV Case Creation & Intake'],
    ['7', 'Case Data Entry (All Sections)'],
    ['8', 'Case Workflow & Actions'],
    ['9', 'Checklist Capture'],
    ['10', 'Follow-Up, Duplicate Check, Case History'],
    ['11', 'Regulatory Submission Management'],
    ['12', 'Reports & SLA Alerts'],
    ['13', 'Admin Panel (Users, Masters, Checklists)'],
    ['14', 'Profile & Password Management'],
    ['15', 'Role-wise Feature Matrix'],
  ];

  const half = Math.ceil(agendaItems.length / 2);
  agendaItems.slice(0, half).forEach((item, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: 0.35, y: 1.15 + i * 0.4, w: 0.35, h: 0.3, fill: { color: CLR.blue }, rectRadius: 0.05 });
    slide.addText(item[0], { x: 0.35, y: 1.15 + i * 0.4, w: 0.35, h: 0.3, fontSize: 10, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(item[1], { x: 0.78, y: 1.15 + i * 0.4, w: 5.6, h: 0.3, fontSize: 13, color: CLR.dark, fontFace: 'Calibri', valign: 'middle' });
  });
  agendaItems.slice(half).forEach((item, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: 6.9, y: 1.15 + i * 0.4, w: 0.35, h: 0.3, fill: { color: CLR.blue }, rectRadius: 0.05 });
    slide.addText(item[0], { x: 6.9, y: 1.15 + i * 0.4, w: 0.35, h: 0.3, fontSize: 10, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(item[1], { x: 7.33, y: 1.15 + i * 0.4, w: 5.65, h: 0.3, fontSize: 13, color: CLR.dark, fontFace: 'Calibri', valign: 'middle' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 3 – SYSTEM OVERVIEW                                               ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('System Overview – What is InnoPV?');
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 12.6, h: 0.8, fill: { color: CLR.light } });
  slide.addText('InnoPV is a web-based Pharmacovigilance (PV) Case Management System for managing the complete lifecycle of adverse event cases – from initial intake to regulatory submission and final closure.', {
    x: 0.5, y: 1.15, w: 12.3, h: 0.7, fontSize: 13.5, color: CLR.navy, fontFace: 'Calibri', valign: 'middle', wrap: true,
  });

  const boxes = [
    { label: 'Case Intake', icon: '📋', color: CLR.blue, x: 0.35, y: 2.1 },
    { label: 'Data Entry', icon: '✏️', color: CLR.cyan, x: 2.85, y: 2.1 },
    { label: 'Workflow', icon: '🔄', color: CLR.green, x: 5.35, y: 2.1 },
    { label: 'Medical Review', icon: '🩺', color: CLR.amber, x: 7.85, y: 2.1 },
    { label: 'Regulatory Submission', icon: '📤', color: '8E44AD', x: 10.35, y: 2.1 },
  ];

  boxes.forEach((b, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: b.x, y: b.y, w: 2.35, h: 1.1, fill: { color: b.color }, rectRadius: 0.12 });
    slide.addText(b.label, { x: b.x, y: b.y + 0.35, w: 2.35, h: 0.45, fontSize: 12, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    if (i < boxes.length - 1) {
      slide.addShape(prs.ShapeType.line, { x: b.x + 2.35, y: b.y + 0.55, w: 0.5, h: 0, line: { color: '95A5A6', width: 1.5 } });
    }
  });

  const features = [
    '✓  ICH E2B & regulatory reporting standards compliance',
    '✓  Multi-role structured workflow with quality checklists',
    '✓  Automated validity & seriousness determination',
    '✓  SLA monitoring with automated email alerts',
    '✓  Full audit trail (21 CFR Part 11 ready)',
    '✓  Case reports in PDF, Word, and Excel formats',
    '✓  Duplicate detection and resolution',
    '✓  Regulatory submission tracking and acknowledgement',
  ];

  const half = 4;
  features.slice(0, half).forEach((f, i) => {
    slide.addText(f, { x: 0.35, y: 3.5 + i * 0.42, w: 6.2, h: 0.38, fontSize: 12, color: CLR.dark, fontFace: 'Calibri' });
  });
  features.slice(half).forEach((f, i) => {
    slide.addText(f, { x: 6.8, y: 3.5 + i * 0.42, w: 6.15, h: 0.38, fontSize: 12, color: CLR.dark, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 4 – USER ROLES OVERVIEW                                           ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('User Roles Overview – 4 Roles in InnoPV');
  roleBox(slide, '👑  Admin', CLR.red, 0.35, 1.15, 3.0, 5.6,
    'Full system administrator access. Manages users, master data, and all cases.',
    [
      '• Full access to all modules',
      '• User creation & management',
      '• Master data management',
      '• Checklist configuration',
      '• Admin dashboard & analytics',
      '• All case operations',
      '• SLA alert management',
      '• All reports & exports',
    ]
  );
  roleBox(slide, '🗂  PV Associate', CLR.blue, 3.6, 1.15, 3.0, 5.6,
    'Primary data entry user. Creates and processes cases.',
    [
      '• Create new PV cases',
      '• Full case data entry',
      '• Duplicate check',
      '• PV Associate checklist',
      '• Submit to PV Manager',
      '• Follow-up management',
      '• Case history & audit trail',
      '• Complete case report (PDF)',
    ]
  );
  roleBox(slide, '📊  PV Manager', CLR.green, 6.85, 1.15, 3.0, 5.6,
    'Supervises case quality and manages regulatory submissions.',
    [
      '• Case review & assignment',
      '• PV Manager checklist',
      '• Forward to Medical Reviewer',
      '• Return to PV Associate',
      '• Regulatory submissions',
      '• SLA alert management',
      '• Case reports & CSV export',
      '• Case closure validation',
    ]
  );
  roleBox(slide, '🩺  Medical Reviewer', CLR.amber, 10.1, 1.15, 3.0, 5.6,
    'Performs medical review and approval of cases.',
    [
      '• Medical review of cases',
      '• Medical Reviewer checklist',
      '• Medically approve cases',
      '• Return cases to PV Manager',
      '• View & enter follow-ups',
      '• Regulatory submissions',
      '• Complete case reports',
      '• Case history & audit trail',
    ]
  );
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 5 – LOGIN & AUTHENTICATION                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Login & Authentication', CLR.navy);

  // Left panel – login steps
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 6.0, h: 5.7, fill: { color: CLR.light } });
  slide.addText('How to Login', { x: 0.5, y: 1.2, w: 5.7, h: 0.4, fontSize: 15, bold: true, color: CLR.navy, fontFace: 'Calibri' });
  const loginSteps = [
    'Open InnoPV in your web browser',
    'Enter your registered Email address',
    'Enter your Password',
    'Optionally check "Remember Me"',
    'Click the Login button',
    'Redirected to role-specific Dashboard',
    'On first login: forced password change before any access',
  ];
  loginSteps.forEach((s, i) => {
    slide.addShape(prs.ShapeType.ellipse, { x: 0.5, y: 1.72 + i * 0.66, w: 0.28, h: 0.28, fill: { color: CLR.blue } });
    slide.addText(`${i + 1}`, { x: 0.5, y: 1.72 + i * 0.66, w: 0.28, h: 0.28, fontSize: 9, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s, { x: 0.88, y: 1.72 + i * 0.66, w: 5.2, h: 0.3, fontSize: 12, color: CLR.dark, fontFace: 'Calibri', valign: 'middle' });
  });

  // Right panel – security
  slide.addShape(prs.ShapeType.rect, { x: 6.7, y: 1.1, w: 6.3, h: 5.7, fill: { color: 'FEF9E7' } });
  slide.addText('Security Features', { x: 6.85, y: 1.2, w: 6.0, h: 0.4, fontSize: 15, bold: true, color: CLR.navy, fontFace: 'Calibri' });
  const security = [
    ['Account Lockout', 'Locked after multiple wrong password attempts'],
    ['Inactive Account', 'Blocked login with admin contact message'],
    ['First Login', 'Mandatory password change on first login'],
    ['CSRF Protection', 'All forms protected against CSRF attacks'],
    ['Password Policy', 'New password must differ from current'],
    ['Forgot Password', 'Self-service via email reset link'],
    ['Session Security', 'Cookie-based; expires on logout'],
    ['Remember Me', 'Persistent session option on trusted devices'],
  ];
  security.forEach((s, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: 6.85, y: 1.72 + i * 0.6, w: 2.1, h: 0.28, fill: { color: CLR.amber }, rectRadius: 0.05 });
    slide.addText(s[0], { x: 6.85, y: 1.72 + i * 0.6, w: 2.1, h: 0.28, fontSize: 9, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s[1], { x: 9.05, y: 1.72 + i * 0.6, w: 3.8, h: 0.28, fontSize: 10, color: CLR.dark, fontFace: 'Calibri', valign: 'middle' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 6 – DASHBOARD                                                     ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Dashboard – Role-Specific Views');
  const dashboards = [
    { role: 'Admin Dashboard', color: CLR.red, x: 0.35, y: 1.15,
      items: ['Total / Open / Closed cases', 'Cases by status (chart)', 'Total & active users', 'Users by role (chart)', 'Recent 8 cases & users', 'Overdue case count'] },
    { role: 'PV Manager Dashboard', color: CLR.green, x: 3.6, y: 1.15,
      items: ['Cases pending review', 'Forwarded cases', 'Overdue & SLA status', 'Monthly case trends', 'Serious / Non-Serious split', 'Valid / Invalid split'] },
    { role: 'PV Associate Dashboard', color: CLR.blue, x: 6.85, y: 1.15,
      items: ['My assigned cases', 'Cases by status', 'Draft cases pending', 'Cases due in 7 days', 'Returned cases pending'] },
    { role: 'Medical Reviewer', color: CLR.amber, x: 10.1, y: 1.15,
      items: ['Cases pending review', 'Medically approved', 'Returned to PV Mgr', 'Review SLA status'] },
  ];
  dashboards.forEach(d => {
    roleBox(slide, d.role, d.color, d.x, d.y, 3.0, 3.5, null, d.items.map(i => '• ' + i));
  });

  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 5.0, w: 12.6, h: 1.7, fill: { color: CLR.light } });
  slide.addText('🔍  PV Analytics Dashboard (All Roles)', { x: 0.5, y: 5.05, w: 12.3, h: 0.35, fontSize: 13, bold: true, color: CLR.navy, fontFace: 'Calibri' });
  const analytics = [
    'Total / Open / Closed cases', 'Serious vs Non-Serious', 'Valid vs Invalid', 'Overdue cases count',
    'Due Soon (next 7 days)', 'Monthly receipt trend (12 months)', 'Cases by assigned role',
  ];
  analytics.forEach((a, i) => {
    const col = i < 4 ? 0 : 1;
    const row = i < 4 ? i : i - 4;
    slide.addText('✓  ' + a, { x: 0.5 + col * 6.5, y: 5.45 + row * 0.35, w: 6.2, h: 0.3, fontSize: 11.5, color: CLR.dark, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 7 – CASE INBOX                                                    ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Case Inbox – Your Personalized Case Queue');
  slide.addText('The Case Inbox shows only cases relevant to your role and assignment. Each role sees different statuses.', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.45, fontSize: 12.5, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });

  const inboxData = [
    { role: 'Admin', color: CLR.red, statuses: ['ALL cases in the system', 'No filter applied', 'Complete visibility'] },
    { role: 'PV Associate', color: CLR.blue, statuses: ['Draft', 'DataEntryInProgress', 'ValidityPending', 'InvalidFollowUpRequired', 'DuplicateCheckPending', 'PvAssociateChecklistPending', 'ReturnedByPvManager', 'AdditionalInformationRequired', 'Reopened'] },
    { role: 'PV Manager', color: CLR.green, statuses: ['SubmittedToPvManager', 'PvManagerReviewPending', 'PvManagerChecklistPending', 'ResubmittedToPvManager', 'ReturnedByMedicalReviewer'] },
    { role: 'Medical Reviewer', color: CLR.amber, statuses: ['ForwardedToMedicalReviewer', 'MedicalReviewPending', 'MedicalReviewerChecklistPending', 'MedicallyApproved'] },
  ];

  inboxData.forEach((d, i) => {
    const x = 0.35 + i * 3.25;
    slide.addShape(prs.ShapeType.roundRect, { x, y: 1.65, w: 3.1, h: 0.38, fill: { color: d.color }, rectRadius: 0.1 });
    slide.addText(d.role, { x, y: 1.65, w: 3.1, h: 0.38, fontSize: 12, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addShape(prs.ShapeType.rect, { x, y: 2.03, w: 3.1, h: 4.55, fill: { color: CLR.offWhite } });
    slide.addShape(prs.ShapeType.rect, { x, y: 2.03, w: 0.06, h: 4.55, fill: { color: d.color } });
    d.statuses.forEach((s, j) => {
      slide.addText('• ' + s, { x: x + 0.14, y: 2.1 + j * 0.46, w: 2.9, h: 0.38, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri', wrap: true });
    });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 8 – CASE CREATION / INTAKE                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('PV Case Creation – Intake Form', CLR.blue);
  slide.addText('Access: Admin & PV Associate only  |  Navigate to: PV Cases → New Case', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.35, fontSize: 11, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });

  // Left – fields
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.55, w: 6.5, h: 4.5, fill: { color: CLR.light } });
  slide.addText('Intake Form Fields', { x: 0.5, y: 1.65, w: 6.2, h: 0.35, fontSize: 13, bold: true, color: CLR.navy, fontFace: 'Calibri' });
  const fields = [
    'Case Source (dropdown)', 'Receipt Date (≤ today)',
    'Initial Reporter Name', 'Patient Identifier',
    'Suspect Product Name', 'Adverse Event Term',
    'Is Patient Identifiable? (Y/N)', 'Is Reporter Identifiable? (Y/N)',
    'Suspect Product Available? (Y/N)', 'Adverse Event Available? (Y/N)',
    'Is Serious? (Y/N)', 'Narrative (free text)',
  ];
  fields.forEach((f, i) => {
    const col = i % 2;
    const row = Math.floor(i / 2);
    slide.addText('▸ ' + f, { x: 0.5 + col * 3.1, y: 2.1 + row * 0.5, w: 3.0, h: 0.42, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });

  // Right – logic
  slide.addShape(prs.ShapeType.rect, { x: 7.1, y: 1.55, w: 5.9, h: 2.1, fill: { color: 'EAFAF1' } });
  slide.addText('✓  Validity Logic', { x: 7.25, y: 1.65, w: 5.6, h: 0.35, fontSize: 13, bold: true, color: CLR.green, fontFace: 'Calibri' });
  slide.addText('A case is VALID if ALL 4 criteria are met:\n  • Patient Identifiable\n  • Reporter Identifiable\n  • Suspect Product Available\n  • Adverse Event Available', {
    x: 7.25, y: 2.08, w: 5.6, h: 1.4, fontSize: 11.5, color: CLR.dark, fontFace: 'Calibri',
  });

  slide.addShape(prs.ShapeType.rect, { x: 7.1, y: 3.85, w: 5.9, h: 2.2, fill: { color: 'FEF9E7' } });
  slide.addText('📅  Due Date Calculation', { x: 7.25, y: 3.95, w: 5.6, h: 0.35, fontSize: 13, bold: true, color: CLR.amber, fontFace: 'Calibri' });

  slide.addShape(prs.ShapeType.roundRect, { x: 7.25, y: 4.4, w: 2.6, h: 0.8, fill: { color: CLR.red }, rectRadius: 0.1 });
  slide.addText('⚡ SERIOUS\nReceipt Date + 7 Days', { x: 7.25, y: 4.4, w: 2.6, h: 0.8, fontSize: 10.5, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
  slide.addShape(prs.ShapeType.roundRect, { x: 10.25, y: 4.4, w: 2.6, h: 0.8, fill: { color: CLR.green }, rectRadius: 0.1 });
  slide.addText('📋 NON-SERIOUS\nReceipt Date + 15 Days', { x: 10.25, y: 4.4, w: 2.6, h: 0.8, fontSize: 10.5, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });

  slide.addText('Case No is auto-generated. Status = DataEntryInProgress (valid) or InvalidFollowUpRequired.', {
    x: 7.25, y: 5.4, w: 5.6, h: 0.5, fontSize: 10, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 9 – CASE DATA ENTRY                                               ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Case Data Entry – 7 Sections');
  slide.addText('After intake, all case data is entered across 7 sections. All roles can VIEW data. Edit allowed only while case is in active processing states.', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.45, fontSize: 12, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });

  const sections = [
    { num: '1', name: 'Patient Details', color: CLR.blue, items: 'Initials, ID, DOB, Age, Gender, Weight, Height, Pregnancy, Medical History, Allergies' },
    { num: '2', name: 'Reporter Details', color: CLR.cyan, items: 'Name, Type, Organization, Contact (Phone/Email/Fax), Is Prescriber?' },
    { num: '3', name: 'Suspect Product', color: CLR.green, items: 'Product, Batch No, Indication, Route, Dose, Frequency, Start/Stop Date, Action Taken, Rechallenge' },
    { num: '4', name: 'Adverse Event', color: CLR.red, items: 'Event Term, Description, Onset/Resolution Date, Outcome, Seriousness, Causality, Severity' },
    { num: '5', name: 'Concomitant Meds', color: CLR.amber, items: 'Drug Name, Indication, Dose, Route, Frequency, Start/Stop Date' },
    { num: '6', name: 'Lab Details', color: '8E44AD', items: 'Test Name, Date, Result Value, Unit, Normal Range, Interpretation' },
    { num: '7', name: 'Attachments', color: CLR.navy, items: 'Upload supporting documents. File type, original name, uploader, date recorded securely.' },
  ];

  sections.forEach((s, i) => {
    const col = i < 4 ? 0 : 1;
    const row = i < 4 ? i : i - 4;
    const x = col === 0 ? 0.35 : 6.85;
    const y = 1.7 + row * 1.32;
    slide.addShape(prs.ShapeType.roundRect, { x, y, w: 0.38, h: 0.38, fill: { color: s.color }, rectRadius: 0.05 });
    slide.addText(s.num, { x, y, w: 0.38, h: 0.38, fontSize: 13, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s.name, { x: x + 0.48, y, w: 5.5, h: 0.38, fontSize: 12, bold: true, color: s.color, fontFace: 'Calibri', valign: 'middle' });
    slide.addText(s.items, { x: x + 0.48, y: y + 0.38, w: 5.6, h: 0.84, fontSize: 10, color: CLR.dark, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 10 – CASE WORKFLOW                                                ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Case Workflow – Step by Step');
  slide.addText('Complete lifecycle from creation to closure across 4 roles', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.35, fontSize: 12, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });

  const steps = [
    { step: '1', label: 'Intake', who: 'PV Assoc', color: CLR.blue, x: 0.2, y: 1.6 },
    { step: '2', label: 'Data Entry', who: 'PV Assoc', color: CLR.blue, x: 1.65, y: 1.6 },
    { step: '3', label: 'Duplicate\nCheck', who: 'PV Assoc', color: CLR.blue, x: 3.1, y: 1.6 },
    { step: '4', label: 'Assoc.\nChecklist', who: 'PV Assoc', color: CLR.blue, x: 4.55, y: 1.6 },
    { step: '5', label: 'Submit to\nPV Manager', who: 'PV Assoc', color: CLR.blue, x: 6.0, y: 1.6 },
    { step: '6', label: 'Mgr Review &\nChecklist', who: 'PV Mgr', color: CLR.green, x: 7.6, y: 1.6 },
    { step: '7', label: 'Forward to\nMed.Rev.', who: 'PV Mgr', color: CLR.green, x: 9.2, y: 1.6 },
    { step: '8', label: 'Medical\nReview', who: 'Med.Rev', color: CLR.amber, x: 10.8, y: 1.6 },
  ];

  steps.forEach((s, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: s.x, y: s.y, w: 1.3, h: 1.1, fill: { color: s.color }, rectRadius: 0.12 });
    slide.addText(s.step, { x: s.x, y: s.y + 0.05, w: 1.3, h: 0.3, fontSize: 10, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s.label, { x: s.x, y: s.y + 0.3, w: 1.3, h: 0.5, fontSize: 9.5, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s.who, { x: s.x, y: s.y + 0.78, w: 1.3, h: 0.28, fontSize: 8.5, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    if (i < steps.length - 1) {
      slide.addShape(prs.ShapeType.line, { x: s.x + 1.3, y: s.y + 0.55, w: 0.35, h: 0, line: { color: '95A5A6', width: 1.5 } });
    }
  });

  // Second row (approval / closure)
  const steps2 = [
    { step: '9', label: 'Approve / Return', who: 'Med.Rev', color: CLR.amber, x: 0.2, y: 3.2 },
    { step: '10', label: 'Finalize Case', who: 'PV Mgr', color: CLR.green, x: 2.2, y: 3.2 },
    { step: '11', label: 'Reg. Submission', who: 'PV Mgr / Med', color: '8E44AD', x: 4.2, y: 3.2 },
    { step: '12', label: 'Case Closure', who: 'Admin / Mgr', color: CLR.navy, x: 6.4, y: 3.2 },
  ];
  steps2.forEach((s, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: s.x, y: s.y, w: 1.8, h: 1.2, fill: { color: s.color }, rectRadius: 0.12 });
    slide.addText(s.step, { x: s.x, y: s.y + 0.05, w: 1.8, h: 0.3, fontSize: 10, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s.label, { x: s.x, y: s.y + 0.3, w: 1.8, h: 0.5, fontSize: 10, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s.who, { x: s.x, y: s.y + 0.82, w: 1.8, h: 0.28, fontSize: 9, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    if (i < steps2.length - 1) {
      slide.addShape(prs.ShapeType.line, { x: s.x + 1.8, y: s.y + 0.6, w: 0.4, h: 0, line: { color: '95A5A6', width: 1.5 } });
    }
  });

  // Return paths
  slide.addShape(prs.ShapeType.rect, { x: 8.5, y: 3.0, w: 4.6, h: 3.7, fill: { color: CLR.light } });
  slide.addText('Return Paths', { x: 8.65, y: 3.1, w: 4.3, h: 0.35, fontSize: 12, bold: true, color: CLR.navy, fontFace: 'Calibri' });
  const returns = [
    '↩  PV Manager → Return to PV Associate\n     with correction comments',
    '↩  Medical Reviewer → Return to PV Manager\n     with medical review comments',
    '↩  Admin can reopen closed cases',
    '✘  Cases can be marked as Duplicate\n     or Invalid at any stage (by authorized users)',
  ];
  returns.forEach((r, i) => {
    slide.addText(r, { x: 8.65, y: 3.55 + i * 0.82, w: 4.3, h: 0.72, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 11 – CHECKLISTS                                                   ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Quality Checklists – Role-Based Quality Gates', CLR.green);
  slide.addText('Checklists are mandatory quality gates at each workflow stage. Configured by Admin. All mandatory items must be checked before advancing.', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.45, fontSize: 12, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });

  const checks = [
    { role: 'PV Associate\nChecklist', color: CLR.blue, stage: 'Before submit to PV Manager', items: ['Data completeness verification', 'Validity criteria confirmed', 'Duplicate check done', 'All mandatory sections filled', 'Narrative complete'] },
    { role: 'PV Manager\nChecklist', color: CLR.green, stage: 'Before forwarding to Medical Reviewer', items: ['Case data quality review', 'Causality assessment reviewed', 'Seriousness criteria verified', 'Regulatory timeline compliance', 'Documentation complete'] },
    { role: 'Medical Reviewer\nChecklist', color: CLR.amber, stage: 'Before medical approval', items: ['Medical accuracy verified', 'Diagnoses and terms reviewed', 'Clinical narrative reviewed', 'Causality assessment confirmed', 'Approval criteria met'] },
  ];

  checks.forEach((c, i) => {
    const x = 0.35 + i * 4.35;
    slide.addShape(prs.ShapeType.roundRect, { x, y: 1.65, w: 4.1, h: 0.45, fill: { color: c.color }, rectRadius: 0.1 });
    slide.addText(c.role, { x, y: 1.65, w: 4.1, h: 0.45, fontSize: 12, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addShape(prs.ShapeType.rect, { x, y: 2.1, w: 4.1, h: 0.38, fill: { color: CLR.light } });
    slide.addText('Stage: ' + c.stage, { x: x + 0.1, y: 2.15, w: 3.9, h: 0.3, fontSize: 9.5, color: c.color, bold: true, fontFace: 'Calibri' });
    slide.addShape(prs.ShapeType.rect, { x, y: 2.48, w: 4.1, h: 3.1, fill: { color: CLR.offWhite } });
    c.items.forEach((item, j) => {
      slide.addShape(prs.ShapeType.roundRect, { x: x + 0.1, y: 2.58 + j * 0.55, w: 0.28, h: 0.28, fill: { color: c.color }, rectRadius: 0.04 });
      slide.addText('✓', { x: x + 0.1, y: 2.58 + j * 0.55, w: 0.28, h: 0.28, fontSize: 10, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
      slide.addText(item, { x: x + 0.46, y: 2.58 + j * 0.55, w: 3.55, h: 0.42, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
    });
  });

  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 5.85, w: 12.6, h: 0.85, fill: { color: 'FEF9E7' } });
  slide.addText('⚙  Checklist Versioning: Admin can create new versions with an Effective From date. System always uses the latest active version. Historical responses are preserved with the checklist version for full audit compliance.', {
    x: 0.5, y: 5.9, w: 12.3, h: 0.75, fontSize: 11, color: CLR.dark, fontFace: 'Calibri',
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 12 – FOLLOW-UP, DUPLICATE, HISTORY                               ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Follow-Up, Duplicate Check & Case History');

  // Follow-Up
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 4.15, h: 5.7, fill: { color: CLR.light } });
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 4.15, h: 0.4, fill: { color: CLR.blue } });
  slide.addText('📩  Case Follow-Up', { x: 0.45, y: 1.13, w: 3.95, h: 0.35, fontSize: 12, bold: true, color: CLR.white, fontFace: 'Calibri' });
  const fuItems = [
    'All 4 roles can add follow-ups',
    'Auto-numbered: FU-001, FU-002...',
    'Fields: Receipt Date, Source, Received From, Description',
    'Cannot add to Duplicate/Invalid cases',
    'Mark as Processed with remarks when case updated',
    'Full follow-up history preserved',
  ];
  fuItems.forEach((f, i) => {
    slide.addText('▸ ' + f, { x: 0.45, y: 1.62 + i * 0.73, w: 3.95, h: 0.65, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });

  // Duplicate Check
  slide.addShape(prs.ShapeType.rect, { x: 4.75, y: 1.1, w: 4.15, h: 5.7, fill: { color: CLR.offWhite } });
  slide.addShape(prs.ShapeType.rect, { x: 4.75, y: 1.1, w: 4.15, h: 0.4, fill: { color: '8E44AD' } });
  slide.addText('🔍  Duplicate Check', { x: 4.85, y: 1.13, w: 3.95, h: 0.35, fontSize: 12, bold: true, color: CLR.white, fontFace: 'Calibri' });
  const dcItems = [
    'Access: Admin, PV Associate, PV Manager',
    'System matches: Patient ID + Product + Event',
    'Reviews potential duplicate case list',
    'View each candidate case before decision',
    'Mark as Duplicate → links to original case',
    '"No Duplicate" → proceed to checklist stage',
    'Duplicate cases archived, not deleted',
  ];
  dcItems.forEach((d, i) => {
    slide.addText('▸ ' + d, { x: 4.85, y: 1.62 + i * 0.73, w: 3.95, h: 0.65, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });

  // History & Audit
  slide.addShape(prs.ShapeType.rect, { x: 9.15, y: 1.1, w: 4.15, h: 5.7, fill: { color: CLR.light } });
  slide.addShape(prs.ShapeType.rect, { x: 9.15, y: 1.1, w: 4.15, h: 0.4, fill: { color: CLR.navy } });
  slide.addText('📜  Case History & Audit', { x: 9.25, y: 1.13, w: 3.95, h: 0.35, fontSize: 12, bold: true, color: CLR.white, fontFace: 'Calibri' });
  const histItems = [
    'All roles can view case history',
    'Workflow Comments: action, from/to role, timestamp',
    'Every field change recorded in Audit Trail',
    'Old value → New value tracked',
    'Performed by + exact timestamp',
    '21 CFR Part 11 compliant audit trail',
    'Cannot be edited or deleted',
  ];
  histItems.forEach((h, i) => {
    slide.addText('▸ ' + h, { x: 9.25, y: 1.62 + i * 0.73, w: 3.95, h: 0.65, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 13 – REGULATORY SUBMISSION                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Regulatory Submission Management', '8E44AD');
  slide.addText('Access: Admin, PV Manager, Medical Reviewer  |  After case is MedicallyApproved / CaseFinalized', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.35, fontSize: 11, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });

  // Fields
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.55, w: 6.5, h: 5.1, fill: { color: CLR.light } });
  slide.addText('Submission Fields', { x: 0.5, y: 1.65, w: 6.2, h: 0.35, fontSize: 13, bold: true, color: CLR.navy, fontFace: 'Calibri' });
  const submFields = [
    ['Submission Type', '15-Day Expedited, Periodic, Annual Safety'],
    ['Recipient Authority', 'CDSCO, FDA, EMA, WHO, NCA'],
    ['Submission Format', 'E2B(XML), MedWatch, CIOMS, NCA Form'],
    ['Due Date', 'Regulatory submission deadline'],
    ['Submitted Date', 'Actual date submission was sent'],
    ['Acknowledgement Date', 'Date authority confirmed receipt'],
    ['Reference No', 'Authority acknowledgement reference'],
    ['File Attachment', 'Upload submission document'],
    ['Remarks', 'Additional notes'],
  ];
  submFields.forEach((f, i) => {
    const bg = i % 2 === 0 ? CLR.offWhite : CLR.white;
    slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 2.1 + i * 0.48, w: 6.5, h: 0.46, fill: { color: bg } });
    slide.addText(f[0], { x: 0.45, y: 2.13 + i * 0.48, w: 2.3, h: 0.38, fontSize: 10.5, bold: true, color: CLR.navy, fontFace: 'Calibri' });
    slide.addText(f[1], { x: 2.85, y: 2.13 + i * 0.48, w: 3.9, h: 0.38, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });

  // Status flow
  slide.addShape(prs.ShapeType.rect, { x: 7.1, y: 1.55, w: 5.9, h: 5.1, fill: { color: CLR.offWhite } });
  slide.addText('Submission Status Flow', { x: 7.25, y: 1.65, w: 5.6, h: 0.35, fontSize: 13, bold: true, color: CLR.navy, fontFace: 'Calibri' });
  const statusFlow = [
    { s: 'Submission Pending', c: CLR.amber, desc: 'Record created, not yet submitted' },
    { s: 'Submitted', c: CLR.blue, desc: 'Sent to regulatory authority' },
    { s: 'Acknowledgement Pending', c: '8E44AD', desc: 'Awaiting authority confirmation' },
    { s: 'Acknowledgement Received', c: CLR.green, desc: 'Authority confirmed receipt' },
    { s: 'Rejected', c: CLR.red, desc: 'Rejected; requires resubmission' },
    { s: 'Cancelled', c: '7F8C8D', desc: 'Submission cancelled' },
  ];
  statusFlow.forEach((sf, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: 7.25, y: 2.15 + i * 0.72, w: 2.4, h: 0.38, fill: { color: sf.c }, rectRadius: 0.08 });
    slide.addText(sf.s, { x: 7.25, y: 2.15 + i * 0.72, w: 2.4, h: 0.38, fontSize: 9.5, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    if (i < statusFlow.length - 1) {
      slide.addShape(prs.ShapeType.line, { x: 8.45, y: 2.53 + i * 0.72, w: 0, h: 0.34, line: { color: '95A5A6', width: 1.5 } });
    }
    slide.addText(sf.desc, { x: 9.75, y: 2.2 + i * 0.72, w: 3.2, h: 0.3, fontSize: 10, color: CLR.dark, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 14 – REPORTS & SLA ALERTS                                         ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Reports & SLA Alerts');

  // Case Report
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 6.0, h: 5.7, fill: { color: CLR.light } });
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 6.0, h: 0.42, fill: { color: CLR.navy } });
  slide.addText('📊  Reports (Admin + PV Manager)', { x: 0.5, y: 1.13, w: 5.8, h: 0.38, fontSize: 12, bold: true, color: CLR.white, fontFace: 'Calibri' });

  slide.addShape(prs.ShapeType.rect, { x: 0.5, y: 1.62, w: 5.7, h: 0.32, fill: { color: CLR.blue } });
  slide.addText('Case Report', { x: 0.6, y: 1.65, w: 5.5, h: 0.28, fontSize: 11, bold: true, color: CLR.white, fontFace: 'Calibri' });
  ['Filter by: date range, status, seriousness, validity, product, event',
   'Summary: Total, Valid, Invalid, Serious, Non-Serious, Closed, Overdue',
   'Export to CSV (timestamped filename)'].forEach((t, i) => {
    slide.addText('• ' + t, { x: 0.55, y: 2.0 + i * 0.4, w: 5.65, h: 0.36, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });

  slide.addShape(prs.ShapeType.rect, { x: 0.5, y: 3.25, w: 5.7, h: 0.32, fill: { color: '8E44AD' } });
  slide.addText('Regulatory Submission Report', { x: 0.6, y: 3.28, w: 5.5, h: 0.28, fontSize: 11, bold: true, color: CLR.white, fontFace: 'Calibri' });
  ['Filter by: Case No, type, authority, status, date range, overdue',
   'Stats: Total, Pending, Submitted, Ack.Pending, Received, Overdue, Rejected, Cancelled',
   'Export to CSV (timestamped filename)'].forEach((t, i) => {
    slide.addText('• ' + t, { x: 0.55, y: 3.65 + i * 0.4, w: 5.65, h: 0.36, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });

  slide.addShape(prs.ShapeType.rect, { x: 0.5, y: 4.9, w: 5.7, h: 0.32, fill: { color: CLR.green } });
  slide.addText('Complete Case Report (All Roles)', { x: 0.6, y: 4.93, w: 5.5, h: 0.28, fontSize: 11, bold: true, color: CLR.white, fontFace: 'Calibri' });
  ['Export full case details in PDF, Word (DOCX), or Excel (XLSX)',
   'Includes all sections: patient, reporter, product, event, labs, etc.'].forEach((t, i) => {
    slide.addText('• ' + t, { x: 0.55, y: 5.3 + i * 0.4, w: 5.65, h: 0.36, fontSize: 10.5, color: CLR.dark, fontFace: 'Calibri' });
  });

  // SLA Alerts
  slide.addShape(prs.ShapeType.rect, { x: 6.7, y: 1.1, w: 6.3, h: 5.7, fill: { color: 'FEF9E7' } });
  slide.addShape(prs.ShapeType.rect, { x: 6.7, y: 1.1, w: 6.3, h: 0.42, fill: { color: CLR.amber } });
  slide.addText('⏰  SLA Alerts (Admin + PV Manager)', { x: 6.85, y: 1.13, w: 6.1, h: 0.38, fontSize: 12, bold: true, color: CLR.white, fontFace: 'Calibri' });

  const alertBoxes = [
    { label: 'Due Soon Alerts', color: CLR.amber, desc: 'Cases due within next 3 days.\nSends reminder email to assigned user + PV Managers.' },
    { label: 'Overdue Alerts', color: CLR.red, desc: 'Cases whose due date has passed.\nSends escalation email for urgent action.' },
    { label: 'Send All Alerts', color: CLR.navy, desc: 'Processes both Due Soon and Overdue in a single batch operation.' },
  ];
  alertBoxes.forEach((a, i) => {
    slide.addShape(prs.ShapeType.roundRect, { x: 6.85, y: 1.65 + i * 1.55, w: 5.9, h: 1.35, fill: { color: 'EBF5FB' }, line: { color: a.color, width: 1 }, rectRadius: 0.12 });
    slide.addShape(prs.ShapeType.rect, { x: 6.85, y: 1.65 + i * 1.55, w: 0.08, h: 1.35, fill: { color: a.color } });
    slide.addText(a.label, { x: 7.05, y: 1.72 + i * 1.55, w: 5.55, h: 0.36, fontSize: 13, bold: true, color: a.color, fontFace: 'Calibri' });
    slide.addText(a.desc, { x: 7.05, y: 2.1 + i * 1.55, w: 5.55, h: 0.75, fontSize: 11, color: CLR.dark, fontFace: 'Calibri' });
  });

  slide.addShape(prs.ShapeType.rect, { x: 6.7, y: 6.3, w: 6.3, h: 0.5, fill: { color: CLR.light } });
  slide.addText('✉  Email alerts include: Case No, Product, Event, Due Date, Current Status, Assigned User', {
    x: 6.85, y: 6.35, w: 6.1, h: 0.4, fontSize: 10, color: CLR.navy, fontFace: 'Calibri',
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 15 – ADMIN PANEL                                                  ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Admin Panel – User & System Management', CLR.red);
  slide.addText('All Admin panel modules are accessible ONLY to the Admin role', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.35, fontSize: 11.5, color: CLR.red, fontFace: 'Calibri', bold: true,
  });

  const adminBoxes = [
    { title: '👤  User Management', color: CLR.red, x: 0.35, y: 1.55, items: [
      'View all users with role, status, lockout info',
      'Create users (auto temp password)',
      'Send credentials by email to new user',
      'Edit: name, mobile, designation, role',
      'Activate / Deactivate user accounts',
      'Unlock locked accounts',
      'Reset user passwords',
    ]},
    { title: '📋  Master Data', color: CLR.blue, x: 4.55, y: 1.55, items: [
      'Common Masters: Reporter Type, Outcome, Route, Frequency, Causality, Severity, Case Source, Action Taken',
      'Product Master: Product catalogue with details',
      'Study Master: Clinical study records',
      'Sponsor Master: Pharma sponsor information',
      'All masters: Create, Edit, Activate/Deactivate',
    ]},
    { title: '✅  Checklist Management', color: CLR.green, x: 8.75, y: 1.55, items: [
      'Create Checklist Master per Role + Stage',
      'Set version number & effective date',
      'Add/Edit/Reorder checklist items',
      'Mark items as Mandatory or Optional',
      'Version control: maintain history',
      'Activate/Deactivate checklist versions',
    ]},
  ];

  adminBoxes.forEach(box => {
    slide.addShape(prs.ShapeType.rect, { x: box.x, y: box.y, w: 4.0, h: 5.15, fill: { color: CLR.offWhite } });
    slide.addShape(prs.ShapeType.rect, { x: box.x, y: box.y, w: 4.0, h: 0.42, fill: { color: box.color } });
    slide.addText(box.title, { x: box.x + 0.1, y: box.y + 0.06, w: 3.8, h: 0.35, fontSize: 11.5, bold: true, color: CLR.white, fontFace: 'Calibri' });
    box.items.forEach((item, i) => {
      const bg = i % 2 === 0 ? CLR.white : CLR.offWhite;
      const itemH = 0.68;
      slide.addShape(prs.ShapeType.rect, { x: box.x, y: box.y + 0.42 + i * itemH, w: 4.0, h: itemH, fill: { color: bg } });
      slide.addShape(prs.ShapeType.rect, { x: box.x, y: box.y + 0.42 + i * itemH, w: 0.06, h: itemH, fill: { color: box.color } });
      slide.addText(item, { x: box.x + 0.14, y: box.y + 0.46 + i * itemH, w: 3.76, h: itemH - 0.1, fontSize: 10, color: CLR.dark, fontFace: 'Calibri', wrap: true, valign: 'middle' });
    });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 16 – PROFILE & PASSWORD                                           ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Profile & Password Management');

  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 6.0, h: 5.7, fill: { color: CLR.light } });
  slide.addShape(prs.ShapeType.rect, { x: 0.35, y: 1.1, w: 6.0, h: 0.42, fill: { color: CLR.navy } });
  slide.addText('👤  My Profile (All Roles)', { x: 0.5, y: 1.13, w: 5.8, h: 0.38, fontSize: 13, bold: true, color: CLR.white, fontFace: 'Calibri' });
  ['Access via top navigation → My Profile',
   'View: Full Name, Email, Mobile, Designation, Department, Role',
   'Profile is read-only; updates done by Admin',
   'Shows current role assignment',
   'Shows MustChangePassword status',
  ].forEach((t, i) => {
    slide.addText('▸ ' + t, { x: 0.5, y: 1.65 + i * 0.55, w: 5.7, h: 0.48, fontSize: 11, color: CLR.dark, fontFace: 'Calibri' });
  });

  slide.addShape(prs.ShapeType.rect, { x: 0.5, y: 4.5, w: 5.7, h: 0.35, fill: { color: CLR.blue } });
  slide.addText('🔐  Change Password', { x: 0.6, y: 4.53, w: 5.5, h: 0.3, fontSize: 11, bold: true, color: CLR.white, fontFace: 'Calibri' });
  ['Enter Current Password', 'Enter New Password (must differ from current)', 'Confirm New Password', 'Click Change Password'].forEach((s, i) => {
    slide.addText(`${i + 1}. ${s}`, { x: 0.6, y: 4.94 + i * 0.42, w: 5.5, h: 0.38, fontSize: 11, color: CLR.dark, fontFace: 'Calibri' });
  });

  slide.addShape(prs.ShapeType.rect, { x: 6.7, y: 1.1, w: 6.3, h: 5.7, fill: { color: CLR.offWhite } });
  slide.addShape(prs.ShapeType.rect, { x: 6.7, y: 1.1, w: 6.3, h: 0.42, fill: { color: CLR.amber } });
  slide.addText('🔑  Forgot Password (Self-Service)', { x: 6.85, y: 1.13, w: 6.1, h: 0.38, fontSize: 13, bold: true, color: CLR.white, fontFace: 'Calibri' });
  ['Click "Forgot Password?" on login page',
   'Enter your registered email address',
   'A time-limited reset link is sent to your email',
   'Click the link in the email',
   'Enter and confirm new password',
   'Login with the new password',
  ].forEach((s, i) => {
    slide.addShape(prs.ShapeType.ellipse, { x: 6.85, y: 1.68 + i * 0.65, w: 0.28, h: 0.28, fill: { color: CLR.amber } });
    slide.addText(`${i + 1}`, { x: 6.85, y: 1.68 + i * 0.65, w: 0.28, h: 0.28, fontSize: 9, bold: true, color: CLR.white, align: 'center', fontFace: 'Calibri' });
    slide.addText(s, { x: 7.23, y: 1.68 + i * 0.65, w: 5.65, h: 0.6, fontSize: 11.5, color: CLR.dark, fontFace: 'Calibri', valign: 'middle' });
  });

  slide.addShape(prs.ShapeType.rect, { x: 6.85, y: 5.85, w: 5.9, h: 0.75, fill: { color: 'FDEDEC' } });
  slide.addShape(prs.ShapeType.rect, { x: 6.85, y: 5.85, w: 0.06, h: 0.75, fill: { color: CLR.red } });
  slide.addText('⚠  First Login: Forced password change. Cannot access any other page until new password is set.', {
    x: 7.02, y: 5.88, w: 5.65, h: 0.65, fontSize: 11, color: CLR.red, fontFace: 'Calibri', bold: true,
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 17 – ROLE-WISE FEATURE MATRIX                                     ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Role-wise Feature Matrix', CLR.navy);
  slide.addText('✔ = Full Access   Read = View Only   ✘ = No Access', {
    x: 0.35, y: 1.1, w: 12.6, h: 0.35, fontSize: 11, color: CLR.mid, fontFace: 'Calibri', italic: true,
  });

  const matrix = [
    ['Feature', 'Admin', 'PV Assoc.', 'PV Mgr', 'Med.Rev.'],
    ['Login & Profile', '✔', '✔', '✔', '✔'],
    ['Dashboard', '✔ (Admin)', '✔ (Assoc)', '✔ (Mgr)', '✔ (Med)'],
    ['PV Analytics Dashboard', '✔', '✔', '✔', '✔'],
    ['Case Inbox', '✔ All', '✔ Own', '✔ Stage', '✔ Stage'],
    ['Create New Case', '✔', '✔', '✘', '✘'],
    ['Case Data Entry', '✔', '✔', 'Read', 'Read'],
    ['Duplicate Check', '✔', '✔', '✔', '✘'],
    ['PV Assoc. Checklist', '✔', '✔', '✘', '✘'],
    ['PV Manager Checklist', '✔', '✘', '✔', '✘'],
    ['Med. Reviewer Checklist', '✔', '✘', '✘', '✔'],
    ['Case Follow-Up', '✔', '✔', '✔', '✔'],
    ['Case Assignment', '✔', '✘', '✔', '✘'],
    ['Regulatory Submission', '✔', '✘', '✔', '✔'],
    ['Case Closure', '✔', '✘', '✔', '✔'],
    ['Case History & Audit', '✔', '✔', '✔', '✔'],
    ['Case Report (CSV)', '✔', '✘', '✔', '✘'],
    ['Submission Report (CSV)', '✔', '✘', '✔', '✘'],
    ['Complete Case Report', '✔', '✔', '✔', '✔'],
    ['SLA Alerts', '✔', '✘', '✔', '✘'],
    ['User Management', '✔', '✘', '✘', '✘'],
    ['Master Data Management', '✔', '✘', '✘', '✘'],
    ['Checklist Management', '✔', '✘', '✘', '✘'],
  ];

  const colW = [3.8, 2.1, 2.1, 2.1, 2.1];
  const colColors = [CLR.navy, CLR.red, CLR.blue, CLR.green, CLR.amber];
  const startY = 1.55;
  const rowH = 0.24;
  const startX = 0.35;

  // Header
  let cx = startX;
  matrix[0].forEach((h, i) => {
    slide.addShape(prs.ShapeType.rect, { x: cx, y: startY, w: colW[i], h: 0.32, fill: { color: colColors[i] } });
    slide.addText(h, { x: cx + 0.05, y: startY, w: colW[i] - 0.1, h: 0.32, fontSize: 10.5, bold: true, color: CLR.white, align: i === 0 ? 'left' : 'center', fontFace: 'Calibri' });
    cx += colW[i];
  });

  matrix.slice(1).forEach((row, ri) => {
    const y = startY + 0.32 + ri * rowH;
    let cxi = startX;
    row.forEach((cell, ci) => {
      const bg = ri % 2 === 0 ? CLR.offWhite : CLR.white;
      slide.addShape(prs.ShapeType.rect, { x: cxi, y, w: colW[ci], h: rowH, fill: { color: bg } });
      const txtColor = cell === '✔' ? CLR.green : cell === '✘' ? CLR.red : cell === 'Read' ? CLR.amber : CLR.dark;
      const bold = cell === '✔' || cell === '✘';
      slide.addText(cell, { x: cxi + 0.05, y: y + 0.02, w: colW[ci] - 0.1, h: rowH - 0.04, fontSize: ci === 0 ? 10 : 10.5, bold, color: txtColor, align: ci === 0 ? 'left' : 'center', fontFace: 'Calibri' });
      cxi += colW[ci];
    });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 18 – CASE STATUS REFERENCE                                        ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = contentSlide('Case Status Reference', CLR.navy);
  const statuses = [
    ['Draft', '95A5A6', 'Saved but not started'],
    ['DataEntryInProgress', CLR.blue, 'Data entry ongoing'],
    ['ValidityPending', CLR.amber, 'Validity under review'],
    ['InvalidFollowUpRequired', CLR.red, 'Invalid; follow-up needed'],
    ['DuplicateCheckPending', '8E44AD', 'Awaiting duplicate check'],
    ['PvAssociateChecklistPending', CLR.blue, 'PV Assoc completing checklist'],
    ['SubmittedToPvManager', CLR.green, 'Submitted to PV Manager'],
    ['PvManagerReviewPending', CLR.green, 'PV Manager reviewing'],
    ['PvManagerChecklistPending', CLR.green, 'PV Mgr checklist in progress'],
    ['ReturnedByPvManager', CLR.red, 'Returned to PV Associate'],
    ['ResubmittedToPvManager', CLR.blue, 'Resubmitted after corrections'],
    ['ForwardedToMedicalReviewer', CLR.amber, 'Forwarded for medical review'],
    ['MedicalReviewPending', CLR.amber, 'Medical Reviewer reviewing'],
    ['MedicalReviewerChecklistPending', CLR.amber, 'Med.Rev checklist in progress'],
    ['ReturnedByMedicalReviewer', CLR.red, 'Returned to PV Manager'],
    ['MedicallyApproved', CLR.green, 'Medically approved'],
    ['CaseFinalized', CLR.green, 'Finalized, ready for submission'],
    ['SubmissionPending', 'E67E22', 'Regulatory submission pending'],
    ['Submitted', CLR.green, 'Submitted to authority'],
    ['AcknowledgementPending', CLR.amber, 'Awaiting authority ack.'],
    ['CaseClosed', CLR.navy, 'Fully closed'],
    ['Reopened', 'E67E22', 'Previously closed, reopened'],
    ['MarkedAsDuplicate', '7F8C8D', 'Identified as duplicate'],
    ['MarkedAsInvalid', '7F8C8D', 'Marked invalid & archived'],
    ['OnHold', '7F8C8D', 'Placed on hold'],
  ];

  const half = Math.ceil(statuses.length / 2);
  const rowH = 0.225;

  statuses.slice(0, half).forEach((s, i) => {
    const y = 1.15 + i * rowH;
    const bg = i % 2 === 0 ? CLR.offWhite : CLR.white;
    slide.addShape(prs.ShapeType.rect, { x: 0.35, y, w: 6.3, h: rowH, fill: { color: bg } });
    slide.addShape(prs.ShapeType.roundRect, { x: 0.4, y: y + 0.03, w: 0.18, h: 0.18, fill: { color: s[1] }, rectRadius: 0.04 });
    slide.addText(s[0], { x: 0.65, y: y + 0.02, w: 2.8, h: rowH - 0.04, fontSize: 9.5, bold: true, color: '2C3E50', fontFace: 'Calibri' });
    slide.addText(s[2], { x: 3.5, y: y + 0.02, w: 3.1, h: rowH - 0.04, fontSize: 9.5, color: CLR.mid, fontFace: 'Calibri' });
  });

  statuses.slice(half).forEach((s, i) => {
    const y = 1.15 + i * rowH;
    const bg = i % 2 === 0 ? CLR.offWhite : CLR.white;
    slide.addShape(prs.ShapeType.rect, { x: 6.85, y, w: 6.3, h: rowH, fill: { color: bg } });
    slide.addShape(prs.ShapeType.roundRect, { x: 6.9, y: y + 0.03, w: 0.18, h: 0.18, fill: { color: s[1] }, rectRadius: 0.04 });
    slide.addText(s[0], { x: 7.15, y: y + 0.02, w: 2.8, h: rowH - 0.04, fontSize: 9.5, bold: true, color: '2C3E50', fontFace: 'Calibri' });
    slide.addText(s[2], { x: 10.0, y: y + 0.02, w: 3.1, h: rowH - 0.04, fontSize: 9.5, color: CLR.mid, fontFace: 'Calibri' });
  });
}

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  SLIDE 19 – THANK YOU / CLOSING                                          ║
// ╚══════════════════════════════════════════════════════════════════════════╝
{
  const slide = prs.addSlide();
  slide.background = { color: CLR.navy };
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 0, w: 0.18, h: 7.5, fill: { color: CLR.blue } });
  slide.addShape(prs.ShapeType.rect, { x: 0, y: 7.1, w: 13.33, h: 0.4, fill: { color: CLR.blue } });
  slide.addText('InnoPV – Pharmacovigilance Case Management System | Confidential', {
    x: 0, y: 7.1, w: 13.33, h: 0.4, fontSize: 9, color: CLR.white, align: 'center', fontFace: 'Calibri',
  });

  slide.addText([
    { text: 'Inno', options: { color: CLR.blue } },
    { text: 'PV', options: { color: CLR.white } },
  ], { x: 0.5, y: 1.4, w: 8, h: 1.2, fontFace: 'Calibri', fontSize: 52, bold: true });

  slide.addShape(prs.ShapeType.line, { x: 0.5, y: 2.75, w: 9, h: 0, line: { color: CLR.blue, width: 2 } });
  slide.addText('User Manual – Summary', { x: 0.5, y: 2.85, w: 12.5, h: 0.7, fontSize: 28, bold: true, color: CLR.white, fontFace: 'Calibri' });

  const summary = [
    '4 User Roles: Admin, PV Associate, PV Manager, Medical Reviewer',
    '13+ Controllers covering the full PV case lifecycle',
    '26 Case statuses from Draft to Closed',
    '3 Quality checklists (one per role/stage)',
    'Full audit trail & workflow comments',
    'Reports: CSV export, PDF/Word/Excel complete case reports',
    'SLA alert emails for due-soon & overdue cases',
    'Admin panel: Users, Master Data, Checklists',
  ];
  const half = 4;
  summary.slice(0, half).forEach((s, i) => {
    slide.addText('✓  ' + s, { x: 0.5, y: 3.7 + i * 0.5, w: 6.0, h: 0.46, fontSize: 12, color: 'BDC3C7', fontFace: 'Calibri' });
  });
  summary.slice(half).forEach((s, i) => {
    slide.addText('✓  ' + s, { x: 6.8, y: 3.7 + i * 0.5, w: 6.0, h: 0.46, fontSize: 12, color: 'BDC3C7', fontFace: 'Calibri' });
  });
}

// ─── Save ────────────────────────────────────────────────────────────────────
const OUTPUT = 'd:/NewPVSoftware/InnoPV02/docs/InnoPV_User_Manual.pptx';
prs.writeFile({ fileName: OUTPUT }).then(() => {
  console.log('✅ PPT generated: ' + OUTPUT);
}).catch(err => {
  console.error('❌ Error:', err);
});
