using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using QuestionApi.Clients;
using QuestionApi.Services;
using Shared.Extensions;
using Shared.Http;
using Shared.Exceptions;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure OpenTelemetry with Jaeger
var serviceName = "QuestionApi";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("QuestionController")
        .AddSource("QuestionService")
        .AddSource("AnswerApiClient")
        .AddSource("HttpClientService")
        .AddOtlpExporter(options =>
        {
            // Jaeger HTTP endpoint (14268 port)
            options.Endpoint = new Uri(builder.Configuration["Jaeger:Endpoint"] ?? "http://localhost:14268/api/traces");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }));

// Register HttpClientService for AnswerApi
var answerApiBaseUrl = builder.Configuration["AnswerApi:BaseUrl"] ?? "http://localhost:5001";
builder.Services.AddHttpClientService(answerApiBaseUrl);

// Register AnswerApiClient
builder.Services.AddScoped<IAnswerApiClient, AnswerApiClient>();

// Register Services
builder.Services.AddScoped<IQuestionService, QuestionService>();

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
