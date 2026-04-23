using Coravel;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NavData.Services;
using NavData.Swagger;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder();

// Add the CIFP service as a singleton
builder.Services.AddSingleton<CifpService>();

// Add the CIFP update service as an Invocable
builder.Services.AddScheduler();
builder.Services.AddTransient<CifpUpdateService>();

builder.Services.AddScoped<ArrivalService>();
builder.Services.AddScoped<DepartureService>();
builder.Services.AddScoped<ApproachService>();
builder.Services.AddScoped<PointService>();
builder.Services.AddScoped<AirportService>();
builder.Services.AddScoped<AirwayService>();

builder.Services.AddControllers();

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.DocumentName = "v1";
        s.Title = "NavDataApi";
        s.Version = "v1";
        s.DocumentProcessors.Add(new TagOrderProcessor("V1"));
    };
});

var app = builder.Build();
app.UseFastEndpoints();
app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
app.MapScalarApiReference(o =>
{
    o.AddDocument(documentName: "v1", title: "v1", isDefault: false);
    o.Layout = ScalarLayout.Modern;
    o.Theme = ScalarTheme.Purple;
    o.Title = "NavDataApi";
    o.SchemaPropertyOrder = PropertyOrder.Preserve;
    o.Telemetry = false;
});
app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Services.UseScheduler(scheduler => { scheduler.Schedule<CifpUpdateService>().Hourly().RunOnceAtStart(); });
app.Run();
