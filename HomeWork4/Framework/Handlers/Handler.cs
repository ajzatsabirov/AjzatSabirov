using System.Net;

namespace HomeWork1.Framework.Handlers;

public abstract class Handler
{
    public Handler Successor { get; set; }
    public abstract Task HandleRequest(HttpListenerContext context);
}