using System.Buffers;

namespace HttpServer;

public enum HttpNodeType
{
    Method,
    Path,
    Version,
    Header,
    Body,
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

public abstract class HttpParser
{
    public bool IsFinished { get; protected set;}
    
    protected HttpNodeType CurrentNode { get; set; }

    public HttpParser()
    {
        Reset();
    }
    
    protected void Reset()
    {
        CurrentNode = HttpNodeType.Method;
        IsFinished = false;
    }
    
    public IEnumerable<HttpNode> Parse(ReadOnlyMemory<byte> buffer)
    {
        while (!buffer.IsEmpty)
        {
            (int index, HttpNode node) = ParseNode(buffer.Span);
            buffer = buffer.Slice(index);
            yield return node;
        }
    }

    protected abstract (int, HttpNode) ParseNode(ReadOnlySpan<byte> buffer); 
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
    
    protected override (int, HttpNode) ParseNode(ReadOnlySpan<byte> buffer)
    {
        switch (CurrentNode)
        {
            case HttpNodeType.Method:
                var method = ParseMethod(buffer);
                CurrentNode = HttpNodeType.Path;
                return method;
            case HttpNodeType.Path:
                var path = ParsePath(buffer);
                CurrentNode = HttpNodeType.Version;
                return path;
            case HttpNodeType.Version:
                var version = ParseVersion(buffer);
                CurrentNode = HttpNodeType.Header;
                return version;
            case HttpNodeType.Header:
                var header = ParseHeader(buffer);
                CurrentNode = header.Item2 is null ? HttpNodeType.Body : HttpNodeType.Header;
                return header.Item2 is not null ? 
                    (header.Item1, (HttpNode)header.Item2!) : 
                    (header.Item1, new HttpNode(HttpNodeType.Header, []));
            case HttpNodeType.Body:
                var body = ParseBody(buffer);
                IsFinished = true;
                return body;
            default:
                throw new HttpParserException(
                    $"Invalid node type: {CurrentNode}");
        }
    }

    private (int, HttpNode) ParseMethod(ReadOnlySpan<byte> buffer)
    {
        int index = buffer.NextSpace();
        var method = buffer[..index];
        return (index + 1, new HttpNode(HttpNodeType.Method, method.ToArray()));
    }

    private (int, HttpNode) ParsePath(ReadOnlySpan<byte> buffer)
    {
        int index = buffer.NextSpace();
        var method = buffer[..index];
        return (index + 1, new HttpNode(HttpNodeType.Path, method.ToArray()));
    }
    
    private (int, HttpNode) ParseVersion(ReadOnlySpan<byte> buffer)
    {
        int index = buffer.NextSpace();
        var method = buffer[..index];
        return (index + 1, new HttpNode(HttpNodeType.Version, method.ToArray()));
    }

    private (int, HttpNode?) ParseHeader(ReadOnlySpan<byte> buffer)
    {
        
    }

    private (int, HttpNode) ParseBody(ReadOnlySpan<byte> buffer)
    {
        
    }
}

internal static class SpanExtensions
{
    public static int NextSpace(this ReadOnlySpan<byte> self)
    {
        int point = 0;
        while (self[point] != ' ')
        {
            point++;
        }

        return point + 1;
    }

    public static int NextNewLine(this ReadOnlySpan<byte> self)
    {
        int point = 0;
        while (self[point] != '\r')
        {
            point++;
        }
        
        return self[point + 1] == '\n' ? point + 2 : point + 1;
    }
}