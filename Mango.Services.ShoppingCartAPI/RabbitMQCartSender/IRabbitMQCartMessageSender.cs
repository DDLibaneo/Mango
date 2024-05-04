namespace Mango.Services.ShoppingCartAPI.RabbitMQCartSender;

public interface IRabbitMQCartMessageSender
{
    void SendMessage(object message, string queueName);
}
