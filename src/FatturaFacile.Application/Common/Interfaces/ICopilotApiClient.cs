// <copyright file="ICopilotApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Typed client of the ERP Copilot REST/SSE API (base path /api/v1/copilot). All calls need a connection; failures
/// are reported as <see cref="CopilotApiException"/>.
/// </summary>
public interface ICopilotApiClient
{
    /// <summary>
    /// Gets a value indicating whether a server address is configured, i.e. live calls can be attempted.
    /// </summary>
    public bool IsConfigured { get; }

    /// <summary>
    /// Lists the conversations of the caller.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversations without messages.</returns>
    public Task<IReadOnlyList<CopilotConversation>> GetConversationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a conversation.
    /// </summary>
    /// <param name="title">The optional title.</param>
    /// <param name="locale">The locale.</param>
    /// <param name="context">The context the conversation starts from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created conversation.</returns>
    public Task<CopilotConversation> CreateConversationAsync(string? title, string locale, CopilotContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a conversation with its messages.
    /// </summary>
    /// <param name="conversationId">The server identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversation.</returns>
    public Task<CopilotConversation> GetConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a conversation on the server.
    /// </summary>
    /// <param name="conversationId">The server identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the call.</returns>
    public Task DeleteConversationAsync(string conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a prompt and streams the assistant turn as server-sent events.
    /// </summary>
    /// <param name="conversationId">The server identifier of the conversation.</param>
    /// <param name="prompt">The prompt.</param>
    /// <param name="locale">The locale.</param>
    /// <param name="context">The active context, injected into the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The events of the turn.</returns>
    public IAsyncEnumerable<CopilotStreamEvent> StreamMessageAsync(string conversationId, string prompt, string locale, CopilotContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a proposed action, optionally overriding editable fields. The call is idempotent.
    /// </summary>
    /// <param name="messageId">The server identifier of the message carrying the action.</param>
    /// <param name="overrides">New values by dotted path, only paths listed in the editable fields.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated message.</returns>
    public Task<CopilotMessage> ConfirmActionAsync(string messageId, IReadOnlyDictionary<string, object?>? overrides, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a proposed action.
    /// </summary>
    /// <param name="messageId">The server identifier of the message carrying the action.</param>
    /// <param name="reason">The optional reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated message.</returns>
    public Task<CopilotMessage> RejectActionAsync(string messageId, string? reason, CancellationToken cancellationToken = default);
}
