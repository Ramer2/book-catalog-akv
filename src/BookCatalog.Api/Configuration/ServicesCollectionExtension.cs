using BookCatalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Api.Configuration;

public static class ServicesCollectionExtension
{
    public static IServiceCollection AddSolutionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddEfCore(services, configuration);
        return services;
    }

    public static void AddEfCore(IServiceCollection services, IConfiguration configuration)
    {
        // var connectionString = configuration.GetConnectionString("DbConnection");
        services.AddDbContext<BookCatalogDbContext>(options =>
        {
            options.UseSqlite("Data Source=temp.db"); 
        });
    }
    
    
}