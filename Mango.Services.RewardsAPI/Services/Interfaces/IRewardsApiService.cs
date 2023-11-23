using Mango.Services.RewardsAPI.Messages;

namespace Mango.Services.RewardsAPI.Services.Interfaces;

public interface IRewardsApiService
{
    Task UpdateRewards(RewardMessage rewardMessage);
}
