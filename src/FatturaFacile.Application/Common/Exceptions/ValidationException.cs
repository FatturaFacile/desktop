// <copyright file="ValidationException.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation.Results;

namespace FatturaFacile.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string? message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class from validation failures.
    /// </summary>
    /// <param name="failures">The collection of validation failures.</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this(failures as IReadOnlyList<ValidationFailure> ?? failures?.ToList() ?? throw new ArgumentNullException(nameof(failures)))
    {
    }

    private ValidationException(IReadOnlyList<ValidationFailure> failures)
        : base(BuildMessage(failures))
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the dictionary of property names and associated error messages.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    private static string BuildMessage(IEnumerable<ValidationFailure>? failures)
    {
        var messages = failures?
            .Select(static f => f.ErrorMessage)
            .Where(static m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.Ordinal)
            .ToList() ?? [];

        return messages.Count == 0 ? "One or more validation failures have occurred." : string.Join(Environment.NewLine, messages);
    }
}
