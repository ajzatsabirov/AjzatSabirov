using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HomeWork4.Configuration;
using HomeWork4.Routing;

namespace HomeWork4.Handlers;

public class ControllersHandler : RequestHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        string prefix = "/" + ConfigurationManager.Instance.Settings.Path.Trim('/') + "/";
        string absolutePath = request.Url?.AbsolutePath ?? "/";
        string relativePath = absolutePath.StartsWith(prefix)
            ? absolutePath.Substring(prefix.Length)
            : absolutePath.TrimStart('/');

        string routeKey = $"{request.HttpMethod}:{relativePath}";

        if (!RouteRegistry.Routes.TryGetValue(routeKey, out var endpoint))
        {
            await HandleNextAsync(context);
            return;
        }

        try
        {
            var controllerInstance = Activator.CreateInstance(endpoint.ControllerType);
            var parameters = endpoint.ActionMethod.GetParameters();
            var args = new object[parameters.Length];

            var values = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (request.QueryString != null)
            {
                foreach (string key in request.QueryString.AllKeys)
                {
                    if (key != null) values[key] = request.QueryString[key];
                }
            }
            
            if (request.HttpMethod == "POST" &&
                request.ContentType != null &&
                request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string body = await reader.ReadToEndAsync();

                foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = pair.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        values[Uri.UnescapeDataString(parts[0])] =
                            Uri.UnescapeDataString(parts[1].Replace('+', ' '));
                    }
                }
            }
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];

                if (values.TryGetValue(param.Name, out string rawValue))
                {
                    try
                    {
                        args[i] = Convert.ChangeType(rawValue, param.ParameterType);
                    }
                    catch
                    {
                        await SendErrorAsync(response, 400, $"Некорректное значение параметра '{param.Name}'");
                        return;
                    }
                }
                else if (param.HasDefaultValue)
                {
                    args[i] = param.DefaultValue;
                }
                else
                {
                    await SendErrorAsync(response, 400, $"Отсутствует обязательный параметр '{param.Name}'");
                    return;
                }
            }

            var result = endpoint.ActionMethod.Invoke(controllerInstance, args);

            if (result is Task task) await task;

            string responseBody = result is string s ? s : "OK";
            byte[] buffer = Encoding.UTF8.GetBytes(responseBody);

            response.StatusCode = 200;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Router] Ошибка контроллера: {ex.InnerException?.Message ?? ex.Message}");
            await SendErrorAsync(response, 500, ex.InnerException?.Message ?? ex.Message);
        }
        finally
        {
            response.Close();
        }
    }

    private static async Task SendErrorAsync(HttpListenerResponse response, int code, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes($"<h1>{code}</h1><p>{message}</p>");
        response.StatusCode = code;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
    }
}