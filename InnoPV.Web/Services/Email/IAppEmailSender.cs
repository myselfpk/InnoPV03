namespace InnoPV.Web.Services.Email;

public interface IAppEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);

    Task SendEmailsAsync(IEnumerable<string> toEmails, string subject, string htmlBody);
}