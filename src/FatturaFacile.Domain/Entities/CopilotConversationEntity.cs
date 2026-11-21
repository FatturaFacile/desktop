// <copyright file="CopilotConversationEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Local offline cache of an ERP Copilot conversation held on the server, so the history stays viewable without a connection.
/// </summary>
public class CopilotConversationEntity : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the local primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the conversation on the server.
    /// </summary>
    public string RemoteId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation locale (it, de, fr, en, ...).
    /// </summary>
    public string Locale { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UI route the conversation was started from.
    /// </summary>
    public string? ContextRoute { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity the conversation was started from.
    /// </summary>
    public string? ContextEntityType { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the entity the conversation was started from.
    /// </summary>
    public string? ContextEntityId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last synchronization with the server.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
