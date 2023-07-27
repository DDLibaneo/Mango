using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Models.Dto.In;

namespace Mango.Services.ShoppingCartAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mapperConfig = new MapperConfiguration(c =>
            {
                c.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
                c.CreateMap<CartHeader, CartHeaderDtoIn>().ReverseMap();
                c.CreateMap<CartHeaderDto, CartHeaderDtoIn>().ReverseMap();
                c.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
                c.CreateMap<CartDetails, CartDetailsDtoIn>().ReverseMap();
                c.CreateMap<CartDetailsDto, CartDetailsDtoIn>().ReverseMap();
            });

            return mapperConfig;
        }
    }
}
