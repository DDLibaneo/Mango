using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;

namespace Mango.Web.Services
{
    public class CartService : ICartService
    {
        private readonly IBaseService _baseService;
        private readonly string CART_ROUTE = "/api/cart/";

        public CartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPI + CART_ROUTE + "ApplyCoupon"
            });
        }

        public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ShoppingCartAPI + CART_ROUTE + $"GetCart/{userId}"
            });
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.ShoppingCartAPI + CART_ROUTE + $"RemoveCart/{id}"
            });
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPI + CART_ROUTE + "CartUpsert"
            });
        }
    }
}
