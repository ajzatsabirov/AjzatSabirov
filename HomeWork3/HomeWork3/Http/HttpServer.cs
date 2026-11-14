using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HomeWork3;

namespace HomeWork3.Http;

public class HttpServer
{
    private HttpListener _listener;
    private bool _isRunning;

    public HttpServer()
    {
        _listener = new HttpListener();
    }

    public async Task StartAsync()
    {
        try
        {
            if (!File.Exists("settings.json"))
            {
                Console.WriteLine("Ошибка: файл settings.json не найден!");
                return;
            }

            var settings = ConfigurationManager.Instance.Settings;

            string path = settings.Path.Trim('/');
            string prefix = $"http://{settings.Host}:{settings.Port}/{path}/";

            _listener.Prefixes.Add(prefix);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine("Сервер запущен и слушает запросы...");
            Console.WriteLine($"Откройте в браузере: http://{settings.Host}:{settings.Port}/{path}/");
            Console.WriteLine("Введите 'stop' для остановки сервера...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при запуске сервера: {ex.Message}");
            return;
        }

        string urlPrefix = "/" + ConfigurationManager.Instance.Settings.Path.Trim('/') + "/";

        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();

                Console.WriteLine($"Получен запрос: {context.Request.HttpMethod} {context.Request.RawUrl}");

                var response = context.Response;

                response.Headers["Server"] = "MiniHttpServer/1.0";

                string requestPath = context.Request.Url?.AbsolutePath ?? "/";

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
                    continue;
                }

                string staticRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "static"));
                string filePath = Path.GetFullPath(Path.Combine(staticRoot, relativePath));

                if (!filePath.StartsWith(staticRoot + Path.DirectorySeparatorChar))
                {
                    await SendNotFoundAsync(response);
                    continue;
                }

                if (Directory.Exists(filePath) && !relativePath.EndsWith("/"))
                {
                    response.StatusCode = 302;
                    response.Headers["Location"] = urlPrefix + relativePath + "/";
                    response.Close();
                    continue;
                }
                
                if (Directory.Exists(filePath))
                {
                    filePath = Path.Combine(filePath, "index.html");
                }

                if (File.Exists(filePath))
                {
                    byte[] buffer = await File.ReadAllBytesAsync(filePath);
                    response.StatusCode = 200;

                    FileInfo fileInfo = new FileInfo(filePath);
                    MimeTypeChecker.CheckType(response, fileInfo);

                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer);
                }
                else
                {
                    await SendNotFoundAsync(response);
                }

                response.Close();
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке запроса: {ex.Message}");
            }
        }
    }

    private async Task SendNotFoundAsync(HttpListenerResponse response)
    {
        response.StatusCode = 404;

        string errorFilePath = Path.Combine(AppContext.BaseDirectory, "static", "404.html");

        byte[] buffer;

        if (File.Exists(errorFilePath))
        {
            buffer = await File.ReadAllBytesAsync(errorFilePath);
        }
        else
        {
            buffer = Encoding.UTF8.GetBytes("<html><body><h1>404 - Страница не найдена</h1></body></html>");
        }

        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _listener.Stop();
        _listener.Close();
        Console.WriteLine("Сервер остановлен.");
    }
}