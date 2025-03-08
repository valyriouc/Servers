using System.Text;
using Servers.HttpServer;
using Servers.Logging;
using HttpMethod = Servers.HttpServer.HttpMethod;

namespace HttpServer.Tests;

public class EmptyLogger : ILogger 
{
    public static ILogger Start(DateTime start)
    {
        return new EmptyLogger();
    }

    public void Log(string source, LogState state, string message)
    {
    }

    public void Log(string source, Exception ex)
    {
    }
}

public class HttpParserShould
{
    [Fact]
    public void ThrowExceptionOnInvalidHttp_OnlyMethod()
    {
        string http =
            """
            GET
            """;
        
        HttpParser parser = new(new EmptyLogger());
        Assert.Throws<HttpParserException>(() => parser.Parse(Encoding.UTF8.GetBytes(http)));
    }

    [Fact]
    public void ThrowExceptionOnInvalidHttp_OnlyMethodAndPath()
    {
        string http =
            """
            GET /
            """;
        
        HttpParser parser = new(new EmptyLogger());
        Assert.Throws<HttpParserException>(() => parser.Parse(Encoding.UTF8.GetBytes(http)));
    }
    
    [Fact]
    public void ParseNoHeadersNoBody_CheckNodeCount()
    {
        string http =
            """
            GET / HTTP/1.1
            """;
        
        HttpParser parser = new(new EmptyLogger());
        parser.Parse(Encoding.UTF8.GetBytes(http));
        HttpRequest request = parser.ToRequest();
        Assert.Equal(HttpMethod.GET, request.Method);
        Assert.Equal("/", request.Uri.Path);
        Assert.Equal("HTTP/1.1", request.Version);
    }

    [Fact]
    public void ParseSingleHeaderNoBody_CheckNodeCount()
    {
        string http =
            """
            GET / HTTP/1.1
            Host: testing.com
            
            """;
        
        HttpParser parser = new(new EmptyLogger());
        var content = Encoding.UTF8.GetBytes(http);
        parser.Parse(content);
        var request = parser.ToRequest();
        Assert.Equal(HttpMethod.GET, request.Method);
        Assert.Equal("/", request.Uri.Path);
        Assert.Equal("HTTP/1.1", request.Version);
        Assert.Single(request.Headers);
        Assert.Equal("testing.com", request.Headers["Host"]);
    }

    [Fact]
    public void ParseMutlipleHeadersNoBody()
    {
        string http =
            """
            GET / HTTP/1.1
            Host: testing.com
            X-Test: 123
            
            """;
        
        HttpParser parser = new(new EmptyLogger());
        var content = Encoding.UTF8.GetBytes(http);
        parser.Parse(content);
        var request = parser.ToRequest();
        Assert.Equal(HttpMethod.GET, request.Method);
        Assert.Equal("/", request.Uri.Path);
        Assert.Equal("HTTP/1.1", request.Version);
        Assert.Equal(2, request.Headers.Count);
        Assert.Equal("testing.com", request.Headers["Host"]);
        Assert.Equal("123", request.Headers["X-Test"]);
    }

    [Fact]
    public void ParseSingleHeaderWithBody_CheckNodeCount()
    {
        string http =
            """
            GET / HTTP/1.1
            Host: testing.com
            
            Hello world!
            """;
        
        HttpParser parser = new(new EmptyLogger());
        parser.Parse(Encoding.UTF8.GetBytes(http));
        var request = parser.ToRequest();
        Assert.Equal(HttpMethod.GET, request.Method);
        Assert.Equal(HttpMethod.GET, request.Method);
        Assert.Equal("/", request.Uri.Path);
        Assert.Equal("HTTP/1.1", request.Version);
        Assert.Single(request.Headers);
        Assert.Equal("testing.com", request.Headers["Host"]);

        Span<byte> body = new byte[256];
        var read = request.Body.Read(body);
        Assert.Equal("Hello world!", Encoding.UTF8.GetString(body[..read]));
    }
}