using System.Net;

namespace InnoPV.Web.Services.Email;

public static class UserCredentialEmailTemplate
{
    public static string BuildCredentialEmailBody(
        string fullName,
        string email,
        string temporaryPassword,
        string loginUrl)
    {
        return $@"
        <html>
        <body style='font-family:Arial, sans-serif; font-size:14px; color:#333;'>
            <h3 style='color:#0d6efd;'>InnoPV Login Credentials</h3>

            <p>Dear {Html(fullName)},</p>

            <p>Your InnoPV user account has been created/updated. Please find your login credentials below:</p>

            <table style='border-collapse:collapse; width:100%; max-width:650px;'>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px; width:180px;'><strong>Login URL</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{Html(loginUrl)}</td>
                </tr>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px;'><strong>User ID / Email</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{Html(email)}</td>
                </tr>
                <tr>
                    <td style='border:1px solid #ddd; padding:8px;'><strong>Temporary Password</strong></td>
                    <td style='border:1px solid #ddd; padding:8px;'>{Html(temporaryPassword)}</td>
                </tr>
            </table>

            <p style='margin-top:16px;'>
                You will be required to change your password after first login.
            </p>

            <p style='color:#777; font-size:12px;'>
                This is an automated email from InnoPV. Please do not reply to this email.
            </p>
        </body>
        </html>";
    }

    private static string Html(string? value)
    {
        return WebUtility.HtmlEncode(value ?? "-");
    }
}