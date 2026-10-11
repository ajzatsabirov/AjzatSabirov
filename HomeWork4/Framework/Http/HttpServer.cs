using System.Net;
using System.Text;
using System.Text.Json;
using System.IO;
using HomeWork1.Framework.Handlers;

public class HttpServer
{
    private HttpListener _listener;
    private bool _isRunning;
    private string _serverUrl;
    
    public class ServerSettings
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8888;
        public string Path { get; set; } = "/connection/";
    }

    public HttpServer()
    {
        string json = File.ReadAllText("settings.json");
        var settings = JsonSerializer.Deserialize<ServerSettings>(json);
    
        string path = settings.Path;
        if (!path.StartsWith("/")) path = "/" + path;
        if (!path.EndsWith("/")) path += "/";
    
        _serverUrl = $"http://{settings.Host}:{settings.Port}{path}";
    
        _listener = new HttpListener();
        _listener.Prefixes.Add(_serverUrl);
    }
    
    public async Task StartAsync()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine("Сервер запущен и слушает запросы...");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Открой в браузере: {_serverUrl}");
        Console.ResetColor();
        
        while (_isRunning) 
        {
            try
            {
                var context = await _listener.GetContextAsync();
                
                Console.WriteLine($"Получен запрос: {context.Request.HttpMethod} {context.Request.RawUrl}");
                
                Handler h1 = new StaticFilesHandler();
                Handler h2 = new ControllerHandler();
                h2.Successor = h1;
                await h2.HandleRequest(context);
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
        Console.WriteLine("Сервер остановлен.");
    }
}