using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Trip.Domain.ValueObjects;

/// <summary>
/// Value object representing geographic location
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
            return Result.Failure<Location>(new Error(
                "Location.InvalidLatitude",
                "Latitude must be between -90 and 90"));
        }

        if (longitude < -180 || longitude > 180)
        {
            return Result.Failure<Location>(new Error(
                "Location.InvalidLongitude",
                "Longitude must be between -180 and 180"));
        }

        return Result.Success(new Location(latitude, longitude));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
