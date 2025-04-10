using Arinc424;

namespace NavData.Services.Utilities;

public static class AltitudeConverter
{
    public static (string? minAltitude, string? maxAltitude) ToAltitudeLimitsStrings(Altitude altitude1,
        Altitude altitude2,
        AltitudeDescription altitudeDescription)
    {
        var (formatted1, formatted2) = (AltitudeFormatter.FormatAltitude(altitude1),
            AltitudeFormatter.FormatAltitude(altitude2));
        return altitudeDescription switch
        {
            AltitudeDescription.Unknown => (null, null),
            AltitudeDescription.AtAboveFirst => (formatted1, null),
            AltitudeDescription.AtBelowFirst => (null, formatted1),
            AltitudeDescription.AtFirst => (formatted1, formatted1),
            AltitudeDescription.AtAboveAtBelow => (formatted2, formatted1),
            AltitudeDescription.AtAboveSecond => (formatted2, null),
            AltitudeDescription.NotBeforeAtAboveSecond => (formatted2, null),
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