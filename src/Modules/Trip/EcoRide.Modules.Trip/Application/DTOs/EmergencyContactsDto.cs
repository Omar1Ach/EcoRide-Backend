namespace EcoRide.Modules.Trip.Application.DTOs;

/// <summary>
/// Emergency contact information for active trips
/// US-005: Emergency button functionality
/// </summary>
public sealed record EmergencyContactsDto(
    string SupportPhone,
    string EmergencyPhone,
    string PolicePhone,
    string Message);

/// <summary>
/// Factory for creating emergency contacts
/// </summary>
public static class EmergencyContacts
{
    public static EmergencyContactsDto GetContacts()
    {
        return new EmergencyContactsDto(
            SupportPhone: "+212 5XX-XXXXX", // EcoRide support
            EmergencyPhone: "150", // Morocco emergency services
            PolicePhone: "19", // Morocco police
            Message: "Emergency services available 24/7. Stay calm and call for help.");
    }
}
