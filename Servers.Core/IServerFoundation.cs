namespace Servers.Core;

public interface IServer : IDisposable, IAsyncDisposable
{
    public Task ProcessAsync(CancellationToken cancellationToken);
}

public interface IServerFoundation<T> : IDisposable, IAsyncDisposable where T : IServer
{
    public Task StartAsync(CancellationToken cancellationToken);
}

public interface IByteProvider : IDisposable, IAsyncDisposable
{
    public bool IsFinished { get; }
    
    public Task<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken);
    
    public int Read(Span<byte> buffer);
    
    public Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);

    public void Write(ReadOnlySpan<byte> buffer);
    
    public Task FlushAsync(CancellationToken cancellationToken);
}