using System.Text;
using Servers.ByteWork;
using Servers.Logging;

namespace Servers.HttpServer;

public enum HttpNodeType
{
    Method,
    Path,
    Status,
    Version,
    Header,
    Body,
}

public enum HttpMethod 
{
    GET,
    POST,
    PUT,
    DELETE,
}


/// <summary>
/// GET / HTTP/1.1
/// header: myheader
/// header2: test
///
/// Some body content
/// </summary>
/// <returns></returns>
public sealed class HttpParser
{
    public bool IsFinished { get; set; } = false;

    private readonly SpecificLogger _logger;
    
    private HttpNodeType _currentNode = HttpNodeType.Method;
    
    private int _pointer = 0;

    private HttpMethod? _method;
    private HttpPath? _path;
    private string? _version;
    private Dictionary<string, string> _headers = new();
    private MemoryStream body = new();
    
    public HttpParser(ILogger logger) => 
        _logger = logger.For("HttpParser");

    public void Reset()
    {
        _method = null;
        _path = null;
        _version = null;
        _headers.Clear();
        body.SetLength(0);
    }
    
    public int Parse(ReadOnlySpan<byte> buffer)
    {
        while (true)
        {
            switch (_currentNode)
            {
                case HttpNodeType.Method:
                    buffer = buffer.SkipWhiteSpace();
                    if (buffer.IsEmpty)
                        throw new HttpParserException("Expected method");
                    buffer =buffer.ConsumeTillWhiteSpace(out var method);
                    _method = method.ToHttpMethod();
                    _currentNode = HttpNodeType.Path;
                    buffer = buffer.SkipWhiteSpace();
                    break;
                case HttpNodeType.Path:
                    if (buffer.IsEmpty)
                        throw new HttpParserException("Expected path");

                    buffer = buffer.SkipWhiteSpace();
                    buffer = buffer.ConsumeTillWhiteSpace(out var path);
                    _path = path.ToHttpPath();
                    _currentNode = HttpNodeType.Version;
                    buffer = buffer.SkipWhiteSpace();
                    break;
                case HttpNodeType.Version:
                    if (buffer.IsEmpty)
                        throw new HttpParserException("Expected version");

                    buffer = buffer.SkipWhiteSpace();
                    buffer = buffer.ConsumeTillNewLine(out var version);
                    _version = Encoding.UTF8.GetString(version);
                    _currentNode = HttpNodeType.Header;
                    buffer = buffer.SkipWhileNewLine();
                    break;
                case HttpNodeType.Header:
                    if (buffer.IsEmpty)
                    {
                        return 0;
                    }

                    buffer = buffer.ConsumeTillNewLine(out ReadOnlySpan<byte> header);
                    buffer = buffer.SkipNewLine();
                    
                    if (header.IsEmpty)
                    {
                        _currentNode = HttpNodeType.Body;
                        continue;
                    }

                    (string, string) r = header.ToHeader();
                    _headers.Add(r.Item1, r.Item2);
                    break;
                case HttpNodeType.Body:
                    if (buffer.IsEmpty)
                    {
                        return 0;
                    }

                    body.Write(buffer[..]);
                    return 0;
               default:
                    throw new HttpParserException("Invalid parser state");
            }
        }
    }
    
    public HttpRequest ToRequest()
    {
        body.Position = 0;
        return new HttpRequest(
            _method ?? throw new HttpParserException("Method is not set"),
            _path ?? throw new HttpParserException("Path is not set"),
            _version ?? throw new HttpParserException("Version is not set"),
            _headers,
            body);
    }
}

file static class LocalExtensions
{
    public static HttpMethod ToHttpMethod(this ReadOnlySpan<byte> buffer)
    {
        switch (buffer)
        {
            case var get when get.SequenceEqual(buffer):
                return HttpMethod.GET;
            case var post when post.SequenceEqual(buffer):
                return HttpMethod.POST;
            case var put when put.SequenceEqual(buffer):
                return HttpMethod.PUT;
            case var delete when delete.SequenceEqual(buffer):
                return HttpMethod.DELETE;
            default:
                throw new HttpParserException("Invalid method");
        }
    }

    public static HttpPath ToHttpPath(this ReadOnlySpan<byte> buffer)
    {
        string content = Encoding.UTF8.GetString(buffer);
        return HttpPath.Parse(content);
    }

    public static (string, string) ToHeader(this ReadOnlySpan<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return (string.Empty, string.Empty);
        }
        
        buffer = buffer.ConsumeTill((x) => x == (byte)':', out ReadOnlySpan<byte> key)[1..];
        buffer = buffer.SkipWhileWhiteSpace();
        return (Encoding.UTF8.GetString(key), Encoding.UTF8.GetString(buffer));
    }
}

public class HttpParserException : Exception
{
    public HttpParserException()
    {
    }

    public HttpParserException(string message) : base(message)
    {
    }

    public HttpParserException(string message, Exception inner) : base(message, inner)
    {
    }
}
