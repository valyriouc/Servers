using Servers.Core;
using Servers.HttpServer.Application;
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
    
    private readonly HttpApplication _application;
    
    public HttpServer(
        IByteProvider provider,
        HttpApplication application,
        ILogger logger)
    {
        _provider = provider;
        _logger = logger.For(nameof(HttpServer));
        _parser = new HttpParser(logger);
        _application = application;
    }

    private async Task<HttpRequest> ProcessRequestAsync(CancellationToken cancellationToken)
    {
        Memory<byte> buffer = new byte[1024];
        int offset = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            buffer[offset..].CopyTo(buffer[..]);
            int read = await _provider.ReadAsync(buffer[offset..], cancellationToken);
            _logger.Info($"Read {read} bytes");

            if (read == 0)
                _parser.IsFinished = true;
            
            _logger.Info($"Parse http request...");
            offset = _parser.Parse(buffer.Span[..read]);
        }

        return _parser.ToRequest();
    }

    private async Task ProcessResponseAsync(HttpResponse response, CancellationToken cancellationToken)
    {
        
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        while (!_provider.IsFinished)
        {
            try
            {
                _logger.Info("Processing request...");
                HttpRequest request = await ProcessRequestAsync(cancellationToken);
                _logger.Info("Server application is processing the request...");
                HttpResponse response = await _application.ServeAsync(request, cancellationToken);
                _logger.Info("Received response from server application.");
                await ProcessResponseAsync(response, cancellationToken);
            }
            catch (HttpStatusException ex)
            {
                _logger.Error(ex);
                HttpResponse response = HttpResponse.FromError(ex);
                await ProcessResponseAsync(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                HttpResponse response = HttpResponse.FromError(ex);
                await ProcessResponseAsync(response, cancellationToken);
            }
            finally
            {
                _parser.Reset();
                await _provider.FlushAsync(cancellationToken);
            }
        }
    }
    
    
    public void Dispose() => _provider.Dispose();

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();
}

public class HttpStatusException : Exception
{
    public HttpStatus State { get; }

    public HttpStatusException(HttpStatus state, string message) : base(message)
    {
        State = state;
    }

    public HttpStatusException(
        HttpStatus state,
        Exception ex) : base(ex.Message, ex)
    {
        State = state;
    }
    
    public HttpStatusException(
        HttpStatus state, 
        string message, 
        Exception inner) : base(message, inner)
    {
        State = state;
    }
    
    public static HttpStatusException BadRequest(string message = "") => 
        new(HttpStatus.BadRequest, message);
    
    public static HttpStatusException Unauthorized(string message = "") => 
        new(HttpStatus.Unauthorized, message);
    
    public static HttpStatusException Forbidden(string message = "") => 
        new(HttpStatus.Forbidden, message);
    
    public static HttpStatusException NotFound(string message = "") => 
        new(HttpStatus.NotFound, message);
    
    public static HttpStatusException InternalServerError(string message = "") => 
        new(HttpStatus.InternalServerError, message);

    public static HttpStatusException InternalServerError(Exception ex) =>
        new(HttpStatus.InternalServerError, ex);
    
    public static HttpStatusException NotImplemented(string message = "") => 
        new(HttpStatus.NotImplemented, message);
}

public static class IEnumerableExtensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (T item in source)
        {
            yield return item;
        }
        
        await Task.CompletedTask;
    }
}