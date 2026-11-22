// <copyright file="JournalValidationException.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// Raised when a journal entry breaks one or more double-entry rules.
/// </summary>
public sealed class JournalValidationException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JournalValidationException"/> class.
    /// </summary>
    public JournalValidationException()
        : this("Journal entry violates the double-entry rules.", [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalValidationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public JournalValidationException(string message)
        : this(message, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalValidationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public JournalValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Violations = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalValidationException"/> class.
    /// </summary>
    /// <param name="violations">The broken rules.</param>
    public JournalValidationException(IReadOnlyList<JournalViolation> violations)
        : this(Describe(violations), violations)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalValidationException"/> class.
    /// </summary>
    /// <param name="violation">The broken rule.</param>
    public JournalValidationException(JournalViolation violation)
        : this(Describe([violation]), [violation])
    {
    }

    private JournalValidationException(string message, IReadOnlyList<JournalViolation> violations)
        : base(message)
    {
        Violations = violations;
    }

    /// <summary>
    /// Gets the broken rules.
    /// </summary>
    public IReadOnlyList<JournalViolation> Violations { get; }

    private static string Describe(IReadOnlyList<JournalViolation> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);

        return "Journal entry violates the double-entry rules: " + string.Join(", ", violations);
    }
}
