using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Handlers;

public class NotFoundHandler : RequestHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        var response = context.Response;
        response.StatusCode = 404;
        response.Headers["Server"] = "MiniHttpServer/1.0";

        string errorFilePath = Path.Combine(AppContext.BaseDirectory, "static", "404.html");
        byte[] buffer;

        if (File.Exists(errorFilePath))
        {
            buffer = await File.ReadAllBytesAsync(errorFilePath);
            response.ContentType = "text/html; charset=utf-8";
        }
        else
        {
            buffer = Encoding.UTF8.GetBytes("<html><body><h1>404 - Страница не найдена</h1></body></html>");
            response.ContentType = "text/html; charset=utf-8";
        }

        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }
}