// <copyright file="PendingAnalyticalOperation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// An analytical entry captured offline and not yet sent.
/// </summary>
/// <param name="Id">The operation identifier.</param>
/// <param name="PayloadJson">The JSON body of the request.</param>
public sealed record PendingAnalyticalOperation(Guid Id, string PayloadJson);
