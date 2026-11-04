// <copyright file="DomainErrorDefinition.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Shared.Errors;

/// <summary>
/// Represents a strongly-typed domain error with a unique semantic code and descriptive message.
/// </summary>
public sealed record DomainErrorDefinition
{
    /// <summary>
    /// Gets a representation of no error.
    /// </summary>
    public static readonly DomainErrorDefinition None = new(string.Empty, string.Empty);

    /// <summary>
    /// Gets a representation of a null or absent value error.
    /// </summary>
    public static readonly DomainErrorDefinition NullValue = new("Common.NullValue", "A specified value is null.");

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainErrorDefinition"/> class.
    /// Initializes a new instance of the <see cref="DomainErrorDefinition"/> record.
    /// </summary>
    /// <param name="code">The unique semantic error code.</param>
    /// <param name="message">The descriptive default error message.</param>
    public DomainErrorDefinition(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Gets the unique semantic error code (e.g., 'Invoice.NotFound', 'Identity.InvalidCredentials').
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets the default human-readable message in English or format template.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Implicitly converts a <see cref="DomainErrorDefinition"/> to its string error code.
    /// </summary>
    /// <param name="error">The domain error definition instance.</param>
    public static implicit operator string(DomainErrorDefinition error) => error?.Code ?? string.Empty;

    /// <summary>
    /// Returns a string representation of the domain error code and message.
    /// </summary>
    /// <returns>A formatted string combining error code and message, or an empty string if code is empty.</returns>
    public override string ToString() => string.IsNullOrWhiteSpace(Code) ? string.Empty : $"{Code}: {Message}";
}
