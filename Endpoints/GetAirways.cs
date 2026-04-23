using FastEndpoints;
using NavData.Services;

namespace NavData.Endpoints;

public class GetAirwaysRequest
{
    public string Identifier { get; set; } = string.Empty;
}

public class GetAirwaysEndpoint(AirwayService airwayService) : Endpoint<GetAirwaysRequest, List<CombinedAirway>>
{
    public override void Configure()
    {
        Get("/airways/{Identifier}", "/v1/airways/{Identifier}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Look up an airway by identifier";
            s.Description = "Returns the sequence of fixes for the given airway identifier (e.g. J584, V23), including altitude limits, courses, and classification.";
            s.Response<List<CombinedAirway>>(200, "Airways matching the identifier, each with their fix sequences");
        });
    }

    public override Task HandleAsync(GetAirwaysRequest req, CancellationToken ct)
    {
        var airways = airwayService.GetCombinedAirways(req.Identifier.ToUpperInvariant()).ToList();
        return Send.OkAsync(airways, ct);
    }
}
