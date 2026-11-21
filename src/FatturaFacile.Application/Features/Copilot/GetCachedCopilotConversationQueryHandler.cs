// <copyright file="GetCachedCopilotConversationQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Handles <see cref="GetCachedCopilotConversationQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetCachedCopilotConversationQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCachedCopilotConversationQuery, CopilotConversation?>
{
    /// <inheritdoc />
    public Task<CopilotConversation?> Handle(GetCachedCopilotConversationQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return HandleCoreAsync(request, cancellationToken);
    }

    private async Task<CopilotConversation?> HandleCoreAsync(GetCachedCopilotConversationQuery request, CancellationToken cancellationToken)
    {
        var found = await unitOfWork.Repository<CopilotConversationEntity>().FindAsync(c => c.RemoteId == request.RemoteId, cancellationToken).ConfigureAwait(false);
        var conversation = found.FirstOrDefault();

        if (conversation is null)
        {
            return null;
        }

        var messages = await unitOfWork.Repository<CopilotMessageEntity>().FindAsync(m => m.ConversationId == conversation.Id, cancellationToken).ConfigureAwait(false);
        return CopilotCacheMapper.ToModel(conversation, messages);
    }
}
