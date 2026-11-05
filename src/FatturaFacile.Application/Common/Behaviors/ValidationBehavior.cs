// <copyright file="ValidationBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;
using MediatR;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that executes all registered <see cref="IValidator{TRequest}"/> rules before invoking handlers.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">The collection of registered validators.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (_validators.Any())
        {
            ValidationContext<TRequest> context = new(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

            List<FluentValidation.Results.ValidationFailure> failures = [.. validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null),];

            if (failures.Count != 0)
            {
                throw new Exceptions.ValidationException(failures);
            }
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
