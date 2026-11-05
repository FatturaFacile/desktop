// <copyright file="ValidationBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that executes all registered
/// <see cref="IValidator{TRequest}"/> rules before invoking handlers.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="validators">
/// The collection of registered validators.
/// </param>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc/>
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        return HandleInternalAsync(
            request,
            next,
            cancellationToken);
    }

    private async Task<TResponse> HandleInternalAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (validators.Any())
        {
            ValidationContext<TRequest> context = new(request);

            var validationResults = await Task
                .WhenAll(
                    validators.Select(
                        validator => validator.ValidateAsync(
                            context,
                            cancellationToken)))
                .ConfigureAwait(false);

            List<ValidationFailure> failures =
            [
                .. validationResults
                    .SelectMany(result => result.Errors)
                    .Where(failure => failure is not null),
            ];

            if (failures.Count > 0)
            {
                throw new Exceptions.ValidationException(failures);
            }
        }

        return await next(cancellationToken)
            .ConfigureAwait(false);
    }
}
