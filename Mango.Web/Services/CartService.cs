using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;

namespace Mango.Web.Services
{
    public class CartService : ICartService
    {
        private readonly IBaseService _baseService;
        private readonly string COUPON_ROUTE = "/api/cart/";

        public CartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> ApplyCouponsAsync(CartDto couponDto)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto couponDto)
        {
            throw new NotImplementedException();
        }
    }
}
