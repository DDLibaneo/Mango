using Mango.Web.Models;

namespace Mango.Web.Services.IService
{
    public interface IOrderService
    {
        Task<ResponseDto?> CreateOrder(CartDto cartDto);
    }
}