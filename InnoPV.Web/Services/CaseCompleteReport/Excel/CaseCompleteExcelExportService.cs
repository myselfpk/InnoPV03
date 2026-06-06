using ClosedXML.Excel;
using InnoPV.Web.Models.CaseCompleteReport;
using System.Net;
using System.Text.RegularExpressions;

namespace InnoPV.Web.Services.CaseCompleteReport.Excel;

public class CaseCompleteExcelExportService
{
    public byte[] Generate(CaseCompleteReportViewModel model)
    {
        using var workbook = new XLWorkbook();

        AddCaseSummarySheet(workbook, model);
        AddPatientSheet(workbook, model);
        AddReporterSheet(workbook, model);
        AddAdverseEventSheet(workbook, model);
        AddSuspectProductSheet(workbook, model);
        AddConcomitantMedicationSheet(workbook, model);
        AddLabDetailsSheet(workbook, model);
        AddNarrativeSheet(workbook, model);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    private static void AddCaseSummarySheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Case Summary");

        AddTitle(ws, "Complete PV Case Report", model.CaseNo);

        var row = 4;

        AddKeyValue(ws, row++, "Case No", model.CaseNo);
        AddKeyValue(ws, row++, "Case Source", model.CaseSource);
        AddKeyValue(ws, row++, "Receipt Date", Dt(model.ReceiptDate));
        AddKeyValue(ws, row++, "Due Date", Dt(model.DueDate));
        AddKeyValue(ws, row++, "Status", model.Status.ToString());
        AddKeyValue(ws, row++, "Validity", model.IsValidCase ? "Valid" : "Invalid");
        AddKeyValue(ws, row++, "Seriousness", model.IsSerious ? "Serious" : "Non-serious");
        AddKeyValue(ws, row++, "Initial Patient Identifier", model.InitialPatientIdentifier);
        AddKeyValue(ws, row++, "Initial Reporter Name", model.InitialReporterName);
        AddKeyValue(ws, row++, "Initial Product Name", model.InitialProductName);
        AddKeyValue(ws, row++, "Initial Event Term", model.InitialEventTerm);
        AddKeyValue(ws, row++, "Initial Narrative", model.InitialNarrative);

        FormatKeyValueSheet(ws);
    }

    private static void AddPatientSheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Patient");

        AddTitle(ws, "Patient Details", model.CaseNo);

        var row = 4;

        AddKeyValue(ws, row++, "Patient Identifier", model.Patient.PatientIdentifier);
        AddKeyValue(ws, row++, "Patient Initials", model.Patient.PatientInitials);
        AddKeyValue(ws, row++, "Date of Birth", Dt(model.Patient.DateOfBirth));
        AddKeyValue(ws, row++, "Age", Join(model.Patient.Age?.ToString(), model.Patient.AgeUnit));
        AddKeyValue(ws, row++, "Gender", model.Patient.Gender);
        AddKeyValue(ws, row++, "Weight", model.Patient.WeightKg.HasValue ? $"{model.Patient.WeightKg} kg" : "-");
        AddKeyValue(ws, row++, "Height", model.Patient.HeightCm.HasValue ? $"{model.Patient.HeightCm} cm" : "-");
        AddKeyValue(ws, row++, "Pregnant", model.Patient.IsPregnant ? "Yes" : "No");
        AddKeyValue(ws, row++, "Pregnancy Remarks", model.Patient.PregnancyRemarks);
        AddKeyValue(ws, row++, "Relevant Medical History", model.Patient.RelevantMedicalHistory);
        AddKeyValue(ws, row++, "Allergy History", model.Patient.AllergyHistory);
        AddKeyValue(ws, row++, "Other Patient Information", model.Patient.OtherPatientInformation);

        FormatKeyValueSheet(ws);
    }

    private static void AddReporterSheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Reporter");

        AddTitle(ws, "Reporter Details", model.CaseNo);

        var row = 4;

        AddKeyValue(ws, row++, "Reporter Name", model.Reporter.ReporterName);
        AddKeyValue(ws, row++, "Reporter Type", model.Reporter.ReporterType);
        AddKeyValue(ws, row++, "Qualification", model.Reporter.Qualification);
        AddKeyValue(ws, row++, "Organization", model.Reporter.OrganizationName);
        AddKeyValue(ws, row++, "Department", model.Reporter.Department);
        AddKeyValue(ws, row++, "Email", model.Reporter.Email);
        AddKeyValue(ws, row++, "Phone", model.Reporter.Phone);
        AddKeyValue(ws, row++, "Address", model.Reporter.Address);
        AddKeyValue(ws, row++, "City", model.Reporter.City);
        AddKeyValue(ws, row++, "State", model.Reporter.State);
        AddKeyValue(ws, row++, "Country", model.Reporter.Country);
        AddKeyValue(ws, row++, "Date of Report", Dt(model.Reporter.DateOfReport));
        AddKeyValue(ws, row++, "Consent for Follow-up", model.Reporter.ConsentForFollowUp ? "Yes" : "No");
        AddKeyValue(ws, row++, "Remarks", model.Reporter.ReporterRemarks);

        FormatKeyValueSheet(ws);
    }

    private static void AddAdverseEventSheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Adverse Event");

        AddTitle(ws, "Adverse Event Details", model.CaseNo);

        var row = 4;

        AddKeyValue(ws, row++, "Event Term", model.AdverseEvent.EventTerm);
        AddKeyValue(ws, row++, "Event Start Date", Dt(model.AdverseEvent.EventStartDate));
        AddKeyValue(ws, row++, "Event End Date", Dt(model.AdverseEvent.EventEndDate));
        AddKeyValue(ws, row++, "Outcome", model.AdverseEvent.Outcome);
        AddKeyValue(ws, row++, "Serious", model.AdverseEvent.IsSerious ? "Yes" : "No");
        AddKeyValue(ws, row++, "Seriousness Criteria", model.AdverseEvent.SeriousnessCriteria);
        AddKeyValue(ws, row++, "Event Description", model.AdverseEvent.EventDescription);
        AddKeyValue(ws, row++, "Treatment Given", model.AdverseEvent.TreatmentGiven);
        AddKeyValue(ws, row++, "Remarks", model.AdverseEvent.EventRemarks);

        FormatKeyValueSheet(ws);
    }

    private static void AddSuspectProductSheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Suspect Product");

        AddTitle(ws, "Suspect Product Details", model.CaseNo);

        var row = 4;

        AddKeyValue(ws, row++, "Product Name", model.SuspectProduct.ProductName);
        AddKeyValue(ws, row++, "Generic Name", model.SuspectProduct.GenericName);
        AddKeyValue(ws, row++, "Batch No", model.SuspectProduct.BatchNo);

        // Agar aapne ExpiryDate ViewModel me add kiya hai, to ye line uncomment kar sakte ho:
        // AddKeyValue(ws, row++, "Expiry Date", Dt(model.SuspectProduct.ExpiryDate));

        AddKeyValue(ws, row++, "Dose", Join(model.SuspectProduct.Dose, model.SuspectProduct.DoseUnit));
        AddKeyValue(ws, row++, "Route", model.SuspectProduct.Route);
        AddKeyValue(ws, row++, "Frequency", model.SuspectProduct.Frequency);
        AddKeyValue(ws, row++, "Therapy Start Date", Dt(model.SuspectProduct.TherapyStartDate));
        AddKeyValue(ws, row++, "Therapy Stop Date", Dt(model.SuspectProduct.TherapyStopDate));
        AddKeyValue(ws, row++, "Indication", model.SuspectProduct.Indication);
        AddKeyValue(ws, row++, "Action Taken", model.SuspectProduct.ActionTaken);
        AddKeyValue(ws, row++, "Dechallenge", model.SuspectProduct.Dechallenge);
        AddKeyValue(ws, row++, "Rechallenge", model.SuspectProduct.Rechallenge);
        AddKeyValue(ws, row++, "Causality Assessment", model.SuspectProduct.CausalityAssessment);
        AddKeyValue(ws, row++, "Remarks", model.SuspectProduct.ProductRemarks);

        FormatKeyValueSheet(ws);
    }

    private static void AddConcomitantMedicationSheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Concomitant Medications");

        AddTitle(ws, "Concomitant Medications", model.CaseNo);

        var headers = new[]
        {
            "Medication Name",
            "Dose",
            "Route",
            "Frequency",
            "Therapy Start Date",
            "Therapy Stop Date",
            "Indication",
            "For Event Treatment",
            "Remarks"
        };

        AddHeaderRow(ws, 4, headers);

        var row = 5;

        if (model.ConcomitantMedications.Any())
        {
            foreach (var item in model.ConcomitantMedications)
            {
                ws.Cell(row, 1).Value = V(item.MedicationName);
                ws.Cell(row, 2).Value = Join(item.Dose, item.DoseUnit);
                ws.Cell(row, 3).Value = V(item.Route);
                ws.Cell(row, 4).Value = V(item.Frequency);
                ws.Cell(row, 5).Value = Dt(item.TherapyStartDate);
                ws.Cell(row, 6).Value = Dt(item.TherapyStopDate);
                ws.Cell(row, 7).Value = V(item.Indication);
                ws.Cell(row, 8).Value = item.IsMedicationForEventTreatment ? "Yes" : "No";
                ws.Cell(row, 9).Value = V(item.Remarks);

                row++;
            }
        }
        else
        {
            ws.Cell(row, 1).Value = "No concomitant medication records available.";
            ws.Range(row, 1, row, headers.Length).Merge();
        }

        FormatTableSheet(ws, headers.Length);
    }

    private static void AddLabDetailsSheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Lab Details");

        AddTitle(ws, "Lab Details", model.CaseNo);

        var headers = new[]
        {
            "Test Name",
            "Test Date",
            "Result",
            "Unit",
            "Normal Range",
            "Clinical Significance",
            "Remarks"
        };

        AddHeaderRow(ws, 4, headers);

        var row = 5;

        if (model.LabDetails.Any())
        {
            foreach (var item in model.LabDetails)
            {
                ws.Cell(row, 1).Value = V(item.TestName);
                ws.Cell(row, 2).Value = Dt(item.TestDate);
                ws.Cell(row, 3).Value = V(item.ResultValue);
                ws.Cell(row, 4).Value = V(item.Unit);
                ws.Cell(row, 5).Value = V(item.NormalRange);
                ws.Cell(row, 6).Value = V(item.ClinicalSignificance);
                ws.Cell(row, 7).Value = V(item.Remarks);

                row++;
            }
        }
        else
        {
            ws.Cell(row, 1).Value = "No lab detail records available.";
            ws.Range(row, 1, row, headers.Length).Merge();
        }

        FormatTableSheet(ws, headers.Length);
    }

    private static void AddNarrativeSheet(XLWorkbook workbook, CaseCompleteReportViewModel model)
    {
        var ws = workbook.Worksheets.Add("Narrative");

        AddTitle(ws, "Case Narrative", model.CaseNo);

        ws.Cell(4, 1).Value = HtmlToText(model.Narrative);

        ws.Range(4, 1, 25, 8).Merge();

        var narrativeCell = ws.Cell(4, 1);
        narrativeCell.Style.Alignment.WrapText = true;
        narrativeCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        narrativeCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        narrativeCell.Style.Fill.BackgroundColor = XLColor.White;

        ws.Row(4).Height = 250;
        ws.Columns().AdjustToContents();

        ws.Column(1).Width = 120;

        ws.SheetView.FreezeRows(3);
    }

    private static void AddTitle(IXLWorksheet ws, string title, string caseNo)
    {
        ws.Cell(1, 1).Value = title;
        ws.Cell(2, 1).Value = $"Case No: {caseNo}";
        ws.Cell(2, 4).Value = $"Generated On: {DateTime.Now:dd-MMM-yyyy HH:mm}";

        ws.Range(1, 1, 1, 6).Merge();

        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 16;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
        ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
        ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Range(2, 1, 2, 6).Style.Font.Bold = true;
        ws.Range(2, 1, 2, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");

        ws.Row(1).Height = 26;
        ws.Row(2).Height = 20;
    }

    private static void AddKeyValue(IXLWorksheet ws, int row, string label, string? value)
    {
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 2).Value = V(value);

        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

        ws.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Range(row, 1, row, 2).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        ws.Range(row, 1, row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        ws.Cell(row, 2).Style.Alignment.WrapText = true;
    }

    private static void AddHeaderRow(IXLWorksheet ws, int row, string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(row, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
    }

    private static void FormatKeyValueSheet(IXLWorksheet ws)
    {
        ws.Column(1).Width = 32;
        ws.Column(2).Width = 90;

        ws.Column(2).Style.Alignment.WrapText = true;

        ws.SheetView.FreezeRows(3);

        ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
        ws.PageSetup.FitToPages(1, 0);
    }

    private static void FormatTableSheet(IXLWorksheet ws, int columnCount)
    {
        var usedRange = ws.RangeUsed();

        if (usedRange != null)
        {
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            usedRange.Style.Alignment.WrapText = true;
        }

        ws.Columns(1, columnCount).AdjustToContents();

        for (var i = 1; i <= columnCount; i++)
        {
            if (ws.Column(i).Width > 35)
            {
                ws.Column(i).Width = 35;
            }
        }

        ws.SheetView.FreezeRows(4);

        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.FitToPages(1, 0);
    }

    private static string V(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string Dt(DateTime? date)
    {
        return date.HasValue ? date.Value.ToString("dd-MMM-yyyy") : "-";
    }

    private static string Dt(DateTime date)
    {
        return date.ToString("dd-MMM-yyyy");
    }

    private static string Join(params string?[] values)
    {
        var result = values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToList();

        return result.Any() ? string.Join(" ", result) : "-";
    }

    private static string HtmlToText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return "Narrative not available.";
        }

        var text = html;

        text = Regex.Replace(text, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</p>", "\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<p.*?>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<li.*?>", "• ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</li>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</ul>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</ol>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<.*?>", string.Empty);

        return WebUtility.HtmlDecode(text).Trim();
    }
}