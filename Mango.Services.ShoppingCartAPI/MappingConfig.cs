using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mapperConfig = new MapperConfiguration(c =>
            {
                c.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
                c.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
            });

            return mapperConfig;
        }
    }
}
