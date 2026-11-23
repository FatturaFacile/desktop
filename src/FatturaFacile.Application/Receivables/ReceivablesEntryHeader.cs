// <copyright file="ReceivablesEntryHeader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The identity of a queued receivables action: who asked for it, for which customer and when.
/// </summary>
/// <param name="CommandId">The command, also the idempotency key.</param>
/// <param name="CustomerId">The customer, or empty when the action is not about one.</param>
/// <param name="Reference">The reference shown in the queue.</param>
/// <param name="OccurredAtUtc">When the user did it.</param>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct ReceivablesEntryHeader(Guid CommandId, Guid CustomerId, string? Reference, DateTime OccurredAtUtc);
