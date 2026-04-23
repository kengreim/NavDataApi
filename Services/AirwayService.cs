using NavData.Services.Utilities;

namespace NavData.Services;

public class AirwayService(CifpService cifpService)
{
    public IEnumerable<CombinedAirway> GetCombinedAirways(string identifier)
    {
        if (cifpService.Data is null)
        {
            throw new Exception("CifpService data is null");
        }

        var airways = cifpService.Data.Airways.Where(a =>
                a.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        foreach (var airway in airways)
        {
            var points = airway.Sequence.Select(p => new AirwayFixPoint
            {
                Identifier = p.Fix.Identifier,
                Latitude = p.Fix.Coordinates.Latitude,
                Longitude = p.Fix.Coordinates.Longitude,
                Type = p.Type.ToString(),
                Level = p.LevelType.ToString(),
                Restriction = p.Restriction.ToString(),
                OutboundCourse = p.Out.Value,
                InboundCourse = p.In.Value,
                DistanceFrom = p.DistanceFrom,
                Minimum = AltitudeFormatter.FormatAltitude(p.Minimum),
                Minimum2 = AltitudeFormatter.FormatAltitude(p.Minimum2),
                Maximum = AltitudeFormatter.FormatAltitude(p.Maximum),
                Descriptions = p.Descriptions == 0 ? [] : p.Descriptions.ToString().Split(", ")
            }).ToList();

            yield return new CombinedAirway
            {
                AirwayIdentifier = airway.Identifier,
                Points = points
            };
        }
    }
}

public class CombinedAirway
{
    public string AirwayIdentifier { get; set; } = string.Empty;
    public List<AirwayFixPoint> Points { get; set; } = [];
}

public class AirwayFixPoint
{
    public string Identifier { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Restriction { get; set; } = string.Empty;
    public double OutboundCourse { get; set; }
    public double InboundCourse { get; set; }
    public double DistanceFrom { get; set; }
    public string? Minimum { get; set; }
    public string? Minimum2 { get; set; }
    public string? Maximum { get; set; }
    public string[] Descriptions { get; set; } = [];
}
