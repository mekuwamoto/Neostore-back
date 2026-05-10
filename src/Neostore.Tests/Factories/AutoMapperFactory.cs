using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Neostore.Application.Mappings;

namespace Neostore.Tests.Factories;

public static class AutoMapperFactory
{
    public static IMapper Create() =>
        new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();
}
