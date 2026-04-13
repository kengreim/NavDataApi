using FastEndpoints;
using NavData.Services;

namespace NavData.Endpoints;

public class GetSidsRequest
{
    public string AirportId { get; set; } = string.Empty;
}

public class GetSidsEndpoint(DepartureService departureService) : Endpoint<GetSidsRequest, List<CombinedDeparture>>
{
    public override void Configure()
    {
        Get("/departures/{AirportId}");
        AllowAnonymous();
    }

    public override Task HandleAsync(GetSidsRequest req, CancellationToken ct)
    {
        var departures = departureService.GetCombinedDepartures(req.AirportId.ToUpperInvariant()).ToList();
        return Send.OkAsync(departures, ct);
    }
}