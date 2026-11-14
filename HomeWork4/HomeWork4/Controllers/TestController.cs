using HomeWork4.Attributes;

namespace HomeWork4.Controllers;

[Controller(route: "test")]
public class TestController
{
    [Get(route: "index")]
    public string Index() => "Hello from controller!";

    [Get(route: "hello")]
    public string Hello() => "Hello World!";

    [Post(route: "submit")]
    public string Submit() => "POST request processed";
}