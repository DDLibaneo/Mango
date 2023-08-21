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
                c.CreateMap<CartHeaders, CartHeadersDto>().ReverseMap();
                c.CreateMap<CartHeaders, CartHeadersDtoIn>().ReverseMap();
                c.CreateMap<CartHeadersDto, CartHeadersDtoIn>().ReverseMap();

                c.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
                c.CreateMap<CartDetails, CartDetailsDtoIn>().ReverseMap();
                c.CreateMap<CartDetailsDto, CartDetailsDtoIn>().ReverseMap();

                c.CreateMap<CartCouponDto, CartDetailsDtoIn>().ReverseMap();
            });

            return mapperConfig;
        }
    }
}
