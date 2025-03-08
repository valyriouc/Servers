using Servers.HttpServer;

namespace HttpServer.Tests.Helpers;

public class SpanExtensionsShould
{
    [Fact]
    public void DetectsNewLineWithBackSlashR()
    {
        ReadOnlySpan<byte> span = "hello\rworld"u8;
        ReadOnlySpan<byte> result = span.DetectNewLine();
        Assert.Single(result.ToArray());
        Assert.Equal((byte)'\r', result[0]);
    }

    [Fact]
    public void DetectsNewLineWithBackSlashN()
    {
        ReadOnlySpan<byte> span = "hello\nworld"u8;
        ReadOnlySpan<byte> result = span.DetectNewLine();
        Assert.Single(result.ToArray());
        Assert.Equal((byte)'\n', result[0]);
    }

    [Fact]
    public void DetectsNewLineWithRN()
    {
        ReadOnlySpan<byte> span = "hello\r\nworld"u8;
        ReadOnlySpan<byte> result = span.DetectNewLine();
        Assert.Equal(2, result.Length);
        Assert.Equal("\r\n"u8.ToArray(), result.ToArray());
    }
}
