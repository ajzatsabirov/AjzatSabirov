namespace HomeWork4.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class PostAttribute : Attribute
{
    public string Route { get; }

    public PostAttribute(string route)
    {
        Route = route;
    }
}