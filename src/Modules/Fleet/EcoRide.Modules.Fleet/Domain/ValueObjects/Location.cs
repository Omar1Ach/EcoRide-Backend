using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Fleet.Domain.ValueObjects;

/// <summary>
/// Geographic location value object (latitude, longitude)
/// </summary>
public sealed class Location : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }

    private Location(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Result<Location> Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            return Result.Failure<Location>(
                new Error("Location.InvalidLatitude", "Latitude must be between -90 and 90"));
        }

        if (longitude < -180 || longitude > 180)
        {
            return Result.Failure<Location>(
                new Error("Location.InvalidLongitude", "Longitude must be between -180 and 180"));
        }

        return Result.Success(new Location(latitude, longitude));
    }

    /// <summary>
    /// Calculates distance to another location in meters using Haversine formula
    /// </summary>
    public double DistanceToInMeters(Location other)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(other.Latitude - Latitude);
        var dLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(Latitude)) *
                Math.Cos(DegreesToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distanceKm = earthRadiusKm * c;

        return distanceKm * 1000; // Convert to meters
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"({Latitude}, {Longitude})";
}
