using Mango.Web.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        public async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
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
    }
}
