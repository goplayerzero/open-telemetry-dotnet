using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

var p0ActivitySource = new ActivitySource("activity.source.name");

const string serviceName = "My Dataset Name";
const string otelEndpoint = "https://sdk.playerzero.app/otlp";
const string otelHeaders = "Authorization=Bearer <api token>,x-pzprod=false";

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
    //    .AddConsoleExporter()
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
    //    .AddConsoleExporter()
        .AddSource(p0ActivitySource.Name)
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
        //  .AddConsoleExporter()
          .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otelEndpoint + "/v1/metrics");
                options.Headers = otelHeaders;
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));

var app = builder.Build();

string HandleRollDice([FromServices]ILogger<Program> logger, string? player)
{
    using var activity = p0ActivitySource.StartActivity("ActivityName");
    if (activity != null)
    {        
        activity.TraceStateString = "userid=1234"; // set any trace-state-info
        activity.AddTag("myTag", "myValue"); // Set any tag
        activity.AddBaggage("myBaggage", "myBaggageValue"); // Set any baggage
    }

    var result = RollDice();

    if (string.IsNullOrEmpty(player))
    {
        logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
    }
    else
    {
        logger.LogInformation("{player} is rolling the dice: {result}", player, result);
    }

    return result.ToString(CultureInfo.InvariantCulture);
}

int RollDice()
{
    int dice = Random.Shared.Next(1, 7);
    if (dice == 4) {
        throw new Exception("Error");
    }
    return dice;
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/rolldice/{player?}", HandleRollDice);

app.Run();
