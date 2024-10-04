using System.Globalization;
using dotnet_simple.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "My Dataset";
const string otelEndpoint = "https://sdk.playerzero.app/otlp";
const string otelHeaders = "Authorization=Bearer <api_token>,x-pzprod=false";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:3000", "http://localhost:3001");
                      });
});

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
       .AddConsoleExporter()
        .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otelEndpoint + "/v1/logs");
                options.Headers = otelHeaders;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddSource(serviceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.SetDbStatementForStoredProcedure = true;
            })
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otelEndpoint + "/v1/traces");
                options.Headers = otelHeaders;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            }))
    .WithMetrics(metrics => metrics
        .AddMeter(serviceName)
        .AddMeter("System.Net.NameResolution")
        .AddMeter("System.Net.Http")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otelEndpoint + "/v1/metrics");
                options.Headers = otelHeaders;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
