// <copyright file="MesExecuteResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The outcome of executing a MES command.
/// </summary>
/// <param name="Status">What became of the command.</param>
/// <param name="ErrorKey">The localization key of the reason when the command was rejected or refused.</param>
/// <param name="Detail">The server's message when it refused the command.</param>
public sealed record MesExecuteResult(MesExecuteStatus Status, string? ErrorKey = null, string? Detail = null);
