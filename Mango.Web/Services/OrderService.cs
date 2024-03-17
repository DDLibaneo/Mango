using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;

namespace Mango.Web.Services;

public class OrderService : IOrderService
{
    private readonly IBaseService _baseService;
    private readonly string ORDER_ROUTE = "/api/order/";

    public OrderService(IBaseService baseService)
    {
        _baseService = baseService;
    }

    public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
    {
        var request = new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = cartDto,
            Url = SD.OrderAPIBase + ORDER_ROUTE + "CreateOrder"
        };

        return await _baseService.SendAsync(request);
    }

    public async Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto)
    {
        var request = new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = stripeRequestDto,
            Url = SD.OrderAPIBase + ORDER_ROUTE + "CreateStripeSession"
        };

        return await _baseService.SendAsync(request);
    }

    public async Task<ResponseDto?> GetAllOrder(string? userId)
    {
        var request = new RequestDto
        {
            ApiType = SD.ApiType.GET,
            Url = SD.OrderAPIBase + ORDER_ROUTE + "GetOrders?userId=" + userId
        };

        var response = await _baseService.SendAsync(request);

        return response;
    }

    public async Task<ResponseDto?> GetOrder(int orderId)
    {
        var request = new RequestDto
        {
            ApiType = SD.ApiType.GET,
            Url = SD.OrderAPIBase + ORDER_ROUTE + "GetOrder/" + orderId
        };

        var response = await _baseService.SendAsync(request);

        return response;
    }

    public async Task<ResponseDto?> UpdateOrderStatus(int orderId, string newStatus)
    {
        var request = new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = newStatus,
            Url = SD.OrderAPIBase + ORDER_ROUTE + "UpdateOrderStatus/" + orderId
        };

        var response = await _baseService.SendAsync(request);

        return response;
    }

    public async Task<ResponseDto?> ValidateStripeSession(int orderHeaderId)
    {
        var request = new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = orderHeaderId,
            Url = SD.OrderAPIBase + ORDER_ROUTE + "ValidateStripeSession"
        };

        return await _baseService.SendAsync(request);
    }
}
