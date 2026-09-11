using Asp.Versioning;
using FinCore.Application.Services;
using FinCore.Server.Exceptions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Serilog Logging
// --------------------------------------------------

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// --------------------------------------------------
// Read FinCore Configuration
// --------------------------------------------------

var finCoreSettings =
    builder.Configuration.GetSection("FinCoreSettings");

var applicationName =
    finCoreSettings["ApplicationName"];

var environment =
    finCoreSettings["Environment"];

// Read Windows Environment Variable
var environmentVariable =
    Environment.GetEnvironmentVariable("FINCORE_ENVIRONMENT");

// --------------------------------------------------
// Add services to the container
// --------------------------------------------------

builder.Services.AddOpenApi();

// --------------------------------------------------
// API Versioning
// --------------------------------------------------

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion =
        new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;
});

// --------------------------------------------------
// Global Exception Handling
// --------------------------------------------------

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

// --------------------------------------------------
// Dependency Injection
// --------------------------------------------------

builder.Services.AddScoped<
    IApplicationService,
    ApplicationService>();

// --------------------------------------------------
// Build Application
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// Global Exception Handler Middleware
// --------------------------------------------------

app.UseExceptionHandler();

// --------------------------------------------------
// Configure HTTP Request Pipeline
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --------------------------------------------------
// Configuration Test Endpoint
// --------------------------------------------------

app.MapGet("/api/config-test", () =>
{
    Log.Information(
        "Configuration test endpoint was called.");

    return Results.Ok(new
    {
        ApplicationName = applicationName,
        Environment = environment,
        EnvironmentVariable = environmentVariable
    });
});

// --------------------------------------------------
// DI Test Endpoint
// --------------------------------------------------

app.MapGet("/api/test", (
    IApplicationService applicationService) =>
{
    Log.Information(
        "DI test endpoint was called.");

    var applicationName =
        applicationService.GetApplicationName();

    return Results.Ok(new
    {
        Message = "Dependency Injection is working!",
        Application = applicationName
    });
});

// --------------------------------------------------
// API Version Test Endpoint
// --------------------------------------------------

app.MapGet("/api/v1/test", () =>
{
    Log.Information(
        "API Version 1 test endpoint was called.");

    return Results.Ok(new
    {
        Message = "API Versioning is working!",
        Version = "1.0"
    });
});

// --------------------------------------------------
// Run Application
// --------------------------------------------------

try
{
    Log.Information(
        "Starting FinCore Digital Banking Platform.");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(
        exception,
        "FinCore application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}