// <copyright file="FieldServiceQueueResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The outcome of queueing a field operation: it is always saved locally, and sent at once when possible.
/// </summary>
/// <param name="ErrorKey">The localization key of the validation error, or null when the operation was queued.</param>
/// <param name="Sent">Whether the server already applied it.</param>
/// <param name="Conflict">Whether the server rejected it.</param>
/// <param name="ClientUuid">The idempotency key of the operation (empty on validation errors).</param>
public sealed record FieldServiceQueueResult(string? ErrorKey, bool Sent, bool Conflict, string ClientUuid);
