using InnoPV.Domain.Enums;
using System.Net;

namespace InnoPV.Web.Services.Email;

public static class PvEmailTemplateHelper
{
    public static string BuildCaseNotificationBody(
        string title,
        string caseNo,
        string? productName,
        string? eventTerm,
        PvCaseStatus status,
        string? assignedRole,
        string? message,
        string? remarks)
    {
        var safeTitle = Html(title);
        var safeCaseNo = Html(caseNo);
        var safeProduct = Html(productName);
        var safeEvent = Html(eventTerm);
        var safeStatus = Html(status.ToString());
        var safeRole = Html(assignedRole);
        var safeMessage = Html(message);
        var safeRemarks = Html(remarks);

        return $@"
        <html>
        <body style='font-family:Arial, sans-serif; font-size:14px; color:#333;'>
            <h3 style='color:#0d6efd;'>{safeTitle}</h3>

            <p>{safeMessage}</p>

            <table style='border-collapse:collapse; width:100%; max-width:700px;'>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px; width:220px;'><strong>Case No</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{safeCaseNo}</td>
                </tr>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px;'><strong>Product</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{safeProduct}</td>
                </tr>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px;'><strong>Adverse Event</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{safeEvent}</td>
                </tr>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px;'><strong>Status</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{safeStatus}</td>
                </tr>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px;'><strong>Assigned Role</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{safeRole}</td>
                </tr>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px;'><strong>Remarks</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{safeRemarks}</td>
                </tr>
            </table>

            <p style='margin-top:16px;'>
                Please login to InnoPV system for further action.
            </p>

            <p style='color:#777; font-size:12px;'>
                This is an automated notification from InnoPV. Please do not reply to this email.
            </p>
        </body>
        </html>";
    }

    private static string Html(string? value)
    {
        return WebUtility.HtmlEncode(value ?? "-");
    }

    public static string BuildSlaAlertSummaryBody(
    string title,
    string message,
    IEnumerable<(string CaseNo, string? ProductName, string? EventTerm, string Seriousness, string DueDate, string DaysInfo, string EscalationLevel, string Status, string? AssignedRole)> cases)
    {
        var rows = new System.Text.StringBuilder();

        foreach (var item in cases)
        {
            rows.Append($@"
            <tr>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.CaseNo)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.ProductName)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.EventTerm)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.Seriousness)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.DueDate)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.DaysInfo)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.EscalationLevel)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.Status)}</td>
                <td style='border:1px solid #ddd; padding:8px;'>{Html(item.AssignedRole)}</td>
            </tr>");
        }

        return $@"
    <html>
    <body style='font-family:Arial, sans-serif; font-size:14px; color:#333;'>
        <h3 style='color:#0d6efd;'>{Html(title)}</h3>

        <p>{Html(message)}</p>

        <table style='border-collapse:collapse; width:100%;'>
            <thead>
                <tr>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Case No</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Product</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Event</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Seriousness</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Due Date</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>SLA/TAT</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Escalation</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Status</th>
                    <th style='border:1px solid #ddd; padding:8px; text-align:left;'>Assigned Role</th>
                </tr>
            </thead>
            <tbody>
                {rows}
            </tbody>
        </table>

        <p style='margin-top:16px;'>
            Please login to InnoPV system for further action.
        </p>

        <p style='color:#777; font-size:12px;'>
            This is an automated SLA/TAT alert from InnoPV. Please do not reply to this email.
        </p>
    </body>
    </html>";
    }
}