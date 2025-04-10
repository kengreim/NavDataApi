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
        Get("/arrivals/{AirportId}");
        AllowAnonymous();
    }

    public override Task HandleAsync(GetStarsRequest req, CancellationToken ct)
    {
        var arrivals = arrivalService.GetCombinedArrivals(req.AirportId.ToUpperInvariant()).ToList();
        return SendAsync(arrivals, cancellation: ct);
    }
}