using HomeWork4.Attributes;
using HomeWork4.Configuration;
using HomeWork4.Services;

namespace HomeWork4.Controllers;

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