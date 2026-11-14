namespace HomeWork4.Services;

public interface IEmailService
{
    void SendEmail(string toEmail, string subject, string body, string? attachmentPath = null);
}