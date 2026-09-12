using BookCatalog.Api.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();
builder.Services.AddSolutionInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseGracefulShutdownLogging();
app.UseSerilogRequestLogging();
app.UseSwaggerDocumentation();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapSolutionHealthChecks();
app.MapControllers();

app.Run();