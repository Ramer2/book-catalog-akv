using BookCatalog.Application;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Infrastructure;
using BookCatalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Api.Configuration;

public static class ServicesCollectionExtension
{
    public static IServiceCollection AddSolutionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddEfCore(services, configuration);
        AddRepositories(services);
        
        AddMediatR(services, configuration);
        
        AddAutomapperProfiles(services);
        
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
    
    public static void AddMediatR(IServiceCollection services, IConfiguration configuration)
    {
        var mediatRLicense = configuration.GetSection("MediatRLicense").Value;
        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = mediatRLicense;
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        });
    }
    
    public static void AddAutomapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(ApplicationAssemblyMarker));
    }
    
    public static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IBookRepository, BookRepository>();
    }
}