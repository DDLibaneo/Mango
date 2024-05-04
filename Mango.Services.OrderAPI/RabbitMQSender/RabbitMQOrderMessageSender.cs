using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.OrderAPI.RabbitMQSender;

public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
{
    private readonly string _hostName;
    private readonly string _username;
    private readonly string _password;

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

                channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: false);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: exchangeName, string.Empty, null, body: body);
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
