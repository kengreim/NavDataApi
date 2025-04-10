using FastEndpoints;
using NavData.Services;

namespace NavData.Endpoints;

public class GetSidsRequest
{
    public string AirportId { get; set; } = string.Empty;
}

public class GetSidsEndpoint : Endpoint<GetSidsRequest, List<CombinedDeparture>>
{
    private readonly DepartureService _departureService;

    public GetSidsEndpoint(DepartureService departureService)
    {
        _departureService = departureService;
    }

    public override void Configure()
    {
        Get("/departures/{AirportId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetSidsRequest req, CancellationToken ct)
    {
        var departures = _departureService.GetCombinedDepartures(req.AirportId.ToUpperInvariant()).ToList();
        await SendAsync(departures);
    }
} 