using AutoMapper;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;

namespace Mango.Services.ProductAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mapperConfig = new MapperConfiguration(c =>
            {
                c.CreateMap<ProductDto, Product>()
                 .ReverseMap();
            });

            return mapperConfig;
        }
    }
}
