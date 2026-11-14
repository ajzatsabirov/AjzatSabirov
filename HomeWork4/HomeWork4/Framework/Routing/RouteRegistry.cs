using System;
using System.Collections.Generic;
using System.Reflection;
using HomeWork4.Attributes;

namespace HomeWork4.Routing;

public static class RouteRegistry
{
    public static Dictionary<string, (Type ControllerType, MethodInfo ActionMethod)> Routes { get; }
        = new(StringComparer.OrdinalIgnoreCase);

    static RouteRegistry()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var ctrl = type.GetCustomAttribute<ControllerAttribute>();
            if (ctrl == null) continue;

            foreach (var method in type.GetMethods())
            {
                var get = method.GetCustomAttribute<GetAttribute>();
                if (get != null) Routes[$"GET:{ctrl.Route}/{get.Route}"] = (type, method);

                var post = method.GetCustomAttribute<PostAttribute>();
                if (post != null) Routes[$"POST:{ctrl.Route}/{post.Route}"] = (type, method);
            }
        }
        Console.WriteLine($"[RouteRegistry] Загружено маршрутов: {Routes.Count}");
    }
}