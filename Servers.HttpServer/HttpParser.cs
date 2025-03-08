using System.Text;
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
    private Memory<byte> _buffer;

    private HttpNodeType _currentNode;
    private List<HttpNode> _nodes;

    private int _pointer = 0;
    
    public HttpParser(ILogger logger)
    {
        _logger = logger.For("HttpParser");
        _nodes = new();
        Reset();
    }

    public void Feed(ReadOnlyMemory<byte> buffer)
    {
        bool result = buffer.TryCopyTo(_buffer);
        if (!result)
        {
            State = ParserState.Error;
        }

        State = ParserState.Parse;
    }

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
            HttpNode? node = ParseNode();
            if (node is null)
            {
                continue;
            }
            _nodes.Add(node.Value);
        }
    }

    private HttpNode? ParseNode()
    {
        ReadOnlySpan<byte> span = _buffer[_pointer..].Span;
        
        switch (_currentNode)
        {
            case HttpNodeType.Method:
                _logger.Info("Parse http method!");
                _pointer += ParseMethod(span, out byte[] method);
                if (State is ParserState.Need)
                {
                    return null;
                }

                _currentNode = HttpNodeType.Path;
                return new HttpNode(HttpNodeType.Method, method);
            case HttpNodeType.Path:
                _logger.Info("Parse http path!");
                _pointer += ParsePath(span, out byte[] path);
                if (State is ParserState.Need)
                {
                    return null;
                }
                
                _currentNode = HttpNodeType.Version;
                return new HttpNode(HttpNodeType.Path, path);
            case HttpNodeType.Version:
                _logger.Info("Parse http version!");
                _pointer += ParseVersion(span, out byte[] version);
                if (State is ParserState.Need)
                {
                    return null;
                }
                
                _currentNode = HttpNodeType.Header;
                return new HttpNode(HttpNodeType.Version, version);
            case HttpNodeType.Header:
                _logger.Info("Parse http header!");
                _pointer += ParseHeader(span, out byte[]? header);
                if (State is ParserState.Need)
                {
                    return null;
                }
                
                _currentNode = header is null ? HttpNodeType.Body : HttpNodeType.Header;
                return header is null ? 
                    null : 
                    new HttpNode(HttpNodeType.Header, header);
            case HttpNodeType.Body:
                _logger.Info("Parse http body!");
                _pointer += ParseBody(span, out byte[] body);
                if (State is ParserState.Need)
                {
                    return null;
                }
                
                return new HttpNode(HttpNodeType.Body, body);
            default:
                throw new HttpParserException("Invalid node type");
        }
    }

    private int ParseMethod(ReadOnlySpan<byte> buffer, out byte[] method)
    {
        if (buffer.IsEmpty)
        {
            State = ParserState.Need;
        }
        
        method = [];
        if (buffer.Length < 3)
        {
            State = ParserState.Need;
            return 0;
        }
        
        int index = buffer.IndexOf((byte)' ');
        if (index == -1)
        {
            State = ParserState.Need;
            return 0;
        }

        method = buffer[..index].ToArray();
        return index + 1;
    }

    private int ParsePath(ReadOnlySpan<byte> buffer, out byte[] path)
    {
        path = [];
        if (buffer.IsEmpty)
        {
            State = ParserState.Need;
        }
        
        int index = buffer.IndexOf((byte)' ');
        if (index == -1)
        {
            State = ParserState.Need;
            return 0;
        }
        
        path = buffer[index..].ToArray();
        return index + 1;
    }

    private int ParseVersion(ReadOnlySpan<byte> buffer, out byte[] version)
    {
        version = [];
        if (buffer.IsEmpty)
        {
                
        }
        
        return 0;
    }

    private int ParseHeader(ReadOnlySpan<byte> buffer, out byte[]? header)
    {
        header = [];
        return 0;
    }

    private int ParseBody(ReadOnlySpan<byte> buffer, out byte[] body)
    {
        body = [];
        return 0;
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

public static class SpanExtensions
{
    public static ReadOnlySpan<byte> DetectNewLine(this ReadOnlySpan<byte> span)
    {
        while (span.Length > 0)
        {
            if (span[0] != (byte)'\n' && span[0] != (byte)'\r')
            {
                span = span[1..];
                continue;
            }
            
            return span[0] == (byte)'\r' && span[1] == (byte)'\n' ? 
                span[..2] :
                span[..1];
        }

        throw new HttpParserException("Invalid http request");
    }
}
