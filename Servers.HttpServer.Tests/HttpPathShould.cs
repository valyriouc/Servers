using Servers.HttpServer;

namespace HttpServer.Tests;

public class HttpPathShould
{
    [Fact]
    public void ThrowWhenEmptyPath() => 
        Assert.Throws<HttpParserException>(() => HttpPath.Parse(""));

    [Theory]
    [InlineData("/")]
    [InlineData("/hello")]
    [InlineData("/hello/world")]
    public void ParsePathOnly(string path)
    {
        var result = HttpPath.Parse(path);
        Assert.Equal(path, result.Path);
    }

    [Theory]
    [InlineData("/hello/world?query=1", 1)]
    [InlineData("/hello/world?query=1&query2=2", 2)]
    public void ParsePathWithQuery(string path, int queryCount)
    {
        var result = HttpPath.Parse(path);
        Assert.Equal("/hello/world", result.Path);
        Assert.Equal(queryCount, result.Query.Count);
    }
}