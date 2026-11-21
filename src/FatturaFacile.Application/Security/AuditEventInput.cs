// <copyright file="AuditEventInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// An event to append to the local audit queue.
/// </summary>
/// <param name="EntityType">The entity type the event is about.</param>
/// <param name="EntityId">The entity identifier.</param>
/// <param name="Action">The action.</param>
/// <param name="Payload">The details (JSON node, JSON text, serializable object or null); it is stored canonically.</param>
public sealed record AuditEventInput(string EntityType, string EntityId, string Action, object? Payload = null);
