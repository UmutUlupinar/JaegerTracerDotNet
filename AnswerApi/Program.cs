using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using AnswerApi.Services;
using Shared.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OpenTelemetry with Jaeger
var serviceName = "AnswerApi";
var serviceVersion = "1.0.0";

var jaegerEndpoint = builder.Configuration["Jaeger:Endpoint"] ?? "http://localhost:4317";
Console.WriteLine($"[OpenTelemetry] Jaeger Endpoint: {jaegerEndpoint}");

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                activity?.SetTag("http.request.method", request.Method);
                activity?.SetTag("http.request.path", request.Path);
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSource("AnswerController")
        .AddSource("AnswerService")
        .SetSampler(new AlwaysOnSampler())
        .AddOtlpExporter(options =>
        {
            // Jaeger OTLP gRPC endpoint (4318 port)
            options.Endpoint = new Uri(jaegerEndpoint);
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            Console.WriteLine($"[OpenTelemetry] OTLP Exporter configured: {options.Endpoint}");
            Console.WriteLine($"[OpenTelemetry] OTLP Protocol: {options.Protocol}");
        }));

// Register Services
builder.Services.AddScoped<IAnswerService, AnswerService>();

// Global exception handler
builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        
        logger.LogError(exception, "Unhandled exception occurred");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception is ApiException apiException 
            ? apiException.StatusCode 
            : 500;

        var response = new
        {
            error = exception?.Message ?? "An error occurred",
            errorCode = exception is ApiException apiEx ? apiEx.ErrorCode : "INTERNAL_ERROR",
            statusCode = context.Response.StatusCode
        };

        await context.Response.WriteAsJsonAsync(response);
    };
});

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
