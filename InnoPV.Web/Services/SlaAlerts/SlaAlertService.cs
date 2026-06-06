using InnoPV.Domain.Enums;
using InnoPV.Domain.Identity;
using InnoPV.Infrastructure.Data;
using InnoPV.Web.Models.SlaAlerts;
using InnoPV.Web.Services.Email;
using InnoPV.Web.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InnoPV.Web.Services.SlaAlerts;

public class SlaAlertService : ISlaAlertService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppEmailSender _emailSender;
    private readonly SlaAlertSettings _settings;

    public SlaAlertService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAppEmailSender emailSender,
        IOptions<SlaAlertSettings> options)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _settings = options.Value;
    }

    public async Task<SlaAlertPreviewViewModel> GetPreviewAsync()
    {
        return await GetPreviewAsync(userId: null);
    }

    public async Task<SlaAlertPreviewViewModel> GetPreviewForUserAsync(string? userId)
    {
        return await GetPreviewAsync(userId);
    }

    private async Task<SlaAlertPreviewViewModel> GetPreviewAsync(string? userId)
    {
        var t7Cases = await GetEscalationCasesAsync(_settings.T7Days, "T-7", userId);
        var t3Cases = await GetEscalationCasesAsync(_settings.T3Days, "T-3", userId);
        var t1Cases = await GetEscalationCasesAsync(_settings.T1Days, "T-1", userId);
        var overdueCases = await GetOverdueCasesAsync(userId);

        var dueSoonCases = t7Cases
            .Concat(t3Cases)
            .Concat(t1Cases)
            .GroupBy(x => x.Id)
            .Select(x => x.OrderBy(y => y.DaysRemaining).First())
            .OrderBy(x => x.DueDate)
            .ToList();

        return new SlaAlertPreviewViewModel
        {
            T7Days = _settings.T7Days,
            T3Days = _settings.T3Days,
            T1Days = _settings.T1Days,
            T7Cases = t7Cases,
            T3Cases = t3Cases,
            T1Cases = t1Cases,
            DueSoonCases = dueSoonCases,
            OverdueCases = overdueCases,
            GeneratedOnUtc = DateTime.UtcNow
        };
    }

    public async Task<SlaAlertSendResultViewModel> SendDueSoonAlertsAsync()
    {
        var dueSoonCases = await GetCombinedEscalationCasesAsync();

        return await SendAlertAsync(
            dueSoonCases,
            "InnoPV SLA Alert: Cases Due Soon",
            "The following PV cases are due in configured escalation windows (T-7, T-3, T-1).");
    }

    public async Task<SlaAlertSendResultViewModel> SendOverdueAlertsAsync()
    {
        var overdueCases = await GetOverdueCasesAsync();

        return await SendAlertAsync(
            overdueCases,
            "InnoPV SLA Alert: Overdue Cases",
            "The following PV cases have crossed their SLA/TAT due date.");
    }

    public async Task<SlaAlertSendResultViewModel> SendAllAlertsAsync()
    {
        var dueSoonCases = _settings.SendDueSoonAlerts
            ? await GetCombinedEscalationCasesAsync()
            : new List<SlaAlertCaseItemViewModel>();

        var overdueCases = _settings.SendOverdueAlerts
            ? await GetOverdueCasesAsync()
            : new List<SlaAlertCaseItemViewModel>();

        var allCases = dueSoonCases
            .Concat(overdueCases)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();

        return await SendAlertAsync(
            allCases,
            "InnoPV SLA Alert: Due Soon / Overdue Cases",
            "The following PV cases are due in configured escalation windows or already overdue.");
    }

    private async Task<List<SlaAlertCaseItemViewModel>> GetCombinedEscalationCasesAsync()
    {
        var escalationCases = new List<SlaAlertCaseItemViewModel>();

        if (_settings.SendT7Alerts)
        {
            escalationCases.AddRange(await GetEscalationCasesAsync(_settings.T7Days, "T-7"));
        }

        if (_settings.SendT3Alerts)
        {
            escalationCases.AddRange(await GetEscalationCasesAsync(_settings.T3Days, "T-3"));
        }

        if (_settings.SendT1Alerts)
        {
            escalationCases.AddRange(await GetEscalationCasesAsync(_settings.T1Days, "T-1"));
        }

        return escalationCases
            .GroupBy(x => x.Id)
            .Select(x => x.OrderBy(y => y.DaysRemaining).First())
            .OrderBy(x => x.DueDate)
            .ToList();
    }

    private async Task<List<SlaAlertCaseItemViewModel>> GetEscalationCasesAsync(
        int targetDays,
        string escalationLevel,
        string? userId = null)
    {
        var today = DateTime.UtcNow.Date;

        var closedStatuses = GetClosedStatuses();

        var query = _context.PvCases
            .Where(x =>
                !x.IsDeleted &&
                x.DueDate.HasValue &&
                x.DueDate.Value.Date == today.AddDays(targetDays) &&
                !closedStatuses.Contains(x.Status));

        query = ApplyUserFilter(query, userId);

        return await BuildCaseItemsAsync(query, escalationLevel);
    }

    private async Task<List<SlaAlertCaseItemViewModel>> GetOverdueCasesAsync(string? userId = null)
    {
        var today = DateTime.UtcNow.Date;

        var closedStatuses = GetClosedStatuses();

        var query = _context.PvCases
            .Where(x =>
                !x.IsDeleted &&
                x.DueDate.HasValue &&
                x.DueDate.Value.Date < today &&
                !closedStatuses.Contains(x.Status));

        query = ApplyUserFilter(query, userId);

        return await BuildCaseItemsAsync(query, "Overdue");
    }

    private static IQueryable<Domain.Entities.PvCase> ApplyUserFilter(
        IQueryable<Domain.Entities.PvCase> query,
        string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return query;
        }

        return query.Where(x =>
            x.CreatedBy == userId ||
            x.CurrentAssignedUserId == userId);
    }

    private async Task<List<SlaAlertCaseItemViewModel>> BuildCaseItemsAsync(
        IQueryable<Domain.Entities.PvCase> query,
        string escalationLevel)
    {
        var cases = await query
            .OrderBy(x => x.DueDate)
            .Select(x => new SlaAlertCaseItemViewModel
            {
                Id = x.Id,
                CaseNo = x.CaseNo,
                ProductName = x.InitialProductName,
                EventTerm = x.InitialEventTerm,
                IsSerious = x.IsSerious,
                ReceiptDate = x.ReceiptDate,
                DueDate = x.DueDate,
                Status = x.Status,
                CurrentAssignedRole = x.CurrentAssignedRole,
                CurrentAssignedUserId = x.CurrentAssignedUserId,
                EscalationLevel = escalationLevel
            })
            .ToListAsync();

        var userIds = cases
            .Where(x => !string.IsNullOrWhiteSpace(x.CurrentAssignedUserId))
            .Select(x => x.CurrentAssignedUserId!)
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(x => userIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email
            })
            .ToListAsync();

        foreach (var item in cases)
        {
            if (!string.IsNullOrWhiteSpace(item.CurrentAssignedUserId))
            {
                var user = users.FirstOrDefault(x => x.Id == item.CurrentAssignedUserId);

                item.CurrentAssignedUserName = user?.Email ?? user?.UserName;
                item.CurrentAssignedUserEmail = user?.Email;
            }
        }

        return cases;
    }

    private async Task<SlaAlertSendResultViewModel> SendAlertAsync(
        List<SlaAlertCaseItemViewModel> cases,
        string subject,
        string message)
    {
        var result = new SlaAlertSendResultViewModel
        {
            T7CaseCount = cases.Count(x => x.EscalationLevel == "T-7"),
            T3CaseCount = cases.Count(x => x.EscalationLevel == "T-3"),
            T1CaseCount = cases.Count(x => x.EscalationLevel == "T-1"),
            DueSoonCaseCount = cases.Count(x => !x.IsOverdue),
            OverdueCaseCount = cases.Count(x => x.IsOverdue)
        };

        if (!cases.Any())
        {
            return result;
        }

        var recipients = await BuildRecipientsAsync(cases);

        result.Recipients = recipients
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        result.EmailRecipientCount = result.Recipients.Count;

        if (!result.Recipients.Any())
        {
            return result;
        }

        var emailRows = cases.Select(x =>
        (
            CaseNo: x.CaseNo,
            ProductName: x.ProductName,
            EventTerm: x.EventTerm,
            Seriousness: x.IsSerious ? "Serious" : "Non-serious",
            DueDate: x.DueDate.HasValue ? x.DueDate.Value.ToString("dd-MMM-yyyy") : "-",
            DaysInfo: x.IsOverdue
                ? $"{Math.Abs(x.DaysRemaining)} day(s) overdue"
                : $"{x.DaysRemaining} day(s) remaining",
            EscalationLevel: x.EscalationLevel,
            Status: x.Status.ToString(),
            AssignedRole: x.CurrentAssignedRole
        ));

        var htmlBody = PvEmailTemplateHelper.BuildSlaAlertSummaryBody(
            subject,
            message,
            emailRows);

        await _emailSender.SendEmailsAsync(
            result.Recipients,
            subject,
            htmlBody);

        return result;
    }

    private async Task<List<string>> BuildRecipientsAsync(List<SlaAlertCaseItemViewModel> cases)
    {
        var recipients = new List<string>();

        if (_settings.SendToAssignedUsers)
        {
            foreach (var item in cases)
            {
                if (!string.IsNullOrWhiteSpace(item.CurrentAssignedUserEmail))
                {
                    recipients.Add(item.CurrentAssignedUserEmail);
                }

                if (!string.IsNullOrWhiteSpace(item.CurrentAssignedRole))
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(item.CurrentAssignedRole);

                    recipients.AddRange(
                        usersInRole
                            .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.Email))
                            .Select(x => x.Email!));
                }
            }
        }

        if (_settings.SendSummaryToAdmin)
        {
            var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);

            recipients.AddRange(
                admins
                    .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.Email))
                    .Select(x => x.Email!));
        }

        if (_settings.SendSummaryToPvManager)
        {
            var pvManagers = await _userManager.GetUsersInRoleAsync(AppRoles.PvManager);

            recipients.AddRange(
                pvManagers
                    .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.Email))
                    .Select(x => x.Email!));
        }

        return recipients
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static PvCaseStatus[] GetClosedStatuses()
    {
        return new[]
        {
            PvCaseStatus.CaseClosed,
            PvCaseStatus.CaseFinalized,
            PvCaseStatus.Submitted,
            PvCaseStatus.MarkedAsDuplicate,
            PvCaseStatus.MarkedAsInvalid
        };
    }
}
