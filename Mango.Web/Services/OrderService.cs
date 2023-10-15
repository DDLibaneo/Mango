﻿using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;

namespace Mango.Web.Services
{
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
                Url = SD.CouponAPIBase + ORDER_ROUTE + "CreateOrder"
            };

            return await _baseService.SendAsync(request);
        }
    }
}
