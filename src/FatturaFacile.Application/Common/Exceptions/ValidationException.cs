// <copyright file="ValidationException.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation.Results;

namespace FatturaFacile.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when one or more application or command validation rules are violated,
/// encapsulating failure details grouped by property name.
/// </summary>
public class ValidationException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException()
        : base(message: "One or more validation failures have occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class from validation failures.
    /// </summary>
    /// <param name="failures">The collection of validation failures.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
                .GroupBy(
                         keySelector: e => e.PropertyName,
                         elementSelector: e => e.ErrorMessage,
                         comparer: StringComparer.Ordinal)
                .ToDictionary(
                              keySelector: failureGroup => failureGroup.Key,
                              elementSelector: failureGroup => failureGroup.ToArray(),
                              comparer: StringComparer.Ordinal);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ValidationException(string? message)
        : base(message: message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(
        string?    message,
        Exception? innerException)
        : base(
               message: message,
               innerException: innerException)
    {
    }

    /// <summary>
    /// Gets a read-only dictionary mapping invalid property names to an array of associated error descriptions.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>(comparer: StringComparer.Ordinal);

}
