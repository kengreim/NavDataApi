namespace NavData.Services.Models;

public class Point
{
    public string Identifier { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? MinAltitude { get; set; }

    public string? MaxAltitude { get; set; }
    //public string AltitudeType { get; set; } = string.Empty;
}