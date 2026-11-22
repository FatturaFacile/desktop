// <copyright file="ProjectActionStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// What happened to a project change.
/// </summary>
public enum ProjectActionStatus
{
    /// <summary>
    /// Saved locally and delivered to the web-api.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// Saved locally and waiting for the network.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Saved locally but refused by the web-api.
    /// </summary>
    Conflict = 2,

    /// <summary>
    /// Refused before saving because the data is not valid.
    /// </summary>
    Rejected = 3,
}
