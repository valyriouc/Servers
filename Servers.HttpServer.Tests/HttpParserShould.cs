using System.Text;
using Servers.HttpServer;
using Servers.Logging;

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
        Assert.Throws<HttpParserException>(() => parser.Parse());
    }

    [Fact]
    public void ThrowExceptionOnInvalidHttp_OnlyMethodAndPath()
    {
        string http =
            """
            GET /
            """;
        
        HttpParser parser = new(new EmptyLogger());
        parser.Feed(Encoding.UTF8.GetBytes(http));
        Assert.Throws<HttpParserException>(() => parser.Parse());
    }

    [Fact]
    public void ThrowExceptionOnInvalidHttp_OnlyMethodAndPathAndVersionNoBodySeparator()
    {
        string http =
            """
            GET / HTTP/1.1
            """;
        
        HttpParser parser = new(new EmptyLogger());
        parser.Feed(Encoding.UTF8.GetBytes(http));
        Assert.Throws<HttpParserException>(() => parser.Parse());
    }
    
    [Fact]
    public void ParseNoHeadersNoBody_CheckNodeCount()
    {
        string http =
            """
            GET / HTTP/1.1
            
            """;
        
        HttpParser parser = new(new EmptyLogger());
        parser.Feed(Encoding.UTF8.GetBytes(http)); 
        parser.Parse();
        IEnumerable<HttpNode> nodes = parser.Retrieve();
        Assert.Equal(3, nodes.Count());
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
        parser.Feed(content);
        parser.Parse();
        IEnumerable<HttpNode> nodes = parser.Retrieve();
        Assert.Equal(5, nodes.Count());
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
        parser.Feed(Encoding.UTF8.GetBytes(http));
        parser.Parse();
        var nodes = parser.Retrieve();
        Assert.Equal(6, nodes.Count());
    }
}