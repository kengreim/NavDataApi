using FastEndpoints;
using NavData.Services;

namespace NavData.Endpoints;

public class GetApproachesRequest
{
    public string AirportId { get; set; } = string.Empty;
}

public class GetApproachesEndpoint(ApproachService approachService)
    : Endpoint<GetApproachesRequest, List<CombinedApproach>>
{
    public override void Configure()
    {
        Get("/approaches/{AirportId}", "/v1/approaches/{AirportId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "List instrument approaches for an airport";
            s.Description = "Returns all combined approach procedures for the given ICAO/FAA airport identifier.";
            s.Response<List<CombinedApproach>>(200, "Combined approaches for the airport");
        });
    }

    public override Task HandleAsync(GetApproachesRequest req, CancellationToken ct)
    {
        var approaches = approachService.GetCombinedApproaches(req.AirportId.ToUpperInvariant()).ToList();
        return Send.OkAsync(approaches, ct);
    }
}