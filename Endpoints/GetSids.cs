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
        Get("/departures/{AirportId}", "/v1/departures/{AirportId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List SIDs (departure procedures) for an airport";
            s.Description = "Returns all SIDs for the given ICAO/FAA airport identifier.";
            s.Response<List<CombinedDeparture>>(200, "Combined SIDs for the airport");
        });
    }

    public override Task HandleAsync(GetSidsRequest req, CancellationToken ct)
    {
        var departures = departureService.GetCombinedDepartures(req.AirportId.ToUpperInvariant()).ToList();
        return Send.OkAsync(departures, ct);
    }
}