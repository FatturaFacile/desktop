// <copyright file="ResolvedRoute.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// The concrete route of an outbox entry.
/// </summary>
/// <param name="Route">The relative route.</param>
/// <param name="ServerOrderId">The server order identifier.</param>
public sealed record ResolvedRoute(string Route, Guid? ServerOrderId);
