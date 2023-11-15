using Mango.Web.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Net;
using Mango.Web.Utility;

namespace Mango.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CartController(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
    }

    [Authorize]
    public async Task<IActionResult> CartIndex()
    {
        return View(await LoadCartDtoBasedOnLoggedInUser());
    }

    [Authorize]
    public async Task<IActionResult> Checkout()
    {
        var cartDto = await LoadCartDtoBasedOnLoggedInUser();

        return View(cartDto);
    }
    
    [HttpPost]
    [ActionName("Checkout")]
    public async Task<IActionResult> Checkout(CartDto cartDto)
    {
        if (!TryValidateModel(cartDto))
            return BadRequest();

        var cartFromService = await LoadCartDtoBasedOnLoggedInUser();

        cartFromService.CartHeader.Phone = cartDto.CartHeader.Phone;
        cartFromService.CartHeader.Email = cartDto.CartHeader.Email;
        cartFromService.CartHeader.Name = cartDto.CartHeader.Name;

        var response = await _orderService.CreateOrder(cartFromService);

        var resultInString = Convert.ToString(response.Result);
        var orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(resultInString);

        if (response != null && response.IsSuccess) 
        {
            // get stripe session and redirect to stripe to place order

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";

            var stripedRequestDto = new StripeRequestDto
            {
                ApprovedUrl = domain + "cart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
                CancelUrl = domain + "cart/checkout",
                OrderHeader = orderHeaderDto
            };

            var stripeResponse = await _orderService.CreateStripeSession(stripedRequestDto);

            var stripeResponseString = Convert.ToString(stripeResponse.Result);
            var stripeResponseDeserialized = JsonConvert.DeserializeObject<StripeRequestDto>(stripeResponseString);

            Response.Headers.Add("Location", stripeResponseDeserialized.StripeSessionUrl);

            return new StatusCodeResult((int)HttpStatusCode.SeeOther);
        }

        return View();
    }

    public async Task<IActionResult> Confirmation(int orderId)
    {
        var response = await _orderService.ValidateStripeSession(orderId);

        if (response != null && response.IsSuccess)
        {
            var jsonResult = Convert.ToString(response.Result);
            var orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(jsonResult);

            if (orderHeader.Status == SD.Status_Approved)
                return View(orderId);
        }

        return View(orderId);
    }

    public async Task<IActionResult> Remove(int cartDetailsId)
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)
            ?.FirstOrDefault()
            ?.Value;

        var response = await _cartService.RemoveFromCartAsync(cartDetailsId);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(CartIndex));
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
    {
        var response = await _cartService.ApplyCouponAsync(cartDto);

        if (response != null && response.IsSuccess) 
        {
            TempData["success"] = "Coupon applied successfully";
            return RedirectToAction(nameof(CartIndex));
        };

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
    {
        cartDto.CartHeader.CouponCode = "";

        var response = await _cartService.ApplyCouponAsync(cartDto);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Coupon removed successfully";
            return RedirectToAction(nameof(CartIndex));
        };

        return View();
    }

    private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
    {
        var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)
            ?.FirstOrDefault()
            ?.Value;

        var response = await _cartService.GetCartByUserIdAsync(userId);

        if (response != null && response.IsSuccess)
        {
            var resultAsString = Convert.ToString(response.Result);
            var cartDto = JsonConvert.DeserializeObject<CartDto>(resultAsString);

            return cartDto;
        }

        return new CartDto();
    }

    [HttpPost]
    public async Task<IActionResult> EmailCart(CartDto cartDto)
    {
        var cart = await LoadCartDtoBasedOnLoggedInUser();

        cart.CartHeader.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)
            ?.FirstOrDefault()
            ?.Value;

        var response = await _cartService.EmailCart(cart);
        
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "email will be processed and sent shortly";
            return RedirectToAction(nameof(CartIndex));
        };

        return View();
    }
}
