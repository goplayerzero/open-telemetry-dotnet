# open-telemetry-dotnet
Demonstration of Open Telemetry Instrumentation with .NET ASP Core

# ASP.NET Core instrumentation configuration

Install the instrumentation NuGet packages from OpenTelemetry that will generate the telemetry, and set them up.
1 Add the packages
```
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.SqlClient
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Exporter.Console
dotnet add package OpenTelemetry.Extensions.Hosting
```

2 Setup the OpenTelemetry code

In Program.cs, add the following lines:
```
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

app.UseAuthorization();

app.MapControllers();

app.Run();

```