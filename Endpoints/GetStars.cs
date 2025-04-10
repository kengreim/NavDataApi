using FastEndpoints;
using NavData.Services;

namespace NavData.Endpoints;

public class GetStarsRequest
{
    public string AirportId { get; set; } = string.Empty;
}

public class GetStarsEndpoint : Endpoint<GetStarsRequest, List<CombinedArrival>>
{
    private readonly ArrivalService _arrivalService;

    public GetStarsEndpoint(ArrivalService arrivalService)
    {
        _arrivalService = arrivalService;
    }

    public override void Configure()
    {
        Get("/arrivals/{AirportId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetStarsRequest req, CancellationToken ct)
    {
        var arrivals = _arrivalService.GetCombinedArrivals(req.AirportId.ToUpperInvariant()).ToList();
        await SendAsync(arrivals);
    }
}

