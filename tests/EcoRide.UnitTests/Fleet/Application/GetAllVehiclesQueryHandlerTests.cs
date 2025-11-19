using EcoRide.Modules.Fleet.Application.Queries.GetAllVehicles;
using EcoRide.Modules.Fleet.Domain.Aggregates;
using EcoRide.Modules.Fleet.Domain.Enums;
using EcoRide.Modules.Fleet.Domain.Repositories;
using EcoRide.Modules.Fleet.Domain.ValueObjects;
using Moq;
using NetTopologySuite.Geometries;

namespace EcoRide.UnitTests.Fleet.Application;

public class GetAllVehiclesQueryHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly GetAllVehiclesQueryHandler _handler;

    public GetAllVehiclesQueryHandlerTests()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _handler = new GetAllVehiclesQueryHandler(_vehicleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnVehicles()
    {
        // Arrange
        var vehicles = CreateTestVehicles(10);
        var query = new GetAllVehiclesQuery(PageNumber: 1, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 10));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Vehicles.Count);
        Assert.Equal(10, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(1, result.Value.TotalPages);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var vehicles = CreateTestVehicles(5, VehicleStatus.Available);
        var query = new GetAllVehiclesQuery(Status: "Available", PageNumber: 1, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync("Available", null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 5));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Vehicles.Count);
        Assert.All(result.Value.Vehicles, v => Assert.Equal("Available", v.Status));

        _vehicleRepositoryMock.Verify(
            x => x.GetAllAsync("Available", null, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithTypeFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var vehicles = CreateTestVehicles(5, type: VehicleType.Scooter);
        var query = new GetAllVehiclesQuery(Type: "Scooter", PageNumber: 1, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync(null, "Scooter", null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 5));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Vehicles.Count);
        Assert.All(result.Value.Vehicles, v => Assert.Equal("Scooter", v.Type));

        _vehicleRepositoryMock.Verify(
            x => x.GetAllAsync(null, "Scooter", null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinBatteryLevelFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var vehicles = CreateTestVehicles(5, batteryLevel: 80);
        var query = new GetAllVehiclesQuery(MinBatteryLevel: 50, PageNumber: 1, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync(null, null, 50, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 5));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Vehicles.Count);
        Assert.All(result.Value.Vehicles, v => Assert.True(v.BatteryLevel >= 50));

        _vehicleRepositoryMock.Verify(
            x => x.GetAllAsync(null, null, 50, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ShouldPassAllFiltersToRepository()
    {
        // Arrange
        var vehicles = CreateTestVehicles(3, VehicleStatus.Available, VehicleType.Bike, 90);
        var query = new GetAllVehiclesQuery(
            Status: "Available",
            Type: "Bike",
            MinBatteryLevel: 80,
            PageNumber: 1,
            PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync("Available", "Bike", 80, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 3));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Vehicles.Count);

        _vehicleRepositoryMock.Verify(
            x => x.GetAllAsync("Available", "Bike", 80, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var vehicles = CreateTestVehicles(10);
        var query = new GetAllVehiclesQuery(PageNumber: 2, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync(null, null, null, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 25)); // Total 25 vehicles

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Vehicles.Count);
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(3, result.Value.TotalPages); // Ceiling(25 / 10) = 3
    }

    [Fact]
    public async Task Handle_WithInvalidPageNumber_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAllVehiclesQuery(PageNumber: 0, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Pagination.Invalid", result.Error.Code);
        Assert.Contains("PageNumber", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithNegativePageNumber_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAllVehiclesQuery(PageNumber: -1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Pagination.Invalid", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAllVehiclesQuery(PageNumber: 1, PageSize: 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Pagination.Invalid", result.Error.Code);
        Assert.Contains("PageSize", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithPageSizeExceedingMaximum_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAllVehiclesQuery(PageNumber: 1, PageSize: 101);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Pagination.Invalid", result.Error.Code);
        Assert.Contains("100", result.Error.Message);
    }

    [Fact]
    public async Task Handle_WithNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllVehiclesQuery(PageNumber: 1, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Vehicle>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Vehicles);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public async Task Handle_ShouldMapVehiclesToDtosCorrectly()
    {
        // Arrange
        var vehicles = CreateTestVehicles(1);
        var vehicle = vehicles[0];
        var query = new GetAllVehiclesQuery(PageNumber: 1, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync(null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value.Vehicles[0];
        Assert.Equal(vehicle.Id, dto.Id);
        Assert.Equal(vehicle.QRCode.Code, dto.QRCode);
        Assert.Equal(vehicle.Status.ToString(), dto.Status);
        Assert.Equal(vehicle.Type.ToString(), dto.Type);
        Assert.Equal(vehicle.BatteryLevel.Value, dto.BatteryLevel);
        Assert.NotNull(dto.Location);
        Assert.NotNull(dto.PricePerMinute);
    }

    [Fact]
    public async Task Handle_WithPartialPageOfResults_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var vehicles = CreateTestVehicles(5); // Only 5 results on last page
        var query = new GetAllVehiclesQuery(PageNumber: 3, PageSize: 10);

        _vehicleRepositoryMock
            .Setup(x => x.GetAllAsync(null, null, null, 3, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((vehicles, 25)); // Total 25 vehicles

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Vehicles.Count);
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(3, result.Value.TotalPages);
    }

    private List<Vehicle> CreateTestVehicles(
        int count,
        VehicleStatus status = VehicleStatus.Available,
        VehicleType type = VehicleType.Scooter,
        int batteryLevel = 100)
    {
        var vehicles = new List<Vehicle>();
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

        for (int i = 0; i < count; i++)
        {
            var qrCode = Domain.ValueObjects.QRCode.Create($"QR{i:D6}").Value;
            var location = new Location(geometryFactory.CreatePoint(new Coordinate(-7.5898 + i * 0.001, 33.5731 + i * 0.001)));
            var battery = BatteryLevel.Create(batteryLevel).Value;
            var pricePerMinute = Money.Create(2.50m, "MAD").Value;

            var vehicle = Vehicle.Create(qrCode, location, type, battery, pricePerMinute).Value;

            // Set status using reflection if needed, or if there's a method to change status
            if (status != VehicleStatus.Available)
            {
                // Assuming we need to use reflection since status might not have public setters
                typeof(Vehicle)
                    .GetProperty(nameof(Vehicle.Status))!
                    .SetValue(vehicle, status);
            }

            vehicles.Add(vehicle);
        }

        return vehicles;
    }
}
