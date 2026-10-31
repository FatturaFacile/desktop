// <copyright file="EntityNotFoundException.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Exception thrown when a requested domain entity cannot be resolved or found by its unique identifier.
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class for the specified entity name and identifier.
    /// </summary>
    /// <param name="entityName">The name or type of the domain entity that was requested.</param>
    /// <param name="id">The unique identifier that failed to resolve to an existing entity.</param>
    public EntityNotFoundException(string entityName, object id)
        : base($"Entity '{entityName}' with identifier '{id}' was not found.") { }
}
