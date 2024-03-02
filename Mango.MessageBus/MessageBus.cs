using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        // tipicamente a connection string fica no appsettings de cada microservice. Cada microservice cria seu messageBus com a 
        // connection string especificada. Cada microservice pode ter seu próprio MessageBus, ou podem compartilhar um MessageBus geral.
        private string connectionString = "Endpoint=sb://mangoweb-daniel.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=0pAYCtKu5F34o9Xtms3P3bVLVPxgjCM1L+ASbDyLSvw=";

        public async Task PublishMessage(object message, string topic_queue_Name)
        {
            await using var client = new ServiceBusClient(connectionString);

            await using ServiceBusSender sender = client.CreateSender(topic_queue_Name);

            var jsonMessage = JsonConvert.SerializeObject(message);

            var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            var finalMessage = new ServiceBusMessage(messageBytes)
            { 
                CorrelationId = Guid.NewGuid().ToString()
            };

            await sender.SendMessageAsync(finalMessage);
        }
    }
}
