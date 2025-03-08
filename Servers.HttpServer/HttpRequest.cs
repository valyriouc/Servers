namespace Servers.HttpServer;

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

    public static HttpPath Parse(string value)
    {
        if (value.Length == 0)
        {
            throw new HttpParserException("Invalid path: empty");
        }
        
        var parts = value.Split('?');
        string path = parts[0]; 
        
        var query = new Dictionary<string, string>();
        if (parts.Length == 2)
        {
            var pairs = parts[1].Split('&');
        
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=');
                query.Add(kv[0], kv[1]);
            }    
        }

        return new HttpPath(path, query);
    }
}