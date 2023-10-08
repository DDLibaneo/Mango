using Mango.Web.Models;

namespace Mango.Web.Services.IService
{
    public interface ICartService
    {
        Task<ResponseDto?> GetCartByUserIdAsync(string userId);

        Task<ResponseDto?> UpsertCartAsync(CartDto cartDto);

        Task<ResponseDto?> RemoveFromCartAsync(int id);

        Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto);
        
        Task<ResponseDto?> EmailCart(CartDto cartDto);
    }
}
