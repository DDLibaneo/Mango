using Mango.Services.EmailAPI.Models.Messages;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging;

public class RabbitMQOrderConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    private readonly string ExchangeName = "";
    private readonly string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

    private IConnection _connection;
    private IModel _channel;

    public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;

        ExchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Password = "guest",
            UserName = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);

        _channel.QueueDeclare(OrderCreated_EmailUpdateQueue, false, false, false, null);

        _channel.QueueBind(OrderCreated_EmailUpdateQueue, ExchangeName, routingKey: "EmailUpdate");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (channel, eventArgs) =>
        {
            var content = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var reward = JsonConvert.DeserializeObject<RewardMessage>(content);

            HandleMessage(reward).GetAwaiter().GetResult();

            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume(OrderCreated_EmailUpdateQueue, false, consumer);

        return Task.CompletedTask;
    }

    private async Task HandleMessage(RewardMessage message)
    {
        await _emailService.LogOrderPlaced(message);
    }
}
