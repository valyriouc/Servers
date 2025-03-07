namespace Servers.HttpServer;

public class HttpResponse
{
    public string Version { get; }

    public HttpStatus StatusCode { get; }
    
    public Dictionary<string, string> Headers { get; }
    
    public Stream Body { get; }

    public HttpResponse(
        string version,
        HttpStatus statusCode,
        Dictionary<string, string> headers,
        Stream body)
    {
        Version = version;
        StatusCode = statusCode;
        Headers = headers;
        Body = body;
    }
}