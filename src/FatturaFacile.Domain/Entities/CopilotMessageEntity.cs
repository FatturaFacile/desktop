// <copyright file="CopilotMessageEntity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Local offline cache of a message of an ERP Copilot conversation, including regulatory citations and structured payloads.
/// </summary>
public class CopilotMessageEntity : ITenantScopedEntity
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
    /// Gets or sets the local identifier of the owning conversation.
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the message on the server.
    /// </summary>
    public string RemoteId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message role (user, assistant, tool_call, tool_result).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Markdown content.
    /// </summary>
    public string ContentMarkdown { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the citations (regulatory snippets and entity links) as JSON.
    /// </summary>
    public string CitationsJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the visualization payload as JSON.
    /// </summary>
    public string? VisualizationJson { get; set; }

    /// <summary>
    /// Gets or sets the proposed action payload as JSON.
    /// </summary>
    public string? ProposedActionJson { get; set; }

    /// <summary>
    /// Gets or sets the status of the proposed action (pending, confirmed, rejected, executed, failed).
    /// </summary>
    public string? ActionStatus { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
