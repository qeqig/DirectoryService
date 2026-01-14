using System.Globalization;
using DirectoryService.Infrastructure;
using DirectoryService.Presentation;
using Framework.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddProgramDependencies(builder.Configuration);

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddDistributedCache(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
    {
        app.MapOpenApi();
        app.UseSwaggerUI(option => option.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
    }

    app.UseExceptionMiddleware();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace DirectoryService.Presentation
{
    public partial class Program;
}