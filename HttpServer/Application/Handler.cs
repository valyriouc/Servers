using System.Text.Json;
using Servers.Logging;

namespace HttpServer.Application;

public class HttpResponseInfo
{
    public string Version { get; }
    
    public HttpStatus StatusCode { get; }
    
    public Dictionary<string, string> Headers { get; }
    

    public HttpResponseInfo(string version, HttpStatus statusCode, Dictionary<string, string> headers)
    {
        Version = version;
        StatusCode = statusCode;
        Headers = headers;
    }
}

public interface IHttpPayload
{
    public HttpResponseInfo Info { get; }
    
    public Task<Stream> IntoBytesAsync(CancellationToken cancellationToken);
}

public class JsonPayload<T> : IHttpPayload
{
    public HttpResponseInfo Info { get; }

    private readonly JsonSerializerOptions? _options;
    private readonly T _payload;
    
    public JsonPayload(
        HttpResponseInfo info, 
        T payload,
        JsonSerializerOptions? options=null)
    {
        Info = info;
        _options = options;
        _payload = payload;
    }
    
    public async Task<Stream> IntoBytesAsync(CancellationToken cancellationToken)
    {
        MemoryStream ms = new();
        await JsonSerializer.SerializeAsync(
            ms,
            _payload,
            _options,
            cancellationToken);
        return ms;
    }
}

public delegate Task<IHttpPayload> HttpHandler(
    HttpRequest request, 
    ILogger logger,
    CancellationToken cancellationToken);