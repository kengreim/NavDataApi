using System.Globalization;
using Arinc424;

namespace NavData.Services.Utilities;

public static class AltitudeFormatter
{
    public static string? FormatAltitude(Altitude altitude)
    {
        return altitude.Unit switch
        {
            AltitudeUnit.Unknown => null,
            AltitudeUnit.Level => $"FL{altitude.Value}",
            AltitudeUnit.Feet => altitude.Value.ToString(),
            AltitudeUnit.Unlimited => "UNL",
            AltitudeUnit.Meters => Math.Round(altitude.Value * 3.28084f).ToString(CultureInfo.InvariantCulture),
            // AltitudeUnit.Unspecified => expr,
            // AltitudeUnit.Ground => expr,
            // AltitudeUnit.Sea => expr,
            // AltitudeUnit.Notam => expr,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}