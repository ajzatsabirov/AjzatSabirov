namespace HomeWork6.Services;

public interface IEmailService
{
    void SendEmail(string toEmail, string subject, string body, string? attachmentPath = null);
}