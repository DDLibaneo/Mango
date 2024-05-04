using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging;

public class RabbitMQCartConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    private IConnection _connection;
    private IModel _channel;

    public RabbitMQCartConsumer(IConfiguration configuration, EmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        var userRegisteredQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
        _channel.QueueDeclare(userRegisteredQueue, false, false, false, null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (channel, eventArgs) =>
        {
            var content = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var cart = JsonConvert.DeserializeObject<CartDto>(content);

            HandleMessage(cart).GetAwaiter().GetResult();

            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        var userRegisteredQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
        _channel.BasicConsume(userRegisteredQueue, false, consumer);

        return Task.CompletedTask;
    }

    private async Task HandleMessage(CartDto cart)
    {
        await _emailService.EmailCartAndLog(cart);
    }
}
