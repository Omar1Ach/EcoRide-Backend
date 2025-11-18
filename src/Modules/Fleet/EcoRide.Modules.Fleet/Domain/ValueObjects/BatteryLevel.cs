using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Fleet.Domain.ValueObjects;

/// <summary>
/// Battery level value object (0-100%)
/// </summary>
public sealed class BatteryLevel : ValueObject
{
    public const int MinLevel = 0;
    public const int MaxLevel = 100;
    public const int LowBatteryThreshold = 20;

    public int Value { get; }

    private BatteryLevel(int value)
    {
        Value = value;
    }

    public static Result<BatteryLevel> Create(int value)
    {
        if (value < MinLevel || value > MaxLevel)
        {
            return Result.Failure<BatteryLevel>(
                new Error("BatteryLevel.OutOfRange", $"Battery level must be between {MinLevel} and {MaxLevel}"));
        }

        return Result.Success(new BatteryLevel(value));
    }

    public bool IsLow() => Value < LowBatteryThreshold;

    public bool IsAvailable() => Value >= LowBatteryThreshold;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}%";

    public static implicit operator int(BatteryLevel batteryLevel) => batteryLevel.Value;
}
