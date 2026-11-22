using HomeWork5.Attributes;
using HomeWork5.Configuration;
using HomeWork5.Services;

namespace HomeWork5.Controllers;

[Controller(route: "testemail")]
public class TestEmailController
{
    [Get(route: "send")]
    public string SendTest()
    {
        try
        {
            var emailSettings = ConfigurationManager.Instance.EmailSettings;
            var emailService = new EmailService(emailSettings);
            
            emailService.SendEmail(
                toEmail: "ajzat2007@mail.ru",
                subject: "Тестовое письмо",
                body: "Привет! Это тестовое письмо от HTTP-сервера."
            );
            
            return "Письмо отправлено! Проверь почту.";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }
}