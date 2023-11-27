using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Models.Messages;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string _serviceBusConnectionString;
    private readonly string _emailCartQueue;
    private readonly string _registerUserQueue;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private readonly string _orderCreated_Topic;
    private readonly string _orderCreated_Email_Subscription;

    private ServiceBusProcessor _emailOrderPlacedProcessor;
    private ServiceBusProcessor _emailCartProcessor;
    private ServiceBusProcessor _registerUserProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;
        
        _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
        
        _emailCartQueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
        _registerUserQueue = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
        
        _orderCreated_Topic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        _orderCreated_Email_Subscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Email_Subscription");

        var client = new ServiceBusClient(_serviceBusConnectionString);

        // this processor is listening to the queue  for any new messages that the queue has to send
        _emailCartProcessor = client.CreateProcessor(_emailCartQueue);
        _registerUserProcessor = client.CreateProcessor(_registerUserQueue);
        _emailOrderPlacedProcessor = client.CreateProcessor(_orderCreated_Topic, _orderCreated_Email_Subscription);
    }

    public async Task Start()
    {
        _emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
        _emailCartProcessor.ProcessErrorAsync += ErrorHandler;

        await _emailCartProcessor.StartProcessingAsync();

        _registerUserProcessor.ProcessMessageAsync += OnUserRegisterRequestReceived;
        _registerUserProcessor.ProcessErrorAsync += ErrorHandler;

        await _registerUserProcessor.StartProcessingAsync();

        _emailOrderPlacedProcessor.ProcessMessageAsync += OnOrderPlacedRequestReceived;
        _emailOrderPlacedProcessor.ProcessErrorAsync += ErrorHandler;

        await _emailOrderPlacedProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await _emailCartProcessor.StopProcessingAsync();
        await _emailCartProcessor.DisposeAsync();

        await _registerUserProcessor.StopProcessingAsync();
        await _registerUserProcessor.DisposeAsync();

        await _emailOrderPlacedProcessor.StopProcessingAsync();
        await _emailOrderPlacedProcessor.DisposeAsync();
    }

    private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
    {
        // we receive the message here
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        var objMessage = JsonConvert.DeserializeObject<CartDto>(body);

        try
        {
            await _emailService.EmailCartAndLog(objMessage);

            // this will tell the data queue that this message has been processed successfully and they can
            // remove it from the queue.
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);
        
        var email = JsonConvert.DeserializeObject<string>(body);

        try
        {
            await _emailService.RegisterUserEmailAndLog(email);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task OnOrderPlacedRequestReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        var rewardMessage = JsonConvert.DeserializeObject<RewardMessage>(body);

        try
        {
            await _emailService.LogOrderPlaced(rewardMessage);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        // the encountered error will be here and you can log it or send the error in an email
        // to someone for example.
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}
