using Arinc424;
using NavData.Services.Models;
using NavData.Services.Utilities;

namespace NavData.Services;

public class DepartureService
{
    private readonly Data424 _data;

    public DepartureService(CifpService cifpService)
    {
        _data = cifpService.Data;
    }

    public IEnumerable<CombinedDeparture> GetCombinedDepartures(string airportIdentifier)
    {
        var departures = _data.AirportDepartures.Where(a => a.Port.Identifier == airportIdentifier);

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
                        Identifier = p.Fix.Identifier,
                        Latitude = p.Fix.Coordinates.Latitude,
                        Longitude = p.Fix.Coordinates.Longitude,
                        MinAltitude = min,
                        MaxAltitude = max
                        //Altitude = AltitudeFormatter.FormatAltitude(p.Altitude)
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
    public List<CombinedSequence> Sequences { get; set; } = new();
}