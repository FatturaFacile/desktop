// <copyright file="ValidationBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;
using MediatR;
using ValidationException = FatturaFacile.Application.Common.Exceptions.ValidationException;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Represents validation behavior.
/// </summary>
/// <param name="validators">The validators.</param>
/// <typeparam name="TRequest">The type of the t request.</typeparam>
/// <typeparam name="TResponse">The type of the t response.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> validators = validators;

    /// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    /// <exception cref="ValidationException">Thrown when request validation rules are violated.</exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(
                context,
                cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        return failures.Count != 0 ? throw new ValidationException(failures) : await next(cancellationToken);
    }
}
