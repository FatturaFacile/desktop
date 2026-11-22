// <copyright file="FieldServiceFlushResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The outcome of sending the outbox of field operations.
/// </summary>
/// <param name="Sent">The operations the server applied or recognised as duplicates.</param>
/// <param name="Conflicts">The operations the server rejected.</param>
/// <param name="Remaining">The operations still waiting.</param>
/// <param name="Offline">Whether the server could not be reached or there is no session.</param>
public sealed record FieldServiceFlushResult(int Sent, int Conflicts, int Remaining, bool Offline);
