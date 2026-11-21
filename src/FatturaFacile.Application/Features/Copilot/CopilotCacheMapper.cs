// <copyright file="CopilotCacheMapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Maps between the Copilot API models and the cached SQLite entities.
/// </summary>
internal static class CopilotCacheMapper
{
    /// <summary>
    /// Copies a conversation into a cache entity.
    /// </summary>
    /// <param name="source">The conversation.</param>
    /// <param name="target">The entity to fill.</param>
    public static void Apply(CopilotConversation source, CopilotConversationEntity target)
    {
        target.RemoteId = source.Id;
        target.Title = source.Title;
        target.Locale = source.Locale;
        target.ContextRoute = source.Context.Route;
        target.ContextEntityType = source.Context.EntityType;
        target.ContextEntityId = source.Context.EntityId;
        target.LastSyncedAt = DateTime.UtcNow;
        target.UpdatedAt = source.UpdatedAt?.UtcDateTime ?? DateTime.UtcNow;

        if (source.CreatedAt is { } created)
        {
            target.CreatedAt = created.UtcDateTime;
        }
    }

    /// <summary>
    /// Copies a message into a cache entity.
    /// </summary>
    /// <param name="source">The message.</param>
    /// <param name="target">The entity to fill.</param>
    public static void Apply(CopilotMessage source, CopilotMessageEntity target)
    {
        target.RemoteId = source.Id;
        target.Role = source.Role;
        target.ContentMarkdown = source.ContentMarkdown;
        target.CitationsJson = CopilotCacheSerializer.Serialize(source.Citations);
        target.VisualizationJson = source.Visualization is null ? null : CopilotCacheSerializer.Serialize(source.Visualization);
        target.ProposedActionJson = source.ProposedAction is null ? null : CopilotCacheSerializer.Serialize(source.ProposedAction);
        target.ActionStatus = source.ActionStatus;
        target.UpdatedAt = DateTime.UtcNow;

        if (source.CreatedAt is { } created)
        {
            target.CreatedAt = created.UtcDateTime;
        }
    }

    /// <summary>
    /// Builds a conversation model from a cache entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="messages">The cached messages.</param>
    /// <returns>The conversation.</returns>
    public static CopilotConversation ToModel(CopilotConversationEntity entity, IReadOnlyList<CopilotMessageEntity> messages) =>
        new(
            entity.RemoteId,
            entity.Title,
            entity.Locale,
            new CopilotContext(entity.ContextRoute, entity.ContextEntityType, entity.ContextEntityId),
            Utc(entity.CreatedAt),
            Utc(entity.UpdatedAt),
            [.. messages.OrderBy(static m => m.CreatedAt).Select(m => ToModel(entity.RemoteId, m))]);

    private static CopilotMessage ToModel(string conversationRemoteId, CopilotMessageEntity entity) =>
        new(
            entity.RemoteId,
            conversationRemoteId,
            entity.Role,
            entity.ContentMarkdown,
            CopilotCacheSerializer.Deserialize<List<CopilotCitation>>(entity.CitationsJson) ?? [],
            CopilotCacheSerializer.Deserialize<CopilotVisualization>(entity.VisualizationJson),
            CopilotCacheSerializer.Deserialize<CopilotProposedAction>(entity.ProposedActionJson),
            entity.ActionStatus,
            Utc(entity.CreatedAt));

    private static DateTimeOffset? Utc(DateTime? value) =>
        value is { } date ? new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc)) : null;
}
