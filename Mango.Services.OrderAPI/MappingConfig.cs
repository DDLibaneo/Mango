using AutoMapper;

namespace Mango.Services.OrderAPI;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var mapperConfig = new MapperConfiguration(c =>
        {

        });

        return mapperConfig;
    }
}
