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
            var cartCouponDto = new CartCouponDto()
            {
                UserId = cartDto.CartHeader.UserId,
                CouponCode = cartDto.CartHeader.CouponCode,
            };

            var response = await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartCouponDto,
                Url = SD.ShoppingCartAPIBase + CART_ROUTE + "ApplyCoupon"
            });

            return response;
        }

        public async Task<ResponseDto?> EmailCart(CartDto cartDto)
        {
            var url = SD.ShoppingCartAPIBase + CART_ROUTE + "EmailCartRequest";

            var response = await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = url
            });

            return response;
        }

        public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ShoppingCartAPIBase + CART_ROUTE + $"GetCart/{userId}"
            });
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.ShoppingCartAPIBase + CART_ROUTE + $"RemoveCart/{id}"
            });
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBase + CART_ROUTE + "CartUpsert"
            });
        }
    }
}
