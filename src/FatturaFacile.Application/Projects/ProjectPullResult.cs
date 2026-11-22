// <copyright file="ProjectPullResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The outcome of refreshing the local portfolio from the web-api.
/// </summary>
/// <param name="Projects">The projects refreshed.</param>
/// <param name="Offline">Whether the web-api could not be reached.</param>
public sealed record ProjectPullResult(int Projects, bool Offline);
