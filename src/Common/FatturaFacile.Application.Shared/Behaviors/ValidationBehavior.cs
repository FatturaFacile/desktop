// <copyright file="ValidationBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Results;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Application.Shared.Behaviors;

/// <summary>
/// Pipeline behavior that runs registered FluentValidation validators prior to request handler execution.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response expected from the pipeline.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="validators">The collection of validators registered for the request type.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    /// <inheritdoc />
    public Task<TResponse> Handle(
        TRequest                          request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken                 cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        return HandleInternalAsync(
            request,
            next,
            cancellationToken);
    }

    private async Task<TResponse> HandleInternalAsync(
        TRequest                          request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken                 cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
           .SelectMany(r => r.Errors)
           .Where(f => f is not null)
           .ToList();

        if (failures.Count > 0)
        {
            var errorMessage = string.Join(
                "; ",
                failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));

            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(
                    errorMessage,
                    "VALIDATION_FAILED");
            }

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(TResponse).GetMethod(
                    "Failure",
                    [typeof(string), typeof(string)]);

                if (failureMethod is not null)
                {
                    return (TResponse)failureMethod.Invoke(
                        null,
                        [errorMessage, "VALIDATION_FAILED"])!;
                }
            }

            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
