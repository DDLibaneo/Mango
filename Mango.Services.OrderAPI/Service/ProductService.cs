using Mango.Services.OrderAPI.Model.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Newtonsoft.Json;

namespace Mango.Services.OrderAPI.Service;

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
            return JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(response.Result));

        return new List<ProductDto>();
    }
}
