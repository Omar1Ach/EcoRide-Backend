using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Trip.Domain.Entities;

/// <summary>
/// Receipt entity for storing trip payment receipts
/// US-006: End Trip & Payment - Receipt generation (TC-055)
/// </summary>
public sealed class Receipt : Entity<Guid>
{
    public Guid TripId { get; private set; }
    public Guid UserId { get; private set; }
    public string ReceiptNumber { get; private set; } = null!;

    // Trip details
    public string VehicleCode { get; private set; } = null!;
    public DateTime TripStartTime { get; private set; }
    public DateTime TripEndTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public int DistanceMeters { get; private set; }

    // Location details
    public double StartLatitude { get; private set; }
    public double StartLongitude { get; private set; }
    public double EndLatitude { get; private set; }
    public double EndLongitude { get; private set; }

    // Cost breakdown
    public decimal BaseCost { get; private set; }
    public decimal TimeCost { get; private set; }
    public decimal TotalCost { get; private set; }

    // Payment details
    public string PaymentMethod { get; private set; } = null!; // "Wallet" or "CreditCard"
    public string PaymentDetails { get; private set; } = null!; // "Paid from Wallet" or "Paid with Visa ****1234"
    public decimal WalletBalanceBefore { get; private set; }
    public decimal WalletBalanceAfter { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private Receipt() { }

    private Receipt(
        Guid id,
        Guid tripId,
        Guid userId,
        string receiptNumber,
        string vehicleCode,
        DateTime tripStartTime,
        DateTime tripEndTime,
        int durationMinutes,
        int distanceMeters,
        double startLatitude,
        double startLongitude,
        double endLatitude,
        double endLongitude,
        decimal baseCost,
        decimal timeCost,
        decimal totalCost,
        string paymentMethod,
        string paymentDetails,
        decimal walletBalanceBefore,
        decimal walletBalanceAfter)
    {
        Id = id;
        TripId = tripId;
        UserId = userId;
        ReceiptNumber = receiptNumber;
        VehicleCode = vehicleCode;
        TripStartTime = tripStartTime;
        TripEndTime = tripEndTime;
        DurationMinutes = durationMinutes;
        DistanceMeters = distanceMeters;
        StartLatitude = startLatitude;
        StartLongitude = startLongitude;
        EndLatitude = endLatitude;
        EndLongitude = endLongitude;
        BaseCost = baseCost;
        TimeCost = timeCost;
        TotalCost = totalCost;
        PaymentMethod = paymentMethod;
        PaymentDetails = paymentDetails;
        WalletBalanceBefore = walletBalanceBefore;
        WalletBalanceAfter = walletBalanceAfter;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Create a new receipt for a completed trip
    /// </summary>
    public static Result<Receipt> Create(
        Guid tripId,
        Guid userId,
        string vehicleCode,
        DateTime tripStartTime,
        DateTime tripEndTime,
        int durationMinutes,
        int distanceMeters,
        double startLatitude,
        double startLongitude,
        double endLatitude,
        double endLongitude,
        decimal baseCost,
        decimal timeCost,
        decimal totalCost,
        string paymentMethod,
        string paymentDetails,
        decimal walletBalanceBefore,
        decimal walletBalanceAfter)
    {
        if (tripId == Guid.Empty)
        {
            return Result.Failure<Receipt>(new Error(
                "Receipt.InvalidTripId",
                "Trip ID is required"));
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure<Receipt>(new Error(
                "Receipt.InvalidUserId",
                "User ID is required"));
        }

        if (string.IsNullOrWhiteSpace(vehicleCode))
        {
            return Result.Failure<Receipt>(new Error(
                "Receipt.InvalidVehicleCode",
                "Vehicle code is required"));
        }

        if (totalCost < 0)
        {
            return Result.Failure<Receipt>(new Error(
                "Receipt.InvalidTotalCost",
                "Total cost cannot be negative"));
        }

        // Generate unique receipt number: RCP-YYYYMMDD-XXXXXX
        var receiptNumber = GenerateReceiptNumber();

        return Result.Success(new Receipt(
            Guid.NewGuid(),
            tripId,
            userId,
            receiptNumber,
            vehicleCode,
            tripStartTime,
            tripEndTime,
            durationMinutes,
            distanceMeters,
            startLatitude,
            startLongitude,
            endLatitude,
            endLongitude,
            baseCost,
            timeCost,
            totalCost,
            paymentMethod,
            paymentDetails,
            walletBalanceBefore,
            walletBalanceAfter));
    }

    private static string GenerateReceiptNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"RCP-{datePart}-{randomPart}";
    }

    public override string ToString()
    {
        return ReceiptNumber;
    }
}
