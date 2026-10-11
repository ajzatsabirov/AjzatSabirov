using HomeWork1.Framework.Attributes;

namespace HomeWork1.Controllers;

[Controller(route: "auth")]
public class AuthController
{
    [Get(route: "login")]
    public void login()
    {
        Console.WriteLine("GET /auth/login вызван");
    }

    [Post(route: "login")]
    public void login(string login, string password)
    {
        Console.WriteLine($"Login: {login} | Password: {password}");
    }
}