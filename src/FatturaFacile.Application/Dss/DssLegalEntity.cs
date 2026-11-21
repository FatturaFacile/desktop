// <copyright file="DssLegalEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// A legal entity of the tenant on the server.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Currency">The accounting currency.</param>
public sealed record DssLegalEntity(string Id, string Name, string Currency);
