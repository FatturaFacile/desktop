// <copyright file="DomainException.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Represents an exceptional error condition occurring within the core domain layer,
/// signaling a violation of business invariants, domain validation constraints, or entity state rules.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message describing the domain rule violation or error condition.</param>
    public DomainException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message describing the domain rule violation or error condition.</param>
    /// <param name="innerException">The inner exception that caused the current domain error.</param>
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
