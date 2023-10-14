using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
        Task RegisterUserEmailAndLog(string email);
    }
}
