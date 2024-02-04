using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Model;
using Mango.Services.OrderAPI.Model.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using Mango.MessageBus;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderController(AppDbContext db, 
    IProductService _productService, IMapper _mapper, 
    IMessageBus _messageBus, IConfiguration _configuration) : ControllerBase
{
    protected ResponseDto _response = new();
    private readonly AppDbContext _db = db;

    [Authorize]
    [HttpGet("GetOrders")]
    public ResponseDto? Get(string? userId = "")
    {
        try
        {
            IEnumerable<OrderHeader> objList;

            if (User.IsInRole(SD.RoleAdmin))
            {
                objList = _db.OrderHeaders
                    .Include(o => o.OrderDetails)
                    .OrderByDescending(o => o.OrderHeaderId)
                    .ToList();
            }
            else
            {
                objList = _db.OrderHeaders
                    .Include(o => o.OrderDetails)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderHeaderId)
                    .ToList();
            }

            _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [Authorize]
    [HttpGet("GetOrders/{id:int}")]
    public ResponseDto? Get(int id)
    {
        try
        {
            var orderHeader = _db.OrderHeaders.Include(o => o.OrderDetails)
                .First(o => o.OrderHeaderId == id);

            _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [Authorize]
    [HttpPost("CreateOrder")]
    public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
    {
        try
        {
            var orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);

            orderHeaderDto.OrderTime = DateTime.Now;
            orderHeaderDto.Status = SD.Status_Pending;
            orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

            var orderCreated = _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto))
                .Entity;

            await _db.SaveChangesAsync();

            orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
            _response.Result = orderHeaderDto;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [Authorize]
    [HttpPost("CreateStripeSession")]
    public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                SuccessUrl = stripeRequestDto.ApprovedUrl,
                CancelUrl = stripeRequestDto.CancelUrl,
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };

            var discountsObj = new List<SessionDiscountOptions>()
            {
                new()
                {
                    Coupon = stripeRequestDto.OrderHeader.CouponCode
                }
            };

            foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20,99 -> 2099
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name
                        }
                    },
                    Quantity = item.Count
                };

                options.LineItems.Add(sessionLineItem);
            }

            if (stripeRequestDto.OrderHeader.Discount > 0)
            {
                options.Discounts = discountsObj; 
            }

            var service = new SessionService();
            var session = service.Create(options);

            stripeRequestDto.StripeSessionUrl = session.Url;

            var orderHeader = _db.OrderHeaders.First(o => o.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
            orderHeader.StripeSessionId = session.Id;

            _db.SaveChanges();

            _response.Result = stripeRequestDto;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.Message = ex.Message;
        }

        return _response;
    }

    [Authorize]
    [HttpPost("ValidateStripeSession")]
    public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
    {
        try
        {
            var orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);

            var service = new SessionService();

            var session = service.Get(orderHeader.StripeSessionId);

            var paymentIntentService = new PaymentIntentService();

            var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                orderHeader.PaymentIntentId = paymentIntent.Id;
                orderHeader.Status = SD.Status_Approved;

                await _db.SaveChangesAsync();

                var rewardsDto = new RewardsDto
                {
                    OrderId = orderHeader.OrderHeaderId,
                    RewardActivity = Convert.ToInt32(orderHeader.OrderTotal),
                    UserId = orderHeader.UserId
                };

                var topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");

                await _messageBus.PublishMessage(rewardsDto, topicName);

                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                
                return _response;
            }

            _response.Message = "payment failed";
            _response.IsSuccess = false;

            return _response;
        }
        catch (Exception ex)
        {
            _response.Message = ex.Message;
            _response.IsSuccess = false;
            
            return _response;
        }
    }

    [Authorize]
    [HttpPost("UpdateOrderStatus/{orderId:int}")]
    public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
    {
        try
        {
            var orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderId);

            if (orderHeader != null)
            {
                if (newStatus == SD.Status_Cancelled)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderHeader.PaymentIntentId
                    };

                    var service = new RefundService();
                    var refund = service.Create(options);
                    orderHeader.Status = newStatus;
                }

                orderHeader.Status = newStatus;

                _db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
        }

        return _response;
    }
}


