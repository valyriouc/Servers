namespace Servers.ByteWork;

public static class ByteSpanExtensions
{
    public static ReadOnlySpan<byte> ConsumeTillWhiteSpace(this ReadOnlySpan<byte> span, out ReadOnlySpan<byte> content)
    {
        ReadOnlySpan<byte> s = span;
        
        int index = 0;
        while (!s.IsEmpty && !s.IsWhiteSpace())
        {
            index++;
            s = s[1..];
        }

        content = span[..index];
        return s;
    }

    public static ReadOnlySpan<byte> ConsumeTillNewLine(this ReadOnlySpan<byte> span, out ReadOnlySpan<byte> content)
    {
        ReadOnlySpan<byte> s = span;
        
        int index = 0;
        while (!s.IsEmpty && !s.IsNewLine())
        {
            index++;
            s = s[1..];
        }

        content = span[..index];
        return s;
    }

    public static ReadOnlySpan<byte> ConsumeTill(this ReadOnlySpan<byte> span, Func<byte, bool> func,
        out ReadOnlySpan<byte> content)
    {
        ReadOnlySpan<byte> s = span;
        
        int index = 0;
        while (!s.IsEmpty && !func(s[0]))
        {
            index++;
            s = s[1..];
        }

        content = span[..index];
        return s;
    }
    
    public static ReadOnlySpan<byte> SkipWhiteSpace(this ReadOnlySpan<byte> span)
    {
        if (span.IsEmpty)
        {
            return span;
        }
        
        if (span[0] == (byte)' ')
        {
            return span[1..];
        }
        
        return span;
    }
    
    public static ReadOnlySpan<byte> SkipWhileWhiteSpace(this ReadOnlySpan<byte> span)
    {
        while (span.IsWhiteSpace())
        {
            span = span.SkipWhiteSpace();
        }

        return span;
    }

    public static ReadOnlySpan<byte> SkipNewLine(this ReadOnlySpan<byte> span)
    {
        if (span.IsEmpty)
        {
            return span;
        }
        
        if (span[0] == (byte)'\r')
        {
            if (span.Length >= 2 && span[1] == (byte)'\n')
            {
                return span[2..];
            }

            return span[1..];
        }
        
        return span[0] == (byte)'\n' ? span[1..] : span;
    }

    public static ReadOnlySpan<byte> SkipWhileNewLine(this ReadOnlySpan<byte> span)
    {
        while (span.IsNewLine())
        {
            span = span.SkipNewLine();
        }

        return span;
    }
    
    public static bool IsNewLine(this ReadOnlySpan<byte> span) => 
        !span.IsEmpty && (span[0] == (byte)'\n' || span[0] == (byte)'\r');

    public static bool IsWhiteSpace(this ReadOnlySpan<byte> span) =>
        !span.IsEmpty && span[0] == (byte)' ';
}
