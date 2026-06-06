using System.Net;

namespace InnoPV.Web.Services.Email;

public static class ForgotPasswordEmailTemplate
{
    public static string BuildForgotPasswordEmailBody(
        string fullName,
        string resetLink)
    {
        return $@"
        <html>
        <body style='font-family:Arial, sans-serif; font-size:14px; color:#333;'>
            <h3 style='color:#0d6efd;'>InnoPV Password Reset Request</h3>

            <p>Dear {Html(fullName)},</p>

            <p>We received a request to reset your InnoPV account password.</p>

            <p>
                Please click the link below to reset your password:
            </p>

            <p>
                <a href='{Html(resetLink)}'
                   style='display:inline-block; padding:10px 16px; background:#0d6efd; color:#ffffff; text-decoration:none; border-radius:4px;'>
                    Reset Password
                </a>
            </p>

            <p>
                If the button does not work, copy and paste the below link into your browser:
            </p>

            <p style='word-break:break-all; color:#555;'>
                {Html(resetLink)}
            </p>

            <p>
                If you did not request a password reset, please ignore this email or contact the system administrator.
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