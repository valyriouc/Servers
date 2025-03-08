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

    public static HttpResponse FromError(HttpStatusException ex) =>
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

    internal IEnumerable<HttpNode> ToNodes()
    {
        yield return new HttpNode(HttpNodeType.Version, Encoding.UTF8.GetBytes(Version));
        yield return new HttpNode(HttpNodeType.Status, Encoding.UTF8.GetBytes($"{(int)StatusCode} {StatusCode}\r\n"));

        foreach (var header in Headers)
        {
            yield return new HttpNode(HttpNodeType.Header, Encoding.UTF8.GetBytes($"{header.Key}: {header.Value}\r\n"));
        }
        
        yield return new HttpNode(HttpNodeType.Body, Encoding.UTF8.GetBytes(Body.ToString()));
    }
}