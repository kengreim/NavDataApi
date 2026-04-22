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
            AltitudeDescription.GlideSecondAtFirst => (formatted1, formatted1),
            AltitudeDescription.GlideSecondAtAboveFirst => (formatted1, null),
            AltitudeDescription.GlideInterceptSecondAtFirst => (formatted1, formatted1),
            AltitudeDescription.GlideInterceptSecondAtAboveFirst => (formatted1, null),
            AltitudeDescription.OptionalAtAbove => (formatted1, formatted2),
            AltitudeDescription.AtVerticalSecondAtAboveFirst => (formatted1, null),
            AltitudeDescription.AtVerticalSecondAtFirst => (formatted1, formatted1),
            AltitudeDescription.AtVerticalSecondAtBelowFirst => (null, formatted1)
            //_ => throw new ArgumentOutOfRangeException(nameof(altitudeDescription), altitudeDescription, null)
        };
    }
}