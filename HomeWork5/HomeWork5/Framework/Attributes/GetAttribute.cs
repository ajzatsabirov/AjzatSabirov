namespace HomeWork5.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class GetAttribute : Attribute
{
    public string Route { get; }

    public GetAttribute(string route)
    {
        Route = route;
    }
}