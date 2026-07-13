namespace EmployeeManagement.Api.Messaging;

/// <summary>Publishes domain events onto RabbitMQ for interested consumers.</summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes <paramref name="payload"/> as JSON to the configured exchange,
    /// using <paramref name="routingKey"/> (e.g. "task.assigned", "task.completed", "project.removed").
    /// </summary>
    void Publish<T>(string routingKey, T payload);
}
