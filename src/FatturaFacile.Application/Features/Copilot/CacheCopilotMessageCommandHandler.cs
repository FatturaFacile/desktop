// <copyright file="CacheCopilotMessageCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Handles <see cref="CacheCopilotMessageCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class CacheCopilotMessageCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CacheCopilotMessageCommand, bool>
{
    /// <inheritdoc />
    public Task<bool> Handle(CacheCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return HandleCoreAsync(request, cancellationToken);
    }

    private async Task<bool> HandleCoreAsync(CacheCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        var message = request.Message;
        var conversation = (await unitOfWork.Repository<CopilotConversationEntity>().FindAsync(c => c.RemoteId == message.ConversationId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (conversation is null)
        {
            return false;
        }

        var messages = unitOfWork.Repository<CopilotMessageEntity>();
        var entity = (await messages.FindAsync(m => m.ConversationId == conversation.Id && m.RemoteId == message.Id, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

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

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
