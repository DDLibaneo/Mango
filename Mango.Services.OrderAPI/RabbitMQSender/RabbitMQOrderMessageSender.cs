using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.OrderAPI.RabbitMQSender;

public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
{
    private readonly string _hostName;
    private readonly string _username;
    private readonly string _password;

    private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
    private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

    private IConnection _connection;

    public RabbitMQOrderMessageSender()
    {
        _hostName = "localhost";
        _username = "guest";
        _password = "guest";
    }

    public void SendMessage(object message, string exchangeName)
    {
        try
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: false);
                channel.QueueDeclare(OrderCreated_EmailUpdateQueue, false, false, false, null);
                channel.QueueDeclare(OrderCreated_RewardsUpdateQueue, false, false, false, null);

                channel.QueueBind(queue: OrderCreated_EmailUpdateQueue, exchange: exchangeName, routingKey: "EmailUpdate");
                channel.QueueBind(queue: OrderCreated_RewardsUpdateQueue, exchange: exchangeName, routingKey: "RewardsUpdate");

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: exchangeName, "EmailUpdate", null, body: body);
                channel.BasicPublish(exchange: exchangeName, "RewardsUpdate", null, body: body);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public void CreateConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                Password = _password,
                UserName = _username
            };

            _connection = factory.CreateConnection();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public bool ConnectionExists()
    {
        if (_connection != null)
            return true;

        CreateConnection();

        return true;
    }
}
