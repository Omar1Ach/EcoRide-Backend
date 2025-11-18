namespace EcoRide.BuildingBlocks.Domain;

/// <summary>
/// Represents an error with a code and message.
/// Used in the Result pattern for functional error handling.
/// </summary>
public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }

    public static implicit operator string(Error error) => error.Code;

    public override string ToString() => $"{Code}: {Message}";
}
