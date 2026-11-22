using HomeWork6.Controllers;
using HomeWork6.Services;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace HomeWork6.Controllers;

[Controller(route: "auth")]
public class AuthController : BaseController
{
    [Post(route: "sendemail")]
    public IHttpResult SendEmail(string to, string subject, string body)
    {
        try
        {
            var emailSettings = ConfigurationManager.Instance.EmailSettings;
            var emailService = new EmailService(emailSettings);

            emailService.SendEmail(to, subject, body);

            return Json(new
            {
                success = true,
                message = "Сообщение отправлено"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthController] Ошибка отправки: {ex.Message}");

            return Json(new
            {
                success = false,
                message = "Не удалось отправить сообщение"
            }, 500);
        }
    }
}