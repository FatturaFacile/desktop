// <copyright file="ProjectPortfolio.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The portfolio read from the local database.
/// </summary>
/// <param name="Projects">The projects and templates.</param>
/// <param name="Pending">The changes waiting to be sent.</param>
/// <param name="Conflicts">The changes the web-api refused.</param>
/// <param name="FromCache">Whether the data is the local copy because the web-api could not be reached.</param>
/// <param name="SyncedAtUtc">The instant (UTC) of the refresh, when the web-api answered.</param>
public sealed record ProjectPortfolio(
    IReadOnlyList<Project> Projects,
    IReadOnlyList<ProjectPendingChange> Pending,
    IReadOnlyList<ProjectPendingChange> Conflicts,
    bool FromCache,
    DateTime? SyncedAtUtc);
