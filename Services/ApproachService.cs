using NavData.Services.Models;
using NavData.Services.Utilities;

namespace NavData.Services;

public class ApproachService(CifpService cifpService)
{
    public IEnumerable<CombinedApproach> GetCombinedApproaches(string airportIdentifier)
    {
        if (cifpService.Data is null)
        {
            throw new Exception("CifpService data is null");
        }

        var approaches = cifpService.Data.AirportApproaches.Where(a =>
                a.Port.Identifier.Equals(airportIdentifier, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        if (approaches.Count == 0)
        {
            approaches = cifpService.Data.AirportApproaches.Where(a =>
                    a.Port.Designator is not null && a.Port.Designator.Equals(airportIdentifier,
                        StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        foreach (var approach in approaches)
        {
            var combinedSequences = new List<CombinedSequence>();

            foreach (var sequence in approach.Sequence)
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

            yield return new CombinedApproach
            {
                ApproachIdentifier = approach.Identifier,
                Sequences = combinedSequences
            };
        }
    }
}

public class CombinedApproach
{
    public string ApproachIdentifier { get; set; } = string.Empty;
    public List<CombinedSequence> Sequences { get; set; } = [];
}