namespace HomeWork4.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ControllerAttribute : Attribute
{
    public string Route { get; }

    public ControllerAttribute(string route)
    {
        Route = route;
    }
}