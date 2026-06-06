using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using InnoPV.Web.Models.CaseCompleteReport;
using System.Net;
using System.Text.RegularExpressions;

namespace InnoPV.Web.Services.CaseCompleteReport.Word;

public class CaseCompleteWordExportService
{
    public byte[] Generate(CaseCompleteReportViewModel model)
    {
        using var stream = new MemoryStream();

        using (var wordDocument = WordprocessingDocument.Create(
                   stream,
                   WordprocessingDocumentType.Document,
                   true))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();

            var body = new Body();

            body.Append(Title("InnoPV — Complete PV Case Report"));
            body.Append(Para($"Case No: {model.CaseNo}"));
            body.Append(Para($"Status: {model.Status}"));
            body.Append(Para($"Generated On: {DateTime.Now:dd-MMM-yyyy HH:mm}"));

            body.Append(SectionHeading("Case Summary"));
            body.Append(KeyValueTable(new Dictionary<string, string?>
            {
                ["Case No"] = model.CaseNo,
                ["Case Source"] = model.CaseSource,
                ["Receipt Date"] = Dt(model.ReceiptDate),
                ["Due Date"] = Dt(model.DueDate),
                ["Validity"] = model.IsValidCase ? "Valid" : "Invalid",
                ["Seriousness"] = model.IsSerious ? "Serious" : "Non-serious",
                ["Product"] = model.InitialProductName,
                ["Event"] = model.InitialEventTerm,
                ["Initial Narrative"] = model.InitialNarrative,
            }));

            body.Append(SectionHeading("1. Patient Details"));
            body.Append(KeyValueTable(new Dictionary<string, string?>
            {
                ["Patient Identifier"] = model.Patient.PatientIdentifier,
                ["Patient Initials"] = model.Patient.PatientInitials,
                ["Date of Birth"] = Dt(model.Patient.DateOfBirth),
                ["Age"] = Join(model.Patient.Age?.ToString(), model.Patient.AgeUnit),
                ["Gender"] = model.Patient.Gender,
                ["Weight"] = model.Patient.WeightKg.HasValue ? $"{model.Patient.WeightKg} kg" : "-",
                ["Height"] = model.Patient.HeightCm.HasValue ? $"{model.Patient.HeightCm} cm" : "-",
                ["Pregnant"] = model.Patient.IsPregnant ? "Yes" : "No",
                ["Pregnancy Remarks"] = model.Patient.PregnancyRemarks,
                ["Relevant Medical History"] = model.Patient.RelevantMedicalHistory,
                ["Allergy History"] = model.Patient.AllergyHistory,
                ["Other Patient Information"] = model.Patient.OtherPatientInformation
            }));

            body.Append(SectionHeading("2. Reporter Details"));
            body.Append(KeyValueTable(new Dictionary<string, string?>
            {
                ["Reporter Name"] = model.Reporter.ReporterName,
                ["Reporter Type"] = model.Reporter.ReporterType,
                ["Qualification"] = model.Reporter.Qualification,
                ["Organization"] = model.Reporter.OrganizationName,
                ["Department"] = model.Reporter.Department,
                ["Email"] = model.Reporter.Email,
                ["Phone"] = model.Reporter.Phone,
                ["Address"] = model.Reporter.Address,
                ["City"] = model.Reporter.City,
                ["State"] = model.Reporter.State,
                ["Country"] = model.Reporter.Country,
                ["Date of Report"] = Dt(model.Reporter.DateOfReport),
                ["Consent for Follow-up"] = model.Reporter.ConsentForFollowUp ? "Yes" : "No",
                ["Remarks"] = model.Reporter.ReporterRemarks
            }));

            body.Append(SectionHeading("3. Adverse Event Details"));
            body.Append(KeyValueTable(new Dictionary<string, string?>
            {
                ["Event Term"] = model.AdverseEvent.EventTerm,
                ["Event Start Date"] = Dt(model.AdverseEvent.EventStartDate),
                ["Event End Date"] = Dt(model.AdverseEvent.EventEndDate),
                ["Outcome"] = model.AdverseEvent.Outcome,
                ["Serious"] = model.AdverseEvent.IsSerious ? "Yes" : "No",
                ["Seriousness Criteria"] = model.AdverseEvent.SeriousnessCriteria,
                ["Event Description"] = model.AdverseEvent.EventDescription,
                ["Treatment Given"] = model.AdverseEvent.TreatmentGiven,
                ["Remarks"] = model.AdverseEvent.EventRemarks
            }));

            body.Append(SectionHeading("4. Suspect Product Details"));
            body.Append(KeyValueTable(new Dictionary<string, string?>
            {
                ["Product Name"] = model.SuspectProduct.ProductName,
                ["Generic Name"] = model.SuspectProduct.GenericName,
                ["Batch No"] = model.SuspectProduct.BatchNo,
                ["Expiry Date"] = Dt(model.SuspectProduct.ExpiryDate),
                ["Dose"] = Join(model.SuspectProduct.Dose, model.SuspectProduct.DoseUnit),
                ["Route"] = model.SuspectProduct.Route,
                ["Frequency"] = model.SuspectProduct.Frequency,
                ["Therapy Start Date"] = Dt(model.SuspectProduct.TherapyStartDate),
                ["Therapy Stop Date"] = Dt(model.SuspectProduct.TherapyStopDate),
                ["Indication"] = model.SuspectProduct.Indication,
                ["Action Taken"] = model.SuspectProduct.ActionTaken,
                ["Dechallenge"] = model.SuspectProduct.Dechallenge,
                ["Rechallenge"] = model.SuspectProduct.Rechallenge,
                ["Causality Assessment"] = model.SuspectProduct.CausalityAssessment,
                ["Remarks"] = model.SuspectProduct.ProductRemarks
            }));

            body.Append(SectionHeading("5. Concomitant Medications"));
            body.Append(ConcomitantTable(model.ConcomitantMedications));

            body.Append(SectionHeading("6. Lab Details"));
            body.Append(LabTable(model.LabDetails));

            body.Append(SectionHeading("7. Narrative"));
            body.Append(Para(HtmlToText(model.Narrative)));

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static Paragraph Title(string text)
    {
        return new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = "200" }),
            new Run(
                new RunProperties(
                    new Bold(),
                    new FontSize { Val = "32" },
                    new Color { Val = "1F4E79" }),
                new Text(text)));
    }

    private static Paragraph SectionHeading(string text)
    {
        return new Paragraph(
            new ParagraphProperties(
                new SpacingBetweenLines { Before = "240", After = "120" }),
            new Run(
                new RunProperties(
                    new Bold(),
                    new FontSize { Val = "24" },
                    new Color { Val = "1F4E79" }),
                new Text(text)));
    }

    private static Paragraph Para(string? text)
    {
        return new Paragraph(
            new Run(
                new RunProperties(new FontSize { Val = "20" }),
                new Text(string.IsNullOrWhiteSpace(text) ? "-" : text)
                {
                    Space = SpaceProcessingModeValues.Preserve
                }));
    }

    private static Table KeyValueTable(Dictionary<string, string?> data)
    {
        var table = BaseTable();

        foreach (var item in data)
        {
            var row = new TableRow();

            row.Append(Cell(item.Key, true));
            row.Append(Cell(V(item.Value), false));

            table.Append(row);
        }

        return table;
    }

    private static Table ConcomitantTable(List<ConcomitantMedicationReportSection> items)
    {
        var table = BaseTable();

        table.Append(HeaderRow("Medication", "Dose", "Route", "Frequency", "Therapy Period", "Indication / Remarks"));

        if (!items.Any())
        {
            table.Append(Row("No concomitant medication records available.", "", "", "", "", ""));
            return table;
        }

        foreach (var item in items)
        {
            table.Append(Row(
                item.MedicationName,
                Join(item.Dose, item.DoseUnit),
                item.Route,
                item.Frequency,
                $"{Dt(item.TherapyStartDate)} to {Dt(item.TherapyStopDate)}",
                Join(item.Indication, item.Remarks)));
        }

        return table;
    }

    private static Table LabTable(List<LabDetailReportSection> items)
    {
        var table = BaseTable();

        table.Append(HeaderRow("Test", "Date", "Result", "Unit", "Normal Range", "Clinical Significance / Remarks"));

        if (!items.Any())
        {
            table.Append(Row("No lab detail records available.", "", "", "", "", ""));
            return table;
        }

        foreach (var item in items)
        {
            table.Append(Row(
                item.TestName,
                Dt(item.TestDate),
                item.ResultValue,
                item.Unit,
                item.NormalRange,
                Join(item.ClinicalSignificance, item.Remarks)));
        }

        return table;
    }

    private static Table BaseTable()
    {
        return new Table(
            new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 })));
    }

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow();

        foreach (var header in headers)
        {
            row.Append(Cell(header, true));
        }

        return row;
    }

    private static TableRow Row(params string?[] values)
    {
        var row = new TableRow();

        foreach (var value in values)
        {
            row.Append(Cell(V(value), false));
        }

        return row;
    }

    private static TableCell Cell(string text, bool header)
    {
        var props = new TableCellProperties(
            new TableCellWidth { Type = TableWidthUnitValues.Auto });

        var runProps = new RunProperties(
            new FontSize { Val = "18" });

        if (header)
        {
            runProps.Append(new Bold());
            runProps.Append(new Color { Val = "1F4E79" });
        }

        return new TableCell(
            props,
            new Paragraph(
                new Run(
                    runProps,
                    new Text(text)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    })));
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
        text = Regex.Replace(text, "<li>", "• ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</li>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<.*?>", string.Empty);

        return WebUtility.HtmlDecode(text).Trim();
    }
}