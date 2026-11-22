using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HomeWork5.Configuration;
using HomeWork5.Handlers;

namespace HomeWork5;

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
        
        var staticFilesHandler = new StaticFilesHandler();
        var controllersHandler = new ControllersHandler();
        var notFoundHandler = new NotFoundHandler();

        staticFilesHandler.SetNext(controllersHandler);
        controllersHandler.SetNext(notFoundHandler);
        
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                await staticFilesHandler.HandleAsync(context);
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

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _listener.Stop();
        _listener.Close();
        Console.WriteLine("Сервер остановлен.");
    }
}