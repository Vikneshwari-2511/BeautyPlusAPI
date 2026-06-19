using BeautyPlusParlour.Configurations;
using BeautyPlusParlour.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;

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
        
        var body = $@"
<div style='margin:0;padding:0;background:#F9F6FB;font-family:Arial, Helvetica, sans-serif;'>

<table align='center' width='620' style='background:white;border-radius:12px;overflow:hidden;
box-shadow:0 4px 15px rgba(0,0,0,0.08);'>

<!-- Header Image -->
    <tr>
     <td> <img src='https://plus.unsplash.com/premium_photo-1683120742902-5a303f35c5e6?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D' width='100%' style='display:block; width:100%; height:auto; border-top-left-radius:16px; border-top-right-radius:16px;'> </td>
    </tr>
<h1 style='margin:0;color:#6b21a8;font-weight:600;letter-spacing:1px;'>
Beauty Plus
</h1>

<p style='margin:5px 0 0 0;color:#999;font-size:14px;'>
Luxury Beauty & Care
</p>

</td>
</tr>

<!-- Gold Divider -->
<tr>
<td style='height:4px;background:#E3A274;'></td>
</tr>

<!-- Body -->
<tr>
<td style='padding:35px;color:#333;line-height:1.7;font-size:15px;'>

<p>Dear <strong>{fullName}!</strong>,</p>

<p>
Greetings from <strong>Beauty Plus Beauty Parlour</strong> ✨
</p>

<p>
Thank you for joining us. To activate your account and begin exploring our beauty services,
please verify your email address by clicking the button below.
</p>

<!-- Button -->
<div style='text-align:center;margin:35px 0;'>

<a href='{link}'
style ='background:#E3A274;color:white;padding:14px 30px;text-decoration:none;
border-radius:30px;font-weight:bold;font-size:15px;display:inline-block;'>

Verify Your Email

</a>

</div>

<p>
For your security, please keep this verification link confidential and do not share it with anyone.
</p>

<p>
If you did not create this account, you may safely ignore this email.
</p>

<p style='margin-top:35px;'>

Warm regards,<br>
<strong>Beauty Plus Team</strong>

</p>

</td>
</tr>

<!-- Footer -->
<tr>
<td style='background:#fafafa;text-align:center;padding:25px;font-size:13px;color:#777;'>

<p style='margin:0 0 8px 0;'>
<strong style='color:#C8A2C8;'>Beauty Plus Beauty Parlour</strong>
</p>

<p style='margin:0 0 15px 0;'>
Your Beauty, Our Passion ✨
</p>

<p>

<a href='#' style='color:#D4AF37;text-decoration:none;margin:0 6px;'>Instagram</a> |
<a href='#' style='color:#D4AF37;text-decoration:none;margin:0 6px;'>Facebook</a> |
<a href='#' style='color:#D4AF37;text-decoration:none;margin:0 6px;'>Website</a>

</p>

<p style='margin-top:15px;font-size:12px;color:#aaa;'>

© {DateTime.Now.Year} Beauty Plus Beauty Parlour. All rights reserved.

</p>

</td>
</tr>

</table>

</div>";
        return SendAsync(toEmail, "Verify your Beauty Plus account", body, ct);
    }

    public Task SendOtpEmailAsync(
        string toEmail, string fullName,
        string otp, CancellationToken ct = default)
    {
        
        var body = $@"
<div style='margin:0;padding:0;background-color:#f5f3ff;font-family:Segoe UI,Arial,sans-serif;'>

  <table align='center' width='100%' cellpadding='0' cellspacing='0' style='max-width:600px;margin:auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.08);'>

    <!-- Header Image -->
    <tr>
       <td> <img src='https://plus.unsplash.com/premium_photo-1683120742902-5a303f35c5e6?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D' width='100%' style='display:block; width:100%; height:auto; border-top-left-radius:16px; border-top-right-radius:16px;'> </td>
    </tr>

    <!-- Content -->
    <tr>
      <td style='padding:30px;text-align:center;'>

        <h2 style='color:#6b21a8;margin-bottom:10px;'>Beauty Plus</h2>
        <h3 style='color:#333;margin-bottom:20px;'>Password Reset OTP</h3>

        <p style='color:#555;font-size:15px;line-height:1.6;'>
          Dear  {fullName},
          <br><br>
          Greetings from <strong>Beauty Plus</strong> 💜
          <br><br>
          Use the OTP below to reset your password. Please do not share this code with anyone.
        </p>

        <!-- OTP Box -->
        <div style='margin:25px auto;padding:15px 25px;background:#f3e8ff;
                    display:inline-block;border-radius:10px;
                    font-size:28px;font-weight:bold;
                    color:#7c3aed;letter-spacing:3px;'>
          {otp}
        </div>

        <p style='color:#777;font-size:14px;margin-top:20px;'>
          This OTP is valid for <strong>10 minutes</strong>.
        </p>

        <p style='color:#999;font-size:13px;margin-top:30px;line-height:1.5;'>
          If you did not request this, please ignore this email or contact our support team.
        </p>

      </td>
    </tr>

    <!-- Footer -->
    <tr>
      <td style='background:#faf5ff;padding:20px;text-align:center;font-size:13px;color:#777;'>

        <p style='margin:0;'>Stay connected</p>

        <p style='margin:10px 0;'>
          <a href='#' style='margin:0 10px;text-decoration:none;color:#7c3aed;'>Facebook</a> |
          <a href='#' style='margin:0 10px;text-decoration:none;color:#7c3aed;'>Instagram</a> |
          <a href='#' style='margin:0 10px;text-decoration:none;color:#7c3aed;'>Website</a>
        </p>

        <p style='margin-top:10px;'>© 2026 Beauty Plus. All rights reserved.</p>

      </td>
    </tr>

  </table>

</div>";
        return SendAsync(toEmail, "Your OTP — Beauty Plus Parlour", body, ct);
    }

    public Task SendPasswordChangedEmailAsync(
        string toEmail, string fullName, CancellationToken ct = default)
    {        
        
var body = $"""
<div style='margin:0;padding:0;background-color:#f5f3ff;font-family:Segoe UI,Arial,sans-serif;'>

  <table align='center' width='100%' cellpadding='0' cellspacing='0'
         style='max-width:600px;margin:auto;background:#ffffff;
         border-radius:16px;overflow:hidden;
         box-shadow:0 4px 20px rgba(0,0,0,0.08);'>

    <!-- Header Banner -->
    <tr>
     <td> <img src='https://plus.unsplash.com/premium_photo-1683120742902-5a303f35c5e6?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D' width='100%' style='display:block; width:100%; height:auto; border-top-left-radius:16px; border-top-right-radius:16px;'> </td>
    </tr>

    <!-- Content -->
    <tr>
      <td style='padding:35px;text-align:center;'>

        <h2 style='color:#6b21a8;margin-bottom:5px;'>
          Beauty Plus
        </h2>

        <p style='color:#999;font-size:13px;margin-bottom:25px;'>
          Premium Beauty & Care
        </p>

        <h3 style='color:#333;margin-bottom:20px;'>
          Password Changed Successfully
        </h3>

        <p style='color:#555;font-size:15px;line-height:1.7;text-align:left;'>

          Hi <strong>{fullName}</strong>,
          <br><br>

          Your password has been successfully reset for your
          <strong>Beauty Plus</strong> account. 🔐

          <br><br>

          If you made this change, no further action is required.

        </p>

        <!-- Alert Box -->
        <div style='margin-top:30px;
                    background:#fff1f2;
                    border-left:4px solid #e11d48;
                    padding:15px 18px;
                    border-radius:8px;
                    text-align:left;
                    color:#555;
                    font-size:14px;
                    line-height:1.6;'>

          <strong style='color:#e11d48;'>Security Notice:</strong><br>

          If you did not make this change, please contact our support team immediately and secure your account.

        </div>

        <p style='margin-top:35px;color:#777;font-size:14px;'>
          Thank you for choosing Beauty Plus 💜
        </p>

      </td>
    </tr>

    <!-- Footer -->
    <tr>
      <td style='background:#faf5ff;
                 padding:22px;
                 text-align:center;
                 font-size:13px;
                 color:#777;'>

        <p style='margin:0 0 12px 0;'>
          Stay connected with us
        </p>

        <!-- Social Icons -->
        <p style='margin-bottom:15px;'>

          <a href='#' style='margin:0 8px;'>
            <img src='https://cdn-icons-png.flaticon.com/512/733/733547.png'
                 width='28'>
          </a>

          <a href='#' style='margin:0 8px;'>
            <img src='https://cdn-icons-png.flaticon.com/512/733/733558.png'
                 width='28'>
          </a>

          <a href='#' style='margin:0 8px;'>
            <img src='https://cdn-icons-png.flaticon.com/512/1006/1006771.png'
                 width='28'>
          </a>

        </p>

        <p style='margin-top:10px;color:#aaa;font-size:12px;'>
          © {DateTime.Now.Year} Beauty Plus Beauty Parlour.
          All rights reserved.
        </p>

      </td>
    </tr>

  </table>

</div>
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
    public async Task SendBookingConfirmationAsync(
    string email,
    string fullName,
    string bookingCode,
    string bookingDate,
    string bookingTime,
    decimal amount,
    CancellationToken ct = default)
    {
        var body = $"""
        <h2>Booking Confirmed!</h2>
        <p>Hi {fullName},</p>
        <p>Your booking has been confirmed. Here are the details:</p>
        <table style="border-collapse:collapse;width:100%;max-width:480px;">
            <tr>
                <td style="padding:8px;border:1px solid #eee;"><strong>Booking Code</strong></td>
                <td style="padding:8px;border:1px solid #eee;">{bookingCode}</td>
            </tr>
            <tr>
                <td style="padding:8px;border:1px solid #eee;"><strong>Date</strong></td>
                <td style="padding:8px;border:1px solid #eee;">{bookingDate}</td>
            </tr>
            <tr>
                <td style="padding:8px;border:1px solid #eee;"><strong>Time</strong></td>
                <td style="padding:8px;border:1px solid #eee;">{bookingTime}</td>
            </tr>
            <tr>
                <td style="padding:8px;border:1px solid #eee;"><strong>Amount Paid</strong></td>
                <td style="padding:8px;border:1px solid #eee;">₹{amount:F2}</td>
            </tr>
        </table>
        <p>Thank you for choosing Beauty Plus Parlour!</p>
        """;

        await SendAsync(email, "Booking Confirmed — Beauty Plus Parlour", body, ct);
    }
}
