using EcoRide.Modules.Trip.Domain.ValueObjects;

namespace EcoRide.UnitTests.Trip.Domain;

/// <summary>
/// Unit tests for Rating value object
/// Tests trip rating feature (US-006)
/// </summary>
public class RatingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_WithValidStars_ShouldSucceed(int stars)
    {
        // Act
        var result = Rating.Create(stars);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(stars, result.Value.Stars);
        Assert.Null(result.Value.Comment);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(10)]
    public void Create_WithInvalidStars_ShouldFail(int stars)
    {
        // Act
        var result = Rating.Create(stars);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Rating.InvalidStars", result.Error.Code);
    }

    [Fact]
    public void Create_WithValidComment_ShouldSucceed()
    {
        // Arrange
        var comment = "Great trip! Very smooth ride.";

        // Act
        var result = Rating.Create(5, comment);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.Stars);
        Assert.Equal(comment, result.Value.Comment);
    }

    [Fact]
    public void Create_WithTooLongComment_ShouldFail()
    {
        // Arrange
        var comment = new string('a', 501); // 501 characters

        // Act
        var result = Rating.Create(5, comment);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Rating.CommentTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithWhitespaceComment_ShouldTrimAndSucceed()
    {
        // Arrange
        var comment = "  Great trip!  ";

        // Act
        var result = Rating.Create(5, comment);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Great trip!", result.Value.Comment);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var rating5 = Rating.Create(5).Value;
        var rating1 = Rating.Create(1).Value;

        // Act & Assert
        Assert.Equal("5 stars", rating5.ToString());
        Assert.Equal("1 star", rating1.ToString());
    }

    [Fact]
    public void Create_WithNullComment_ShouldSucceed()
    {
        // Act
        var result = Rating.Create(4, null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Comment);
    }

    [Fact]
    public void Create_WithEmptyComment_ShouldSucceed()
    {
        // Act
        var result = Rating.Create(4, "");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Comment); // Empty string treated as null
    }

    [Fact]
    public void Create_WithMaxLengthComment_ShouldSucceed()
    {
        // Arrange
        var comment = new string('a', 500); // Exactly 500 characters

        // Act
        var result = Rating.Create(5, comment);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(500, result.Value.Comment?.Length);
    }
}
