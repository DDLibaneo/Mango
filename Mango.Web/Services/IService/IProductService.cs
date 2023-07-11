using Mango.Web.Models;

namespace Mango.Web.Services.IService
{
    public interface IProductService
    {
        Task<ResponseDto?> GetProductByIdAsync(int id);

        Task<ResponseDto?> GetAllProductsAsync();

        Task<ResponseDto?> CreateProductAsync(ProductDto productDto);

        Task<ResponseDto?> UpdateProductAsync(ProductDto productDto);

        Task<ResponseDto?> DeleteProductAsync(int id);
    }
}
