namespace EcoRide.BuildingBlocks.Domain;

/// <summary>
/// Base exception for domain-specific errors.
/// Use this for exceptional cases that violate business rules.
/// For normal error flows, prefer using the Result pattern.
/// </summary>
public class DomainException : Exception
{
    public Error Error { get; }

    public DomainException(Error error) : base(error.Message)
    {
        Error = error;
    }

    public DomainException(Error error, Exception innerException)
        : base(error.Message, innerException)
    {
        Error = error;
    }

    public DomainException(string code, string message) : this(new Error(code, message))
    {
    }

    public DomainException(string code, string message, Exception innerException)
        : this(new Error(code, message), innerException)
    {
    }
}
