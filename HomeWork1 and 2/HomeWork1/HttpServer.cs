using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HomeWork1;

public class HttpServer
{
    private HttpListener _listener;
    private bool _isRunning;
    private ServerSettings _settings;

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

            string json = await File.ReadAllTextAsync("settings.json");
            _settings = JsonSerializer.Deserialize<ServerSettings>(json);

            if (_settings == null)
            {
                Console.WriteLine("Ошибка: не удалось прочитать настройки сервера.");
                return;
            }

            string path = _settings.Path.Trim('/');
            string prefix = $"http://{_settings.Host}:{_settings.Port}/{path}/";
            
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine("Сервер запущен и слушает запросы...");
            Console.WriteLine($"Откройте в браузере: http://{_settings.Host}:{_settings.Port}/{path}/");
            Console.WriteLine("Введите 'stop' для остановки сервера...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при запуске сервера: {ex.Message}");
            return;
        }

        while (_isRunning) 
        {
            try
            {
                var context = await _listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                Console.WriteLine($"Получен запрос: {request.HttpMethod} {request.Url.AbsolutePath}");

                if (request.Url.AbsolutePath.Contains("/img/") && request.Url.AbsolutePath.EndsWith(".svg"))
                {
                    string urlPath = request.Url.AbsolutePath;
                    string pathPrefix = "/" + _settings.Path.Trim('/') + "/";
                    string relativePath = urlPath.Substring(pathPrefix.Length);
                    string filePath = Path.Combine(AppContext.BaseDirectory, relativePath);

                    if (File.Exists(filePath))
                    {
                        byte[] buffer = await File.ReadAllBytesAsync(filePath);
                        response.ContentType = "image/svg+xml";
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer);
                    }
                    else
                    {
                        Console.WriteLine($"Файл не найден: {filePath}");
                        response.StatusCode = 404;
                    }
                }
                else
                {
                    string htmlFilePath = Path.Combine(AppContext.BaseDirectory, "search-engine.html");
                    
                    if (File.Exists(htmlFilePath))
                    {
                        string htmlText = await File.ReadAllTextAsync(htmlFilePath, Encoding.UTF8);
                        byte[] buffer = Encoding.UTF8.GetBytes(htmlText);
                        response.ContentType = "text/html; charset=utf-8";
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer);
                    }
                    else
                    {
                        response.StatusCode = 404;
                        string error = "<h1>Файл search-engine.html не найден!</h1>";
                        byte[] buffer = Encoding.UTF8.GetBytes(error);
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer);
                    }
                }
                
                response.Close();
            }
            catch (HttpListenerException)
            {
                break; 
            }
        }
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