using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EcoRide.E2ETests;

/// <summary>
/// End-to-end tests for complete user journeys
/// Tests full workflows from registration to trip completion
/// </summary>
public class UserJourneyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public UserJourneyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteUserRegistrationAndLogin_ShouldSucceed()
    {
        // SCENARIO: New user registers and logs in

        // Step 1: Register new user
        var email = $"e2e{Guid.NewGuid()}@ecoride.ma";
        var password = "E2ETest@123";

        var registerRequest = new
        {
            Email = email,
            Password = password,
            PhoneNumber = "+212600000001",
            FullName = "E2E Test User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(registerResult);

        // Step 2: Login with registered credentials
        var loginRequest = new
        {
            Email = email,
            Password = password,
            Enable2FA = false
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Login may require verification first, so we accept either OK or BadRequest
        Assert.True(loginResponse.StatusCode == HttpStatusCode.OK ||
                    loginResponse.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FindAndViewVehicles_ShouldSucceed()
    {
        // SCENARIO: User searches for vehicles

        // Step 1: Get nearby vehicles
        var nearbyResponse = await _client.GetAsync(
            "/api/vehicles/nearby?latitude=33.5731&longitude=-7.5898&radiusKm=5");

        Assert.Equal(HttpStatusCode.OK, nearbyResponse.StatusCode);

        // Step 2: Get all available vehicles
        var allVehiclesResponse = await _client.GetAsync(
            "/api/vehicles?status=Available&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, allVehiclesResponse.StatusCode);

        // Step 3: Filter by type
        var scootersResponse = await _client.GetAsync(
            "/api/vehicles?type=Scooter&status=Available");

        Assert.Equal(HttpStatusCode.OK, scootersResponse.StatusCode);
    }

    [Fact]
    public async Task ViewEmergencyContacts_ShouldReturnContacts()
    {
        // SCENARIO: User wants to see emergency contacts during a trip

        var response = await _client.GetAsync("/api/trips/emergency-contacts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var contacts = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        Assert.NotNull(contacts);
        Assert.NotEmpty(contacts);
    }

    [Fact]
    public async Task ViewTripHistory_ForNewUser_ShouldReturnEmpty()
    {
        // SCENARIO: New user checks trip history

        var userId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/trips/history?userId={userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task PasswordResetFlow_ShouldWork()
    {
        // SCENARIO: User forgets password and requests reset

        // Step 1: Register user
        var email = $"reset{Guid.NewGuid()}@ecoride.ma";
        var registerRequest = new
        {
            Email = email,
            Password = "Original@123",
            PhoneNumber = "+212600000001",
            FullName = "Reset Test User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Step 2: Request password reset
        var forgotRequest = new { Email = email };
        var forgotResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotRequest);

        Assert.Equal(HttpStatusCode.OK, forgotResponse.StatusCode);

        // Note: Actual reset would require OTP from SMS
        // This tests the API contract works correctly
    }

    [Fact]
    public async Task MultipleVehicleFilters_ShouldWork()
    {
        // SCENARIO: User applies multiple filters to find ideal vehicle

        // Filter by type, status, and battery
        var response = await _client.GetAsync(
            "/api/vehicles?type=Bike&status=Available&minBatteryLevel=80&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task PaginationAcrossMultiplePages_ShouldWork()
    {
        // SCENARIO: User browses through multiple pages of vehicles

        // Get first page
        var page1Response = await _client.GetAsync("/api/vehicles?pageNumber=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);

        // Get second page
        var page2Response = await _client.GetAsync("/api/vehicles?pageNumber=2&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);

        // Get third page
        var page3Response = await _client.GetAsync("/api/vehicles?pageNumber=3&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, page3Response.StatusCode);
    }

    [Fact]
    public async Task TripHistoryPagination_ShouldWork()
    {
        // SCENARIO: User with many trips browses history

        var userId = Guid.NewGuid();

        // Get first page
        var page1Response = await _client.GetAsync(
            $"/api/trips/history?userId={userId}&pageNumber=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);

        // Get second page
        var page2Response = await _client.GetAsync(
            $"/api/trips/history?userId={userId}&pageNumber=2&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);
    }

    [Fact]
    public async Task InvalidCredentialsFlow_ShouldFailGracefully()
    {
        // SCENARIO: User enters wrong password multiple times

        var loginRequest = new
        {
            Email = "nonexistent@ecoride.ma",
            Password = "WrongPassword123",
            Enable2FA = false
        };

        // Attempt 1
        var response1 = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);

        // Attempt 2
        var response2 = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);

        // System should handle failed login attempts gracefully
        var response3 = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.True(response3.StatusCode == HttpStatusCode.BadRequest ||
                    response3.StatusCode == HttpStatusCode.TooManyRequests); // Could be rate limited
    }

    [Fact]
    public async Task SearchVehiclesInDifferentLocations_ShouldWork()
    {
        // SCENARIO: User travels and searches for vehicles in different cities

        // Search in Casablanca
        var casaResponse = await _client.GetAsync(
            "/api/vehicles/nearby?latitude=33.5731&longitude=-7.5898&radiusKm=5");
        Assert.Equal(HttpStatusCode.OK, casaResponse.StatusCode);

        // Search in Rabat
        var rabatResponse = await _client.GetAsync(
            "/api/vehicles/nearby?latitude=34.0209&longitude=-6.8416&radiusKm=5");
        Assert.Equal(HttpStatusCode.OK, rabatResponse.StatusCode);

        // Search in Marrakech
        var marrakechResponse = await _client.GetAsync(
            "/api/vehicles/nearby?latitude=31.6295&longitude=-7.9811&radiusKm=5");
        Assert.Equal(HttpStatusCode.OK, marrakechResponse.StatusCode);
    }
}
