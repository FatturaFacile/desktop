// <copyright file="GetCachedCopilotConversationsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Copilot;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Copilot;

/// <summary>
/// Handles <see cref="GetCachedCopilotConversationsQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetCachedCopilotConversationsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCachedCopilotConversationsQuery, IReadOnlyList<CopilotConversation>>
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<CopilotConversation>> Handle(GetCachedCopilotConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await unitOfWork.Repository<CopilotConversationEntity>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        return [.. conversations.OrderByDescending(static c => c.UpdatedAt ?? c.CreatedAt).Select(static c => CopilotCacheMapper.ToModel(c, []))];
    }
}
