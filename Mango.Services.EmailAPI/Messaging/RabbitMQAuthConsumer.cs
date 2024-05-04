using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging;

public class RabbitMQAuthConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
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

        //_emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
        //_registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");

        var userRegisteredQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
        _channel.QueueDeclare(userRegisteredQueue, false, false, false, null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (channel, eventArgs) =>
        {
            var content = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var email = JsonConvert.DeserializeObject<string>(content) ?? string.Empty;
            
            HandleMessage(email).GetAwaiter().GetResult();
            
            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        var userRegisteredQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
        _channel.BasicConsume(userRegisteredQueue, false, consumer);

        return Task.CompletedTask;
    }

    private async Task HandleMessage(string email)
    {
        await _emailService.RegisterUserEmailAndLog(email);
    }
}
