// <copyright file="CacheCopilotConversationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Handles <see cref="CacheCopilotConversationCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class CacheCopilotConversationCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CacheCopilotConversationCommand, bool>
{
    /// <inheritdoc />
    public Task<bool> Handle(CacheCopilotConversationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return HandleCoreAsync(request, cancellationToken);
    }

    private async Task<bool> HandleCoreAsync(CacheCopilotConversationCommand request, CancellationToken cancellationToken)
    {
        var conversations = unitOfWork.Repository<CopilotConversationEntity>();
        var messages = unitOfWork.Repository<CopilotMessageEntity>();
        var remote = request.Conversation;

        var conversation = (await conversations.FindAsync(c => c.RemoteId == remote.Id, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (conversation is null)
        {
            conversation = new CopilotConversationEntity();
            CopilotCacheMapper.Apply(remote, conversation);
            await conversations.AddAsync(conversation, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            CopilotCacheMapper.Apply(remote, conversation);
            conversations.Update(conversation);
        }

        if (remote.Messages.Count > 0)
        {
            var existing = await messages.FindAsync(m => m.ConversationId == conversation.Id, cancellationToken).ConfigureAwait(false);

            foreach (var message in remote.Messages)
            {
                var entity = existing.FirstOrDefault(m => string.Equals(m.RemoteId, message.Id, StringComparison.Ordinal));

                if (entity is null)
                {
                    entity = new CopilotMessageEntity { ConversationId = conversation.Id };
                    CopilotCacheMapper.Apply(message, entity);
                    await messages.AddAsync(entity, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    CopilotCacheMapper.Apply(message, entity);
                    messages.Update(entity);
                }
            }
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
