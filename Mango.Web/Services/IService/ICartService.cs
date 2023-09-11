using Mango.Web.Models;

namespace Mango.Web.Services.IService
{
    public interface ICartService
    {
        Task<ResponseDto?> GetCartByUserIdAsync(string userId);

        Task<ResponseDto?> UpsertCartAsync(CartDto couponDto);

        Task<ResponseDto?> RemoveFromCartAsync(int id);

        Task<ResponseDto?> ApplyCouponsAsync(CartDto couponDto);
    }
}
