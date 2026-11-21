// <copyright file="SyncOperation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Kind of local change queued for the server.
/// </summary>
public enum SyncOperation
{
    /// <summary>
    /// A new record is created on the server.
    /// </summary>
    Create = 0,

    /// <summary>
    /// An existing record is updated on the server.
    /// </summary>
    Update = 1,

    /// <summary>
    /// The record is deleted on the server.
    /// </summary>
    Delete = 2,
}
