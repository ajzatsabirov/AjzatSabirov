using System.Net;
using System.Text;
using System.Text.Json;

public class HttpServer
{
    private HttpListener _listener;
    private bool _isRunning;
    
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
        
        string prefix = $"http://{settings.Host}:{settings.Port}{path}";
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
    }
    
    public async Task StartAsync()
    {
        _listener.Start();
        _isRunning = true;
        Console.WriteLine("Сервер запущен и слушает запросы...");

        while (_isRunning) 
        {
            try
            {
                var context = await _listener.GetContextAsync();
                
                Console.WriteLine($"Получен запрос: {context.Request.HttpMethod} {context.Request.RawUrl}");
                
                var response = context.Response;
                
                string htmlFilePath = Path.Combine(AppContext.BaseDirectory, "search-engine.html");
                
                string htmlText;
                if (File.Exists(htmlFilePath))
                {
                    htmlText = File.ReadAllText(htmlFilePath, Encoding.UTF8);
                }
                else
                {
                    htmlText = "<html><body><h1>Файл search-engine.html не найден!</h1></body></html>";
                }
                
                response.ContentType = "text/html; charset=utf-8";
                byte[] buffer = Encoding.UTF8.GetBytes(htmlText);
                response.ContentLength64 = buffer.Length;
                
                await response.OutputStream.WriteAsync(buffer);
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
        Console.WriteLine("Сервер остановлен.");
    }
}