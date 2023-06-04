using Mango.Web.Models;
using Mango.Web.Services.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        public async Task<IActionResult> CouponIndex()
        {
            var list = new List<CouponDto>();

            var response = await _couponService.GetAllCouponsAsync();

            if (response != null && response.IsSuccess)
            {
                var result = Convert.ToString(response.Result);
                list = JsonConvert.DeserializeObject<List<CouponDto>>(result);
            }

            return View(list);
        }
    }
}
