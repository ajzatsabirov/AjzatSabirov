using System.Net;

namespace MiniHttpServer.Framework.Content;

public static class MimeTypeProvider
{
    public static void CheckType(HttpListenerResponse response, FileInfo fileInfo)
    {
        string extension = fileInfo.Extension.ToLower();

        switch (extension)
        {
            case ".html":
            case ".htm":
                response.ContentType = "text/html; charset=utf-8";
                break;
            case ".css":
                response.ContentType = "text/css; charset=utf-8";
                break;
            case ".js":
                response.ContentType = "text/javascript; charset=utf-8";
                break;
            case ".json":
                response.ContentType = "application/json; charset=utf-8";
                break;
            case ".png":
                response.ContentType = "image/png";
                break;
            case ".jpg":
            case ".jpeg":
                response.ContentType = "image/jpeg";
                break;
            case ".gif":
                response.ContentType = "image/gif";
                break;
            case ".svg":
                response.ContentType = "image/svg+xml";
                break;
            case ".ico":
                response.ContentType = "image/x-icon";
                break;
            case ".woff":
                response.ContentType = "font/woff";
                break;
            case ".woff2":
                response.ContentType = "font/woff2";
                break;
            case ".pdf":
                response.ContentType = "application/pdf";
                break;
            default:
                response.ContentType = "application/octet-stream";
                break;
        }
    }
}