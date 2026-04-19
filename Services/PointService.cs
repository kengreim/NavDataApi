using NavData.Services.Models;

namespace NavData.Services;

public class PointService(CifpService cifpService)
{
    public IEnumerable<PointLookupResult> GetPointsByIdentifier(string identifier)
    {
        if (cifpService.Data is null)
        {
            throw new Exception("CifpService data is null");
        }

        foreach (var fix in cifpService.Data.EnrouteWaypoints.Where(a =>
                     a.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase)))
        {
            yield return new PointLookupResult
            {
                Identifier = fix.Identifier,
                Name = fix.Name,
                Latitude = fix.Coordinates.Latitude,
                Longitude = fix.Coordinates.Longitude,
                Type = "EnrouteWaypoint"
            };
        }

        foreach (var fix in cifpService.Data.AirportTerminalWaypoints.Where(a =>
                     a.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase)))
        {
            yield return new PointLookupResult
            {
                Identifier = fix.Identifier,
                Name = fix.Name,
                Latitude = fix.Coordinates.Latitude,
                Longitude = fix.Coordinates.Longitude,
                Type = "AirportTerminalWaypoint",
                AirportIdentifier = fix.Port.Identifier ?? fix.Port.Designator
            };
        }

        foreach (var fix in cifpService.Data.HeliportTerminalWaypoints.Where(a =>
                     a.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase)))
        {
            yield return new PointLookupResult
            {
                Identifier = fix.Identifier,
                Name = fix.Name,
                Latitude = fix.Coordinates.Latitude,
                Longitude = fix.Coordinates.Longitude,
                Type = "HeliportTerminalWaypoint",
                AirportIdentifier = fix.Port.Identifier ?? fix.Port.Designator
            };
        }

        foreach (var fix in cifpService.Data.Omnidirects.Where(a =>
                     a.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase)))
        {
            yield return new PointLookupResult
            {
                Identifier = fix.Identifier,
                Name = fix.Name,
                Latitude = fix.Coordinates.Latitude,
                Longitude = fix.Coordinates.Longitude,
                Type = "VhfNavaid",
                Subtype = fix.Type.ToString(),
                AirportIdentifier = fix.Port?.Identifier ?? fix.Port?.Designator
            };
        }

        foreach (var fix in cifpService.Data.Nondirects.Where(a =>
                     a.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase)))
        {
            yield return new PointLookupResult
            {
                Identifier = fix.Identifier,
                Name = fix.Name,
                Latitude = fix.Coordinates.Latitude,
                Longitude = fix.Coordinates.Longitude,
                Type = "NdbNavaid",
                Subtype = fix.Type.ToString()
            };
        }
    }
}
