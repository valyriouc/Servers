namespace Servers.Core;

public interface IServer
{
    public Task<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
}

public interface IServerFoundation<T> where T : IServer
{
    public Task StartAsync(CancellationToken cancellationToken);
}