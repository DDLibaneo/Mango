using Mango.Services.RewardsAPI.Services.Interfaces;
using Mango.Services.RewardsAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Mango.Services.RewardsAPI.Models;
using Mango.Services.RewardsAPI.Models.Messages;

namespace Mango.Services.RewardsAPI.Services;

public class RewardsService(DbContextOptions<AppDbContext> dbOptions) : IRewardsService
{
    private readonly DbContextOptions<AppDbContext> _dbOptions = dbOptions;

    public async Task UpdateRewards(RewardMessage rewardMessage)
    {
		try
		{
			var rewards = new Rewards
			{
				OrderId = rewardMessage.OrderId,
				RewardsActivity = rewardMessage.RewardsActivity,
                UserId = rewardMessage.UserId,
                RewardsDate = DateTime.Now
			};

			await using var _db = new AppDbContext(_dbOptions);
			await _db.Rewards.AddAsync(rewards);
			await _db.SaveChangesAsync();

		}
		catch (Exception ex)
		{
			throw;
		}
    }
}
