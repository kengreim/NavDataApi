using NavData.Services.Models;
using NavData.Services.Utilities;

namespace NavData.Services;

public class ArrivalService(CifpService cifpService)
{

    public IEnumerable<CombinedArrival> GetCombinedArrivals(string airportIdentifier)
    {
        if (cifpService.Data is null)
        {
            throw new Exception("CifpService data is null");
        }
        
        var arrivals = cifpService.Data.AirportArrivals.Where(a =>
                a.Port.Identifier.Equals(airportIdentifier, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
        if (arrivals.Count == 0)
        {
            arrivals = cifpService.Data.AirportArrivals.Where(a =>
                    a.Port.Designator is not null && a.Port.Designator.Equals(airportIdentifier,
                        StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        foreach (var arrival in arrivals)
        {
            var combinedSequences = new List<CombinedSequence>();

            foreach (var sequence in arrival.Sequence)
            {
                var points = sequence.Sequence.Select(p =>
                {
                    var (min, max) =
                        AltitudeConverter.ToAltitudeLimitsStrings(p.Altitude, p.Altitude2, p.AltitudeDescription);
                    return new Point
                    {
                        Identifier = p.Fix.Identifier,
                        Latitude = p.Fix.Coordinates.Latitude,
                        Longitude = p.Fix.Coordinates.Longitude,
                        MinAltitude = min,
                        MaxAltitude = max
                        //AltitudeType = p.AltitudeDescription.ToString()
                    };
                }).ToList();

                var combinedSequence = new CombinedSequence
                {
                    Transition = sequence.Transition,
                    TransitionType = sequence.Type.ToString(),
                    Points = points
                };

                combinedSequences.Add(combinedSequence);
            }

            yield return new CombinedArrival
            {
                ArrivalIdentifier = arrival.Identifier,
                Sequences = combinedSequences
            };
        }
    }
}

public class CombinedArrival
{
    public string ArrivalIdentifier { get; set; } = string.Empty;
    public List<CombinedSequence> Sequences { get; set; } = [];
}

public class CombinedSequence
{
    public string Transition { get; set; } = string.Empty;
    public string TransitionType { get; set; } = string.Empty;
    public List<Point> Points { get; set; } = [];
}