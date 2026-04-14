using NavData.Services.Models;
using NavData.Services.Utilities;

namespace NavData.Services;

public class DepartureService(CifpService cifpService)
{
    public IEnumerable<CombinedDeparture> GetCombinedDepartures(string airportIdentifier)
    {
        if (cifpService.Data is null)
        {
            throw new Exception("CifpService data is null");
        }

        var departures = cifpService.Data.AirportDepartures.Where(a =>
                a.Port.Identifier.Equals(airportIdentifier, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        if (departures.Count == 0)
        {
            departures = cifpService.Data.AirportDepartures.Where(a =>
                    a.Port.Designator is not null && a.Port.Designator.Equals(airportIdentifier,
                        StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        foreach (var departure in departures)
        {
            var combinedSequences = new List<CombinedSequence>();

            foreach (var sequence in departure.Sequence)
            {
                var points = sequence.Sequence.Select(p =>
                {
                    var (min, max) =
                        AltitudeConverter.ToAltitudeLimitsStrings(p.Altitude, p.Altitude2, p.AltitudeDescription);
                    return new Point
                    {
                        Identifier = p.Fix?.Identifier,
                        Latitude = p.Fix?.Coordinates.Latitude,
                        Longitude = p.Fix?.Coordinates.Longitude,
                        MinAltitude = min,
                        MaxAltitude = max,
                        LegType = p.LegType.ToString(),
                        Course = p.Course.Value,
                        Descriptions = p.Descriptions == 0 ? [] : p.Descriptions.ToString().Split(", ")
                    };
                }).ToList();

                var combinedSequence = new CombinedSequence
                {
                    Transition = sequence.Transition ?? string.Empty,
                    TransitionType = sequence.Types.ToString(),
                    Points = points
                };

                combinedSequences.Add(combinedSequence);
            }

            yield return new CombinedDeparture
            {
                DepartureIdentifier = departure.Identifier,
                Sequences = combinedSequences
            };
        }
    }
}

public class CombinedDeparture
{
    public string DepartureIdentifier { get; set; } = string.Empty;
    public List<CombinedSequence> Sequences { get; set; } = [];
}