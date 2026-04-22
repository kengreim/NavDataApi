using FastEndpoints;
using NavData.Services;
using NavData.Services.Models;

namespace NavData.Endpoints;

public class GetPointRequest
{
    public string Identifier { get; set; } = string.Empty;
}

public class GetPointEndpoint(PointService pointService) : Endpoint<GetPointRequest, List<PointLookupResult>>
{
    public override void Configure()
    {
        Get("/points/{Identifier}", "/v1/points/{Identifier}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Look up navigation points by identifier";
            s.Description = "Returns all navigation points (waypoints, VHF navaids, NDBs) matching the given ARINC 424 identifier. " +
                            "Identifiers are not globally unique, so multiple results may be returned with a Type discriminator. ";
            s.Response<List<PointLookupResult>>(200, "Matching points (possibly multiple per identifier)");
        });
    }

    public override Task HandleAsync(GetPointRequest req, CancellationToken ct)
    {
        var matches = pointService.GetPointsByIdentifier(req.Identifier.ToUpperInvariant()).ToList();
        return Send.OkAsync(matches, ct);
    }
}
