using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace Mango.Services.ShoppingCartAPI.Service
{
    public class ProductService : IProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            var client = _httpClientFactory.CreateClient("Product");
            var responseRequest = await client.GetAsync($"/api/product");
            var apiContent = await responseRequest.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

            if (response != null && response.IsSuccess) 
            {
                return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(response.Result));
            }

            return new List<ProductDto>();
        }
    }
}
