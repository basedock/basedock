namespace BaseDock.Infrastructure.Security;

using StackExchange.Redis;

public sealed class RedisConnectionFactory : IDisposable
{
    private readonly Lazy<ConnectionMultiplexer> _connection;
    private bool _disposed;

    public RedisConnectionFactory(string connectionString)
    {
        _connection = new Lazy<ConnectionMultiplexer>(() =>
            ConnectionMultiplexer.Connect(connectionString));
    }

    public IDatabase GetDatabase() => _connection.Value.GetDatabase();

    public IServer GetServer()
    {
        var endpoints = _connection.Value.GetEndPoints();
        return _connection.Value.GetServer(endpoints[0]);
    }

    public bool IsConnected => _connection.IsValueCreated && _connection.Value.IsConnected;

    public void Dispose()
    {
        if (_disposed) return;

        if (_connection.IsValueCreated)
        {
            _connection.Value.Dispose();
        }

        _disposed = true;
    }
}
