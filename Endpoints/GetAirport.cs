using FastEndpoints;
using NavData.Services;

namespace NavData.Endpoints;

public class GetAirportRequest
{
    public string AirportId { get; set; } = string.Empty;
}

public class GetAirportEndpoint(AirportService airportService) : Endpoint<GetAirportRequest, AirportInfo>
{
    public override void Configure()
    {
        Get("/airports/{AirportId}", "/v1/airports/{AirportId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get airport metadata";
            s.Description = "Returns metadata for the given ICAO/FAA airport identifier, including coordinates, elevation, magnetic variation, and runway information.";
            s.Response<AirportInfo>(200, "Airport metadata");
            s.Response(404, "Airport not found");
        });
    }

    public override Task HandleAsync(GetAirportRequest req, CancellationToken ct)
    {
        var airport = airportService.GetAirportInfo(req.AirportId.ToUpperInvariant());
        if (airport is null)
        {
            return Send.NotFoundAsync(ct);
        }

        return Send.OkAsync(airport, ct);
    }
}
