namespace HttpServer;

public sealed class HttpRequest
{
    public HttpMethod Method { get; }
    
    public HttpPath Uri { get; }
    
    public string Version { get; }
    
    public Dictionary<string, string> Headers { get; }
    
    public Stream Body { get; }

    public HttpRequest(
        HttpMethod method,
        HttpPath uri,
        string version,
        Dictionary<string, string> headers,
        Stream body)
    {
        Method = method;
        Uri = uri;
        Version = version;
        Headers = headers;
        Body = body;
    }
}

public class HttpPath
{
    public string Path { get; }
    
    public IEnumerable<string> PathIter => Path.Split('/');

    public Dictionary<string, string> Query { get; }
    

    public HttpPath(string path, Dictionary<string, string> query)
    {
        Path = path;
        Query = query;
    }

    internal static HttpPath Parse(string value)
    {
        var parts = value.Split('?');
        if (parts.Length > 2)
        {
            throw new HttpParserException("Invalid path: expected query");
        }
        
        string path = parts[0];
        var pairs = parts[1].Split('&');
        var query = new Dictionary<string, string>();
        
        foreach (var pair in pairs)
        {
            var kv = pair.Split('=');
            query.Add(kv[0], kv[1]);
        }

        return new HttpPath(path, query);
    }
}