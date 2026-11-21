// <copyright file="DeleteCachedCopilotConversationCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Handles <see cref="DeleteCachedCopilotConversationCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class DeleteCachedCopilotConversationCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteCachedCopilotConversationCommand, bool>
{
    /// <inheritdoc />
    public Task<bool> Handle(DeleteCachedCopilotConversationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return HandleCoreAsync(request, cancellationToken);
    }

    private async Task<bool> HandleCoreAsync(DeleteCachedCopilotConversationCommand request, CancellationToken cancellationToken)
    {
        var conversations = unitOfWork.Repository<CopilotConversationEntity>();
        var conversation = (await conversations.FindAsync(c => c.RemoteId == request.RemoteId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (conversation is null)
        {
            return false;
        }

        var messages = unitOfWork.Repository<CopilotMessageEntity>();

        foreach (var message in await messages.FindAsync(m => m.ConversationId == conversation.Id, cancellationToken).ConfigureAwait(false))
        {
            messages.Delete(message);
        }

        conversations.Delete(conversation);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
