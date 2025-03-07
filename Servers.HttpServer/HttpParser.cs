using System.Text;
using Servers.Logging;

namespace Servers.HttpServer;

public enum HttpNodeType
{
    Method,
    Path,
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

public readonly struct HttpNode
{
    public HttpNodeType Type { get; }

    public byte[] Value { get; }
    
    public HttpNode(HttpNodeType type, byte[] value)
    {
        Type = type;
        Value = value;
    }
}

public static class HttpNodeExtensions
{
    public static HttpMethod ToMethod(this HttpNode self)
    {
        if (self.Type is not HttpNodeType.Method)
        {
            throw new HttpParserException("Invalid node type: expected method");
        }
        
        return HttpMethod.GET;
    }

    public static string ToPath(this HttpNode self)
    {
        if (self.Type is not HttpNodeType.Path)
        {
            throw new HttpParserException("Invalid node type: expected path");
        }
        
        return Encoding.UTF8.GetString(self.Value);
    }

    public static string ToVersion(this HttpNode self)
    {
        if (self.Type is not HttpNodeType.Version)
        {
            throw new HttpParserException("Invalid node type: expected version");
        }
        
        return Encoding.UTF8.GetString(self.Value);
    }

    public static string ToHeader(this HttpNode self)
    {
        if (self.Type is not HttpNodeType.Header)
        {
            throw new HttpParserException("Invalid node type: expected header");
        }
        
        return Encoding.UTF8.GetString(self.Value);
    }
}

public enum ParserState
{
    Need,
    Parse,
    Done,
    Error
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
    public ParserState State { get; private set; }

    private readonly SpecificLogger _logger;
    private ReadOnlyMemory<byte> _buffer;

    private HttpNodeType _currentNode;
    private List<HttpNode> _nodes;
    
    public HttpParser(ILogger logger)
    {
        _logger = logger.For("HttpParser");
        _nodes = new();
        Reset();
    }

    public void Feed(ReadOnlyMemory<byte> buffer) => 
        _buffer = buffer;

    public void Reset()
    {
        State = ParserState.Need;
        _currentNode = HttpNodeType.Method;
        _nodes.Clear();
    }

    public IEnumerable<HttpNode> Retrieve() => _nodes;
    
    public void Parse()
    {
        while (State is ParserState.Parse)
        {
            HttpNode node = ParseNode();
            _nodes.Add(node);
        }
    }

    private HttpNode ParseNode()
    {
        ReadOnlySpan<byte> span = _buffer.Span;
        
        switch (_currentNode)
        {
            case HttpNodeType.Method:

                return new HttpNode(HttpNodeType.Method, []);
            case HttpNodeType.Path:
                
                return new HttpNode(HttpNodeType.Path, []);
                break;
            case HttpNodeType.Version:
                
                return new HttpNode(HttpNodeType.Version, []);
                break;
            case HttpNodeType.Header:
                
                return new HttpNode(HttpNodeType.Header, []);
                break;
            case HttpNodeType.Body:
                
                return new HttpNode(HttpNodeType.Body, []);
                break;
            default:
                throw new HttpParserException("Invalid node type");
        }
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
