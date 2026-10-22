// <copyright file="ValidationBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

using MediatR;

using ValidationException = FatturaFacile.Application.Common.Exceptions.ValidationException;

namespace FatturaFacile.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that executes FluentValidation validators before invoking the inner request handler.
/// </summary>
/// <typeparam name="TRequest">The type of the request being validated in the pipeline.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the request handler.</typeparam>
/// <param name="validators">The collection of registered FluentValidation validators for the request.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{

    /// <summary>
    /// The collection of validators registered for request type <typeparamref name="TRequest"/>.
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> validators = validators;

    /// <summary>
    /// Evaluates all configured FluentValidation validators against the incoming request and forwards execution or throws on failure.
    /// </summary>
    /// <param name="request">The incoming request instance to validate.</param>
    /// <param name="next">The delegate representing the subsequent pipeline action.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous pipeline operation containing <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validation failures are identified.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is <see langword="null"/>.</exception>
    public async Task<TResponse> Handle(
        TRequest                          request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken                 cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argument: next);

        if (!validators.Any())
        {
            return await next(t: cancellationToken);
        }

        var context = new ValidationContext<TRequest>(instanceToValidate: request);

        var validationResults = await Task.WhenAll(
                                                   tasks: validators.Select(selector: v => v.ValidateAsync(
                                                                                                           context: context,
                                                                                                           cancellation: cancellationToken)));

        var failures = validationResults
                      .SelectMany(selector: r => r.Errors)
                      .Where(predicate: f => f != null)
                      .ToList();

        return failures.Count != 0 ? throw new ValidationException(failures: failures) : await next(t: cancellationToken);
    }

}
