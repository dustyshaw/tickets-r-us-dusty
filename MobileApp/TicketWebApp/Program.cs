using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MyMetrics;
using MyTraces;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TicketClassLib.Services;
using TicketWebApp.Components;
using TicketWebApp.Data;
using TicketWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Added logging for otel 
builder.Services.AddLogging();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddSingleton<ITicketService, ApiTicketService>();
builder.Services.AddSingleton<IEventService, ApiEventService>();
builder.Services.AddDbContextFactory<PostgresContext>(optionsBuilder => optionsBuilder.UseNpgsql("Name=TicketsDB"));
builder.Services.AddScoped<EmailSender>();

builder.Services.AddHealthChecks();

const string serviceName = "dustys-service";

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://otel-collector:4317"); // in docker compose, this is the OTLP receiver port
            })
        .AddConsoleExporter(); // optional
});


builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(serviceName))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddConsoleExporter()
          .AddSource(GetAllEventsTrace.EventsTraceServiceName)
          .AddSource(GetOneEventTrace.OneEventTraceServiceName)
          .AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://otel-collector:4317"); // in docker compose, this is the OTLP receiver port
            })
            .AddZipkinExporter(o =>
            {
                o.Endpoint = new Uri("http://zipkin:9411");
            })
            )
      .WithMetrics(metrics => metrics
          //.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(EventsMetic.MyMeterName))
          .AddMeter(EventsMetic.EventsMeter.Name)
          .AddAspNetCoreInstrumentation()
          .AddOtlpExporter(opts =>
            opts.Endpoint = new Uri("http://otel-collector:4317")
          )
          //.AddConsoleExporter()
          );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


app.MapHealthChecks("/healthCheck", new HealthCheckOptions
{
    AllowCachingResponses = false,
    ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blazor API V1");
});

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();
app.Run();

public partial class Program { }
