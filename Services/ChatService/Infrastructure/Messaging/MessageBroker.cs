using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using SocialApp.ChatService.Application.Interfaces;

namespace SocialApp.ChatService.Infrastructure.Messaging;

public class MessageBroker : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "socialapp.topic";
    private readonly ILogger<MessageBroker>? _logger;

    public MessageBroker(
        string hostName = "localhost",
        int port = 5672,
        string userName = "guest",
        string password = "guest",
        ILogger<MessageBroker>? logger = null)
    {
        _logger = logger;
        var factory = new ConnectionFactory()
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            DispatchConsumersAsync = true
        };

        int retryCount = 0;
        int maxRetries = 5;
        while (true)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);
                _logger?.LogInformation($"MessageBroker connected to {hostName}:{port}");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger?.LogError($"Failed to connect to RabbitMQ after {maxRetries} retries. Error: {ex.Message}");
                    throw;
                }
                _logger?.LogWarning($"RabbitMQ connection failed (attempt {retryCount}/{maxRetries}). Retrying in 2 seconds... Error: {ex.Message}");
                System.Threading.Thread.Sleep(2000);
            }
        }
    }

    public bool IsConnected => _connection?.IsOpen ?? false;

    public void Publish(string routingKey, string message)
    {
        if (!IsConnected)
        {
            _logger?.LogError("RabbitMQ connection is not open");
            throw new InvalidOperationException("RabbitMQ connection is not open");
        }

        try
        {
            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.DeliveryMode = 2; // Persistent delivery
            _channel.BasicPublish(exchange: _exchangeName, routingKey: routingKey, basicProperties: properties, body: body);

            _logger?.LogDebug($"Message published: RoutingKey={routingKey}, Size={body.Length}");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error publishing message: {ex.Message}");
            throw;
        }
    }

    public void Subscribe(string routingKey, Func<string, bool> onMessage)
    {
        try
        {
            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName, exchange: _exchangeName, routingKey: routingKey);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageContent = Encoding.UTF8.GetString(body);
                var result = onMessage(messageContent);
                if (result)
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
            };
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            _logger?.LogInformation($"Subscribed to routing key: {routingKey}");
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Error subscribing: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _logger?.LogInformation("MessageBroker disposed");
    }
}
