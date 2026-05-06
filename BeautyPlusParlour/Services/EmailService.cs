using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BeautyPlusParlour.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings) =>
        _settings = settings.Value;

    public Task SendVerificationEmailAsync(
        string toEmail, string fullName,
        string link, CancellationToken ct = default)
    {
        var body = $"""
            <h2>Welcome to Beauty Plus Parlour, {fullName}!</h2>
            <p>Please verify your email to activate your account:</p>
            <a href="{link}"
               style="background:#9b2335;color:#fff;padding:12px 28px;
                      text-decoration:none;border-radius:6px;display:inline-block;
                      font-family:sans-serif;font-size:15px;">
              Verify Email
            </a>
            <p style="color:#666;font-size:13px;margin-top:16px;">
              This link expires in 24 hours.
            </p>
            """;

        return SendAsync(toEmail, "Verify your Beauty Plus account", body, ct);
    }

    public Task SendOtpEmailAsync(
        string toEmail, string fullName,
        string otp, CancellationToken ct = default)
    {
        var body = $"""
            <h2>Password Reset OTP</h2>
            <p>Hi {fullName},</p>
            <p>Your one-time password is:</p>
            <h1 style="letter-spacing:10px;color:#9b2335;font-size:36px;">{otp}</h1>
            <p>This OTP expires in <strong>10 minutes</strong>.</p>
            <p style="color:#666;font-size:13px;">
              Do not share this code with anyone.
            </p>
            """;

        return SendAsync(toEmail, "Your OTP — Beauty Plus Parlour", body, ct);
    }

    public Task SendPasswordChangedEmailAsync(
        string toEmail, string fullName, CancellationToken ct = default)
    {
        var body = $"""
            <h2>Password Changed</h2>
            <p>Hi {fullName},</p>
            <p>Your password was successfully reset.</p>
            <p style="color:#cc0000;">
              If you did not make this change, please contact us immediately.
            </p>
            """;

        return SendAsync(toEmail, "Password Reset Successful — Beauty Plus Parlour", body, ct);
    }

    // ── private ────────────────────────────────────────────────────────────
    private async Task SendAsync(
        string toEmail, string subject,
        string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = WrapHtml(htmlBody) };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _settings.SmtpHost, _settings.SmtpPort,
            SecureSocketOptions.StartTls, ct);

        await client.AuthenticateAsync(
            _settings.SmtpUser, _settings.SmtpPassword, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    private static string WrapHtml(string body) => $"""
        <html>
          <body style="font-family:sans-serif;max-width:560px;margin:auto;padding:32px;color:#222;">
            {body}
            <hr style="border:none;border-top:1px solid #eee;margin-top:40px;">
            <p style="color:#999;font-size:12px;text-align:center;">
              Beauty Plus Parlour &mdash; This is an automated message, please do not reply.
            </p>
          </body>
        </html>
        """;
    // Notification for other modules to send custom emails (e.g. booking updates)
    public Task SendGeneralAsync(
        string toEmail, string fullName,
        string subject, string htmlBody,
        CancellationToken ct = default)
    {
        var body = $"""
        <h2>{subject}</h2>
        <p>Hi {fullName},</p>
        {htmlBody}
        """;

        return SendAsync(toEmail, subject, body, ct);
    }
}
