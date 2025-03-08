using System.Text;
using Servers.ByteWork;

namespace Server.ByteWork.Tests;

public class SpanExtensionsShould
{
    [Theory]
    [InlineData("\r")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void IsNewLineReturnsTrue(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        Assert.True(span.IsNewLine());
    }

    [Theory]
    [InlineData("He")]
    [InlineData("T")]
    [InlineData("")]
    public void IsNewLineReturnsFalse(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        Assert.False(span.IsNewLine());
    }

    [Theory]
    [InlineData(" ")]
    public void IsWhiteSpaceReturnsTrue(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        Assert.True(span.IsWhiteSpace());
    }

    [Theory]
    [InlineData("H")]
    [InlineData("")]
    public void IsWhiteSpaceReturnsFalse(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        Assert.False(span.IsWhiteSpace());
    }
    
    [Theory]
    [InlineData(" Hello")]
    public void SkipWhiteSpaceReturnContentWithoutWhiteSpace(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        span = span.SkipWhiteSpace();
        Assert.Equal("Hello", Encoding.UTF8.GetString(span));
    }

    [Theory]
    [InlineData(" Hello")]
    [InlineData("  Hello")]
    [InlineData("   Hello")]
    [InlineData("       Hello")]
    [InlineData("           Hello")]
    public void SkipWhileWhiteSpaceReturnsCleanContent(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        span = span.SkipWhileWhiteSpace();
        Assert.Equal("Hello", Encoding.UTF8.GetString(span));
    }
    
    [Theory]
    [InlineData("\rHello")]
    [InlineData("\nHello")]
    [InlineData("\r\nHello")]
    public void SkipNewLineReturnContentWithoutNewLines(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        span = span.SkipNewLine();
        Assert.Equal("Hello", Encoding.UTF8.GetString(span));
    }
    
    [Theory]
    [InlineData("\rHello")]
    [InlineData("\r\rHello")]
    [InlineData("\r\r\rHello")]
    [InlineData("\nHello")]
    [InlineData("\n\nHello")]
    [InlineData("\n\n\nHello")]
    [InlineData("\r\nHello")]
    [InlineData("\r\n\r\nHello")]
    [InlineData("\r\n\r\n\r\nHello")]
    public void SkipWhileNewLineReturnsCleanContent(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        span = span.SkipWhileNewLine();
        Assert.Equal("Hello", Encoding.UTF8.GetString(span));
    }

    [Theory]
    [InlineData("Hello World")]
    [InlineData("Hello")]
    public void ConsumeTillWhiteSpaceReturnsContentWithoutWhiteSpace(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        span = span.ConsumeTillWhiteSpace(out ReadOnlySpan<byte> content);
        Assert.Equal("Hello", Encoding.UTF8.GetString(content));
    }

    [Fact]
    public void ConsumeTillWhitespaceCanHandleEmptySpan()
    {
        ReadOnlySpan<byte> span = ReadOnlySpan<byte>.Empty;
        span = span.ConsumeTillWhiteSpace(out ReadOnlySpan<byte> content);
        Assert.Equal("", Encoding.UTF8.GetString(content));
    }

    [Theory]
    [InlineData("Hello\rWorld")]
    [InlineData("Hello\nWorld")]
    [InlineData("Hello\r\nWorld")]
    [InlineData("Hello")]
    public void ConsumeTillNewLineReturnsContentWithoutNewLines(string value)
    {
        ReadOnlySpan<byte> span = Encoding.UTF8.GetBytes(value);
        span = span.ConsumeTillNewLine(out ReadOnlySpan<byte> content);
        Assert.Equal("Hello", Encoding.UTF8.GetString(content));
    }

    [Fact]
    public void ConsumeTillNewLineCanHandleEmptySpan()
    {
        ReadOnlySpan<byte> span = ReadOnlySpan<byte>.Empty;
        span = span.ConsumeTillNewLine(out ReadOnlySpan<byte> content);
        Assert.Equal("", Encoding.UTF8.GetString(content));
    }

    [Fact]
    public void ConsumeTill1()
    {
        ReadOnlySpan<byte> span = "HelloWorld"u8;
        span = span.ConsumeTill(x => x == (byte)'W', out ReadOnlySpan<byte> content);
        Assert.Equal("Hello", Encoding.UTF8.GetString(content));
    }
    
    [Fact]
    public void ConsumeTill2()
    {
        ReadOnlySpan<byte> span = "HelloWorld"u8;
        span = span.ConsumeTill(x => x == (byte)'o', out ReadOnlySpan<byte> content);
        Assert.Equal("Hell", Encoding.UTF8.GetString(content));
    }
    
    [Fact]
    public void ConsumeTill3()
    {
        ReadOnlySpan<byte> span = "HelloWorld"u8;
        span = span.ConsumeTill(x => x == (byte)'r', out ReadOnlySpan<byte> content);
        Assert.Equal("HelloWo", Encoding.UTF8.GetString(content));
    }
}

