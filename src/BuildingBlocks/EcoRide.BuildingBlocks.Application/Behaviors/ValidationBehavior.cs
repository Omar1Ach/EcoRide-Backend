using EcoRide.BuildingBlocks.Domain;
using FluentValidation;
using MediatR;

namespace EcoRide.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Pipeline behavior that validates commands and queries using FluentValidation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                .ToList();

            var error = new Error(
                "Validation.Failed",
                string.Join("; ", errors));

            return CreateValidationResult<TResponse>(error);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(Error error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(error) as TResult)!;
        }

        // Get the generic argument type (e.g., for Result<AuthResponse>, get AuthResponse)
        var resultType = typeof(TResult);
        if (!resultType.IsGenericType || resultType.GenericTypeArguments.Length == 0)
        {
            throw new InvalidOperationException($"TResult must be a generic type: {resultType.Name}");
        }

        var valueType = resultType.GenericTypeArguments[0];

        // Create Result.Failure<T>(error) - note: Failure<T> is on base Result class, not Result<T>
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) });

        if (failureMethod == null)
        {
            throw new InvalidOperationException($"Failure method not found on Result");
        }

        var genericFailureMethod = failureMethod.MakeGenericMethod(valueType);
        var validationResult = genericFailureMethod.Invoke(null, new object[] { error });

        if (validationResult == null)
        {
            throw new InvalidOperationException("Validation result is null");
        }

        return (TResult)validationResult;
    }
}
