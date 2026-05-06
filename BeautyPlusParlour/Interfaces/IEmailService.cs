namespace BeautyPlusParlour.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(
        string toEmail, string fullName,
        string link, CancellationToken ct = default);

    Task SendOtpEmailAsync(
        string toEmail, string fullName,
        string otp, CancellationToken ct = default);

    Task SendPasswordChangedEmailAsync(
        string toEmail, string fullName,
        CancellationToken ct = default);
    // ADD to existing interface
    Task SendGeneralAsync(
        string toEmail, string fullName,
        string subject, string htmlBody,
        CancellationToken ct = default);
}