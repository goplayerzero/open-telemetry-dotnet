# open-telemetry-dotnet
Demonstration of Open Telemetry Instrumentation with .NET ASP Core

# ASP.NET Core instrumentation configuration

Install the instrumentation NuGet packages from OpenTelemetry that will generate the telemetry, and set them up.
1 Add the packages
```
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Exporter.Console
```

2 Setup the OpenTelemetry code

In Program.cs, add the following lines:
```
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "My Dataset Name";
const string otelEndpoint = "https://sdk.playerzero.app/otlp";
const string otelHeaders = "Authorization=Bearer <api token>,x-pzprod=false";

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
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
          .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otelEndpoint + "/v1/metrics");
                options.Headers = otelHeaders;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));

var app = builder.Build();

```