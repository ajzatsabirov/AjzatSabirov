using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace HomeWork6.Controllers;

[Controller(route: "user")] 
public class UserController : BaseController
{
    [Get(route: "profile")]
    public IHttpResult Profile()
    {
        var user = new
        {
            Name = "Айзат",
            Age = 19,
            Email = "ajzat2007@mail.ru",
            IsActive = true,
            users = new[]
            {
                new { Name = "Сергей", Age = 20 },
                new { Name = "Дмитрий", Age = 21 },
                new { Name = "Анна", Age = 19 }
            }
        };
        
        return Page("Templates/User/Profile.html", user);
    }
}