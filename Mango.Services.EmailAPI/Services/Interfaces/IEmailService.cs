using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Models.Messages;

namespace Mango.Services.EmailAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
        Task RegisterUserEmailAndLog(string email);
        Task LogOrderPlaced(RewardMessage rewardDto);
    }
}
