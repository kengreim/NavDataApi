using Coravel;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NavData.Services;

var builder = WebApplication.CreateBuilder();

// Add the CIFP service as a singleton
builder.Services.AddSingleton<CifpService>();

// Add the CIFP update service as an Invocable
builder.Services.AddScheduler();
builder.Services.AddTransient<CifpUpdateService>();

builder.Services.AddScoped<ArrivalService>();
builder.Services.AddScoped<DepartureService>();

builder.Services.AddControllers();

builder.Services.AddFastEndpoints();

var app = builder.Build();
app.UseFastEndpoints();
app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Services.UseScheduler(scheduler => { scheduler.Schedule<CifpUpdateService>().Hourly().RunOnceAtStart(); });
app.Run();
