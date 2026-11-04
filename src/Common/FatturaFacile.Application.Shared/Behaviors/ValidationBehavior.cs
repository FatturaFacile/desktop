using FluentValidation;
using MediatR;
using FatturaFacile.Application.Shared.Results;

namespace FatturaFacile.Application.Shared.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
            
            // If TResponse is Result or Result<T>
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errorMessage, "VALIDATION_FAILED");
            }

            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse);
                var failureMethod = resultType.GetMethod("Failure", [typeof(string), typeof(string)]);
                if (failureMethod != null)
                {
                    return (TResponse)failureMethod.Invoke(null, [errorMessage, "VALIDATION_FAILED"])!;
                }
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
