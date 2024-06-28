using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "June28.ActivitySource";
const string otelEndpoint = "https://sdk.playerzero.app/otlp";
const string otelHeaders = "Authorization=Bearer 666af2fef6b93a24518cf726,x-pzprod=false";

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

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
