using InnoPV.Web.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace InnoPV.Web.Services.Email;

public class AppEmailSender : IAppEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<AppEmailSender> _logger;

    public AppEmailSender(
        IOptions<EmailSettings> options,
        ILogger<AppEmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return;
        }

        await SendEmailsAsync(new[] { toEmail }, subject, htmlBody);
    }

    public async Task SendEmailsAsync(IEnumerable<string> toEmails, string subject, string htmlBody)
    {
        var recipients = toEmails
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!recipients.Any())
        {
            return;
        }

        if (!_settings.EnableEmail)
        {
            _logger.LogInformation("Email disabled. Subject: {Subject}. Recipients: {Recipients}",
                subject,
                string.Join(", ", recipients));

            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            foreach (var recipient in recipients)
            {
                message.To.Add(recipient);
            }

            using var smtpClient = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                smtpClient.Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password);
            }

            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully. Subject: {Subject}. Recipients: {Recipients}",
                subject,
                string.Join(", ", recipients));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending failed. Subject: {Subject}. Recipients: {Recipients}",
                subject,
                string.Join(", ", recipients));
        }
    }
}