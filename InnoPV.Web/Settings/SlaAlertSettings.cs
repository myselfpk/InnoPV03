namespace InnoPV.Web.Settings;

public class SlaAlertSettings
{
    public bool EnableAutomaticAlerts { get; set; } = false;

    public int T7Days { get; set; } = 7;

    public int T3Days { get; set; } = 3;

    public int T1Days { get; set; } = 1;

    public bool SendT7Alerts { get; set; } = true;

    public bool SendT3Alerts { get; set; } = true;

    public bool SendT1Alerts { get; set; } = true;

    public bool SendDueSoonAlerts { get; set; } = true;

    public bool SendOverdueAlerts { get; set; } = true;

    public bool SendToAssignedUsers { get; set; } = true;

    public bool SendSummaryToAdmin { get; set; } = true;

    public bool SendSummaryToPvManager { get; set; } = true;

    public int DailyRunHour { get; set; } = 9;

    public int DailyRunMinute { get; set; } = 0;

    public int CheckIntervalMinutes { get; set; } = 30;
}