using AutoMapper;
using BookCatalog.Application.Profiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookCatalog.Tests.TestUtils;

internal static class MapperFactory
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<BookProfile>(),
            NullLoggerFactory.Instance);
        return config.CreateMapper();
    }
}
