using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace HomeWork6.Controllers;

[Controller(route: "test")]
public class TestController : BaseController
{
    [Get(route: "index")]
    public IHttpResult Index()
    {
        return Html("<h1>Hello from controller!</h1>");
    }

    [Get(route: "info")]
    public IHttpResult Info()
    {
        return Json(new
        {
            Id = 1,
            Message = "Hello World!",
            Success = true
        });
    }

    [Get(route: "requestinfo")]
    public IHttpResult RequestInfo()
    {
        return Json(new
        {
            method = Context.Request.HttpMethod,
            path = Context.Request.Url?.LocalPath,
            userAgent = Context.Request.Headers["User-Agent"]
        });
    }
}