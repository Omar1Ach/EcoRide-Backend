using EcoRide.Modules.Trip.Application.DTOs;

namespace EcoRide.UnitTests.Trip.Application;

/// <summary>
/// Tests for Emergency Contacts functionality
/// Tests TC-043: Click Emergency - shows phone numbers
/// </summary>
public class EmergencyContactsTests
{
    [Fact]
    public void TC043_GetEmergencyContacts_ReturnsAllContactNumbers()
    {
        // TC-043: Click Emergency - shows phone numbers
        // Act
        var contacts = EmergencyContacts.GetContacts();

        // Assert
        Assert.NotNull(contacts);
        Assert.NotNull(contacts.SupportPhone);
        Assert.NotNull(contacts.EmergencyPhone);
        Assert.NotNull(contacts.PolicePhone);
        Assert.NotNull(contacts.Message);
    }

    [Fact]
    public void GetContacts_ReturnsEcoRideSupportNumber()
    {
        // Act
        var contacts = EmergencyContacts.GetContacts();

        // Assert
        Assert.Contains("+212", contacts.SupportPhone); // Morocco country code
    }

    [Fact]
    public void GetContacts_ReturnsMoroccoEmergencyServices()
    {
        // Act
        var contacts = EmergencyContacts.GetContacts();

        // Assert - Morocco emergency services number
        Assert.Equal("150", contacts.EmergencyPhone);
    }

    [Fact]
    public void GetContacts_ReturnsMoroccoPoliceNumber()
    {
        // Act
        var contacts = EmergencyContacts.GetContacts();

        // Assert - Morocco police number
        Assert.Equal("19", contacts.PolicePhone);
    }

    [Fact]
    public void GetContacts_ReturnsHelpfulMessage()
    {
        // Act
        var contacts = EmergencyContacts.GetContacts();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(contacts.Message));
        Assert.Contains("Emergency", contacts.Message);
    }

    [Fact]
    public void GetContacts_ConsistentResults()
    {
        // Emergency contacts should be consistent across calls
        // Act
        var contacts1 = EmergencyContacts.GetContacts();
        var contacts2 = EmergencyContacts.GetContacts();

        // Assert
        Assert.Equal(contacts1.SupportPhone, contacts2.SupportPhone);
        Assert.Equal(contacts1.EmergencyPhone, contacts2.EmergencyPhone);
        Assert.Equal(contacts1.PolicePhone, contacts2.PolicePhone);
        Assert.Equal(contacts1.Message, contacts2.Message);
    }
}
