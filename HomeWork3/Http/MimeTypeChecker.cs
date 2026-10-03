using System.Net;
using System.IO;

public static class MimeTypeChecker
{
    public static void CheckType(HttpListenerResponse response, FileInfo fileInfo)
    {
        string extension = fileInfo.Extension.ToLower();
        
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                response.ContentType = "image/jpeg";
                break;
                
            case ".png":
                response.ContentType = "image/png";
                break;
                
            case ".html":
            case ".htm":
                response.ContentType = "text/html; charset=utf-8";
                break;
                
            case ".css":
                response.ContentType = "text/css; charset=utf-8";
                break;
                
            case ".svg":
                response.ContentType = "image/svg+xml";
                break;

            default:
                // Если тип неизвестен, браузер просто скачает файл
                response.ContentType = "application/octet-stream";
                break;
        }
    }
}