using System;
using System.IO;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using HomeWork5.Configuration;

namespace HomeWork5.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(EmailSettings settings)
    {
        _settings = settings;
    }

    public void SendEmail(string toEmail, string subject, string body, string? attachmentPath = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.TextBody = body;

        if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
        {
            bodyBuilder.Attachments.Add(attachmentPath);
        }

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            Console.WriteLine($"[EmailService] Подключение к {_settings.SmtpServer}:{_settings.Port}...");
            client.Connect(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
            client.Authenticate(_settings.SenderEmail, _settings.Password);
            client.Send(new SendRequest(message));
            Console.WriteLine("[EmailService] Письмо успешно отправлено!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService] Ошибка: {ex.Message}");
            throw;
        }
        finally
        {
            client.Disconnect(true);
        }
    }
}