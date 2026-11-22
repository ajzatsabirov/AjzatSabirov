using System.Reflection;
using MiniHttpServer.Framework.Attributes;

namespace MiniHttpServer.Framework.Routing;

public static class RouteRegistry
{
    public static Dictionary<string, (Type ControllerType, MethodInfo ActionMethod)> Routes { get; }
        = new Dictionary<string, (Type, MethodInfo)>(StringComparer.OrdinalIgnoreCase);

    static RouteRegistry()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            foreach (Type type in types)
            {
                ControllerAttribute ctrl = type.GetCustomAttribute<ControllerAttribute>();
                if (ctrl == null) continue;

                foreach (MethodInfo method in type.GetMethods())
                {
                    GetAttribute get = method.GetCustomAttribute<GetAttribute>();
                    if (get != null)
                    {
                        Routes[$"GET:{ctrl.Route}/{get.Route}"] = (type, method);
                    }

                    PostAttribute post = method.GetCustomAttribute<PostAttribute>();
                    if (post != null)
                    {
                        Routes[$"POST:{ctrl.Route}/{post.Route}"] = (type, method);
                    }
                }
            }
        }

        Console.WriteLine($"[RouteRegistry] Загружено маршрутов: {Routes.Count}");
    }
}