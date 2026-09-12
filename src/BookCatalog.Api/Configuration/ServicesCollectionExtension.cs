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
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace BookCatalog.Api.Configuration;

public static class ServicesCollectionExtension
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });

        return builder;
    }

    public static IServiceCollection AddSolutionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddShutdownTimeout(services);
        AddSwaggerDocumentation(services);
        AddExceptionFilters(services);
        AddDatabaseConfigurationAndEfCore(services, configuration);
        AddSolutionHealthChecks(services);
        AddRepositories(services);
        AddServices(services);
        AddMediatR(services, configuration);

        // Pipeline behaviors run in registration order (first registered = outermost).
        // Chain: Logging -> Validation -> Transaction -> Handler
        AddLogging(services);
        AddValidators(services);
        AddTransactions(services);
        AddAutomapperProfiles(services);

        return services;
    }

    public static void AddShutdownTimeout(this IServiceCollection services, int timeoutSeconds = 30)
    {
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(timeoutSeconds);
        });
    }

    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
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

    public static void AddDatabaseConfigurationAndEfCore(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                var connStr = config.GetConnectionString("DbConnection") ?? config["DbConnection"];
                options.ConnectionString = connStr ?? string.Empty;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<BookCatalogDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(dbOptions.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });
        });
    }

    public static void AddSolutionHealthChecks(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<BookCatalogDbContext>(
                name: "database",
                tags: new[] { "ready" });
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

    public static void UseGracefulShutdownLogging(this WebApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() => Log.Information("Application started successfully."));
        lifetime.ApplicationStopping.Register(() => Log.Information("Application is stopping: draining in-flight requests..."));
        lifetime.ApplicationStopped.Register(() => Log.Information("Application stopped cleanly."));
    }

    public static void UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }

    public static void MapSolutionHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        });
    }
}
