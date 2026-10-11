using System.Net;
using System.Text;
using System.IO;

namespace HomeWork1.Framework.Handlers;

public class StaticFilesHandler : Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {
        var response = context.Response;
    
        string requestPath = context.Request.Url?.AbsolutePath ?? "/";
        string relativePath = requestPath;
    
        if (relativePath.StartsWith("/connection/"))
        {
            relativePath = relativePath.Substring("/connection/".Length);
        }
        
        if (string.IsNullOrEmpty(relativePath) || relativePath == "/")
        {
            relativePath = "search-engine.html";
        }
    
        string filePath = Path.Combine(AppContext.BaseDirectory, "static", relativePath);
        
        if (File.Exists(filePath))
        {
            response.StatusCode = 200;
            FileInfo fileInfo = new FileInfo(filePath);
            MimeTypeChecker.CheckType(response, fileInfo);
        
            byte[] buffer = File.ReadAllBytes(filePath);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.Close();
        }
        else
        {
            if (Successor != null)
            {
                await Successor.HandleRequest(context);
            }
            else
            {
                response.StatusCode = 404;
                response.ContentType = "text/html; charset=utf-8";
                byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h1>404 - Страница не найдена</h1></body></html>");
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
                response.Close();
            }
        }
    }
}