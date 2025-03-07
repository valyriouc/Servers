using Servers.Core;
using Servers.Logging;

namespace Servers.HttpServer;

public struct HttpServerConfiguration
{
    public bool IsTls { get; } = false;

    public HttpServerConfiguration(bool isTls)
    {
        IsTls = isTls;
    }
}

public class HttpServer : IServer
{
    private readonly IByteProvider _provider;
    private readonly SpecificLogger _logger;
    private readonly HttpParser _parser;

    public HttpServer(
        IByteProvider provider,
        ILogger logger)
    {
        _provider = provider;
        _logger = logger.For(nameof(HttpServer));
        _parser = new HttpParser(logger);
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        Memory<byte> buffer = new byte[1024];
        while (_provider.IsFinished)
        {
            int read = await _provider.ReadAsync(buffer, cancellationToken);
            _logger.Info($"Read {read} bytes");
            if (read == 0)
            {
                _parser.Reset();
                continue;
            }
            _logger.Info($"Parse http request...");
            
            
        }
    }
    
    public void Dispose() => _provider.Dispose();

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();
}