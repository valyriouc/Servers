using System.Text;

namespace Servers.HttpServer;

public class HttpResponse
{
    public string Version { get; }

    public HttpStatus StatusCode { get; }
    
    public Dictionary<string, string> Headers { get; }
    
    public StringBuilder Body { get; }

    public HttpResponse(
        string version,
        HttpStatus statusCode,
        Dictionary<string, string> headers,
        StringBuilder body)
    {
        Version = version;
        StatusCode = statusCode;
        Headers = headers;
        Body = body;
    }

    public static HttpResponse FromException(HttpStatusException ex) =>
        new(
            "HTTP/1.1",
            ex.State,
            new Dictionary<string, string>()
            {
                { "Server", "h4ck3r" },
                { "Content-Type", "text/plain" },
                { "Content-Length", $"{Encoding.UTF8.GetBytes(ex.Message).Length}"}
            },
            new StringBuilder(ex.Message));

    public static HttpResponse FromError(Exception ex) =>
        new(
            "HTTP/1.1",
            HttpStatus.InternalServerError,
            new Dictionary<string, string>()
            {
                { "Server", "h4ck3r" },
                { "Content-Type", "text/plain" },
                { "Content-Length", $"{Encoding.UTF8.GetBytes(ex.Message).Length}"}
            },
            new StringBuilder(ex.Message));
    
}