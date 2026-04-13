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
        Get("/points/{Identifier}");
        AllowAnonymous();
    }

    public override Task HandleAsync(GetPointRequest req, CancellationToken ct)
    {
        var matches = pointService.GetPointsByIdentifier(req.Identifier.ToUpperInvariant()).ToList();
        return Send.OkAsync(matches, ct);
    }
}
