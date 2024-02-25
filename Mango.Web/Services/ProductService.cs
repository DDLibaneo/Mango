﻿using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;

namespace Mango.Web.Services
{
	public class ProductService(IBaseService baseService) : IProductService
	{
		private readonly IBaseService _baseService = baseService;
		private readonly string PRODUCT_ROUTE = "/api/product/";

		public async Task<ResponseDto?> GetAllProductsAsync()
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.GET,
				Url = SD.ProductAPIBase + PRODUCT_ROUTE
			});
		}

		public async Task<ResponseDto?> GetProductByIdAsync(int id)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType= SD.ApiType.GET,
				Url = SD.ProductAPIBase + PRODUCT_ROUTE + id
			});
		}

		public async Task<ResponseDto?> CreateProductAsync(ProductDto productDto)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.POST,
				Data = productDto,
				Url = SD.ProductAPIBase + PRODUCT_ROUTE,
				ContentType = SD.ContentType.MultipartFormData
			});
		}

		public async Task<ResponseDto?> UpdateProductAsync(ProductDto productDto)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.PUT,
				Data = productDto,
				Url = SD.ProductAPIBase + PRODUCT_ROUTE,
				ContentType = SD.ContentType.MultipartFormData
			});
		}

		public async Task<ResponseDto?> DeleteProductAsync(int id)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.DELETE,
				Url = SD.ProductAPIBase + PRODUCT_ROUTE + id
			});
		}
	}
}
