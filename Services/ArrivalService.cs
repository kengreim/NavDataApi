using Arinc424;
using NavData.Services.Models;
using NavData.Services.Utilities;

namespace NavData.Services;

public class ArrivalService
{
    private readonly Data424 _data;

    public ArrivalService(CifpService cifpService)
    {
        _data = cifpService.Data;
    }

    public IEnumerable<CombinedArrival> GetCombinedArrivals(string airportIdentifier)
    {
        var arrivals = _data.AirportArrivals.Where(a => a.Port.Identifier == airportIdentifier);

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