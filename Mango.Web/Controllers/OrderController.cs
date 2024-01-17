using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class OrderController(IOrderService orderService) : Controller
    {
        private readonly IOrderService _orderService = orderService;

        public IActionResult OrderIndex()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll() 
        {
            IEnumerable<OrderHeaderDto> list;

            string userId = string.Empty;

            if (!User.IsInRole(SD.RoleAdmin))
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

            var response = _orderService.GetAllOrder(userId).GetAwaiter().GetResult();

            if (response != null && response.IsSuccess)
                list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
            else 
                list = new List<OrderHeaderDto>();

            return Json(new { data = list });
        }
    }
}
