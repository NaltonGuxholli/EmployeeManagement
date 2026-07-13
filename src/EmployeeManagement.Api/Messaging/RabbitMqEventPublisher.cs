using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace EmployeeManagement.Api.Messaging;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "employee-management.events";
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Publishes domain events (e.g. task assigned/completed, project removed,
/// user created) to a RabbitMQ topic exchange. Connection is lazily created
/// and reused; failures to publish are logged but never bubble up and break
/// the request that triggered them (event publishing is fire-and-forget).
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly Lazy<IConnection?> _connection;

    public RabbitMqEventPublisher(Microsoft.Extensions.Options.IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _connection = new Lazy<IConnection?>(CreateConnection);
    }

    private IConnection? CreateConnection()
    {
        if (!_options.Enabled) return null;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = false
            };
            return factory.CreateConnection("employee-management-api");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not connect to RabbitMQ at {Host}:{Port}. Events will not be published.",
                _options.HostName, _options.Port);
            return null;
        }
    }

    public void Publish<T>(string routingKey, T payload)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("RabbitMQ publishing disabled. Skipping event {RoutingKey}.", routingKey);
            return;
        }

        try
        {
            var connection = _connection.Value;
            if (connection is null)
            {
                _logger.LogWarning("No RabbitMQ connection available. Skipping event {RoutingKey}.", routingKey);
                return;
            }

            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var properties = channel.CreateBasicProperties();
            properties.ContentType = "application/json";
            properties.DeliveryMode = 2; // persistent
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            channel.BasicPublish(_options.ExchangeName, routingKey, properties, body);
            _logger.LogInformation("Published event {RoutingKey} to exchange {Exchange}.", routingKey, _options.ExchangeName);
        }
        catch (Exception ex)
        {
            // Event publishing must never fail the primary business operation.
            _logger.LogError(ex, "Failed to publish event {RoutingKey}.", routingKey);
        }
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value?.Dispose();
        }
    }
}
