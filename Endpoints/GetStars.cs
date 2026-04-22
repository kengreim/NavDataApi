using FastEndpoints;
using NavData.Services;

namespace NavData.Endpoints;

public class GetStarsRequest
{
    public string AirportId { get; set; } = string.Empty;
}

public class GetStarsEndpoint(ArrivalService arrivalService) : Endpoint<GetStarsRequest, List<CombinedArrival>>
{
    public override void Configure()
    {
        Get("/arrivals/{AirportId}", "/v1/arrivals/{AirportId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List STARs (arrival procedures) for an airport";
            s.Description = "Returns all STARs for the given ICAO/FAA airport identifier.";
            s.Response<List<CombinedArrival>>(200, "Combined STARs for the airport");
        });
    }

    public override Task HandleAsync(GetStarsRequest req, CancellationToken ct)
    {
        var arrivals = arrivalService.GetCombinedArrivals(req.AirportId.ToUpperInvariant()).ToList();
        return Send.OkAsync(arrivals, ct);
    }
}