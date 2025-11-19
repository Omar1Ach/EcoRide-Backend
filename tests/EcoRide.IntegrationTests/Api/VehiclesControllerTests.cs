using System.Net;
using System.Net.Http.Json;
using EcoRide.IntegrationTests.Infrastructure;
using EcoRide.Modules.Fleet.Application.DTOs;

namespace EcoRide.IntegrationTests.Api;

/// <summary>
/// Integration tests for VehiclesController
/// Tests vehicle discovery and retrieval endpoints
/// </summary>
public class VehiclesControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public VehiclesControllerTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetNearbyVehicles_WithValidCoordinates_ShouldReturn200()
    {
        // Arrange
        var latitude = 33.5731;
        var longitude = -7.5898;
        var radiusKm = 5;

        // Act
        var response = await _client.GetAsync(
            $"/api/vehicles/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetNearbyVehiclesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicles);
    }

    [Fact]
    public async Task GetNearbyVehicles_WithInvalidLatitude_ShouldReturn400()
    {
        // Arrange - latitude must be between -90 and 90
        var latitude = 91.0;
        var longitude = -7.5898;
        var radiusKm = 5;

        // Act
        var response = await _client.GetAsync(
            $"/api/vehicles/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetNearbyVehicles_WithInvalidLongitude_ShouldReturn400()
    {
        // Arrange - longitude must be between -180 and 180
        var latitude = 33.5731;
        var longitude = 181.0;
        var radiusKm = 5;

        // Act
        var response = await _client.GetAsync(
            $"/api/vehicles/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetNearbyVehicles_WithInvalidRadius_ShouldReturn400()
    {
        // Arrange - radius must be between 0.1 and 50
        var latitude = 33.5731;
        var longitude = -7.5898;
        var radiusKm = 0.0;

        // Act
        var response = await _client.GetAsync(
            $"/api/vehicles/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetNearbyVehicles_WithTypeFilter_ShouldReturn200()
    {
        // Arrange
        var latitude = 33.5731;
        var longitude = -7.5898;
        var radiusKm = 5;
        var type = "Scooter";

        // Act
        var response = await _client.GetAsync(
            $"/api/vehicles/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}&type={type}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetNearbyVehiclesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicles);

        // If there are vehicles, they should all be of the requested type
        if (result.Vehicles.Any())
        {
            Assert.All(result.Vehicles, v => Assert.Equal(type, v.Type));
        }
    }

    [Fact]
    public async Task GetNearbyVehicles_WithMinBatteryFilter_ShouldReturn200()
    {
        // Arrange
        var latitude = 33.5731;
        var longitude = -7.5898;
        var radiusKm = 5;
        var minBattery = 50;

        // Act
        var response = await _client.GetAsync(
            $"/api/vehicles/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}&minBatteryLevel={minBattery}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetNearbyVehiclesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicles);

        // If there are vehicles, they should all have battery >= minBattery
        if (result.Vehicles.Any())
        {
            Assert.All(result.Vehicles, v => Assert.True(v.BatteryLevel >= minBattery));
        }
    }

    [Fact]
    public async Task GetAllVehicles_WithDefaultPagination_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/vehicles");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetAllVehiclesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicles);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task GetAllVehicles_WithCustomPageSize_ShouldReturn200()
    {
        // Arrange
        var pageSize = 10;

        // Act
        var response = await _client.GetAsync($"/api/vehicles?pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetAllVehiclesResponse>();
        Assert.NotNull(result);
        Assert.Equal(pageSize, result.PageSize);
    }

    [Fact]
    public async Task GetAllVehicles_WithInvalidPageNumber_ShouldReturn400()
    {
        // Arrange - page number must be >= 1
        var pageNumber = 0;

        // Act
        var response = await _client.GetAsync($"/api/vehicles?pageNumber={pageNumber}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllVehicles_WithInvalidPageSize_ShouldReturn400()
    {
        // Arrange - page size must be between 1 and 100
        var pageSize = 101;

        // Act
        var response = await _client.GetAsync($"/api/vehicles?pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllVehicles_WithStatusFilter_ShouldReturn200()
    {
        // Arrange
        var status = "Available";

        // Act
        var response = await _client.GetAsync($"/api/vehicles?status={status}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetAllVehiclesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicles);

        // If there are vehicles, they should all have the requested status
        if (result.Vehicles.Any())
        {
            Assert.All(result.Vehicles, v => Assert.Equal(status, v.Status));
        }
    }

    [Fact]
    public async Task GetAllVehicles_WithTypeFilter_ShouldReturn200()
    {
        // Arrange
        var type = "Bike";

        // Act
        var response = await _client.GetAsync($"/api/vehicles?type={type}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetAllVehiclesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicles);

        // If there are vehicles, they should all be of the requested type
        if (result.Vehicles.Any())
        {
            Assert.All(result.Vehicles, v => Assert.Equal(type, v.Type));
        }
    }

    [Fact]
    public async Task GetAllVehicles_WithMultipleFilters_ShouldReturn200()
    {
        // Arrange
        var status = "Available";
        var type = "Scooter";
        var minBattery = 70;
        var pageSize = 5;

        // Act
        var response = await _client.GetAsync(
            $"/api/vehicles?status={status}&type={type}&minBatteryLevel={minBattery}&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetAllVehiclesResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Vehicles);
        Assert.Equal(pageSize, result.PageSize);

        // If there are vehicles, they should match all filters
        if (result.Vehicles.Any())
        {
            Assert.All(result.Vehicles, v =>
            {
                Assert.Equal(status, v.Status);
                Assert.Equal(type, v.Type);
                Assert.True(v.BatteryLevel >= minBattery);
            });
        }
    }

    [Fact]
    public async Task GetAllVehicles_Page2_ShouldReturn200()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 10;

        // Act
        var response = await _client.GetAsync($"/api/vehicles?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetAllVehiclesResponse>();
        Assert.NotNull(result);
        Assert.Equal(pageNumber, result.PageNumber);
    }
}
