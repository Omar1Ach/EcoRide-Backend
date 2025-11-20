using EcoRide.BuildingBlocks.Domain;

namespace EcoRide.Modules.Trip.Domain.ValueObjects;

/// <summary>
/// Rating value object (1-5 stars)
/// US-006: End Trip & Payment - Trip rating feature
/// </summary>
public sealed class Rating : ValueObject
{
    public const int MinStars = 1;
    public const int MaxStars = 5;

    public int Stars { get; }
    public string? Comment { get; }

    private Rating(int stars, string? comment)
    {
        Stars = stars;
        Comment = comment;
    }

    /// <summary>
    /// Create a new rating with validation
    /// </summary>
    public static Result<Rating> Create(int stars, string? comment = null)
    {
        if (stars < MinStars || stars > MaxStars)
        {
            return Result.Failure<Rating>(new Error(
                "Rating.InvalidStars",
                $"Rating must be between {MinStars} and {MaxStars} stars"));
        }

        // Validate comment length if provided
        if (!string.IsNullOrWhiteSpace(comment))
        {
            var trimmedComment = comment.Trim();
            if (trimmedComment.Length > 500)
            {
                return Result.Failure<Rating>(new Error(
                    "Rating.CommentTooLong",
                    "Rating comment cannot exceed 500 characters"));
            }
            comment = trimmedComment;
        }
        else
        {
            // Convert empty/whitespace strings to null
            comment = null;
        }

        return Result.Success(new Rating(stars, comment));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Stars;
        yield return Comment;
    }

    public override string ToString()
    {
        return $"{Stars} star{(Stars == 1 ? "" : "s")}";
    }
}
