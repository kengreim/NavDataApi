using Arinc424;

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
                    var (min, max) = ConvertAltitude(p.Altitude, p.Altitude2, p.AltitudeDescription);
                    return new Point
                    {
                        Identifier = p.Fix.Identifier,
                        Latitude = p.Fix.Coordinates.Latitude,
                        Longitude = p.Fix.Coordinates.Longitude,
                        MinAltitude = min,
                        MaxAltitude = max,
                        AltitudeType = p.AltitudeDescription.ToString()
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

    private static string? FormatAltitude(Altitude altitude)
    {
        return altitude.Unit switch
        {
            AltitudeUnit.Unknown => null,
            AltitudeUnit.Level => $"FL{altitude.Value}",
            AltitudeUnit.Feet => altitude.Value.ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static (string? minAltitude, string? maxAltitude) ConvertAltitude(Altitude altitude1, Altitude altitude2,
        AltitudeDescription altitudeDescription)
    {
        return altitudeDescription switch
        {
            AltitudeDescription.Unknown => (null, null),
            AltitudeDescription.AtAboveFirst => (FormatAltitude(altitude1), null),
            AltitudeDescription.AtBelowFirst => (null, FormatAltitude(altitude1)),
            AltitudeDescription.AtFirst => (FormatAltitude(altitude1), FormatAltitude(altitude1)),
            AltitudeDescription.AtAboveAtBelow => (FormatAltitude(altitude2), FormatAltitude(altitude1)),
            AltitudeDescription.AtAboveSecond => (FormatAltitude(altitude2), null),
            AltitudeDescription.NotBeforeAtAboveSecond => (FormatAltitude(altitude2), null),
            // AltitudeDescription.GlideSecondAtFirst => expr,
            // AltitudeDescription.GlideSecondAtAboveFirst => expr,
            // AltitudeDescription.GlideInterceptSecondAtFirst => expr,
            // AltitudeDescription.GlideInterceptSecondAtAboveFirst => expr,
            // AltitudeDescription.OptionalAtAbove => expr,
            // AltitudeDescription.AtVerticalSecondAtAboveFirst => expr,
            // AltitudeDescription.AtVerticalSecondAtFirst => expr,
            // AltitudeDescription.AtVerticalSecondAtBelowFirst => expr,
            _ => throw new ArgumentOutOfRangeException(nameof(altitudeDescription), altitudeDescription, null)
        };
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

public class Point
{
    public string Identifier { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? MinAltitude { get; set; }
    public string? MaxAltitude { get; set; }
    public string AltitudeType { get; set; } = string.Empty;
}