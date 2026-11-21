// <copyright file="MesFlushResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The outcome of sending the commands of the outbox.
/// </summary>
/// <param name="Sent">The commands the server accepted.</param>
/// <param name="Conflicts">The commands the server refused.</param>
/// <param name="Remaining">The commands still waiting.</param>
/// <param name="Offline">Whether the server could not be reached (or the session is missing).</param>
public sealed record MesFlushResult(int Sent, int Conflicts, int Remaining, bool Offline);
