using Azure;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class CouponService : ICouponService
    {
        public readonly IHttpClientFactory _httpClientFactory;

        public CouponService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CouponDto> GetCouponAsync(string couponCode)
        {
            var client = _httpClientFactory.CreateClient("Coupon");
            var requestResponse = await client.GetAsync($"/api/coupon/GetByCode/{couponCode}");
            var apiContent = await requestResponse.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

            if (result != null && result.IsSuccess)
            {
                var resultInString = Convert.ToString(result.Result);
                return JsonConvert.DeserializeObject<CouponDto>(resultInString);
            }

            return new CouponDto();
        }
    }
}
