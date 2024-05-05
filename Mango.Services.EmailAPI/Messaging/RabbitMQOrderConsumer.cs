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

    private IConnection _connection;
    private IModel _channel;

    private string _queueName = string.Empty;

    public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
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

        var exchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

        _queueName = _channel.QueueDeclare().QueueName;

        _channel.QueueBind(_queueName, exchangeName, string.Empty);
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

        _channel.BasicConsume(_queueName, false, consumer);

        return Task.CompletedTask;
    }

    private async Task HandleMessage(RewardMessage message)
    {
        await _emailService.LogOrderPlaced(message);
    }
}
