using BookCatalog.Api.Configuration;
using BookCatalog.Api.ExceptionHandling.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSolutionInfrastructure(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<NotFoundExceptionFilter>();
    options.Filters.Add<ValidationExceptionFilter>();
    options.Filters.Add<UnhandledExceptionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
