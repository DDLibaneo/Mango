using AutoMapper;
using Mango.Services.OrderAPI.Model;
using Mango.Services.OrderAPI.Model.Dto;

namespace Mango.Services.OrderAPI;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var mapperConfig = new MapperConfiguration(c =>
        {
            c.CreateMap<OrderHeaderDto, CartHeaderDto>()
                .ForMember(destination => destination.CartTotal, i => i.MapFrom(source => source.OrderTotal))
                .ReverseMap();

            c.CreateMap<CartDetailsDto, OrderDetailsDto>()
                .ForMember(dest => dest.ProductName, i => i.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, i => i.MapFrom(src => src.Product.Price));

            c.CreateMap<OrderDetailsDto, CartDetailsDto>();

            c.CreateMap<OrderDetailsDto, OrderDetails>().ReverseMap();
            c.CreateMap<OrderHeaderDto, OrderHeader>().ReverseMap();
        });

        return mapperConfig;
    }
}
