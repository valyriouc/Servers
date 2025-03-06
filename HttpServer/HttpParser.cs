using System.Text;

namespace HttpServer;

public enum HttpNodeType
{
    Method,
    Path,
    Version,
    Header,
    BodySep,
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

public abstract class HttpParser
{
    protected HttpNodeType CurrentNode { get; set; }

    public HttpParser()
    {
        Reset();
    }
    
    protected void Reset()
    {
        CurrentNode = HttpNodeType.Method;
    }
    
    public IEnumerable<HttpNode> Parse(ReadOnlySpan<byte> buffer)
    {
        List<HttpNode> nodes = new();
        
        while (!buffer.IsEmpty)
        {
            buffer = ParseNode(buffer, out var node);
            if (buffer.IsEmpty && node.Type is HttpNodeType.Method or HttpNodeType.Path)
            {
                throw new HttpParserException("Invalid request: requires method, path, version and body separator");
            }
            nodes.Add(node);
        }

        return nodes;
    }

    protected abstract ReadOnlySpan<byte> ParseNode(ReadOnlySpan<byte> buffer, out HttpNode node); 
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

public sealed class V1HttpParser : HttpParser
{
    public V1HttpParser() : base()
    {
        Reset();
    }
    
    protected override ReadOnlySpan<byte> ParseNode(ReadOnlySpan<byte> buffer, out HttpNode node)
    {
        switch (CurrentNode)
        {
            case HttpNodeType.Method:
                buffer = ParseMethod(buffer, out node);
                CurrentNode = HttpNodeType.Path;
                return buffer;
            case HttpNodeType.Path:
                buffer = ParsePath(buffer, out node);
                CurrentNode = HttpNodeType.Version;
                return buffer;
            case HttpNodeType.Version:
                buffer = ParseVersion(buffer, out node);
                CurrentNode = HttpNodeType.Header;
                return buffer;
            case HttpNodeType.Header:
                buffer = ParseHeader(buffer, out node);
                CurrentNode = node.Type is HttpNodeType.BodySep ? 
                    HttpNodeType.Body : 
                    HttpNodeType.Header;
                return buffer;
            case HttpNodeType.Body:
                if (buffer.IsEmpty)
                {
                    node = new HttpNode(HttpNodeType.Body, []);
                    return ReadOnlySpan<byte>.Empty;
                }
                buffer = ParseBody(buffer, out node);
                return buffer;
            default:
                throw new HttpParserException(
                    $"Invalid node type: {CurrentNode}");
        }
    }

    private ReadOnlySpan<byte> ParseMethod(ReadOnlySpan<byte> buffer, out HttpNode node)
    {
        buffer = buffer.ConsumeTillNextSpace(out var value)[1..];
        node = new HttpNode(HttpNodeType.Method, value);
        return buffer;
    }

    private ReadOnlySpan<byte> ParsePath(ReadOnlySpan<byte> buffer, out HttpNode node)
    {
       buffer = buffer.ConsumeTillNextSpace(out var value)[1..];
       node = new HttpNode(HttpNodeType.Path, value);
       return buffer;
    }
    
    private ReadOnlySpan<byte> ParseVersion(ReadOnlySpan<byte> buffer, out HttpNode node)
    {
        buffer = buffer.ConsumeTillNextNewLine(out byte[] value).ConsumeNewLine();
        node = new HttpNode(HttpNodeType.Version, value);
        return buffer;
    }

    private ReadOnlySpan<byte> ParseHeader(ReadOnlySpan<byte> buffer, out HttpNode node)
    {
        buffer = buffer.ConsumeTillNextNewLine(out byte[] value).ConsumeNewLine();
        if (value.Length == 0)
        {
            node = new HttpNode(HttpNodeType.BodySep, []);
            return buffer;
        }

        node = new HttpNode(HttpNodeType.Header, value);
        return buffer;
    }

    private ReadOnlySpan<byte> ParseBody(ReadOnlySpan<byte> buffer, out HttpNode node)
    {
        node = new HttpNode(HttpNodeType.Body, buffer.ToArray());
        return ReadOnlySpan<byte>.Empty;
    }
}

internal static class SpanExtensions
{
    public static ReadOnlySpan<byte> ConsumeTillNextSpace(this ReadOnlySpan<byte> self, out byte[] value)
    {
        int point = 0;
        while (self[point] != ' ')
        {
            point++;
        }

        value = self[..point].ToArray();
        return self[point..];
    }

    public static ReadOnlySpan<byte> ConsumeTillNextNewLine(this ReadOnlySpan<byte> self, out byte[] value)
    {
        int point = 0;
        while (self[point] != '\r' && self[point] != '\n')
        {
            point++;
        }

        value = self[..point].ToArray();
        return self[point..];
    }

    public static ReadOnlySpan<byte> ConsumeNewLine(this ReadOnlySpan<byte> self)
    {
        if (self[0] == '\r' && self[1] == '\n')
        {
            return self[2..];
        }

        return self[1..];
    }
}