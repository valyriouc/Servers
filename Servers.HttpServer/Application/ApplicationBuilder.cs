using Servers.Logging;

namespace Servers.HttpServer.Application;

public class ApplicationBuilder
{
    private readonly Dictionary<string, HttpHandler> _handlers = new();

    public ApplicationBuilder WithEndpoint(string path, HttpHandler handler)
    {
        _handlers.Add(path, handler);
        return this;
    }

    public ApplicationBuilder WithEndpoint(string path, HttpEndpoint endpoint)
    {
        _handlers.Add(path, endpoint.HandleAsync);
        return this;
    }
}

public abstract class HttpEndpoint
{
    public abstract Task<IHttpPayload> HandleAsync(
        HttpRequest request,
        ILogger logger, 
        CancellationToken cancellationToken);
}