using System.Net;
using System.Net.Sockets;
using Servers.Logging;

namespace Servers.Core;

public readonly struct SocketServerConfiguration(
    SocketType socketType,
    AddressFamily addressFamily,
    ProtocolType protocolType,
    IPAddress ipAddress,
    ushort port)
{
    public ushort Port { get; } = port;

    public IPAddress IpAddress { get; } = ipAddress;

    public SocketType SocketType { get; } = socketType;

    public AddressFamily AddressFamily { get; } = addressFamily;

    public ProtocolType ProtocolType { get; } = protocolType;

    public SocketServerConfiguration(
        SocketType socketType,
        AddressFamily addressFamily,
        ProtocolType protocolType,
        string ipAddress,
        ushort port) : this(socketType, addressFamily, protocolType, IPAddress.Parse(ipAddress), port)
    {
        
    }
}

public class SocketServer<T> : IServerFoundation<T> 
    where T : IServer
{
    private readonly Func<IByteProvider, T> _serverFactory;
    private readonly SocketServerConfiguration _configuration;

    private readonly Socket _listener;
    private readonly List<Task> _tasks = new();

    private readonly SpecificLogger _logger;
    
    public SocketServer(
        SocketServerConfiguration configuration,    
        Func<IByteProvider, T> serverFactory,
        ILogger logger)
    {
        _configuration = configuration;
        _listener = new Socket(
            _configuration.AddressFamily,
            _configuration.SocketType,
            _configuration.ProtocolType);
        _serverFactory = serverFactory;
        _logger = logger.For(nameof(SocketServer<T>));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using Socket listener = new Socket(
            _configuration.AddressFamily,
            _configuration.SocketType,
            _configuration.ProtocolType);
        
        _logger.Info($"Starting server on {_configuration.IpAddress}:{_configuration.Port}");
        EndPoint endPoint = new IPEndPoint(_configuration.IpAddress, _configuration.Port);
        listener.Bind(endPoint);
        listener.Listen();

        while (!cancellationToken.IsCancellationRequested)
        {
            Socket client = await listener.AcceptAsync(cancellationToken);
            _logger.Info($"Accepted connection from {client.RemoteEndPoint}");
            T server = _serverFactory.Invoke(new SocketByteProvider(client)); 
            Task t = Task.Run(async () => await server.ProcessAsync(cancellationToken), cancellationToken);
            _tasks.Add(t);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task.WaitAll(_tasks.ToArray());
        if (_listener is IAsyncDisposable listenerAsyncDisposable)
            await listenerAsyncDisposable.DisposeAsync();
        else
            _listener.Dispose();
    }

    public void Dispose()
    {
        Task.WaitAll(_tasks.ToArray());
        _listener.Dispose();
    }
}

public class SocketByteProvider : IByteProvider
{
    private readonly Socket _socket;

    public bool IsFinished => !_socket.Connected;
    
    public SocketByteProvider(Socket socket)
    {
        _socket = socket;    
    }

    public void Dispose() => _socket.Dispose();

    public async ValueTask DisposeAsync()
    {
        if (_socket is IAsyncDisposable socketAsyncDisposable)
            await socketAsyncDisposable.DisposeAsync();
        else
            _socket.Dispose();
    }

    public async Task<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken) => 
        await _socket.ReceiveAsync(buffer, cancellationToken);

    public int Read(Span<byte> buffer) => _socket.Receive(buffer);

    public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken) => 
        await _socket.SendAsync(buffer, cancellationToken);

    public void Write(ReadOnlySpan<byte> buffer) => _socket.Send(buffer);

    public async Task FlushAsync(CancellationToken cancellationToken) => await Task.CompletedTask;
}