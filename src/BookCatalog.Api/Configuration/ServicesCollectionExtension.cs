using BookCatalog.Application;
using BookCatalog.Application.Behaviors;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Book;
using BookCatalog.Application.Services.Isbn;
using BookCatalog.Infrastructure;
using BookCatalog.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
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

        AddServices(services);

        AddMediatR(services, configuration);

        AddLogging(services);

        AddValidators(services);

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

    public static void AddValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }

    public static void AddLogging(IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    }

    public static void AddAutomapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(ApplicationAssemblyMarker));
    }

    public static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IBookRepository, BookRepository>();
    }

    public static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IIsbnService, IsbnService>();
    }
}