using System.Net;
using System.Net.Http.Json;
using EcoRide.IntegrationTests.Infrastructure;
using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.IntegrationTests.Api;

/// <summary>
/// Integration tests for TripsController
/// Tests trip lifecycle endpoints
/// </summary>
public class TripsControllerTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public TripsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StartTrip_WithoutReservation_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            QRCode = "ECO-SCTR-0001",
            StartLatitude = 33.5731,
            StartLongitude = -7.5898
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartTrip_WithInvalidQRCode_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            QRCode = "INVALID",
            StartLatitude = 33.5731,
            StartLongitude = -7.5898
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartTrip_WithInvalidLatitude_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            QRCode = "ECO-SCTR-0001",
            StartLatitude = 91.0, // Invalid
            StartLongitude = -7.5898
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartTrip_WithInvalidLongitude_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            QRCode = "ECO-SCTR-0001",
            StartLatitude = 33.5731,
            StartLongitude = 181.0 // Invalid
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveTripStats_WithoutActiveTrip_ShouldReturn404()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/trips/active?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveTripStats_WithInvalidUserId_ShouldReturn404()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var response = await _client.GetAsync($"/api/trips/active?userId={userId}");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEmergencyContacts_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/trips/emergency-contacts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<EmergencyContact>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetTripHistory_WithValidUserId_ShouldReturn200()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/trips/history?userId={userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetTripHistoryResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Trips);
        Assert.Equal(0, result.TotalCount); // No trips for new user
    }

    [Fact]
    public async Task GetTripHistory_WithInvalidPageNumber_ShouldReturn400()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageNumber = 0; // Invalid

        // Act
        var response = await _client.GetAsync($"/api/trips/history?userId={userId}&pageNumber={pageNumber}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTripHistory_WithInvalidPageSize_ShouldReturn400()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageSize = 101; // Invalid (max is 100)

        // Act
        var response = await _client.GetAsync($"/api/trips/history?userId={userId}&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTripHistory_WithCustomPageSize_ShouldReturn200()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageSize = 10;

        // Act
        var response = await _client.GetAsync($"/api/trips/history?userId={userId}&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetTripHistoryResponse>();
        Assert.NotNull(result);
        Assert.Equal(pageSize, result.PageSize);
    }

    [Fact]
    public async Task GetTripHistory_Page2_ShouldReturn200()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageNumber = 2;

        // Act
        var response = await _client.GetAsync($"/api/trips/history?userId={userId}&pageNumber={pageNumber}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetTripHistoryResponse>();
        Assert.NotNull(result);
        Assert.Equal(pageNumber, result.PageNumber);
    }

    [Fact]
    public async Task EndTrip_WithoutActiveTrip_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            EndLatitude = 33.5731,
            EndLongitude = -7.5898
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips/end", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EndTrip_WithInvalidLatitude_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            EndLatitude = 91.0, // Invalid
            EndLongitude = -7.5898
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips/end", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EndTrip_WithInvalidLongitude_ShouldReturn400()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            EndLatitude = 33.5731,
            EndLongitude = 181.0 // Invalid
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/trips/end", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

/// <summary>
/// Emergency contact DTO for deserialization
/// </summary>
public record EmergencyContact(string Name, string Number, string Description);
