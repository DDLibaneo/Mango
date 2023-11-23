using Azure.Messaging.ServiceBus;
using Mango.Services.RewardsAPI.Models.Messages;
using Mango.Services.RewardsAPI.Services;
using Mango.Services.RewardsAPI.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardsAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string serviceBusConnectionString;
    private readonly string orderCreatedTopic;
    private readonly string orderCreatedRewardSubscription;
    private readonly IConfiguration _configuration;
    private readonly RewardsService _rewardsService;

    private ServiceBusProcessor _rewardProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration, RewardsService rewardsService)
    {
        _configuration = configuration;
        _rewardsService = rewardsService;
        
        serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
        orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        orderCreatedRewardSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");

        var client = new ServiceBusClient(serviceBusConnectionString);

        // this processor is listening to the queue  for any new messages that the queue has to send
        _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardSubscription);
    }

    public async Task Start()
    {
        _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
        _rewardProcessor.ProcessErrorAsync += ErrorHandler;

        await _rewardProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await _rewardProcessor.StopProcessingAsync();
        await _rewardProcessor.DisposeAsync();
    }

    private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs args)
    {
        // we receive the message here
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        var objMessage = JsonConvert.DeserializeObject<RewardMessage>(body);

        try
        {
            await _rewardsService.UpdateRewards(objMessage);

            // this will tell the data queue that this message has been processed successfully and they can
            // remove it from the queue.
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
