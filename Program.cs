using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NavData.Services;

var builder = WebApplication.CreateBuilder();

// Add the CIFP service as a singleton
var lines = File.ReadLines("CIFP/FAACIFP18");
builder.Services.AddSingleton(new CifpService(lines));
builder.Services.AddScoped<ArrivalService>();

builder.Services.AddFastEndpoints();

var app = builder.Build();
app.UseFastEndpoints();
app.Run();

// var lines = File.ReadLines("CIFP/FAACIFP18");
// var meta = Meta424.Create(Supplement.V18);
// var data = Data424.Create(meta, lines, out var skipped, out var invalid);
//
//
// var arrivalService = new ArrivalService(data);
// var combinedArrivals = arrivalService.GetCombinedArrivals("KSFO");
//
// foreach (var arrival in combinedArrivals)
// {
//     Console.WriteLine($"Arrival: {arrival.ArrivalIdentifier}");
//     foreach (var sequence in arrival.Sequences)
//     {
//         Console.WriteLine($"  Transition: {sequence.Transition}");
//         Console.WriteLine("  Points:");
//         foreach (var point in sequence.Points)
//         {
//             Console.WriteLine($"    - {point}");
//         }
//     }
//
//     Console.WriteLine();
// }