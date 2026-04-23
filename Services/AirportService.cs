namespace NavData.Services;

public class AirportService(CifpService cifpService)
{
    public AirportInfo? GetAirportInfo(string airportIdentifier)
    {
        if (cifpService.Data is null)
        {
            throw new Exception("CifpService data is null");
        }

        var airport = cifpService.Data.Airports.FirstOrDefault(a =>
            a.Identifier.Equals(airportIdentifier, StringComparison.InvariantCultureIgnoreCase));

        if (airport is null)
        {
            airport = cifpService.Data.Airports.FirstOrDefault(a =>
                a.Designator is not null && a.Designator.Equals(airportIdentifier,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        if (airport is null)
        {
            return null;
        }

        return new AirportInfo
        {
            Identifier = airport.Identifier,
            IcaoCode = airport.Icao.ToString(),
            Designator = airport.Designator,
            Name = airport.Name,
            Latitude = airport.Coordinates.Latitude,
            Longitude = airport.Coordinates.Longitude,
            Elevation = airport.Elevation,
            Variation = airport.Variation,
            CourseType = airport.CourseType.ToString(),
            TimeZone = airport.TimeZone,
            Privacy = airport.Privacy.ToString(),
            LongestRunwayLength = airport.LongestRunwayLength,
            LongestRunwayType = airport.LongestRunwayType.ToString()
        };
    }
}

public class AirportInfo
{
    public string Identifier { get; set; } = string.Empty;
    public string IcaoCode { get; set; } = string.Empty;
    public string? Designator { get; set; }
    public string? Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Elevation { get; set; }
    public float Variation { get; set; }
    public string CourseType { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public string Privacy { get; set; } = string.Empty;
    public int LongestRunwayLength { get; set; }
    public string LongestRunwayType { get; set; } = string.Empty;
}
