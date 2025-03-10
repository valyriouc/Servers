namespace Servers.HttpServer.Application;

/// <summary>
/// Does validation and checks if the request matches all the requirements
/// and then passes it into a handler that is responsible for the request
/// </summary>
public class HttpApplication 
{
    internal async Task<HttpResponse> ServeAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        throw HttpStatusException.BadRequest("Invalid request");
        await Task.CompletedTask;
    }
}