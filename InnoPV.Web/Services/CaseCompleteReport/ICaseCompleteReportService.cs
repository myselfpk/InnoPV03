using InnoPV.Web.Models.CaseCompleteReport;

namespace InnoPV.Web.Services.CaseCompleteReport;

public interface ICaseCompleteReportService
{
    Task<CaseCompleteReportViewModel?> GetCaseCompleteReportAsync(long caseId);
}