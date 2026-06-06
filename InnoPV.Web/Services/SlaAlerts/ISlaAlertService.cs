using InnoPV.Web.Models.SlaAlerts;

namespace InnoPV.Web.Services.SlaAlerts;

public interface ISlaAlertService
{
    Task<SlaAlertPreviewViewModel> GetPreviewAsync();

    Task<SlaAlertPreviewViewModel> GetPreviewForUserAsync(string? userId);

    Task<SlaAlertSendResultViewModel> SendDueSoonAlertsAsync();

    Task<SlaAlertSendResultViewModel> SendOverdueAlertsAsync();

    Task<SlaAlertSendResultViewModel> SendAllAlertsAsync();
}
