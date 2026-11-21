// <copyright file="AuditAcknowledgement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Server acknowledgement of one uploaded entry.
/// </summary>
/// <param name="SequenceNumber">The acknowledged sequence number.</param>
/// <param name="ServerReceivedAt">The server timestamp (UTC) of the entry.</param>
/// <param name="ServerReference">The identifier assigned by the server, if any.</param>
public sealed record AuditAcknowledgement(long SequenceNumber, DateTime ServerReceivedAt, string? ServerReference);
