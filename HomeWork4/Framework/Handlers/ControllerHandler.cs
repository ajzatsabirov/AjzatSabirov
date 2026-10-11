using System.Net;
using System.Reflection;
using System.Text;
using HomeWork1.Framework.Attributes;

namespace HomeWork1.Framework.Handlers;

public class ControllerHandler : Handler
{
    private readonly Dictionary<string, (Type ControllerType, MethodInfo ActionMethod)> _routes = new(StringComparer.OrdinalIgnoreCase);

    public ControllerHandler()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            var controllerAttr = type.GetCustomAttribute<ControllerAttribute>();
            if (controllerAttr != null)
            {
                string controllerRoute = controllerAttr.Route;

                foreach (var method in type.GetMethods())
                {
                    var getAttr = method.GetCustomAttribute<GetAttribute>();
                    if (getAttr != null)
                    {
                        string path = $"/connection/{controllerRoute}/{getAttr.Route}";
                        _routes[$"GET:{path}"] = (type, method);
                    }

                    var postAttr = method.GetCustomAttribute<PostAttribute>();
                    if (postAttr != null)
                    {
                        string path = $"/connection/{controllerRoute}/{postAttr.Route}";
                        _routes[$"POST:{path}"] = (type, method);
                    }
                }
            }
        }

        Console.WriteLine($"[System] Маршрутизатор загружен. Найдено эндпоинтов: {_routes.Count}");
    }

    public override async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.Url.LocalPath.Contains("."))
        {
            if (Successor != null) await Successor.HandleRequest(context);
            return;
        }

        string method = request.HttpMethod;
        string fullPath = request.Url.LocalPath;
        
        string[] segments = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length < 3)
        {
            if (Successor != null) await Successor.HandleRequest(context);
            return;
        }
        
        // Базовый путь: /connection/auth/login (первые 3 сегмента)
        string basePath = $"/{segments[0]}/{segments[1]}/{segments[2]}";
        string routeKey = $"{method}:{basePath}";

        Console.WriteLine($"[Router] Ищу маршрут: {routeKey}");
        Console.WriteLine($"[Router] Полный путь: {fullPath}");
        Console.WriteLine($"[Router] Доступные маршруты: {string.Join(", ", _routes.Keys)}");

        if (_routes.TryGetValue(routeKey, out var endpoint))
        {
            Console.WriteLine($"[Router] Маршрут найден: {endpoint.ControllerType.Name}.{endpoint.ActionMethod.Name}");
            
            try
            {
                var controllerInstance = Activator.CreateInstance(endpoint.ControllerType);
                var methodParameters = endpoint.ActionMethod.GetParameters();
                var invokeArgs = new object[methodParameters.Length];
                
                int paramStartIndex = 3;

                for (int i = 0; i < methodParameters.Length; i++)
                {
                    var param = methodParameters[i];
                    int segmentIndex = paramStartIndex + i;

                    string stringValue = segmentIndex < segments.Length ? segments[segmentIndex] : string.Empty;
                    invokeArgs[i] = Convert.ChangeType(stringValue, param.ParameterType);
                    
                    Console.WriteLine($"[Params] Параметр {param.Name}: {stringValue}");
                }

                endpoint.ActionMethod.Invoke(controllerInstance, invokeArgs);

                response.StatusCode = 200;
                response.ContentType = "text/html; charset=utf-8";
                byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h1>Контроллер сработал! Смотри консоль.</h1></body></html>");
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                response.StatusCode = 500;
                byte[] buffer = Encoding.UTF8.GetBytes($"<h1>Ошибка: {ex.Message}</h1>");
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
                response.Close();
            }
            
            return;
        }

        Console.WriteLine($"[Router]  Маршрут не найден, передаю дальше");
        if (Successor != null)
        {
            await Successor.HandleRequest(context);
        }
        else
        {
            response.StatusCode = 404;
            response.Close();
        }
    }
}