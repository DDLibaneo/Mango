using Mango.Web.Models;
using Mango.Web.Services;
using Mango.Web.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex()
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

		public async Task<IActionResult> ProductCreate()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ProductCreate(ProductDto model)
		{
            if (!ModelState.IsValid)
                return View(model);
            
			var response = await _productService.CreateProductAsync(model);

			if (response != null && response.IsSuccess)
			{
				TempData["success"] = "Product created successfully";
				return RedirectToAction(nameof(ProductIndex));
			}
			else
				TempData["error"] = response?.Message;
			
			return View(model);
		}

        public async Task<IActionResult> ProductEdit(int productId)
        {
            var response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                var result = Convert.ToString(response.Result);
                var model = JsonConvert.DeserializeObject<ProductDto>(result);

                return View(model);
            }
            else
                TempData["error"] = response?.Message;

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductDto productDto)
        {
            if (!ModelState.IsValid)
                return View(productDto);            

            var response = await _productService.UpdateProductAsync(productDto);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product edited successfully";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
                TempData["error"] = response?.Message;

            return View(productDto);
        }

        public async Task<IActionResult> ProductDelete(int productId)
        {
            var response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                var result = Convert.ToString(response.Result);
                var model = JsonConvert.DeserializeObject<ProductDto>(result);

                return View(model);
            }
            else
                TempData["error"] = response?.Message;

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDto productDto)
        {
            var response = await _productService.DeleteProductAsync(productDto.ProductId);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product deleted successfully";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
                TempData["error"] = response?.Message;

            return View(productDto);
        }
    }
}
