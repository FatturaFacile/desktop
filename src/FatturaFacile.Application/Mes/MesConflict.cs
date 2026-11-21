// <copyright file="MesConflict.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// A command the server refused after the operator issued it offline, shown until the operator dismisses it.
/// </summary>
/// <param name="CommandId">The command (outbox entry) identifier.</param>
/// <param name="Action">The action that was refused.</param>
/// <param name="OccurredAtUtc">When the operator acted (UTC).</param>
/// <param name="Error">The server's reason.</param>
public sealed record MesConflict(Guid CommandId, MesAction Action, DateTime OccurredAtUtc, string Error);
