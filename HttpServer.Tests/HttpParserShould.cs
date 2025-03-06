using System.Text;

namespace HttpServer.Tests;

public class HttpParserShould
{
    [Fact]
    public void ThrowExceptionOnInvalidHttp_OnlyMethod()
    {
        string http =
            """
            GET
            """;
        
        V1HttpParser parser = new();
        Assert.Throws<HttpParserException>(() => parser.Parse(Encoding.UTF8.GetBytes(http)));
    }

    [Fact]
    public void ThrowExceptionOnInvalidHttp_OnlyMethodAndPath()
    {
        string http =
            """
            GET /
            """;
        
        V1HttpParser parser = new();
        Assert.Throws<HttpParserException>(() => parser.Parse(Encoding.UTF8.GetBytes(http)));
    }

    [Fact]
    public void ThrowExceptionOnInvalidHttp_OnlyMethodAndPathAndVersionNoBodySeparator()
    {
        string http =
            """
            GET / HTTP/1.1
            """;
        
        V1HttpParser parser = new();
        Assert.Throws<HttpParserException>(() => parser.Parse(Encoding.UTF8.GetBytes(http)));
    }
    
    [Fact]
    public void ParseNoHeadersNoBody_CheckNodeCount()
    {
        string http =
            """
            GET / HTTP/1.1
            
            """;
        
        V1HttpParser parser = new();
        IEnumerable<HttpNode> nodes = parser.Parse(Encoding.UTF8.GetBytes(http)).ToList();
        
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
        
        V1HttpParser parser = new();
        var content = Encoding.UTF8.GetBytes(http);
        IEnumerable<HttpNode> nodes = parser.Parse(content).ToList();
        
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
        
        V1HttpParser parser = new();
        IEnumerable<HttpNode> nodes = parser.Parse(Encoding.UTF8.GetBytes(http)).ToList();
        
        Assert.Equal(6, nodes.Count());
    }
}