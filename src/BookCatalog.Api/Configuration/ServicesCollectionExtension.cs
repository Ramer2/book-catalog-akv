using BookCatalog.Api.ExceptionHandling.Filters;
using BookCatalog.Application;
using BookCatalog.Application.Behaviors;
using BookCatalog.Application.Interfaces.Persistence;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Interfaces.Transactions;
using BookCatalog.Application.Services.Author;
using BookCatalog.Application.Services.Book;
using BookCatalog.Application.Services.Isbn;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Application.Services.User;
using BookCatalog.Infrastructure;
using BookCatalog.Infrastructure.Persistence;
using BookCatalog.Infrastructure.Repositories;
using BookCatalog.Infrastructure.Transactions;
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
        AddExceptionFilters(services);
        AddEfCore(services, configuration);
        AddRepositories(services);

        AddServices(services);

        AddMediatR(services, configuration);

        // Pipeline behaviors run in registration order (first registered = outermost).
        // The desired chain is: Logging -> Validation -> Transaction -> Handler
        // so that validation short-circuits BEFORE we open a database transaction,
        // and logging captures the whole thing (including rollbacks).
        AddLogging(services);

        AddValidators(services);

        AddTransactions(services);

        AddAutomapperProfiles(services);

        return services;
    }

    public static void AddExceptionFilters(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<UnhandledExceptionFilter>();
            options.Filters.Add<NotFoundExceptionFilter>();
            options.Filters.Add<ValidationExceptionFilter>();
            options.Filters.Add<BookAlreadyBorrowedExceptionFilter>();
        });
    }

    public static void AddEfCore(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DbConnection");
        services.AddDbContext<BookCatalogDbContext>(options => { options.UseNpgsql(connectionString); });
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

    public static void AddTransactions(IServiceCollection services)
    {
        services.AddScoped<ITransactionProvider, TransactionProvider>();
        services.AddSingleton<IDbExceptionInterpreter, NpgsqlDbExceptionInterpreter>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
    }

    public static void AddAutomapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(ApplicationAssemblyMarker));
    }

    public static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
    }

    public static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IIsbnService, IsbnService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILoanService, LoanService>();
    }
}
