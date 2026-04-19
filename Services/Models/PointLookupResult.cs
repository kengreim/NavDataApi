namespace NavData.Services.Models;

public class PointLookupResult
{
    public string Identifier { get; set; } = string.Empty;
    public string? Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Subtype { get; set; }
    public string? AirportIdentifier { get; set; }
}
