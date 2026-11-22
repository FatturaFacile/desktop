// <copyright file="FieldServiceConflictRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A queued field operation the server rejected, formatted for display.
/// </summary>
/// <param name="Id">The outbox entry.</param>
/// <param name="Text">The localized description of the operation and of the error.</param>
internal sealed record FieldServiceConflictRow(Guid Id, string Text);
