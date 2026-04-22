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
        Get("/approaches/{AirportId}");
        AllowAnonymous();
    }

    public override Task HandleAsync(GetApproachesRequest req, CancellationToken ct)
    {
        var approaches = approachService.GetCombinedApproaches(req.AirportId.ToUpperInvariant()).ToList();
        return Send.OkAsync(approaches, ct);
    }
}