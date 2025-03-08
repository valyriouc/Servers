using System.Net.Sockets;
using System.Text;
using Servers.Core;
using Servers.HttpServer;
using Servers.HttpServer.Application;
using Servers.Logging;

namespace ServerPlayground;

public class TestServer : IServer
{
    private readonly SpecificLogger _logger;
    private readonly IByteProvider _provider;
    
    public TestServer(IByteProvider provider, ILogger logger)
    {
        _logger = logger.For(nameof(TestServer));
        _provider = provider;
    }

    public void Dispose() =>
        // TODO release managed resources here
        _provider.Dispose();

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        Memory<byte> buffer = new byte[1024];
        while (!_provider.IsFinished) 
        {
            var read = await _provider.ReadAsync(buffer, cancellationToken);
            if (read == 0) continue;
            string content = Encoding.UTF8.GetString(buffer.Span[..read]);
            _logger.Info(content);
        }
       
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        ILogger logger = ConsoleLogger.Start(DateTime.Now);
        await using SocketServer<HttpServer> server = new SocketServer<HttpServer>(
            new(
                SocketType.Stream,
                AddressFamily.InterNetwork, 
                ProtocolType.Tcp, 
                "127.0.0.1", 
                8080),
            (provider) => new HttpServer(provider, new HttpApplication(), logger),
            logger);

        CancellationTokenSource cts = new();
        await server.StartAsync(cts.Token);
    }
}