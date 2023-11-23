using Mango.Services.RewardsAPI.Models.Messages;

namespace Mango.Services.RewardsAPI.Services.Interfaces;

public interface IRewardsService
{
    Task UpdateRewards(RewardMessage rewardMessage);
}
