using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HomeWork4.Configuration;
using HomeWork4.Content;

namespace HomeWork4.Handlers;

public class StaticFilesHandler : RequestHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        string requestPath = request.Url?.AbsolutePath ?? "/";
        string urlPrefix = "/" + ConfigurationManager.Instance.Settings.Path.Trim('/') + "/";

        string relativePath = requestPath;
        if (relativePath.StartsWith(urlPrefix))
        {
            relativePath = relativePath.Substring(urlPrefix.Length);
        }

        if (string.IsNullOrEmpty(relativePath))
        {
            response.StatusCode = 302;
            response.Headers["Location"] = urlPrefix + "SearcherFinder/";
            response.Close();
            return;
        }

        string staticRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "static"));
        string filePath = Path.GetFullPath(Path.Combine(staticRoot, relativePath));

        if (!filePath.StartsWith(staticRoot + Path.DirectorySeparatorChar))
        {
            await HandleNextAsync(context);
            return;
        }

        if (Directory.Exists(filePath))
        {
            if (!relativePath.EndsWith("/"))
            {
                response.StatusCode = 302;
                response.Headers["Location"] = urlPrefix + relativePath + "/";
                response.Close();
                return;
            }
            filePath = Path.Combine(filePath, "index.html");
        }

        if (File.Exists(filePath))
        {
            response.StatusCode = 200;
            response.Headers["Server"] = "MiniHttpServer/1.0";

            byte[] buffer = await File.ReadAllBytesAsync(filePath);
            
            FileInfo fileInfo = new FileInfo(filePath);
            MimeTypeProvider.CheckType(response, fileInfo);

            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.Close();
        }
        else
        {
            await HandleNextAsync(context);
        }
    }
}