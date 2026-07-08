using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using SocialApp.UserService.Domain.Entities;
using SocialApp.UserService.Domain.Repositories;

namespace SocialApp.UserService.Infrastructure.Messaging;

public class RabbitMqListenerService : BackgroundService
{
    private readonly ILogger<RabbitMqListenerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly string _exchangeName = "socialapp.topic";

    public RabbitMqListenerService(
        ILogger<RabbitMqListenerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        try
        {
            var rabbitConfig = configuration.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["HostName"] ?? "localhost",
                Port = int.TryParse(rabbitConfig["Port"], out int port) ? port : 5672,
                UserName = rabbitConfig["UserName"] ?? "guest",
                Password = rabbitConfig["Password"] ?? "guest",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic, durable: true);
            
            // Declare and bind the queue for UserService
            var queueName = "user.service.registered.queue";
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queueName, exchange: _exchangeName, routingKey: "auth.user.registered");

            _logger.LogInformation("RabbitMQ Connected & Listening for auth.user.registered events.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ");
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null) return Task.CompletedTask;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message: {Message}", message);

                await ProcessMessageAsync(message, stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: "user.service.registered.queue", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(string message, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var userEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(message);
        if (userEvent != null && userEvent.UserId != Guid.Empty)
        {
            var existingProfile = await userRepository.GetByAuthUserIdAsync(userEvent.UserId, stoppingToken);
            if (existingProfile == null)
            {
                var newProfile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    AuthUserId = userEvent.UserId,
                    Username = userEvent.Username ?? string.Empty,
                    Email = userEvent.Email ?? string.Empty,
                    Phone = userEvent.Phone ?? string.Empty,
                    FirstName = userEvent.Username ?? string.Empty, // Default value
                    LastName = string.Empty,
                    IsActive = true
                };

                await userRepository.AddAsync(newProfile, stoppingToken);
                await userRepository.SaveChangesAsync(stoppingToken);
                
                _logger.LogInformation("Created UserProfile for AuthUserId: {AuthUserId}", userEvent.UserId);
            }
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
