using Mango.Web.Models;
using Mango.Web.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
			var list = new List<ProductDto>();

			var response = await _productService.GetAllProductsAsync();

			if (response != null && response.IsSuccess)
			{
				var result = response.Result.ToString();

				list = JsonConvert.DeserializeObject<List<ProductDto>>(result);

				TempData["success"] = "Products found successfully";
			}
			else
				TempData["error"] = response?.Message;

			return View(list);
		}

        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDto productDto = new();

            var response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                var result = response.Result.ToString();

                productDto = JsonConvert.DeserializeObject<ProductDto>(result);

                TempData["success"] = "Products found successfully";
            }
            else
                TempData["error"] = response?.Message;

            return View(productDto);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}